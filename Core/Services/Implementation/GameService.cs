using Domain.Contract;
using Domain.Contracts;
using Domain.Entities;
using Services.Abstraction.Contracts;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Services.Specifications.SpecificationsServices;

namespace Services.Implementation
{
    public class GameService : IGameService
    {
        private readonly IUniteOfWork _uow;
        private readonly IScoringService _scoring;

        public GameService(IUniteOfWork uow, IScoringService scoring)
        {
            _uow = uow;
            _scoring = scoring;
        }

        private IGenericRepository<GameSession, int> Sessions => _uow.GetRepository<GameSession, int>();
        private IGenericRepository<Player, int> Players => _uow.GetRepository<Player, int>();
        private IGenericRepository<Question, int> Questions => _uow.GetRepository<Question, int>();

        // ===== المدرّس =====
        public async Task<SessionCreatedDto> CreateSessionAsync(int hostId, int quizId, int durationMinutes)
        {
            if (durationMinutes < 1 || durationMinutes > 180)
                throw new InvalidOperationException("Duration must be between 1 and 180 minutes");

            if (!await Questions.AnyAsync(new QuestionsByQuizSpec(quizId)))
                throw new InvalidOperationException("Quiz has no questions");

            var session = new GameSession
            {
                QuizId = quizId,
                HostId = hostId,
                PinCode = await GeneratePinAsync(),
                Status = SessionStatus.Waiting,
                DurationMinute = durationMinutes
            };
            await Sessions.AddAsync(session);
            await _uow.SaveChangeAsync();
            return new SessionCreatedDto { 
               SessionId=session.Id,
                PinCode= session.PinCode, 
                DurationMinutes=durationMinutes };
        }

        public async Task<int> VerifyHostAsync(string pinCode, int hostId)
        {
            var session = await Sessions.GetAsync(new ActiveSessionByPinSpec(pinCode))
                          ?? throw new KeyNotFoundException("Game not found");
            if (session.HostId != hostId) throw new UnauthorizedAccessException();
            return session.Id;
        }

        public async Task<GameStartedDto> StartAsync(int sessionId, int hostId)
        {
            var session = await GetHostedSession(sessionId, hostId);
            if (session.Status != SessionStatus.Waiting)
                throw new InvalidOperationException("Game already started");

            session.Status = SessionStatus.Running;
            session.StartedAt = DateTime.UtcNow;
            session.EndAt = DateTime.UtcNow.AddMinutes(session.DurationMinute);
            await _uow.SaveChangeAsync();

            return new GameStartedDto {
               EndAt= session.EndAt.Value,
             DurationMinutes= session.DurationMinute };
        }

        public async Task EndSessionAsync(int sessionId, int hostId)
        {
            var session = await GetHostedSession(sessionId, hostId);
            if (session.Status == SessionStatus.Finished) return;

            session.Status = SessionStatus.Finished;
            session.EndAt = DateTime.UtcNow;
            await _uow.SaveChangeAsync();
        }

        // ===== اللاعبين =====
        public async Task<PlayerIdentityDto> JoinAsync(string pinCode, string nickname)
        {
            // 🔴 استخدام ActiveSessionByPinSpec بدلاً من WaitingSessionByPinSpec
            var session = await Sessions.GetAsync(new ActiveSessionByPinSpec(pinCode))
                          ?? throw new KeyNotFoundException("Game not found or already finished");

            nickname = nickname.Trim();
            if (string.IsNullOrEmpty(nickname) || nickname.Length > 20)
                throw new InvalidOperationException("Nickname is required (max 20 characters)");

            if (await Players.AnyAsync(new PlayerByNicknameSpec(session.Id, nickname)))
                throw new InvalidOperationException("Nickname already taken");

            var player = new Player
            {
                GameSessionId = session.Id,
                PlayerName = nickname,
                CurrentQuestionIdex = 0,
                ReconnectToken = Guid.NewGuid()   // سر اللاعب، بيستخدمه بس لو النت فصل
            };

            await Players.AddAsync(player);
            await _uow.SaveChangeAsync();

            return new PlayerIdentityDto(player.Id, session.Id, player.ReconnectToken);
        }

