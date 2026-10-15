using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quiz.Extensions;

namespace Presentation.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _game;
        public GamesController(IGameService game) => _game = game;

        // المدرّس بيعمل جيم جديد ويحدد مدته بالدقايق، ويرجعله PIN
        // body: { "durationMinutes": 15 }
        [Authorize(Roles ="Player,Admin")]
        [HttpPost("{quizId:int}/sessions")]
        public async Task<ActionResult<SessionCreatedDto>> CreateSession(int quizId, CreateSessionDto dto) =>
            Ok(await _game.CreateSessionAsync(User.GetUserId(), quizId, dto.DurationMinutes));

        // الـ Leaderboard الحالي
        [HttpGet("sessions/{sessionId:int}/leaderboard")]
        public async Task<ActionResult<List<PlayerScoreDto>>> Leaderboard(int sessionId) =>
            Ok(await _game.GetLeaderboardAsync(sessionId));
    }
}

