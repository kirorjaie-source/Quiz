using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // الـ Enums تتخزن كنص (أوضح عند القراءة)
            modelBuilder.Entity<Quiz>().Property(q => q.QuizCategory).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.questionType).HasConversion<string>();
            modelBuilder.Entity<GameSession>().Property(s => s.Status).HasConversion<string>();

            // ---- Indexes ----
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<GameSession>().HasIndex(s => s.PinCode);
            modelBuilder.Entity<Player>().HasIndex(p => p.ReconnectToken).IsUnique();
            modelBuilder.Entity<Player>()
                .HasIndex(p => new { p.GameSessionId, p.PlayerName }).IsUnique();   // مفيش اسمين زي بعض في جيم
            modelBuilder.Entity<PlayerAnswer>()
                .HasIndex(a => new { a.PlayerId, a.QuestionId }).IsUnique();      // إجابة واحدة لكل سؤال

            // منع الحذف المتسلسل من ناحية الطالب عشان نحل مشكلة الـ Multiple Cascade Paths
            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.Player)
                .WithMany(p => p.PlayerAnswers)
                .HasForeignKey(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); // أو DeleteBehavior.NoAction

            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.Question)
                .WithMany(q => q.PlayerAnswers)
                .HasForeignKey(pa => pa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.SelectedAnswer)
                .WithMany(a => a.PlayerAnswers)
                .HasForeignKey(pa => pa.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> users { get; set; }
        public DbSet<Quiz> quizzes { get; set; }
        public DbSet<Question> questions { get; set; }
        public DbSet<Player> players { get; set; }
        public DbSet<PlayerAnswer> playerAnswers { get; set; }
        public DbSet<GameSession> gameSessions { get; set; }
        public DbSet<Answer> answers { get; set; }
    }
}