        public async Task<PlayerIdentityDto> RejoinAsync(string pinCode, Guid reconnectToken)
        {
            var player = await Players.GetAsync(new PlayerByTokenSpec(reconnectToken, pinCode))
                         ?? throw new KeyNotFoundException("Player not found");
            return new PlayerIdentityDto(player.Id, player.GameSessionId, player.ReconnectToken);
        }

        public async Task<QuestionForPlayerDto?> GetPlayerQuestionAsync(int playerId)
        {
            var (player, session) = await LoadPlayerAsync(playerId);
            EnsureRunning(session);

            if (player.FinishedAt != null) return null;

            // التايمر بتاع السؤال بيبدأ أول مرة يتطلب فيها فقط
            if (player.QuestionStartAt == null)
            {
                player.QuestionStartAt = DateTime.UtcNow;
                await _uow.SaveChangeAsync();
            }
            return await BuildQuestion(session.QuizId, player.CurrentQuestionIdex);
        }

        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int playerId, int optionId)
        {
            var (player, session) = await LoadPlayerAsync(playerId);
            EnsureRunning(session);
            if (player.FinishedAt != null)
                throw new InvalidOperationException("You already finished the quiz");

            var question = await GetQuestionAt(session.QuizId, player.CurrentQuestionIdex);

            var option = question.Answers.FirstOrDefault(o => o.Id == optionId)
                         ?? throw new InvalidOperationException("Invalid option for this question");

            // الوقت بيتحسب في السيرفر علشان محدش يغش
            var elapsed = (DateTime.UtcNow - (player.QuestionStartAt ?? DateTime.UtcNow)).TotalSeconds;
            var inTime = elapsed <= question.TimeLimitInSeconds + 1; // 1 ثانية سماح للشبكة
            var isCorrect = option.IsCorrect && inTime;

            var points = _scoring.Calculate(isCorrect, elapsed, question.TimeLimitInSeconds, question.Points, player.AnswerStreak);

            player.AnswerStreak = isCorrect ? player.AnswerStreak + 1 : 0;
            player.TotalScore += points;

            await _uow.GetRepository<PlayerAnswer, int>().AddAsync(new PlayerAnswer
            {
                PlayerId = playerId,
                QuestionId = question.Id,
                SelectedAnswerId = optionId,
                IsCorrect = isCorrect,
                ResponseTimeInSeconds = elapsed,
                PointsEarned = points
            });

            var result = new AnswerResultDto { 
               IsCorrect= isCorrect,
               PointsEarned= points,
                TotalScore= player.TotalScore,
                Streak= player.AnswerStreak };
            return await AdvanceAsync(player, session, result);
        }

        // الكلاينت بيندهها لما وقت السؤال يخلص من غير ما اللاعب يجاوب
        public async Task<SubmitAnswerResponseDto> SkipQuestionAsync(int playerId)
        {
            var (player, session) = await LoadPlayerAsync(playerId);
            EnsureRunning(session);
            if (player.FinishedAt != null)
                throw new InvalidOperationException("You already finished the quiz");

            var question = await GetQuestionAt(session.QuizId, player.CurrentQuestionIdex);

            var elapsed = (DateTime.UtcNow - (player.QuestionStartAt ?? DateTime.UtcNow)).TotalSeconds;
            if (elapsed < question.TimeLimitInSeconds - 1)
                throw new InvalidOperationException("Question time has not expired yet");

            player.AnswerStreak = 0;
            await _uow.GetRepository<PlayerAnswer, int>().AddAsync(new PlayerAnswer
            {
                PlayerId = playerId,
                QuestionId = question.Id,
                SelectedAnswer= null,
                IsCorrect = false,
                ResponseTimeInSeconds = question.TimeLimitInSeconds,
                PointsEarned = 0
            });

            var result = new AnswerResultDto {
                 IsCorrect=false,PointsEarned= 0,TotalScore= player.TotalScore,Streak= 0 };
            return await AdvanceAsync(player, session, result);
        }

        public async Task<List<PlayerScoreDto>> GetLeaderboardAsync(int sessionId)
        {
            var players = await Players.GetAllAsync(new LeaderboardSpec(sessionId));
            return players.Select(p => new PlayerScoreDto {PlayerId= p.Id,PlayerName= p.PlayerName,TotalScore= p.TotalScore,Streak= p.AnswerStreak }).ToList();
        }

