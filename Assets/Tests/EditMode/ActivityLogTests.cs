using System.Collections.Generic;
using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The buckets that measure the last hour of open game, against "### Como a referência da última
    /// hora é medida" in progress.md.
    ///
    /// This is the reference the whole offline progression multiplies, so an error here is an error
    /// in everything the player is paid while away.
    /// </summary>
    public class ActivityLogTests
    {
        [Test]
        public void TheBucketLengthAndCountAreWhatTheSpecSays()
        {
            Assert.AreEqual(600f, ActivityLog.BucketSeconds, "Ten minutes, as stated in progress.md.");
            Assert.AreEqual(6, ActivityLog.BucketsKept, "Six of them, which is the hour of reference.");
        }

        [Test]
        public void AFreshLogHasNothingAndNoRate()
        {
            ActivityLog log = new ActivityLog();

            Assert.AreEqual(0f, log.CoveredSeconds);
            Assert.AreEqual(0.0, log.MoneyPerHour);
            Assert.AreEqual(0.0, log.ExperiencePerHour);
        }

        [Test]
        public void TenMinutesFillExactlyOneBucket()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(ActivityLog.BucketSeconds);

            Assert.AreEqual(1, log.Buckets.Count);
            Assert.AreEqual(ActivityLog.BucketSeconds, log.CoveredSeconds);
        }

        [Test]
        public void PassingTenMinutesOpensTheNextBucket()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(ActivityLog.BucketSeconds + 1f);

            Assert.AreEqual(2, log.Buckets.Count);
            Assert.AreEqual(1f, log.Buckets[1].Seconds, 0.001f, "The overflow was not carried into the new bucket.");
        }

        /// <summary>
        /// The overflow is carried rather than dropped, for the same reason the combat loop carries
        /// its remainder: dropping it would make every bucket a little short of ten minutes, and so
        /// make every rate a little higher than the truth.
        /// </summary>
        [Test]
        public void TimeIsNeverLostWhileRollingOverManyBuckets()
        {
            ActivityLog log = new ActivityLog();

            for (int i = 0; i < 3600 * 60; i++)
            {
                log.Advance(1f / 60f);
            }

            Assert.AreEqual(ActivityLog.BucketsKept * ActivityLog.BucketSeconds, log.CoveredSeconds, 1f);
        }

        [Test]
        public void OnlyTheSixMostRecentBucketsAreKept()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(ActivityLog.BucketSeconds * 20f);

            Assert.AreEqual(ActivityLog.BucketsKept, log.Buckets.Count);
        }

        /// <summary>An hour of buckets covers an hour, which is what the rate divides by.</summary>
        [Test]
        public void TheKeptBucketsCoverAtMostAnHour()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(ActivityLog.BucketSeconds * 20f);

            Assert.AreEqual(3600f, log.CoveredSeconds, 0.001f);
        }

        /// <summary>Anything earned after a bucket filled up belongs to the next one.</summary>
        [Test]
        public void RecordingAfterABucketFilledOpensTheNextOne()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(ActivityLog.BucketSeconds);
            log.RecordMoney(10);

            Assert.AreEqual(2, log.Buckets.Count);
            Assert.AreEqual(10L, log.Buckets[1].Money);
            Assert.AreEqual(0L, log.Buckets[0].Money);
        }

        // --- The rate ---

        /// <summary>
        /// The example in progress.md: two buckets holding $100 give $300 an hour, because they
        /// cover twenty minutes. A short session is not read as a weak hour.
        /// </summary>
        [Test]
        public void TwoBucketsHoldingAHundredGiveThreeHundredAnHour()
        {
            ActivityLog log = new ActivityLog();
            log.RecordMoney(100);
            log.Advance(ActivityLog.BucketSeconds * 2f);

            Assert.AreEqual(2, log.Buckets.Count);
            Assert.AreEqual(1200f, log.CoveredSeconds);
            Assert.AreEqual(300.0, log.MoneyPerHour, 0.001);
        }

        [Test]
        public void AFullHourOfBucketsReportsTheSumAsTheRate()
        {
            ActivityLog log = new ActivityLog();
            log.RecordMoney(1000);
            log.Advance(3600f);

            Assert.AreEqual(1000.0, log.MoneyPerHour, 0.001);
        }

        [Test]
        public void EveryCounterHasItsOwnRate()
        {
            ActivityLog log = new ActivityLog();

            log.RecordExperience(50);
            log.RecordEnemyDefeated();
            log.RecordDamageDealt(700);
            log.RecordDamageTaken(300);
            log.RecordHealing(20);
            log.Advance(1800f);

            Assert.AreEqual(100.0, log.ExperiencePerHour, 0.001);
            Assert.AreEqual(2.0, log.EnemiesPerHour, 0.001);
            Assert.AreEqual(1400.0, log.DamageDealtPerHour, 0.001);
            Assert.AreEqual(600.0, log.DamageTakenPerHour, 0.001);
            Assert.AreEqual(40.0, log.HealingPerHour, 0.001);
        }

        [Test]
        public void NonPositiveAmountsAreIgnored()
        {
            ActivityLog log = new ActivityLog();
            log.Advance(600f);

            log.RecordMoney(0);
            log.RecordMoney(-100);
            log.RecordExperience(-5);

            Assert.AreEqual(0L, log.TotalMoney);
            Assert.AreEqual(0L, log.TotalExperience);
        }

        // --- Coming back from a save ---

        /// <summary>
        /// Returning does not clear the buckets. They are stale, but they still describe the same
        /// team, which did not get weaker while the game was closed. Clearing would make the first
        /// minutes back read as a weak hour and punish a second short break.
        /// </summary>
        [Test]
        public void RestoringPutsTheBucketsBackAsTheyWere()
        {
            List<ActivityBucket> saved = new List<ActivityBucket>
            {
                new ActivityBucket { Seconds = 600f, Money = 40, Experience = 400 },
                new ActivityBucket { Seconds = 600f, Money = 60, Experience = 600 }
            };

            ActivityLog log = new ActivityLog();
            log.Restore(saved);

            Assert.AreEqual(2, log.Buckets.Count);
            Assert.AreEqual(1200f, log.CoveredSeconds);
            Assert.AreEqual(100L, log.TotalMoney);
            Assert.AreEqual(300.0, log.MoneyPerHour, 0.001);
        }

        [Test]
        public void RestoringMoreThanSixBucketsKeepsTheNewest()
        {
            List<ActivityBucket> saved = new List<ActivityBucket>();

            for (int i = 0; i < 10; i++)
            {
                saved.Add(new ActivityBucket { Seconds = 600f, Money = i });
            }

            ActivityLog log = new ActivityLog();
            log.Restore(saved);

            Assert.AreEqual(ActivityLog.BucketsKept, log.Buckets.Count);
            Assert.AreEqual(9L, log.Buckets[log.Buckets.Count - 1].Money, "The newest bucket was not the one kept.");
        }
    }
}
