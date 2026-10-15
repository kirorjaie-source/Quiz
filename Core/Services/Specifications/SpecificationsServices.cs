using Domain.Contract;
using Domain.Contracts;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    internal class SpecificationsServices
    {
        // ---- Auth ----
        public class UserByEmailSpec : BaseSpecifications<User, int>
        {
            public UserByEmailSpec(string email) : base(u => u.Email == email) { }
        }

        // ---- Quiz ----
        public class PublicQuizzesSpec : BaseSpecifications<Quiz, int>
        {
            public PublicQuizzesSpec(string? search, QuizCategory? category)
                : base(q => !q.IsDeleted && q.IsPublic
                    && (string.IsNullOrEmpty(search) || q.Title.Contains(search))
                    && (!category.HasValue || q.QuizCategory == category.Value))
            {
                AddInclude(q => q.Questions);
                AddOrderByDescending(q => q.CreatedAt);
            }
        }

        public class MyQuizzesSpec : BaseSpecifications<Quiz, int>
        {
            public MyQuizzesSpec(int userId) : base(q => !q.IsDeleted && q.CreatorId == userId)
            {
                AddInclude(q => q.Questions);
                AddOrderByDescending(q => q.CreatedAt);
            }
        }

        public class QuizByIdSpec : BaseSpecifications<Quiz, int>
        {
            public QuizByIdSpec(int id) : base(q => q.Id == id && !q.IsDeleted) { }
        }

        public class QuestionsByQuizSpec : BaseSpecifications<Question, int>   // للعدّ بس (CountAsync/AnyAsync)
        {
            public QuestionsByQuizSpec(int quizId) : base(q => q.QuizId == quizId) { }
        }

        public class QuestionWithQuizSpec : BaseSpecifications<Question, int>
        {
            public QuestionWithQuizSpec(int id) : base(q => q.Id == id) => AddInclude(q => q.Quiz);
        }

        // ---- Game ----
        public class SessionByIdSpec : BaseSpecifications<GameSession, int>
        {
            public SessionByIdSpec(int id) : base(s => s.Id == id) { }
        }

        public class ActiveSessionByPinSpec : BaseSpecifications<GameSession, int>   // أي جيم لسه ما خلصش
        {
            public ActiveSessionByPinSpec(string pin) : base(s => s.PinCode == pin && s.Status !=SessionStatus.Finished ) { }
        }

        public class WaitingSessionByPinSpec : BaseSpecifications<GameSession, int>
        {
            public WaitingSessionByPinSpec(string pin) : base(s => s.PinCode == pin && s.Status == SessionStatus.Waiting) { }
        }

        public class ExpiredSessionsSpec : BaseSpecifications<GameSession, int>
        {
            public ExpiredSessionsSpec(DateTime now)
                : base(s => s.Status == SessionStatus.Running && s.EndAt != null && s.EndAt <= now) { }
        }

        public class PlayerByNicknameSpec : BaseSpecifications<Player, int>
        {
            public PlayerByNicknameSpec(int sessionId, string playername)
                : base(p => p.GameSessionId == sessionId && p.PlayerName == playername) { }
        }

        public class PlayerWithSessionSpec : BaseSpecifications<Player, int>
        {
            public PlayerWithSessionSpec(int playerId) : base(p => p.Id == playerId)
                => AddInclude(p => p.GameSession);
        }

        public class PlayerByTokenSpec : BaseSpecifications<Player, int>
        {
            public PlayerByTokenSpec(Guid token, string pin)
                : base(p => p.ReconnectToken == token && p.GameSession.PinCode == pin)
                => AddInclude(p => p.GameSession);
        }

        public class LeaderboardSpec : BaseSpecifications<Player, int>
        {
            public LeaderboardSpec(int sessionId) : base(p => p.GameSessionId == sessionId)
                => AddOrderByDescending(p => p.TotalScore);
        }

        public class PlayersAheadSpec : BaseSpecifications<Player, int>      // للترتيب: كام لاعب نقاطه أعلى
        {
            public PlayersAheadSpec(int sessionId, int score)
                : base(p => p.GameSessionId == sessionId && p.TotalScore > score) { }
        }

        public class UnfinishedPlayersSpec : BaseSpecifications<Player, int>
        {
            public UnfinishedPlayersSpec(int sessionId)
                : base(p => p.GameSessionId == sessionId && p.FinishedAt == null) { }
        }

        // السؤال رقم (index) في الكويز: ترتيب + Skip + Take(1) + الاختيارات
        public class QuestionAtIndexSpec : BaseSpecifications<Question, int>
        {
            public QuestionAtIndexSpec(int quizId, int index) : base(q => q.QuizId == quizId)
            {
                AddOrderBy(q => q.OrderNumber);
                ApplyPaging(index, 1);
                AddInclude(q => q.Answers);
            }
        }
    }
}
