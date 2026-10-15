using System.Numerics;

namespace Domain.Entities
{
    public class PlayerAnswer:BaseEntity<int>
    {
        public double ResponseTimeInSeconds { get; set; }
        public int PointsEarned { get; set; }
        public int? OptionId { get; set; }
        public bool IsCorrect { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
        public int SelectedAnswerId { get; set; }
        public Answer SelectedAnswer { get; set; } = null!;
    }
}