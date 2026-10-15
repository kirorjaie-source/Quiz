using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record SessionCreatedDto
    {
        public int SessionId { get; set; }
        public string PinCode { get; set; } = null!;
        public int DurationMinutes { get; set; }
    }
}
