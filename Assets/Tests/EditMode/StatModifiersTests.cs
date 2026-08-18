using HerOClock.Abilities;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The stacking rule of buffs-and-debuffs.md: the same buff arriving again **keeps the stronger
    /// value and restarts the duration**, and never stacks.
    ///
    /// Pure arithmetic, so it is checked by handing it numbers. It is also the rule with the widest
    /// blast radius in the game: get it wrong and a party of four carrying the same slow pins an
    /// enemy at zero.
    /// </summary>
    public class StatModifiersTests
    {
        private const string Source = "ability";

        private static StatModifiers WithSlow(float value, float duration)
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply(Source, ModifiableStat.MovementSpeed, StatModifierMode.Percent, value, duration);
            return modifiers;
        }

        [Test]
        public void APercentBuffIsAShareOfWhatTheCharacterHas()
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply(Source, ModifiableStat.AttackSpeed, StatModifierMode.Percent, 20f, 5f);

            Assert.AreEqual(1.2f, modifiers.Apply(ModifiableStat.AttackSpeed, 1f), 0.0001f);
        }

        [Test]
        public void AFlatBuffAddsPointsOnTop()
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply(Source, ModifiableStat.Power, StatModifierMode.Flat, 15f, 5f);

            Assert.AreEqual(25f, modifiers.Apply(ModifiableStat.Power, 10f), 0.0001f);
        }

        [Test]
        public void ANegativeValueIsADebuff()
        {
            Assert.AreEqual(0.9f, WithSlow(-10f, 5f).Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        [Test]
        public void AStatNobodyBuffedComesBackUntouched()
        {
            Assert.AreEqual(7f, WithSlow(-10f, 5f).Apply(ModifiableStat.AttackSpeed, 7f), 0.0001f);
        }

        // --- The same buff arriving twice ---

        /// <summary>
        /// The rule the whole section exists for. Two heroes with the same slow do not make an
        /// enemy twice as slow.
        /// </summary>
        [Test]
        public void TheSameBuffTwiceDoesNotStack()
        {
            StatModifiers modifiers = WithSlow(-10f, 5f);
            modifiers.Apply(Source, ModifiableStat.MovementSpeed, StatModifierMode.Percent, -10f, 5f);

            Assert.AreEqual(1, modifiers.Count, "The second application was added instead of refreshing the first.");
            Assert.AreEqual(0.9f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        [Test]
        public void TheStrongerValueSurvives()
        {
            StatModifiers modifiers = WithSlow(-10f, 5f);
            modifiers.Apply(Source, ModifiableStat.MovementSpeed, StatModifierMode.Percent, -25f, 5f);

            Assert.AreEqual(0.75f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        [Test]
        public void AWeakerValueDoesNotReplaceAStrongerOne()
        {
            StatModifiers modifiers = WithSlow(-25f, 5f);
            modifiers.Apply(Source, ModifiableStat.MovementSpeed, StatModifierMode.Percent, -10f, 5f);

            Assert.AreEqual(0.75f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        /// <summary>
        /// The duration is always the new one, even when the value that survives is the old one.
        /// Refreshing a weak effect must not be worse than doing nothing.
        /// </summary>
        [Test]
        public void AWeakerApplicationStillRefreshesTheDuration()
        {
            StatModifiers modifiers = WithSlow(-25f, 5f);
            modifiers.Tick(4f);

            modifiers.Apply(Source, ModifiableStat.MovementSpeed, StatModifierMode.Percent, -10f, 5f);
            modifiers.Tick(2f);

            Assert.AreEqual(1, modifiers.Count, "The refreshed buff expired on the old clock.");
            Assert.AreEqual(0.75f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        /// <summary>
        /// Two **different** abilities touching the same stat still add up. The rule is about one
        /// source arriving again, not about the stat.
        /// </summary>
        [Test]
        public void DifferentSourcesOnTheSameStatStillAddUp()
        {
            StatModifiers modifiers = WithSlow(-10f, 5f);
            modifiers.Apply("another-ability", ModifiableStat.MovementSpeed, StatModifierMode.Percent, -10f, 5f);

            Assert.AreEqual(2, modifiers.Count);
            Assert.AreEqual(0.8f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        /// <summary>
        /// Percentages from different sources are added before being applied, so two 20% buffs give
        /// 40% and not 44%. Multiplying them would make the result depend on the order they arrived
        /// in, which nobody could predict.
        /// </summary>
        [Test]
        public void PercentagesAddUpBeforeBeingApplied()
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply("one", ModifiableStat.AttackSpeed, StatModifierMode.Percent, 20f, 5f);
            modifiers.Apply("two", ModifiableStat.AttackSpeed, StatModifierMode.Percent, 20f, 5f);

            Assert.AreEqual(1.4f, modifiers.Apply(ModifiableStat.AttackSpeed, 1f), 0.0001f);
        }

        /// <summary>
        /// Flat first, percentage second, so a percentage always reads as a share of the whole.
        /// The other order would give two answers for the same pair of buffs.
        /// </summary>
        [Test]
        public void FlatIsAppliedBeforePercent()
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply("flat", ModifiableStat.Power, StatModifierMode.Flat, 10f, 5f);
            modifiers.Apply("percent", ModifiableStat.Power, StatModifierMode.Percent, 100f, 5f);

            Assert.AreEqual(40f, modifiers.Apply(ModifiableStat.Power, 10f), 0.0001f);
        }

        // --- Running out ---

        [Test]
        public void ABuffExpiresWhenItsDurationRunsOut()
        {
            StatModifiers modifiers = WithSlow(-10f, 5f);
            modifiers.Tick(5f);

            Assert.AreEqual(0, modifiers.Count);
            Assert.AreEqual(1f, modifiers.Apply(ModifiableStat.MovementSpeed, 1f), 0.0001f);
        }

        [Test]
        public void ABuffSurvivesUntilItsDurationIsUp()
        {
            StatModifiers modifiers = WithSlow(-10f, 5f);
            modifiers.Tick(4.9f);

            Assert.AreEqual(1, modifiers.Count);
        }

        [Test]
        public void ABuffWithNoDurationNeverArrives()
        {
            StatModifiers modifiers = new StatModifiers();
            modifiers.Apply(Source, ModifiableStat.Power, StatModifierMode.Flat, 10f, 0f);

            Assert.AreEqual(0, modifiers.Count);
        }
    }
}
