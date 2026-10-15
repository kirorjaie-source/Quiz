using Domain.Contract;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            // تعديل: إدراج issuer و audience المقروءين من appsettings.json
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}