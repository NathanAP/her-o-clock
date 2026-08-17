using System;

namespace HerOClock.Progression
{
    /// <summary>
    /// What the player earned while the game was closed.
    ///
    /// **This is arithmetic, never a simulation.** The rate of the last hour is multiplied by the
    /// time away and the ceilings are applied. No battle is ever replayed, and that is only possible
    /// because progress.md forbids the three things that would demand it: no item drops, no stage
    /// advancement, and at most one level. Allowing any of the three offline would mean writing a
    /// combat simulator, which is a far larger decision than it looks.
    ///
    /// Pure C# and no clock of its own: the current instant is handed in, so an absence can be
    /// checked by stating two dates.
    /// </summary>
    public static class OfflineProgress
    {
        /// <summary>Share of the online rate kept while away, as stated in progress.md.</summary>
        public const double DefaultRateShare = 0.25;

        /// <summary>Highest share the progress tree can raise the rate to.</summary>
        public const double MaxRateShare = 0.40;

        /// <summary>
        /// Money ceiling, in hours of the **online** rate. A player earning $1,000 an hour takes at
        /// most 12 x $1,000, and not 12 x the offline rate.
        /// </summary>
        public const double MoneyCapHours = 12.0;

        /// <summary>
        /// How long the player was away.
        ///
        /// A current instant **earlier** than the save's gives zero. A clock running backwards is
        /// not a case to model, it is a sign that the clock cannot be used, and the alternative
        /// would be a negative credit. Against a clock pushed forward there is no local defence at
        /// all, and the ceilings are what make it pointless: a month away pays the same as 12 hours.
        /// </summary>
        public static double ElapsedHours(DateTime savedAtUtc, DateTime nowUtc)
        {
            double hours = (nowUtc - savedAtUtc).TotalHours;
            return hours <= 0.0 ? 0.0 : hours;
        }

        /// <summary>
        /// The absence expressed as hours of open game, which is the one factor everything else is
        /// built from.
        ///
        /// The money ceiling is written in progress.md as hours of the online rate, so it lands
        /// here as a ceiling on this factor rather than on the money alone. That is what keeps the
        /// statistics consistent with the credit by construction: capping the money and leaving the
        /// enemy count uncapped is exactly the contradiction the spec forbids.
        /// </summary>
        public static double EffectiveHours(double elapsedHours, double rateShare)
        {
            if (elapsedHours <= 0.0)
            {
                return 0.0;
            }

            double share = Math.Min(Math.Max(rateShare, 0.0), MaxRateShare);

            return Math.Min(elapsedHours * share, MoneyCapHours);
        }

        /// <summary>
        /// What the absence was worth, from the buckets of the last hour of open game.
        ///
        /// The offline gain deliberately does **not** go back into the buckets. If it did, the
        /// reference would start measuring itself, and every absence would inflate the next one.
        /// </summary>
        public static OfflineCredit Credit(ActivityLog log, double elapsedHours, double rateShare)
        {
            OfflineCredit credit = new OfflineCredit();

            if (log == null)
            {
                return credit;
            }

            double hours = EffectiveHours(elapsedHours, rateShare);

            credit.EffectiveHours = hours;
            credit.Money = Scale(log.MoneyPerHour, hours);
            credit.ExperienceOffered = Scale(log.ExperiencePerHour, hours);
            credit.EnemiesDefeated = Scale(log.EnemiesPerHour, hours);
            credit.DamageDealt = Scale(log.DamageDealtPerHour, hours);
            credit.DamageTaken = Scale(log.DamageTakenPerHour, hours);
            credit.Healing = Scale(log.HealingPerHour, hours);

            return credit;
        }

        /// <summary>
        /// How much of the offered experience one character may actually take.
        ///
        /// The ceiling is **one level, keeping the fraction of progress the character had inside
        /// its level**. Somebody who left 95% of the way to level 10 comes back at level 10, 95% of
        /// the way to 11, however long they were gone.
        ///
        /// Preserving the fraction rather than granting a fixed amount of experience is what makes
        /// the ceiling worth exactly one level anywhere in the game. Granting "the cost of a level"
        /// would pay less than a level, because the next level always costs more than the current
        /// one.
        /// </summary>
        public static long ExperienceFor(long offered, int level, long currentXp, int maxLevel)
        {
            if (offered <= 0 || level >= maxLevel)
            {
                return 0;
            }

            long toNext = ExperienceTable.XpToNextLevel(level);

            if (toNext <= 0)
            {
                return 0;
            }

            long remaining = Math.Max(0L, toNext - currentXp);
            long cap;

            if (level + 1 >= maxLevel)
            {
                // Arriving at the last level is the whole ceiling. Experience past it has nowhere
                // to go, so offering more than this would be offering nothing.
                cap = remaining;
            }
            else
            {
                double fraction = toNext <= 0 ? 0.0 : (double)currentXp / toNext;
                fraction = Math.Min(Math.Max(fraction, 0.0), 1.0);

                long insideNext = (long)Math.Round(fraction * ExperienceTable.XpToNextLevel(level + 1));
                cap = remaining + insideNext;
            }

            return Math.Min(offered, cap);
        }

        private static long Scale(double perHour, double hours)
        {
            if (perHour <= 0.0 || hours <= 0.0)
            {
                return 0;
            }

            return (long)Math.Round(perHour * hours);
        }
    }
}
