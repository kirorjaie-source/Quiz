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

            modelBuilder.Entity<Quiz>().Property(q => q.QuizCategory).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.questionType).HasConversion<string>();
            modelBuilder.Entity<GameSession>().Property(s => s.Status).HasConversion<string>();

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<GameSession>().HasIndex(s => s.PinCode);
            modelBuilder.Entity<Player>().HasIndex(p => p.ReconnectToken).IsUnique();
            modelBuilder.Entity<Player>()
                .HasIndex(p => new { p.GameSessionId, p.PlayerName }).IsUnique();
            modelBuilder.Entity<PlayerAnswer>()
                .HasIndex(a => new { a.PlayerId, a.QuestionId }).IsUnique();

            
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz).WithMany(z => z.Questions)
                .HasForeignKey(q => q.QuizId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question).WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.GameSession).WithMany(s => s.Players)
                .HasForeignKey(p => p.GameSessionId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.Player).WithMany(p => p.PlayerAnswers)
                .HasForeignKey(pa => pa.PlayerId).OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.Question).WithMany(q => q.PlayerAnswers)
                .HasForeignKey(pa => pa.QuestionId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerAnswer>()
                .HasOne(pa => pa.SelectedAnswer).WithMany(a => a.PlayerAnswers)
                .HasForeignKey(pa => pa.SelectedAnswerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSession>()
                .HasOne(s => s.Quiz).WithMany(q => q.GameSessions)
                .HasForeignKey(s => s.QuizId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Creator).WithMany(u => u.Quizzes)
                .HasForeignKey(q => q.CreatorId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GameSession>()
                .HasOne<User>().WithMany()
                .HasForeignKey(s => s.HostId).OnDelete(DeleteBehavior.NoAction);
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
