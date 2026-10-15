using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstraction.Contracts
{
    public interface IScoringService
    {
        int Calculate(bool isCorrect, double responseSeconds, int timeLimit, int basePoints, int previousStreak);
    }
}
