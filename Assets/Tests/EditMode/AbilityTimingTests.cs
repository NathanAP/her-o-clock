using HerOClock.Abilities;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The two timing sums of "### Recarga" in abilities.md, against the test character written
    /// into the spec itself.
    ///
    /// The spec gives two lists that look almost identical and produce different numbers, which is
    /// exactly the sort of thing that gets implemented once and used for both. The difference is
    /// the recovery: it delays the **next ability** but not this one's **recharge**.
    /// </summary>
    public class AbilityTimingTests
    {
        /// <summary>The test character of abilities.md: 1 second of wind up, no cast, no recovery.</summary>
        private static AbilityDefinition A()
        {
            return TestAbility.Timed("a", 1f, 0f, 0f);
        }

        /// <summary>Two seconds of wind up, two of casting, no recovery.</summary>
        private static AbilityDefinition B()
        {
            return TestAbility.Timed("b", 2f, 2f, 0f);
        }

        /// <summary>Instant, one second of casting, one of recovery.</summary>
        private static AbilityDefinition C()
        {
            return TestAbility.Timed("c", 0f, 1f, 1f);
        }

        // --- When the next ability is free to start ---

        [Test]
        public void AbilityAFreesTheNextOneAfterOneSecond()
        {
            Assert.AreEqual(1f, A().BusySeconds(1), 0.0001f);
        }

        [Test]
        public void AbilityBFreesTheNextOneAfterFourSeconds()
        {
            Assert.AreEqual(4f, B().BusySeconds(1), 0.0001f);
        }

        [Test]
        public void AbilityCFreesTheNextOneAfterTwoSeconds()
        {
            Assert.AreEqual(2f, C().BusySeconds(1), 0.0001f);
        }

        // --- When the cooldown starts running ---

        [Test]
        public void AbilityAStartsRechargingAfterOneSecond()
        {
            Assert.AreEqual(1f, A().SecondsUntilCooldownStarts(1), 0.0001f);
        }

        [Test]
        public void AbilityBStartsRechargingAfterFourSeconds()
        {
            Assert.AreEqual(4f, B().SecondsUntilCooldownStarts(1), 0.0001f);
        }

        [Test]
        public void AbilityCStartsRechargingAfterOneSecond()
        {
            Assert.AreEqual(1f, C().SecondsUntilCooldownStarts(1), 0.0001f);
        }

        /// <summary>
        /// The whole point of having two sums. Ability C frees the next ability at 2 seconds but
        /// began recharging at 1, because the recovery is outside the recharge.
        /// </summary>
        [Test]
        public void TheRecoveryDelaysTheNextAbilityButNotTheRecharge()
        {
            AbilityDefinition c = C();

            Assert.AreEqual(2f, c.BusySeconds(1), 0.0001f);
            Assert.AreEqual(1f, c.SecondsUntilCooldownStarts(1), 0.0001f);

            Assert.AreNotEqual(c.BusySeconds(1), c.SecondsUntilCooldownStarts(1),
                "The two sums came out the same, so the recovery is being counted in both or in neither.");
        }

        // --- Ranked values ---

        [Test]
        public void ASingleEntryMeansTheSameValueAtEveryRank()
        {
            RankedValue value = RankedValue.Constant(20f);

            Assert.AreEqual(20f, value.At(1), 0.0001f);
            Assert.AreEqual(20f, value.At(5), 0.0001f);
        }

        [Test]
        public void OneEntryPerRankScalesWithTheRank()
        {
            RankedValue value = new RankedValue(10f, 20f, 30f, 40f, 50f);

            Assert.AreEqual(10f, value.At(1), 0.0001f);
            Assert.AreEqual(30f, value.At(3), 0.0001f);
            Assert.AreEqual(50f, value.At(5), 0.0001f);
        }

        /// <summary>
        /// A rank past the end of the array is clamped rather than allowed to throw. The validator
        /// is what reports a badly sized array; crashing in front of a player would only turn an
        /// authoring mistake into a worse one.
        /// </summary>
        [Test]
        public void ARankPastTheEndIsClampedRatherThanThrowing()
        {
            RankedValue value = new RankedValue(10f, 20f);

            Assert.AreEqual(20f, value.At(9), 0.0001f);
            Assert.AreEqual(10f, value.At(0), 0.0001f);
        }

        [Test]
        public void AnEmptyValueReadsAsZero()
        {
            Assert.AreEqual(0f, new RankedValue().At(1), 0.0001f);
        }
    }
}
