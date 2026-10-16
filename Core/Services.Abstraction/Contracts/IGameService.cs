using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstraction.Contracts
{
    public interface IGameService
    {
        Task<SessionCreatedDto> CreateSessionAsync(int hostId, int quizId, int durationMinutes);
        Task<PlayerIdentityDto> JoinAsync(string pinCode, string nickname);
        Task<PlayerIdentityDto> RejoinAsync(string pinCode, Guid reconnectToken);
        Task<int> VerifyHostAsync(string pinCode, int hostId);  
        Task<GameStartedDto> StartAsync(int sessionId, int hostId);
        Task EndSessionAsync(int sessionId, int hostId);

        Task<QuestionForPlayerDto?> GetPlayerQuestionAsync(int playerId);
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int playerId, int optionId);
       Task<SubmitAnswerResponseDto> SkipQuestionAsync(int playerId);

        Task<List<PlayerScoreDto>> GetLeaderboardAsync(int sessionId);
        Task<List<(int SessionId, string Pin)>> CloseExpiredSessionsAsync();
    }
}
