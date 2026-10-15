using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GameSession : BaseEntity<int>
    {
        public string PinCode { get; set; } = null!;
        
        public int CurrentQuestionIndex { get; set; }
        public int DurationMinute { get; set; }

        public DateTime? StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndAt { get; set; } = DateTime.UtcNow;
        public SessionStatus Status { get; set; } = SessionStatus.Waiting;
        public Quiz Quiz { get; set; } = null!;
        public int QuizId { get; set; }
        public int HostId { get; set; }
        public ICollection<Player> Players { get; set; } = new HashSet<Player>();
    }
}