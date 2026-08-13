using System;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// How a character spends the 5 attribute points it receives on every level, in percentages.
    ///
    /// For heroes this is the default the player can later replace with their own choices.
    /// For minions and villains it is the only way they grow, since nobody spends points for them.
    /// </summary>
    [Serializable]
    public struct AttributeGrowth
    {
        [Range(0, 100)] public int Power;
        [Range(0, 100)] public int Agility;
        [Range(0, 100)] public int Specialty;
        [Range(0, 100)] public int Constitution;

        public int Total
        {
            get { return Power + Agility + Specialty + Constitution; }
        }

        public int ShareOf(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.Power: return Power;
                case Attribute.Agility: return Agility;
                case Attribute.Specialty: return Specialty;
                default: return Constitution;
            }
        }

        /// <summary>
        /// Splits a number of points between the four attributes, always landing on the exact total.
        ///
        /// Rounding each share on its own does not work: a 33/33/34 sheet spending 5 points would
        /// give 1.65, 1.65 and 1.70, and rounding those separately hands out 4 or 6 points instead
        /// of 5. The leftover is therefore given to the largest remainders, and ties go to the
        /// attribute that comes first, so the result is always the same for the same input.
        /// </summary>
        public static void Distribute(int points, AttributeGrowth growth, int[] result)
        {
            for (int i = 0; i < 4; i++)
            {
                result[i] = 0;
            }

            int totalShare = growth.Total;

            if (points <= 0 || totalShare <= 0)
            {
                return;
            }

            int given = 0;

            // Whole points first, keeping each remainder to decide who gets the leftover.
            double[] remainders = new double[4];

            for (int i = 0; i < 4; i++)
            {
                double exact = (double)points * growth.ShareOf((Attribute)i) / totalShare;
                int whole = (int)exact;

                result[i] = whole;
                remainders[i] = exact - whole;
                given += whole;
            }

            while (given < points)
            {
                int best = -1;

                for (int i = 0; i < 4; i++)
                {
                    if (remainders[i] > 0 && (best < 0 || remainders[i] > remainders[best]))
                    {
                        best = i;
                    }
                }

                if (best < 0)
                {
                    break;
                }

                result[best]++;
                remainders[best] = 0;
                given++;
            }
        }
    }
}
