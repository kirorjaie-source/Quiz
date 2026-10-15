using Domain.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record QuestionForPlayerDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public QuestionType QuestionType { get; set; } 
        public string? ImageUrl { get; set; }
        public int TimeLimit { get; set; }
        public int Points { get; set; }
        public int Number { get; set; }
        public int Total { get; set; }
      public List<OptionForPlayerDto> Options { get; set; } = null!; 

    }
}
