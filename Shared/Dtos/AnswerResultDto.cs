using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record AnswerResultDto
    {
        public int PointsEarned { get; set; }
        public int TotalScore { get; set; }
        public int Streak { get; set; }
        public bool IsCorrect { get; set; }
    }
}
