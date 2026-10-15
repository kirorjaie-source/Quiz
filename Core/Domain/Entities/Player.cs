namespace Domain.Entities
{
    public class Player:BaseEntity<int>
    {
        public string PlayerName { get; set; } = null!;
        public int TotalScore { get; set; }
        public Guid ReconnectToken { get; set; }
        public int CurrentQuestionIdex { get; set; }
        public int AnswerStreak { get; set; }
        public DateTime JoinAt { get; set; }
        public DateTime? QuestionStartAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public GameSession GameSession { get; set; } = null!;
        public int GameSessionId { get; set; }
        public ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new HashSet<PlayerAnswer>();
    }
}