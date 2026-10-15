using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : BaseEntity<int>
    {
        public string UserName { get; set; } = null!;
     
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Url { get; set; }
        public UserRoles roles { get; set; } = UserRoles.Creator;
        public DateTime Created_at { get; set; }= DateTime.UtcNow;
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    }
}
