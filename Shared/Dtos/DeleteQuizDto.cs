using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record DeletedQuizDto(int Id, string Title, string CreatorName, DateTime CreatedAt, int QuestionCount);
}
