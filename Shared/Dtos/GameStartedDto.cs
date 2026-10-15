using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record GameStartedDto
    {
        public DateTime EndAt { get; set; }

        public int DurationMinutes { get; set; }
    }
}
