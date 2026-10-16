using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.Extensions;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto) =>
            Ok(await _auth.RegisterAsync(dto));

        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto) =>
            Ok(await _auth.LoginAsync(dto));
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await _auth.ChangePasswordAsync(User.GetUserId(), dto);
            return NoContent();
        }
        [HttpPost("admin/reset-password")]
        public async Task<IActionResult> AdminResetPassword(
        [FromHeader(Name = "X-Admin")] string? adminKey,
         AdminResetPasswordDto dto)
        {
            await _auth.ResetPasswordAsync(adminKey, dto);
            return NoContent();
        }

    }
}