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
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        public AdminController(IAdminService admin) => _admin = admin;

        [HttpGet("quizzes/deleted")]
        public async Task<ActionResult<List<DeletedQuizDto>>> GetDeleted(
            [FromHeader(Name = "X-Admin-Key")] string? adminKey) =>
            Ok(await _admin.GetDeletedQuizzesAsync(adminKey));

        [HttpPost("quizzes/{id:int}/restore")]
        public async Task<IActionResult> Restore(int id, [FromHeader(Name = "X-Admin-Key")] string? adminKey)
        {
            await _admin.RestoreQuizAsync(adminKey, id);
            return NoContent();
        }

        [HttpDelete("quizzes/{id:int}")]
        public async Task<IActionResult> Purge(int id, [FromHeader(Name = "X-Admin-Key")] string? adminKey)
        {
            await _admin.PurgeQuizAsync(adminKey, id);
            return NoContent();
        }
    }
}

