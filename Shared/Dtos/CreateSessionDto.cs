using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos
{
    public record CreateSessionDto
    {
        [Range(1,180)]
        public int DurationMinutes { get; set; }
    }
}
