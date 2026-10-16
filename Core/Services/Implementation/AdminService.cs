using Domain.Contract;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Services.Abstraction.Contracts;
using Services.Specifications;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IUniteOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AdminService(IUniteOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<List<DeletedQuizDto>> GetDeletedQuizzesAsync(string? adminKey)
        {
            EnsureAdmin(adminKey);

            var quizzes = await _unitOfWork.GetRepository<Quiz, int>().GetAllAsync(new DeletedQuizzesSpec());
            return quizzes
                .Select(q => new DeletedQuizDto(q.Id, q.Title, q.Creator.UserName, q.CreatedAt, q.Questions.Count))
                .ToList();
        }

        public async Task RestoreQuizAsync(string? adminKey, int quizId)
        {
            EnsureAdmin(adminKey);

            var quiz = await _unitOfWork.GetRepository<Quiz, int>().GetAsync(new DeletedQuizByIdSpec(quizId))
                       ?? throw new KeyNotFoundException("Deleted quiz not found");

            quiz.IsDeleted = false;
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task PurgeQuizAsync(string? adminKey, int quizId)
        {
            EnsureAdmin(adminKey);

            var quizzes = _unitOfWork.GetRepository<Quiz, int>();
            var quiz = await quizzes.GetAsync(new DeletedQuizByIdSpec(quizId))   
                       ?? throw new KeyNotFoundException("Deleted quiz not found");

            var sessionsRepo = _unitOfWork.GetRepository<GameSession, int>();
            var sessions = (await sessionsRepo.GetAllAsync(new SessionsByQuizSpec(quizId))).ToList();

            if (sessions.Any(s => s.Status == SessionStatus.Running))
                throw new InvalidOperationException("A game is still running for this quiz");

            foreach (var session in sessions)
                sessionsRepo.DeleteAsync(session);

            quizzes.DeleteAsync(quiz);

            await _unitOfWork.SaveChangeAsync();   
        }

        private void EnsureAdmin(string? adminKey)
        {
            var expected = _configuration["Admin:Key"];
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(adminKey) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(adminKey), Encoding.UTF8.GetBytes(expected)))
                throw new UnauthorizedAccessException("Invalid admin key");
        }
    }
}