        // الـ Timer بيندهها كل كام ثانية: بتقفل أي لعبة وقتها خلص
        public async Task<List<(int SessionId, string Pin)>> CloseExpiredSessionsAsync()
        {
            var expired = (await Sessions.GetAllAsync(new ExpiredSessionsSpec(DateTime.UtcNow))).ToList();

            foreach (var s in expired)
            {
                s.Status = SessionStatus.Finished;
                s.EndAt = s.EndAt;
            }
            if (expired.Count > 0) await _uow.SaveChangeAsync();

            return expired.Select(s => (s.Id, s.PinCode)).ToList();
        }

        // ---- helpers ----
        // بينقل اللاعب للسؤال اللي بعده (أو بينهي الكويز بتاعه)
        private async Task<SubmitAnswerResponseDto> AdvanceAsync(Player player, GameSession session, AnswerResultDto result)
        {
            var total = await Questions.CountAsync(new QuestionsByQuizSpec(session.QuizId));
            player.CurrentQuestionIdex++;

            if (player.CurrentQuestionIdex >= total)
            {
                player.FinishedAt = DateTime.UtcNow;
                player.QuestionStartAt = null;
                await _uow.SaveChangeAsync();

                var rank = 1 + await Players.CountAsync(new PlayersAheadSpec(session.Id, player.TotalScore));

                // لو كل اللاعبين خلصوا، اقفل اللعبة بدري
                var anyoneLeft = await Players.AnyAsync(new UnfinishedPlayersSpec(session.Id));
                if (!anyoneLeft)
                {
                    session.Status = SessionStatus.Finished;
                    session.EndAt = DateTime.UtcNow;
                    await _uow.SaveChangeAsync();
                }

                return new SubmitAnswerResponseDto { 
                    Result=result,
                    NextQuestion=null,Finished= true,Rank= rank,GameEnded=!anyoneLeft };
            }

            player.QuestionStartAt = DateTime.UtcNow; // تايمر السؤال الجديد
            await _uow.SaveChangeAsync();

            var next = await BuildQuestion(session.QuizId, player.CurrentQuestionIdex);
            return new SubmitAnswerResponseDto {Result=result,NextQuestion= next,Finished= false, Rank=null,GameEnded= false };
        }

        private async Task<(Player, GameSession)> LoadPlayerAsync(int playerId)
        {
            var player = await Players.GetAsync(new PlayerWithSessionSpec(playerId))
                         ?? throw new KeyNotFoundException("Player not found");
            return (player, player.GameSession);
        }

        private static void EnsureRunning(GameSession session)
        {
            if (session.Status != SessionStatus.Running)
                throw new InvalidOperationException("Game is not running");
            if (session.EndAt != null && session.EndAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Time is over"); // الـ Timer هيقفلها
        }

        private async Task<GameSession> GetHostedSession(int sessionId, int hostId)
        {
            var session = await Sessions.GetAsync(new SessionByIdSpec(sessionId))
                          ?? throw new KeyNotFoundException("Session not found");
            if (session.HostId != hostId) throw new UnauthorizedAccessException();
            return session;
        }

        private async Task<Question> GetQuestionAt(int quizId, int index) =>
            await Questions.GetAsync(new QuestionAtIndexSpec(quizId, index))
            ?? throw new KeyNotFoundException("Question not found");

        private async Task<QuestionForPlayerDto?> BuildQuestion(int quizId, int index)
        {
            var q = await Questions.GetAsync(new QuestionAtIndexSpec(quizId, index));
            if (q == null) return null;

            var total = await Questions.CountAsync(new QuestionsByQuizSpec(quizId));

            return new QuestionForPlayerDto
            {
                Id=q.Id,
                Text=q.Text, 
                ImageUrl=q.ImageUrl,
                QuestionType=q.questionType, 
              TimeLimit=q.TimeLimitInSeconds,
                Points=q.Points,
               Number= index + 1,Total= total,

                Options=q.Answers.OrderBy(o => o.OrderNumber)
                         .Select(o => new OptionForPlayerDto { Id=o.Id,Text= o.Text }).ToList()};
        }

        private async Task<string> GeneratePinAsync()
        {
            while (true)
            {
                var pin = Random.Shared.Next(100000, 999999).ToString();
                if (!await Sessions.AnyAsync(new ActiveSessionByPinSpec(pin))) return pin;
            }
        }
    }
}
