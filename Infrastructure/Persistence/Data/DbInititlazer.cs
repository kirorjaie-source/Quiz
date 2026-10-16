using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class DbInititlazer : IDbInititlazer
    {
        private readonly AppDbContext _appDbContext;

        public DbInititlazer(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task InititlazerAsync()
        {
            try
            {
                if (_appDbContext.Database.GetPendingMigrations().Any())
                {
                    await _appDbContext.Database.MigrateAsync();
                }

                if (!_appDbContext.users.Any())
                {
                    var demoUser = new User
                    {
                       
                        UserName= "KerolosRjaie",
                       
                        Email = "Kero@gmail.com", 
                        Password="1234",
                        Quizzes = new List<Quiz>
        {
            new Quiz
            {
                Title = "مسابقة C# التجريبية",
                Description = "مسابقة لاختبار النظام",
                CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "ما هو نوع البيانات المستخدم لتخزين الأرقام الصحيحة؟",
                        TimeLimitInSeconds = 20,
                        Points = 1000,
                        Answers = new List<Answer>
                        {
                            new Answer { Text = "int", IsCorrect = true },
                            new Answer { Text = "string", IsCorrect = false },
                            new Answer { Text = "bool", IsCorrect = false },
                            new Answer { Text = "DateTime", IsCorrect = false }
                        }
                    }
                }
            }
        }
                    };

                    await _appDbContext.users.AddAsync(demoUser);
                    await _appDbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
