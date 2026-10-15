using Domain.Contract;

namespace Domain.Entities
{
    public class Question:BaseEntity<int>
    {
        public string Text { get; set; } = null!;
        public int TimeLimitInSeconds { get; set; }
        public string? ImageUrl { get; set; }
        public int OrderNumber { get; set; }
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public QuestionType questionType { get; set; } = QuestionType.MultipleChoice;
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new HashSet<PlayerAnswer>();
    }
}