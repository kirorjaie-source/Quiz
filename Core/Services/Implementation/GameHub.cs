using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class GameHub : Hub
    {
        private readonly IGameService _game;
        public GameHub(IGameService game) => _game = game;

        private int CurrentUserId =>
            int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        private (int SessionId, string Pin) Host =>
            Context.Items["hostSessionId"] is int sid && Context.Items["hostPin"] is string pin
                ? (sid, pin)
                : throw new HubException("Call HostJoin first");

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

        [Authorize]
        public async Task HostJoin(string pin)
        {
            var sessionId = await _game.VerifyHostAsync(pin, CurrentUserId);
            Context.Items["hostSessionId"] = sessionId;
            Context.Items["hostPin"] = pin;
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
        }

        [Authorize]
        public async Task StartGame()
        {
            var host = Host;
            try
            {
                var started = await _game.StartAsync(host.SessionId, CurrentUserId);
                await Clients.Group(host.Pin).SendAsync("GameStarted", started);
            }
            catch (Exception ex) when (ex.Message.Contains("already started") || ex is InvalidOperationException)
            {
            }
        }

        [Authorize]
        public async Task EndGame()
        {
            var host = Host;
            await _game.EndSessionAsync(host.SessionId, CurrentUserId);
            var board = await _game.GetLeaderboardAsync(host.SessionId);
            await Clients.Group(host.Pin).SendAsync("GameFinished", board);
        }

        public async Task JoinGame(string pin, string nickname)
        {
            var player = await _game.JoinAsync(pin, nickname);
            SaveMe(player, pin);
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);

            await Clients.Caller.SendAsync("Joined", new { reconnectToken = player.ReconnectToken });
            await Clients.Group(pin).SendAsync("PlayerJoined", nickname);

            try
            {
                var q = await _game.GetPlayerQuestionAsync(player.PlayerId);
                if (q != null)
                {
                    await Clients.Caller.SendAsync("QuestionStarted", q);
                }
            }
            catch
            {
            }
        }

        public async Task RejoinGame(string pin, Guid reconnectToken)
        {
            var player = await _game.RejoinAsync(pin, reconnectToken);
            SaveMe(player, pin);
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);
            await GetMyQuestion();
        }

        public async Task GetMyQuestion()
        {
            var q = await _game.GetPlayerQuestionAsync(Me.PlayerId);
            if (q == null) await Clients.Caller.SendAsync("QuizFinished");
            else await Clients.Caller.SendAsync("QuestionStarted", q);
        }

        public async Task SubmitAnswer(int optionId)
        {
            try
            {
                var me = Me;
                var result = await _game.SubmitAnswerAsync(me.PlayerId, optionId);
                await HandleResponse(me.Pin, me.SessionId, result);
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                throw new HubException("Exist Error");
            }
        }

        public async Task SkipQuestion()
        {
            var me = Me;
            var result = await _game.SkipQuestionAsync(me.PlayerId);
            await HandleResponse(me.Pin, me.SessionId, result);
        }

        private async Task HandleResponse(string pin, int sessionId, SubmitAnswerResponseDto resp)
        {
            await Clients.Caller.SendAsync("AnswerResult", resp);

            var board = await _game.GetLeaderboardAsync(sessionId);
            await Clients.Group(pin).SendAsync("Leaderboard", board);

            if (resp.GameEnded)
                await Clients.Group(pin).SendAsync("GameFinished", board);
        }
    }
}