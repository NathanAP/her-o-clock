using System;
using System.Collections.Generic;

namespace HerOClock.Progression
{
    /// <summary>
    /// The last hour of open game, kept in ten minute buckets.
    ///
    /// This is the reference the offline progression multiplies, and keeping a measurement rather
    /// than a formula is the whole idea: it adjusts itself as the player gets stronger, with no
    /// parallel curve to keep in sync.
    ///
    /// Time comes in as simulation steps, not as wall clock. A game sitting behind a hitch, or one
    /// the operating system stopped drawing, has not earned anything during that time, and it is
    /// the fighting that the rate is meant to describe.
    ///
    /// Pure C# with no Unity in it, so the rules can be checked by handing it numbers.
    /// </summary>
    public class ActivityLog
    {
        /// <summary>Length of one bucket, as stated in progress.md.</summary>
        public const float BucketSeconds = 600f;

        /// <summary>How many buckets are kept. Six of ten minutes is the hour of reference.</summary>
        public const int BucketsKept = 6;

        /// <summary>
        /// How close to full counts as full.
        ///
        /// Time arrives one sixtieth of a second at a time, so a bucket is filled by tens of
        /// thousands of additions and lands a rounding error short of its length. Without this, the
        /// remaining sliver would be smaller than what a float can add at that magnitude, and the
        /// bucket would never quite close.
        /// </summary>
        private const float Full = BucketSeconds - 0.001f;

        private readonly List<ActivityBucket> buckets = new List<ActivityBucket>();

        /// <summary>Oldest first, so the last one is the bucket being filled right now.</summary>
        public IReadOnlyList<ActivityBucket> Buckets
        {
            get { return buckets; }
        }

        /// <summary>
        /// Advances the clock of the log, rolling into a new bucket whenever the current one fills.
        ///
        /// The overflow is carried into the next bucket rather than dropped, for the same reason
        /// the combat loop carries its remainder: throwing it away would make every bucket a little
        /// shorter than ten minutes, and the rate a little higher than the truth.
        /// </summary>
        public void Advance(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            while (seconds > 0f)
            {
                ActivityBucket current = Current();

                float room = BucketSeconds - current.Seconds;
                float taken = Math.Min(seconds, room);

                current.Seconds += taken;
                seconds -= taken;
            }
        }

        public void RecordExperience(long amount)
        {
            if (amount > 0)
            {
                Current().Experience += amount;
            }
        }

        public void RecordMoney(long amount)
        {
            if (amount > 0)
            {
                Current().Money += amount;
            }
        }

        public void RecordEnemyDefeated()
        {
            Current().EnemiesDefeated++;
        }

        public void RecordDamageDealt(long amount)
        {
            if (amount > 0)
            {
                Current().DamageDealt += amount;
            }
        }

        public void RecordDamageTaken(long amount)
        {
            if (amount > 0)
            {
                Current().DamageTaken += amount;
            }
        }

        public void RecordHealing(long amount)
        {
            if (amount > 0)
            {
                Current().Healing += amount;
            }
        }

        /// <summary>How much time the kept buckets cover, which is the denominator of every rate.</summary>
        public float CoveredSeconds
        {
            get
            {
                float total = 0f;

                for (int i = 0; i < buckets.Count; i++)
                {
                    total += buckets[i].Seconds;
                }

                return total;
            }
        }

        public long TotalExperience
        {
            get { return Sum(bucket => bucket.Experience); }
        }

        public long TotalMoney
        {
            get { return Sum(bucket => bucket.Money); }
        }

        public long TotalEnemiesDefeated
        {
            get { return Sum(bucket => bucket.EnemiesDefeated); }
        }

        public long TotalDamageDealt
        {
            get { return Sum(bucket => bucket.DamageDealt); }
        }

        public long TotalDamageTaken
        {
            get { return Sum(bucket => bucket.DamageTaken); }
        }

        public long TotalHealing
        {
            get { return Sum(bucket => bucket.Healing); }
        }

        public double ExperiencePerHour
        {
            get { return PerHour(TotalExperience); }
        }

        public double MoneyPerHour
        {
            get { return PerHour(TotalMoney); }
        }

        public double EnemiesPerHour
        {
            get { return PerHour(TotalEnemiesDefeated); }
        }

        public double DamageDealtPerHour
        {
            get { return PerHour(TotalDamageDealt); }
        }

        public double DamageTakenPerHour
        {
            get { return PerHour(TotalDamageTaken); }
        }

        public double HealingPerHour
        {
            get { return PerHour(TotalHealing); }
        }

        /// <summary>
        /// Replaces the buckets with the ones read from a save.
        ///
        /// Coming back from an absence does not clear them. They are stale, but they still describe
        /// the same team, which did not get weaker while the game was closed. Clearing would make
        /// the first minutes after returning read as a weak hour, and punish exactly the player who
        /// takes a second short break straight after.
        /// </summary>
        public void Restore(IReadOnlyList<ActivityBucket> saved)
        {
            buckets.Clear();

            if (saved == null)
            {
                return;
            }

            for (int i = 0; i < saved.Count; i++)
            {
                if (saved[i] != null)
                {
                    buckets.Add(saved[i]);
                }
            }

            Trim();
        }

        /// <summary>
        /// The bucket being filled, opening one when there is none or the last one is full.
        ///
        /// Recording goes through here as well as advancing, so a reward that arrives after a
        /// bucket filled up lands in the next one rather than overflowing the old.
        /// </summary>
        private ActivityBucket Current()
        {
            if (buckets.Count == 0 || buckets[buckets.Count - 1].Seconds >= Full)
            {
                buckets.Add(new ActivityBucket());
                Trim();
            }

            return buckets[buckets.Count - 1];
        }

        private void Trim()
        {
            while (buckets.Count > BucketsKept)
            {
                buckets.RemoveAt(0);
            }
        }

        private long Sum(Func<ActivityBucket, long> of)
        {
            long total = 0;

            for (int i = 0; i < buckets.Count; i++)
            {
                total += of(buckets[i]);
            }

            return total;
        }

        private double PerHour(long total)
        {
            float covered = CoveredSeconds;
            return covered <= 0f ? 0.0 : total * 3600.0 / covered;
        }
    }
}
