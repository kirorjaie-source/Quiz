using Domain.Contract;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Abstraction.Contracts;
using Services.Specifications;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUniteOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var repo = _unitOfWork.GetRepository<User, int>();

            var existing = await repo.GetAsync(new Specifications.UserByEmailSpec(registerDto.Email));
            if (existing is not null)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                roles = registerDto.rolesDto
            };

            await repo.AddAsync(user);
            await _unitOfWork.SaveChangeAsync();

            return new AuthResultDto(CreateToken(user), user.Id, user.UserName);
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _unitOfWork.GetRepository<User, int>()
                 .GetAsync(new Specifications.UserByEmailSpec(loginDto.Email));

            if (user is null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            return new AuthResultDto(CreateToken(user), user.Id, user.UserName);
        }

        private string CreateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role,user.roles.ToString()??"Player")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _unitOfWork.GetRepository<User, int>().GetAsync(userId)
                       ?? throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task ResetPasswordAsync(string? adminKey, AdminResetPasswordDto dto)
        {
            var expected = _configuration["Admin:Key"];
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(adminKey) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(adminKey), Encoding.UTF8.GetBytes(expected)))
                throw new UnauthorizedAccessException("Invalid admin key");

            var user = await _unitOfWork.GetRepository<User, int>()
                           .GetAsync(new UserByEmailSpec(dto.Email.Trim().ToLower()))
                       ?? throw new KeyNotFoundException("User not found");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}