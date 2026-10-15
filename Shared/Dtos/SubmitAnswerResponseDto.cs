using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record SubmitAnswerResponseDto
    {
        public AnswerResultDto Result { get; set; } = null!;
        public QuestionForPlayerDto? NextQuestion { get; set; }
        public bool Finished { get; set; }
        public bool GameEnded { get; set; }
        public int? Rank { get; set; }
    }
}
