using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstraction.Contracts
{
    public interface IAdminService
    {
        Task<List<DeletedQuizDto>> GetDeletedQuizzesAsync(string? adminKey);
        Task RestoreQuizAsync(string? adminKey, int quizId);   // يرجّعه للمستخدم
        Task PurgeQuizAsync(string? adminKey, int quizId);     // يمسحه نهائياً من الداتا بيز
    }
}
