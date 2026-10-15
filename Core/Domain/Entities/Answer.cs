using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Answer : BaseEntity<int>
    {
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; } = false;

        public Question Question { get; set; } = null!;
        public int QuestionId { get; set; }
        public int OrderNumber { get; set; }
        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new HashSet<PlayerAnswer>();
    }
}