using Domain.Contract;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public interface IQuizService
    {
        Task<int> CreateAsync(int userId, QuizCreateDto dto, bool isAdmin = false);
        Task UpdateAsync(int userId, int quizId, QuizCreateDto dto, bool isAdmin = false);
        Task DeleteAsync(int userId, int quizId,bool isAdmin=false);
        Task<List<QuizSummaryDto>> GetMineAsync(int userId);
        Task<List<QuizSummaryDto>> GetPublicAsync(string? search, QuizCategory? category);
        Task<int> AddQuestionAsync(int userId, int quizId, QuestionCreateDto dto, bool isAdmin = false);
        Task DeleteQuestionAsync(int userId, int questionId, bool isAdmin = false);
    }

   
}
