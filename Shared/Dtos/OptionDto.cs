using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
   public record OptionDto
    {
        [Required]
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
