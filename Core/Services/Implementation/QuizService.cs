using Domain.Contract;
using Domain.Entities;
using Services.Specifications;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Services.Specifications.SpecificationsServices;

namespace Services.Implementation
{
    public class QuizService : IQuizService
    {
        private readonly IUniteOfWork _uow;
        public QuizService(IUniteOfWork uow) => _uow = uow;

        public async Task<int> CreateAsync(int userId, QuizCreateDto dto,bool isAdmin)
        {
            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsPublic = dto.IsPublic,
                QuizCategory = dto.Category,
                CreatorId = userId,
            };
            await _uow.GetRepository<Quiz, int>().AddAsync(quiz);
            await _uow.SaveChangeAsync();
            return quiz.Id;
        }

        public async Task UpdateAsync(int userId, int quizId, QuizCreateDto dto, bool isAdmin = false)
        {
            var quiz = await GetOwnedQuiz(userId, quizId, isAdmin);
            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.ImageUrl = dto.ImageUrl;
            quiz.IsPublic = dto.IsPublic;
            quiz.QuizCategory = dto.Category;
            await _uow.SaveChangeAsync();
        }

        public async Task DeleteAsync(int userId, int quizId, bool isAdmin = false)
        {
            var quiz = await GetOwnedQuiz(userId, quizId, isAdmin);
            quiz.IsDeleted = true; // soft delete
            await _uow.SaveChangeAsync();
        }

        public async Task<List<QuizSummaryDto>> GetMineAsync(int userId)
        {
            var quizzes = await _uow.GetRepository<Quiz, int>().GetAllAsync(new MyQuizzesSpec(userId));
            return quizzes.Select(ToSummary).ToList();
        }

        public async Task<List<QuizSummaryDto>> GetPublicAsync(string? search, QuizCategory? category)
        {
            var quizzes = await _uow.GetRepository<Quiz, int>().GetAllAsync(new PublicQuizzesSpec(search, category));
            return quizzes.Select(ToSummary).ToList();
        }

        public async Task<int> AddQuestionAsync(int userId, int quizId, QuestionCreateDto dto, bool isAdmin = false)
        {
            var quiz = await GetOwnedQuiz(userId, quizId, isAdmin);
            ValidateQuestion(dto);

            var questions = _uow.GetRepository<Question, int>();
            var order = await questions.CountAsync(new QuestionsByQuizSpec(quiz.Id)) + 1;

            var question = new Question
            {
                QuizId = quiz.Id,
                Text = dto.Text,
                ImageUrl = dto.ImageUrl,
                questionType = dto.QuestionType,
                TimeLimitInSeconds = dto.TimeLimit,
                Points = dto.Points,
                OrderNumber = order,
                Answers = dto.Options
                    .Select((o, i) => new Answer { Text = o.Text, IsCorrect = o.IsCorrect, OrderNumber = i + 1 })
                    .ToList()
            };
            await questions.AddAsync(question);
            await _uow.SaveChangeAsync();
            return question.Id;
        }

        public async Task DeleteQuestionAsync(int userId, int questionId, bool isAdmin = false)
        {
            var questions = _uow.GetRepository<Question, int>();
            var question = await questions.GetAsync(new QuestionWithQuizSpec(questionId))
                           ?? throw new KeyNotFoundException("Question not found");

            // شرط السماح: إذا كان أدمن أو صاحب الكويز نفسه
            if (!isAdmin && question.Quiz.CreatorId != userId)
                throw new UnauthorizedAccessException();

            questions.DeleteAsync(question);
            await _uow.SaveChangeAsync();
        }

        // ---- helpers ----
        private static QuizSummaryDto ToSummary(Quiz q) =>
            new QuizSummaryDto
            {
                Id = q.Id,
                ImageUrl = q.ImageUrl,
                QuestionCount = q.Questions.Count(),
                Title = q.Title,
                Category = q.QuizCategory,
            };

        private async Task<Quiz> GetOwnedQuiz(int userId, int quizId, bool isAdmin = false)
        {
            var quiz = await _uow.GetRepository<Quiz, int>().GetAsync(new QuizByIdSpec(quizId))
                       ?? throw new KeyNotFoundException("Quiz not found");

            if (!isAdmin && quiz.CreatorId != userId)
                throw new UnauthorizedAccessException();

            return quiz;
        }

        private static void ValidateQuestion(QuestionCreateDto dto1)
        {
            var count = dto1.Options.Count;
            var correct = dto1.Options.Count(o => o.IsCorrect);

            if (dto1.QuestionType == QuestionType.TrueFalse && count != 2)
                throw new InvalidOperationException("True/False needs exactly 2 options");
            if (dto1.QuestionType == QuestionType.MultipleChoice && (count < 2 || count > 4))
                throw new InvalidOperationException("Multiple choice needs 2 to 4 options");
            if (correct != 1)
                throw new InvalidOperationException("Exactly one option must be correct");
        }

        
    }
}