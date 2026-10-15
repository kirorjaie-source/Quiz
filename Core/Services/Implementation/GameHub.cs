using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Services.Implementation
{
    public class GameHub : Hub
    {
        private readonly IGameService _game;
        public GameHub(IGameService game) => _game = game;

        private int CurrentUserId =>
            int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // ---- المدرّس: السيرفر بيحفظ الجيم بتاعه على الـ connection بعد التحقق ----
        private (int SessionId, string Pin) Host =>
            Context.Items["hostSessionId"] is int sid && Context.Items["hostPin"] is string pin
                ? (sid, pin)
                : throw new HubException("Call HostJoin first");

        // ---- اللاعب: السيرفر بيحفظ هويته على الـ connection (الكلاينت مايبعتش playerId) ----
        private (int PlayerId, int SessionId, string Pin) Me =>
            Context.Items["playerId"] is int pid
            && Context.Items["sessionId"] is int sid
            && Context.Items["pin"] is string pin
                ? (pid, sid, pin)
                : throw new HubException("You must join a game first");

        private void SaveMe(PlayerIdentityDto player, string pin)
        {
            Context.Items["playerId"] = player.PlayerId;
            Context.Items["sessionId"] = player.SessionId;
            Context.Items["pin"] = pin;
        }

        // ===== المدرّس =====
        [Authorize]
        public async Task HostJoin(string pin)
        {
            var sessionId = await _game.VerifyHostAsync(pin, CurrentUserId); // لازم يكون صاحب الجيم
            Context.Items["hostSessionId"] = sessionId;
            Context.Items["hostPin"] = pin;
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
        }

        // بيبدأ اللعبة للكل، واللاعبين بعدها بيطلبوا أول سؤال بتاعهم
        [Authorize]
        public async Task StartGame()
        {
            var host = Host;
            var started = await _game.StartAsync(host.SessionId, CurrentUserId);
            await Clients.Group(host.Pin).SendAsync("GameStarted", started);
        }

        [Authorize]
        public async Task EndGame()
        {
            var host = Host;
            await _game.EndSessionAsync(host.SessionId, CurrentUserId);
            var board = await _game.GetLeaderboardAsync(host.SessionId);
            await Clients.Group(host.Pin).SendAsync("GameFinished", board);
        }

        // ===== اللاعب (من غير حساب) =====
        public async Task JoinGame(string pin, string nickname)
        {
            var player = await _game.JoinAsync(pin, nickname);
            SaveMe(player, pin);
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);

            // الفرونت يحفظ reconnectToken في localStorage علشان يرجع بيه لو النت فصل
            await Clients.Caller.SendAsync("Joined", new { reconnectToken = player.ReconnectToken });
            await Clients.Group(pin).SendAsync("PlayerJoined", nickname);
        }

        public async Task RejoinGame(string pin, Guid reconnectToken)
        {
            var player = await _game.RejoinAsync(pin, reconnectToken);
            SaveMe(player, pin);
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            await GetMyQuestion();
        }

        // أول سؤال (بعد GameStarted) أو السؤال الحالي بعد إعادة الاتصال
        public async Task GetMyQuestion()
        {
            var q = await _game.GetPlayerQuestionAsync(Me.PlayerId);
            if (q == null) await Clients.Caller.SendAsync("QuizFinished");
            else await Clients.Caller.SendAsync("QuestionStarted", q);
        }

        // بيرجع النتيجة + السؤال الجاي في نفس الرد (من غير ما حد يدوس Next)
        public async Task SubmitAnswer(int optionId)
        {
            var me = Me;
            await HandleResponse(me.Pin, me.SessionId, await _game.SubmitAnswerAsync(me.PlayerId, optionId));
        }

        public async Task SkipQuestion()
        {
            var me = Me;
            await HandleResponse(me.Pin, me.SessionId, await _game.SkipQuestionAsync(me.PlayerId));
        }

        private async Task HandleResponse(string pin, int sessionId, SubmitAnswerResponseDto resp)
        {
            await Clients.Caller.SendAsync("AnswerResult", resp);

            // المدرّس يشوف إن لاعب خلص + الترتيب بيتحدث
            var board = await _game.GetLeaderboardAsync(sessionId);
            await Clients.Group(pin).SendAsync("Leaderboard", board);

            if (resp.GameEnded)
                await Clients.Group(pin).SendAsync("GameFinished", board);
        }
    }
}
