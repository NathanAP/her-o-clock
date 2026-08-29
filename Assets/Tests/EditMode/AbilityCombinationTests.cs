using HerOClock.Abilities;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Abilities running inside a real battle, driven by the real <see cref="BattleDirector"/>.
    ///
    /// The other ability tests check one piece at a time. This one checks that the pieces meet:
    /// the director creating the casters, the ordering that gives the basic attack priority, the
    /// buffs reaching the stats mid fight, and the whole thing still ending the same way twice.
    ///
    /// It is also the layer the project cares most about, because the pitch is that a build outside
    /// the meta stays playable. A combination that only works by accident shows up here.
    /// </summary>
    public class AbilityCombinationTests
    {
        private const int Seed = 20260818;

        private TestBattle battle;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        private CharacterDefinition Sturdy(string id, CharacterKind kind, int power, int constitution)
        {
            return battle.Sheet(id, kind, power: power, agility: 10, constitution: constitution);
        }

        // --- The pieces meeting ---

        /// <summary>
        /// The blink of a speedster, written the way Tempo's is: untargetable and intangible first,
        /// then damage down a chain, then a reposition next to the last enemy. Four effects, one
        /// ability, and the order is what makes it survivable.
        /// </summary>
        [Test]
        public void AChainBlinkAppliesEveryEffectInOneUse()
        {
            CharacterDefinition heroSheet = Sturdy("hero", CharacterKind.Hero, 40, 30);

            heroSheet.Give(TestAbility.Timed("fastAndFurious", 0f, 0.2f, 0f, 20f)
                .Shaped(AbilityShape.Chain, AbilityWho.Enemies, 3)
                .WithStatus(StatusKind.Untargetable, 1f, EffectTarget.Self)
                .WithStatus(StatusKind.Intangible, 1f, EffectTarget.Self)
                .WithDamage(50f, powerScaling: 0.2f)
                .WithMove());

            heroSheet.Abilities[0].Targeting.MaxTargets = RankedValue.Constant(3f);
            heroSheet.Abilities[0].Targeting.JumpRange = 3;

            Character hero = battle.Spawn(heroSheet, Team.Heroes, 3, 1);

            CharacterDefinition minion = Sturdy("minion", CharacterKind.Minion, 2, 60);
            Character first = battle.Spawn(minion, Team.Enemies, 3, 3);
            Character second = battle.Spawn(minion, Team.Enemies, 4, 4);
            Character third = battle.Spawn(minion, Team.Enemies, 5, 5);

            BattleDirector director = battle.Direct(new BattleRandom(Seed), hero, first, second, third);

            // Long enough for the ability to come out once, and far short of its cooldown.
            for (int i = 0; i < 60; i++)
            {
                director.Tick(BattleDirector.FixedStep);
            }

            Assert.Less(first.CurrentHealth, first.Stats.MaxHealth, "The chain never reached its first link.");
            Assert.Less(third.CurrentHealth, third.Stats.MaxHealth, "The chain never reached its third link.");

            Assert.IsTrue(hero.IsUntargetable, "The hero should still be untargetable a second in.");
            Assert.IsTrue(hero.IsIntangible, "The hero should still be intangible a second in.");

            Assert.AreEqual(1, GridPosition.Distance(hero.Position, third.Position),
                "The hero did not end up beside the last enemy of the chain.");
        }

        /// <summary>
        /// The pair Tempo carries: untargetable takes her out of the enemies' target chain, and
        /// intangible covers what was already coming. Together they are a second of safety, which
        /// is what makes diving into a group of enemies a build rather than a mistake.
        /// </summary>
        [Test]
        public void UntargetableAndIntangibleTogetherBuyASecondOfSafety()
        {
            CharacterDefinition heroSheet = Sturdy("hero", CharacterKind.Hero, 10, 5);

            heroSheet.Give(TestAbility.Timed("dive", 0f, 0f, 0f, 60f)
                .Shaped(AbilityShape.Self, AbilityWho.Self)
                .WithStatus(StatusKind.Untargetable, 2f, EffectTarget.Self)
                .WithStatus(StatusKind.Intangible, 2f, EffectTarget.Self));

            Character hero = battle.Spawn(heroSheet, Team.Heroes, 3, 4);

            CharacterDefinition brute = Sturdy("brute", CharacterKind.Minion, 60, 40);
            Character one = battle.Spawn(brute, Team.Enemies, 3, 5);
            Character two = battle.Spawn(brute, Team.Enemies, 4, 5);

            BattleDirector director = battle.Direct(new BattleRandom(Seed), hero, one, two);

            for (int i = 0; i < 100; i++)
            {
                director.Tick(BattleDirector.FixedStep);
            }

            Assert.AreEqual(hero.Stats.MaxHealth, hero.CurrentHealth,
                "The hero took damage while it was supposed to be untouchable.");
        }

        /// <summary>
        /// A taunt applied by an ability has to actually redirect the enemy, which means it has to
        /// reach the target selector the basic attack uses. Two separate systems agreeing.
        ///
        /// Both heroes stand within the enemy's reach on purpose. Without the taunt the standard
        /// chain would pick the fragile one, since the two are the same distance away and rule 4
        /// takes the lower maximum health. So the only thing that can move the blow is the taunt.
        /// </summary>
        [Test]
        public void ATauntFromAnAbilityPullsTheEnemyOffSomebodyElse()
        {
            CharacterDefinition tankSheet = Sturdy("tank", CharacterKind.Hero, 5, 200);

            tankSheet.Give(TestAbility.Timed("provoke", 0f, 0f, 0f, 60f)
                .Shaped(AbilityShape.Single, AbilityWho.Enemies, 8)
                .WithStatus(StatusKind.Taunted, 10f));

            Character tank = battle.Spawn(tankSheet, Team.Heroes, 4, 4);
            Character fragile = battle.Spawn(Sturdy("fragile", CharacterKind.Hero, 5, 3), Team.Heroes, 5, 4);
            Character enemy = battle.Spawn(Sturdy("enemy", CharacterKind.Minion, 30, 40), Team.Enemies, 5, 5);

            BattleDirector director = battle.Direct(new BattleRandom(Seed), tank, fragile, enemy);

            for (int i = 0; i < 240; i++)
            {
                director.Tick(BattleDirector.FixedStep);
            }

            Assert.AreSame(tank, enemy.TauntedBy, "The taunt never reached the enemy.");

            Assert.AreEqual(fragile.Stats.MaxHealth, fragile.CurrentHealth,
                "The enemy kept hitting the fragile hero standing right next to it, so the taunt did nothing.");

            Assert.Less(tank.CurrentHealth, tank.Stats.MaxHealth,
                "Nobody was attacked at all, so the test proves nothing about who was chosen.");
        }

        /// <summary>
        /// A buff has to be worth something in a real fight, not only in a stat readout. The same
        /// pair of characters, with and without a speed buff, cannot take the same time.
        /// </summary>
        [Test]
        public void ASpeedBuffMakesARealFightShorter()
        {
            Assert.Less(SecondsToWin(true), SecondsToWin(false),
                "The attack speed buff made no difference to how long the fight took.");
        }

        private float SecondsToWin(bool buffed)
        {
            using (TestBattle arena = new TestBattle())
            {
                CharacterDefinition heroSheet = arena.Sheet("hero", CharacterKind.Hero, power: 20, agility: 10, constitution: 40);

                if (buffed)
                {
                    heroSheet.Give(TestAbility.Timed("haste", 0f, 0f, 0f, 600f)
                        .Shaped(AbilityShape.Self, AbilityWho.Self)
                        .WithBuff(ModifiableStat.AttackSpeed, 100f, 600f, EffectTarget.Self));
                }

                Character hero = arena.Spawn(heroSheet, Team.Heroes, 3, 1);
                Character enemy = arena.Spawn(
                    arena.Sheet("enemy", CharacterKind.Minion, power: 1, constitution: 40), Team.Enemies, 3, 2);

                BattleDirector director = arena.Direct(new BattleRandom(Seed), hero, enemy);

                int steps = TestBattle.RunUntilOver(director, 120f);

                Assert.IsFalse(enemy.IsAlive, "The setup never resolved, so the timing means nothing.");

                return steps * BattleDirector.FixedStep;
            }
        }

        // --- The guarantee the whole project rests on ---

        /// <summary>
        /// Abilities are the largest new source of decisions in the game, and the promise is that
        /// the same seed and the same starting state replay blow for blow. If anything in here ever
        /// reaches for an unordered collection or a random tie-break, this is what says so.
        /// </summary>
        [Test]
        public void TheSameSeedStillProducesTheSameFightWithAbilitiesInPlay()
        {
            string first = RunAndDescribe();
            string second = RunAndDescribe();

            Assert.AreEqual(first, second, "The same battle came out differently the second time.");
        }

        private string RunAndDescribe()
        {
            using (TestBattle arena = new TestBattle())
            {
                CharacterDefinition heroSheet = arena.Sheet("hero", CharacterKind.Hero, power: 30, agility: 20, constitution: 40);

                heroSheet.Give(TestAbility.Timed("chain", 0f, 0.2f, 0.1f, 3f)
                    .Shaped(AbilityShape.Chain, AbilityWho.Enemies, 4)
                    .WithDamage(20f, powerScaling: 0.3f)
                    .WithBuff(ModifiableStat.MovementSpeed, -20f, 2f));

                heroSheet.Abilities[0].Targeting.MaxTargets = RankedValue.Constant(2f);
                heroSheet.Abilities[0].Targeting.JumpRange = 3;

                Character hero = arena.Spawn(heroSheet, Team.Heroes, 3, 1);

                CharacterDefinition minion = arena.Sheet("minion", CharacterKind.Minion, power: 8, agility: 5, constitution: 25);
                Character one = arena.Spawn(minion, Team.Enemies, 2, 5);
                Character two = arena.Spawn(minion, Team.Enemies, 4, 5);
                Character three = arena.Spawn(minion, Team.Enemies, 3, 6);

                BattleDirector director = arena.Direct(new BattleRandom(Seed), hero, one, two, three);

                int steps = TestBattle.RunUntilOver(director, 120f);

                return steps + "|" + hero.CurrentHealth + "|" + one.CurrentHealth
                    + "|" + two.CurrentHealth + "|" + three.CurrentHealth
                    + "|" + hero.Position + "|" + one.Position + "|" + two.Position + "|" + three.Position;
            }
        }
    }
}
