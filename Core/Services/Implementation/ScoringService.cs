using Services.Abstraction.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ScoringService : IScoringService
    {
        public int Calculate(bool isCorrect, double responseSeconds, int timeLimit, int basePoints, int previousStreak)
        {
            if (!isCorrect) return 0;

            // كل ما تجاوب أسرع تاخد نقاط أكتر (من 100% لـ 50%)
            var ratio = Math.Clamp(responseSeconds / timeLimit, 0, 1);
            var points = (int)Math.Round(basePoints * (1 - ratio / 2));

            // مكافأة الـ streak: 100 نقطة لكل إجابة صح متتالية (بحد أقصى 500)
            var bonus = Math.Min(previousStreak, 5) * 100;

            return points + bonus;
        }
    }
}
