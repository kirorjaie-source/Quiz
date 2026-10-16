using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Abstraction.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class GameTimerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IHubContext<GameHub> _hub;

        public GameTimerService(IServiceScopeFactory scopes, IHubContext<GameHub> hub)
        {
            _scopes = scopes;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var game = scope.ServiceProvider.GetRequiredService<IGameService>();

                    foreach (var (sessionId, pin) in await game.CloseExpiredSessionsAsync())
                    {
                        var board = await game.GetLeaderboardAsync(sessionId);
                        await _hub.Clients.Group(pin).SendAsync("GameFinished", board);
                    }
                }
                catch (Exception) { }

                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
        }
    }
}
