using Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Quiz:BaseEntity<int>
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsPublic { get; set; }
        public QuizCategory QuizCategory { get; set; } 
        public User Creator { get; set; } = null!;
        public int CreatorId { get; set; }
        public ICollection<Question> Questions { get; set; } = new HashSet<Question>();
        public ICollection<GameSession> GameSessions { get; set; } = new HashSet<GameSession>();
    }


}
