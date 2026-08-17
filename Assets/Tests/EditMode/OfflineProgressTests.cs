using System;
using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The progress made while the game was closed, against "## Progressão offline" in progress.md.
    ///
    /// Every number asserted here is copied from an example in the spec, never read off a run of the
    /// code. That is the whole point: the two statements of the same number are independent, and a
    /// disagreement between them is what these tests exist to surface.
    /// </summary>
    public class OfflineProgressTests
    {
        private const double Tolerance = 0.0001;

        /// <summary>Hours in a month, which is the absence the spec's examples use.</summary>
        private const double AMonth = 24.0 * 30.0;

        private static ActivityLog EarningPerHour(long money, long experience)
        {
            ActivityLog log = new ActivityLog();

            log.RecordMoney(money);
            log.RecordExperience(experience);
            log.Advance(3600f);

            return log;
        }

        [Test]
        public void TheRateAndTheCeilingsAreWhatTheSpecSays()
        {
            Assert.AreEqual(0.25, OfflineProgress.DefaultRateShare, Tolerance, "25% of the online rate.");
            Assert.AreEqual(0.40, OfflineProgress.MaxRateShare, Tolerance, "Up to 40% through the progress tree.");
            Assert.AreEqual(12.0, OfflineProgress.MoneyCapHours, Tolerance, "Twelve hours of the online earnings.");
        }

        // --- The examples in progress.md ---

        /// <summary>
        /// "Se o jogador ganhava $1.000 por hora e permaneceu 1 mês fora, ao voltar ele terá
        /// recebido no máximo $12.000."
        /// </summary>
        [Test]
        public void AMonthAwayEarningAThousandAnHourPaysTwelveThousand()
        {
            ActivityLog log = EarningPerHour(1000, 0);

            OfflineCredit credit = OfflineProgress.Credit(log, AMonth, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(12000L, credit.Money);
        }

        /// <summary>
        /// "Se o jogador ganhava $600 por hora e permaneceu 2 horas fora, ele recebe $300."
        ///
        /// This is the example that exercises the 25% itself, with the ceiling nowhere near.
        /// </summary>
        [Test]
        public void TwoHoursAwayEarningSixHundredAnHourPaysThreeHundred()
        {
            ActivityLog log = EarningPerHour(600, 0);

            OfflineCredit credit = OfflineProgress.Credit(log, 2.0, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(300L, credit.Money);
            Assert.Less(300L, (long)(600 * OfflineProgress.MoneyCapHours),
                "The spec says this example does not come close to the ceiling of $7,200.");
        }

        /// <summary>
        /// "Se o jogador saiu faltando 5% para seus personagens subirem ao nível 10 e permaneceu 1
        /// mês fora, ao voltar seus personagens estarão no nível 10 com 95% de progresso."
        ///
        /// The example is what pins down what "no more than one level" means. It is one level with
        /// the fraction of progress preserved, and no other reading produces 95%.
        /// </summary>
        [Test]
        public void AMonthAwayGainsExactlyOneLevelKeepingTheFraction()
        {
            long toTen = ExperienceTable.XpToNextLevel(9);
            long ninetyFivePercent = (long)Math.Round(0.95 * toTen);

            LevelProgress progress = new LevelProgress(9, ExperienceTable.MaxLevel);
            progress.Restore(9, ninetyFivePercent, 0);

            long granted = OfflineProgress.ExperienceFor(
                long.MaxValue / 4, progress.Level, progress.CurrentXp, ExperienceTable.MaxLevel);

            progress.Award(granted);

            Assert.AreEqual(10, progress.Level, "A month away has to be worth exactly one level.");

            double fraction = (double)progress.CurrentXp / ExperienceTable.XpToNextLevel(10);
            Assert.AreEqual(0.95, fraction, 0.001, "The progress inside the new level was not preserved.");
        }

        // --- The ceilings ---

        /// <summary>
        /// The money ceiling is measured against the **online** rate, not against the 25%. Reading it
        /// the other way would pay a quarter of what the spec promises.
        /// </summary>
        [Test]
        public void TheMoneyCeilingCountsHoursOfTheOnlineRate()
        {
            ActivityLog log = EarningPerHour(1000, 0);

            OfflineCredit credit = OfflineProgress.Credit(log, AMonth, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(12L * 1000L, credit.Money);
            Assert.AreNotEqual(12L * 250L, credit.Money);
        }

        [Test]
        public void RaisingTheRateOnlyGetsToTheSameCeilingSooner()
        {
            ActivityLog log = EarningPerHour(1000, 0);

            long atDefault = OfflineProgress.Credit(log, AMonth, OfflineProgress.DefaultRateShare).Money;
            long atMaximum = OfflineProgress.Credit(log, AMonth, OfflineProgress.MaxRateShare).Money;

            Assert.AreEqual(atDefault, atMaximum,
                "A long absence is limited by the ceiling, not by the rate, so raising the rate must add nothing.");
        }

        [Test]
        public void TheRateNeverGoesAboveTheMaximumTheTreeSells()
        {
            Assert.AreEqual(
                OfflineProgress.EffectiveHours(10.0, OfflineProgress.MaxRateShare),
                OfflineProgress.EffectiveHours(10.0, 0.9),
                Tolerance,
                "A rate above the maximum was honoured, so the ceiling on the rate means nothing.");
        }

        [Test]
        public void TenHoursAwayAtTheHighestRateCountsAsFour()
        {
            Assert.AreEqual(4.0, OfflineProgress.EffectiveHours(10.0, OfflineProgress.MaxRateShare), Tolerance);
        }

        [Test]
        public void AtMaxLevelThereIsNothingLeftToGain()
        {
            long granted = OfflineProgress.ExperienceFor(
                999999L, ExperienceTable.MaxLevel, 0, ExperienceTable.MaxLevel);

            Assert.AreEqual(0L, granted);
        }

        /// <summary>
        /// One level short of the maximum, the ceiling is only what it takes to arrive there.
        /// Experience past the last level has nowhere to go.
        /// </summary>
        [Test]
        public void OneLevelBelowTheMaximumTheCeilingIsReachingIt()
        {
            int level = ExperienceTable.MaxLevel - 1;
            long toMax = ExperienceTable.XpToNextLevel(level);

            long granted = OfflineProgress.ExperienceFor(long.MaxValue / 4, level, 0, ExperienceTable.MaxLevel);

            Assert.AreEqual(toMax, granted);
        }

        [Test]
        public void AShortAbsenceIsPaidWithoutHittingAnyCeiling()
        {
            long toNext = ExperienceTable.XpToNextLevel(5);
            long offered = toNext / 4;

            Assert.AreEqual(offered, OfflineProgress.ExperienceFor(offered, 5, 0, ExperienceTable.MaxLevel),
                "An offer below the ceiling has to be paid in full.");
        }

        // --- The clock ---

        /// <summary>
        /// A clock that went backwards is not a case to model, it is a clock that cannot be used.
        /// The alternative would be crediting a negative absence.
        /// </summary>
        [Test]
        public void AClockThatWentBackwardsCreditsNothing()
        {
            DateTime saved = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);
            DateTime now = saved.AddHours(-5);

            Assert.AreEqual(0.0, OfflineProgress.ElapsedHours(saved, now), Tolerance);
        }

        [Test]
        public void NoTimePassingCreditsNothing()
        {
            DateTime saved = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

            Assert.AreEqual(0.0, OfflineProgress.ElapsedHours(saved, saved), Tolerance);
        }

        [Test]
        public void TimeAwayIsMeasuredFromTheSave()
        {
            DateTime saved = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

            Assert.AreEqual(3.5, OfflineProgress.ElapsedHours(saved, saved.AddMinutes(210)), Tolerance);
        }

        // --- Statistics come from the same multiplication ---

        /// <summary>
        /// progress.md forbids the statistics of an absence from being reached by a second path. They
        /// are the same buckets times the same factor, so a screen saying 1,432 enemies next to the
        /// experience of 800 is impossible by construction.
        /// </summary>
        [Test]
        public void EveryStatisticUsesTheSameFactorAsTheMoney()
        {
            ActivityLog log = new ActivityLog();

            log.RecordMoney(100);
            log.RecordExperience(400);
            log.RecordEnemyDefeated();
            log.RecordDamageDealt(1000);
            log.RecordDamageTaken(500);
            log.RecordHealing(50);
            log.Advance(3600f);

            OfflineCredit credit = OfflineProgress.Credit(log, 8.0, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(2.0, credit.EffectiveHours, Tolerance, "Eight hours at 25% is two hours of play.");

            Assert.AreEqual(200L, credit.Money);
            Assert.AreEqual(800L, credit.ExperienceOffered);
            Assert.AreEqual(2L, credit.EnemiesDefeated);
            Assert.AreEqual(2000L, credit.DamageDealt);
            Assert.AreEqual(1000L, credit.DamageTaken);
            Assert.AreEqual(100L, credit.Healing);
        }

        [Test]
        public void AnEmptyLogPaysNothing()
        {
            OfflineCredit credit = OfflineProgress.Credit(new ActivityLog(), AMonth, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(0L, credit.Money);
            Assert.AreEqual(0L, credit.ExperienceOffered);
        }

        [Test]
        public void NoLogAtAllPaysNothingRatherThanCrashing()
        {
            OfflineCredit credit = OfflineProgress.Credit(null, AMonth, OfflineProgress.DefaultRateShare);

            Assert.AreEqual(0.0, credit.EffectiveHours);
            Assert.AreEqual(0L, credit.Money);
        }
    }
}
