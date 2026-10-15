using Domain.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record QuestionCreateDto
    {
        public string Text { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public QuestionType QuestionType { get; set; } 
        [Range(5,250)]
        public int TimeLimit { get; set; }
        [Range(1, 5000)]
        public int Points { get; set; }
        [Required,MinLength(2),MaxLength(4)]
      public List<OptionDto> Options { get; set; } = null!;
    }
}
