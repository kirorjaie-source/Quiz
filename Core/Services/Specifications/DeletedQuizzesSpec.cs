using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class DeletedQuizzesSpec : BaseSpecifications<Quiz, int>
    {
        public DeletedQuizzesSpec() : base(q => q.IsDeleted)
        {
            AddInclude(q => q.Questions);
            AddInclude(q => q.Creator);
            AddOrderByDescending(q => q.CreatedAt);
        }
    }

    public class DeletedQuizByIdSpec : BaseSpecifications<Quiz, int>
    {
        public DeletedQuizByIdSpec(int id) : base(q => q.Id == id && q.IsDeleted) { }
    }

    public class SessionsByQuizSpec : BaseSpecifications<GameSession, int>
    {
        public SessionsByQuizSpec(int quizId) : base(s => s.QuizId == quizId) { }
    }
}
