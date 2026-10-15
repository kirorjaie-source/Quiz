using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record PlayerScoreDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = null!;
        public int TotalScore { get; set; }
        public int Streak { get; set; }
    }
}
