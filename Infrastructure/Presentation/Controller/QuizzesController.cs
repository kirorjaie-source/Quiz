using Domain.Contract;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.Extensions;
using Services.Specifications;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizzes;
        public QuizzesController(IQuizService quizzes) => _quizzes = quizzes;

        [HttpGet("categories")]
        public IActionResult GetCategories() => Ok(Enum.GetNames<QuizCategory>());

        [HttpGet("public")]
        public async Task<ActionResult<List<QuizSummaryDto>>> GetPublic(string? search, QuizCategory? category) =>
            Ok(await _quizzes.GetPublicAsync(search, category));

        [Authorize]
        [HttpGet("mine")]
        public async Task<ActionResult<List<QuizSummaryDto>>> GetMine() =>
            Ok(await _quizzes.GetMineAsync(User.GetUserId()));

        [Authorize(Roles = "Admin, Creator")]
        [HttpPost]
        public async Task<IActionResult> Create(QuizCreateDto dto)
        {
            var id = await _quizzes.CreateAsync(User.GetUserId(), dto);
            return Ok(new { id });
        }

        [Authorize(Roles = "Admin, Creator")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, QuizCreateDto dto)
        {

            var isAdmin = User.IsInRole("Admin");
            await _quizzes.UpdateAsync(User.GetUserId(), id, dto, isAdmin);
            return NoContent();
        }

        [Authorize(Roles = "Admin, Creator")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            await _quizzes.DeleteAsync(User.GetUserId(), id, isAdmin);
            return NoContent();
        }

        [Authorize(Roles = "Admin, Creator")]
        [HttpPost("{quizId:int}/questions")]
        public async Task<IActionResult> AddQuestion(int quizId, QuestionCreateDto dto)
        {
            var isAdmin = User.IsInRole("Admin");
            var id = await _quizzes.AddQuestionAsync(User.GetUserId(), quizId, dto, isAdmin);
            return Ok(new { id });
        }

        [Authorize(Roles = "Admin, Creator")]
        [HttpDelete("questions/{questionId:int}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var isAdmin = User.IsInRole("Admin");
            await _quizzes.DeleteQuestionAsync(User.GetUserId(), questionId, isAdmin);
            return NoContent();
        }
    }
}