using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record OptionForPlayerDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
    }
}
