using Microsoft.AspNetCore.Mvc;
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

        // تسجيل مدرّس جديد
        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto) =>
            Ok(await _auth.RegisterAsync(dto));

        // تسجيل دخول مدرّس موجود
        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto) =>
            Ok(await _auth.LoginAsync(dto));
    }
}