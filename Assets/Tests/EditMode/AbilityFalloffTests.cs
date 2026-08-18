using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// How much of an ability's damage survives the distance, against "### A queda de dano por
    /// distância" in abilities.md.
    ///
    /// The numbers here are the worked example from the spec and not something read back out of
    /// the code: 100 of base damage with a falloff of 0.15 is 100, 85, 61 and 38 at one, two, four
    /// and seven cells. Seven is the longest distance the board allows.
    ///
    /// The curve is multiplicative on purpose, so it never reaches zero. Being in the line always
    /// counts for something and being close always counts for more.
    /// </summary>
    public class AbilityFalloffTests
    {
        private const float SpecFalloff = 0.15f;
        private const float SpecBaseDamage = 100f;

        private TestBattle battle;
        private BattleRandom random;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
            random = new BattleRandom(20260818);
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        /// <summary>
        /// A target with no agility and no armour, so nothing between the base damage and the
        /// health bar can move. Any difference the test sees is the falloff and only the falloff.
        /// </summary>
        private Character Bare(string id, int column, int row, Team team)
        {
            return battle.Spawn(
                battle.Sheet(id, team == Team.Heroes ? CharacterKind.Hero : CharacterKind.Minion,
                    power: 0, agility: 0, constitution: 100, physicalArmor: 0),
                team, column, row);
        }

        private int DamageTaken(Character target)
        {
            return target.Stats.MaxHealth - target.CurrentHealth;
        }

        private void Breathe(Character user, float falloff, params Character[] targets)
        {
            AbilityDefinition ability = TestAbility.Instant("breath")
                .Shaped(AbilityShape.Line)
                .WithDamage(SpecBaseDamage, falloff: falloff);

            AbilityResolver.Apply(user, ability, 1, new List<Character>(targets), battle.Grid, random, null);
        }

        /// <summary>
        /// The four distances of the spec's example, in one battle, so they are all measured
        /// against the same user and the same base damage.
        /// </summary>
        [Test]
        public void DamageFallsOffWithDistanceExactlyAsTheSpecSays()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);

            Character oneAway = Bare("a", 1, 2, Team.Enemies);
            Character twoAway = Bare("b", 1, 3, Team.Enemies);
            Character fourAway = Bare("c", 1, 5, Team.Enemies);
            Character sevenAway = Bare("d", 1, 8, Team.Enemies);

            Breathe(user, SpecFalloff, oneAway, twoAway, fourAway, sevenAway);

            Assert.AreEqual(100, DamageTaken(oneAway), "The adjacent target has to take the full number on the sheet.");
            Assert.AreEqual(85, DamageTaken(twoAway));
            Assert.AreEqual(61, DamageTaken(fourAway));
            Assert.AreEqual(38, DamageTaken(sevenAway));
        }

        /// <summary>
        /// The whole reason the curve multiplies instead of subtracting. A subtractive falloff
        /// would zero out past some distance and would need a floor written by hand on every
        /// ability that used it.
        /// </summary>
        [Test]
        public void DamageNeverFallsToZeroHoweverFarTheTargetIs()
        {
            Character user = Bare("gadrat", 1, 1, Team.Heroes);
            Character farthest = Bare("far", 6, 8, Team.Enemies);

            Breathe(user, SpecFalloff, farthest);

            Assert.Greater(DamageTaken(farthest), 0, "A target on the board is never worth zero damage.");
        }

        /// <summary>
        /// The field is optional, and every ability written before it existed left it at zero.
        /// This is what says none of them moved.
        /// </summary>
        [Test]
        public void NoFalloffMeansTheSameDamageAtAnyDistance()
        {
            Character user = Bare("hero", 1, 1, Team.Heroes);

            Character close = Bare("close", 1, 2, Team.Enemies);
            Character far = Bare("far", 1, 8, Team.Enemies);

            Breathe(user, 0f, close, far);

            Assert.AreEqual(DamageTaken(close), DamageTaken(far));
            Assert.AreEqual(100, DamageTaken(far));
        }

        /// <summary>
        /// The diagonal counts as one cell, the same way the rest of the game measures. Two targets
        /// the board considers equally far have to take equal damage.
        /// </summary>
        [Test]
        public void TheDiagonalCountsAsOneCellLikeEverywhereElse()
        {
            Character user = Bare("hero", 3, 1, Team.Heroes);

            Character straight = Bare("straight", 3, 2, Team.Enemies);
            Character diagonal = Bare("diagonal", 4, 2, Team.Enemies);

            Breathe(user, SpecFalloff, straight, diagonal);

            Assert.AreEqual(DamageTaken(straight), DamageTaken(diagonal));
        }

        /// <summary>
        /// The falloff lands on the base damage, at step 1 of the damage order in attributes.md,
        /// so the armour still bites on the value that is already reduced. If it were applied after
        /// the mitigation the two would trade places and a heavily armoured target would take a
        /// different number.
        /// </summary>
        [Test]
        public void TheFalloffHitsTheBaseDamageAndMitigationStillAppliesOnTop()
        {
            Character user = Bare("hero", 1, 1, Team.Heroes);

            Character armoured = battle.Spawn(
                battle.Sheet("armoured", CharacterKind.Minion, constitution: 100, physicalArmor: 50),
                Team.Enemies, 1, 3);

            Breathe(user, SpecFalloff, armoured);

            // 85 after the falloff, then the armour curve of attributes.md takes its share.
            Assert.Greater(DamageTaken(armoured), 0);
            Assert.Less(DamageTaken(armoured), 85, "The armour was skipped, so the falloff replaced the mitigation.");
        }
    }
}
