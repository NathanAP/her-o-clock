using System.Collections.Generic;
using System.Text;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Movement;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Runs whole fights in memory and checks the promises the fixed step was introduced to make.
    ///
    /// These are the closest thing this game has to an end to end test. There is no player input
    /// to drive, so what a fight has to be checked against is its own numbers: the rate blows
    /// land at, and the fight coming out identical every time.
    /// </summary>
    public class BattleSimulationTests
    {
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

        // --- Attack cadence ---

        /// <summary>
        /// The rate blows actually land at has to match the sheet.
        ///
        /// Before the overshoot started being carried, a blow could only land on a step boundary
        /// and the interval was rounded up to whole steps, so a character at 4 attacks per second
        /// made 3.75 of them. The loss grew with speed, which punished exactly the characters
        /// that pay for speed. A 1% tolerance is far tighter than the 6.25% that was being lost.
        /// </summary>
        [TestCase(0, 1.00f)]
        [TestCase(5, 1.05f)]
        [TestCase(150, 2.50f)]
        [TestCase(250, 3.50f)]
        [TestCase(300, 4.00f)]
        public void BlowsLandAtTheRateTheSheetPromises(int agility, float expectedPerSecond)
        {
            // No damage base means every blow deals nothing, so the target survives the whole run
            // and the count is not cut short by a death. The blow still happens and is still
            // reported. POW alone cannot do this any more: it multiplies the base rather than being
            // the damage, so a character with no base does nothing however much of it it carries.
            CharacterDefinition attackerSheet = battle.Sheet(
                "attacker", CharacterKind.Hero, power: 0, agility: agility, baseDamage: 0);
            CharacterDefinition targetSheet = battle.Sheet("target", CharacterKind.Minion);

            Character attacker = battle.Spawn(attackerSheet, Team.Heroes, 3, 4);
            Character target = battle.Spawn(targetSheet, Team.Enemies, 3, 5);

            Assert.AreEqual(expectedPerSecond, attacker.Stats.AttacksPerSecond, 0.001f,
                "The sheet does not produce the attack speed this case is about.");

            int blows = 0;
            CharacterAttacker timer = new CharacterAttacker(attacker, new BattleRandom(1));
            timer.Attacked += (_, __, ___) => blows++;

            IReadOnlyList<Character> enemies = new[] { target };

            const float seconds = 200f;
            int steps = Mathf.RoundToInt(seconds / BattleDirector.FixedStep);

            for (int step = 0; step < steps; step++)
            {
                timer.Tick(BattleDirector.FixedStep, enemies, false);
            }

            // One blow is lost to the wind up every character pays before its first attack.
            float expected = expectedPerSecond * seconds;
            Assert.AreEqual(expected, blows, expected * 0.01f);
        }

        /// <summary>
        /// The timer is held at zero while the character walks, so nobody can charge up several
        /// blows by stepping in and out of range and unload them at once.
        /// </summary>
        [Test]
        public void TimeSpentWalkingDoesNotBankBlows()
        {
            CharacterDefinition sheet = battle.Sheet("attacker", CharacterKind.Hero, power: 0);
            Character attacker = battle.Spawn(sheet, Team.Heroes, 3, 4);
            Character target = battle.Spawn(battle.Sheet("target", CharacterKind.Minion), Team.Enemies, 3, 5);

            int blows = 0;
            CharacterAttacker timer = new CharacterAttacker(attacker, new BattleRandom(1));
            timer.Attacked += (_, __, ___) => blows++;

            IReadOnlyList<Character> enemies = new[] { target };

            // Ten seconds of walking at one attack per second would bank ten blows.
            int walking = Mathf.RoundToInt(10f / BattleDirector.FixedStep);
            for (int step = 0; step < walking; step++)
            {
                timer.Tick(BattleDirector.FixedStep, enemies, true);
            }

            Assert.AreEqual(0, blows, "Nothing should land while walking.");

            // One single step standing still is allowed to land the one blow that was due.
            timer.Tick(BattleDirector.FixedStep, enemies, false);

            Assert.AreEqual(1, blows, "Exactly one blow was owed, not ten.");
        }

        // --- Thorns ---

        /// <summary>
        /// attributes.md: the damage thorns sends back is physical, and life steal reacts to
        /// physical damage, so whoever reflected it heals from it.
        ///
        /// It is the rule that lets somebody build a tank who recovers by being hit. Without it,
        /// the two defensive sources of healing in the game would never talk to each other and a
        /// thorns build would be strictly worse than plain life steal.
        /// </summary>
        [Test]
        public void ReflectedDamageHealsTheOneWhoReflectedIt()
        {
            CharacterDefinition attackerSheet = battle.Sheet("attacker", CharacterKind.Minion, power: 100, constitution: 500);

            CharacterDefinition thornySheet = battle.Sheet("thorny", CharacterKind.Hero, power: 0, constitution: 500);
            thornySheet.Stats.ThornsPercent = 50f;
            thornySheet.Stats.LifeStealPercent = 50f;

            Character thorny = battle.Spawn(thornySheet, Team.Heroes, 3, 4);
            Character attacker = battle.Spawn(attackerSheet, Team.Enemies, 3, 5);

            // Wounded first, so there is room to be healed into.
            thorny.TakeDamage(2000);
            int wounded = thorny.CurrentHealth;

            BattleDirector director = battle.Direct(new BattleRandom(1), thorny, attacker);

            // One blow lands after the attacker's wind up, and its reflection with it.
            TestBattle.RunUntilOver(director, 1.5f);

            Assert.Less(thorny.CurrentHealth, thorny.Stats.MaxHealth, "The setup should leave it wounded.");
            Assert.Greater(thorny.CurrentHealth, wounded - 100,
                "Taking 100 and reflecting 50 with half of that stolen back should have healed some of it.");
        }

        [Test]
        public void ReflectingWithoutLifeStealHealsNothing()
        {
            CharacterDefinition attackerSheet = battle.Sheet("attacker", CharacterKind.Minion, power: 100, constitution: 500);

            CharacterDefinition thornySheet = battle.Sheet("thorny", CharacterKind.Hero, power: 0, constitution: 500);
            thornySheet.Stats.ThornsPercent = 50f;

            Character thorny = battle.Spawn(thornySheet, Team.Heroes, 3, 4);
            Character attacker = battle.Spawn(attackerSheet, Team.Enemies, 3, 5);

            thorny.TakeDamage(2000);
            int wounded = thorny.CurrentHealth;

            BattleDirector director = battle.Direct(new BattleRandom(1), thorny, attacker);
            TestBattle.RunUntilOver(director, 1.5f);

            Assert.LessOrEqual(thorny.CurrentHealth, wounded, "Nothing should have healed it.");
        }

        // --- Reproducibility ---

        /// <summary>
        /// The same seed and the same starting state have to give the same fight, blow by blow.
        /// It is the promise the Console prints on every run, and the reason a reported oddity
        /// can be investigated instead of guessed at.
        /// </summary>
        [Test]
        public void TheSameSeedGivesTheSameFight()
        {
            Assert.AreEqual(FightLog(4242), FightLog(4242));
        }

        [Test]
        public void ADifferentSeedGivesADifferentFight()
        {
            Assert.AreNotEqual(FightLog(4242), FightLog(9001));
        }

        /// <summary>
        /// Runs a fight with evasion in play and writes down every blow, so two runs can be
        /// compared exactly rather than by their outcome alone.
        /// </summary>
        private string FightLog(int seed)
        {
            TestBattle run = new TestBattle();

            try
            {
                // Agility on both sides so evasion is rolled constantly and the random sequence
                // really drives the fight.
                CharacterDefinition heroSheet = run.Sheet("hero", CharacterKind.Hero, power: 8, agility: 120, constitution: 20);
                CharacterDefinition minionSheet = run.Sheet("minion", CharacterKind.Minion, power: 5, agility: 90, constitution: 12);

                Character hero = run.Spawn(heroSheet, Team.Heroes, 3, 3);
                Character other = run.Spawn(heroSheet, Team.Heroes, 4, 3);
                Character minionA = run.Spawn(minionSheet, Team.Enemies, 3, 6);
                Character minionB = run.Spawn(minionSheet, Team.Enemies, 4, 6);

                StringBuilder log = new StringBuilder();

                BattleDirector director = run.Direct(new BattleRandom(seed), hero, other, minionA, minionB);
                director.Attacked += (attacker, target, result) => log
                    .Append(attacker.name).Append(" hit ").Append(target.name)
                    .Append(" for ").Append(result.Damage)
                    .Append(result.Evaded ? result.PerfectEvasion ? " perfect" : " evaded" : "")
                    .Append('\n');

                int steps = TestBattle.RunUntilOver(director, 120f);

                log.Append("ended after ").Append(steps).Append(" steps\n");
                log.Append("hero health ").Append(hero.CurrentHealth).Append('\n');
                log.Append("other health ").Append(other.CurrentHealth).Append('\n');

                Assert.Greater(log.Length, 200, "The fight produced almost nothing, so this proves little.");

                return log.ToString();
            }
            finally
            {
                run.Dispose();
            }
        }

        // --- A fight actually resolves ---

        [Test]
        public void AStrongerSideWinsAndTheFightEnds()
        {
            CharacterDefinition heroSheet = battle.Sheet("hero", CharacterKind.Hero, power: 30, constitution: 50);
            CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 1, constitution: 2);

            Character hero = battle.Spawn(heroSheet, Team.Heroes, 3, 4);
            Character minion = battle.Spawn(minionSheet, Team.Enemies, 3, 5);

            BattleDirector director = battle.Direct(new BattleRandom(1), hero, minion);

            bool? heroesWon = null;
            director.BattleEnded += won => heroesWon = won;

            TestBattle.RunUntilOver(director, 60f);

            Assert.IsTrue(heroesWon.HasValue, "The fight never ended.");
            Assert.IsTrue(heroesWon.Value);
            Assert.IsFalse(minion.IsAlive);
            Assert.IsTrue(hero.IsAlive);
        }

        /// <summary>
        /// A melee character has to close the distance on its own. It is the rule that lets the
        /// front line reach the villain once the minions in front of it are gone.
        /// </summary>
        [Test]
        public void AMeleeCharacterWalksIntoRangeBeforeFighting()
        {
            CharacterDefinition heroSheet = battle.Sheet("hero", CharacterKind.Hero, power: 20, constitution: 50);
            CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 1, constitution: 2);

            Character hero = battle.Spawn(heroSheet, Team.Heroes, 3, 1);
            Character minion = battle.Spawn(minionSheet, Team.Enemies, 3, 8);

            Assert.AreEqual(7, GridPosition.Distance(hero.Position, minion.Position));

            BattleDirector director = battle.Direct(new BattleRandom(1), hero, minion);
            TestBattle.RunUntilOver(director, 60f);

            Assert.IsFalse(minion.IsAlive, "The hero never got there.");
        }

        /// <summary>
        /// A character whose weapon has a minimum range steps back when an enemy is inside it,
        /// and the cell it picks is one it could shoot from.
        ///
        /// Only the archer's own mover is ticked here, so nothing else moves while the rule is
        /// being checked. Driving a whole fight instead would mix in the chaser's movement, and
        /// what that produces is a balance question rather than a question about this rule.
        /// </summary>
        [Test]
        public void ARangedCharacterStepsOutOfItsMinimumRange()
        {
            CharacterDefinition archerSheet = battle.Sheet(
                "archer", CharacterKind.Hero, power: 4, constitution: 50, minRange: 2, maxRange: 4);
            CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 0, constitution: 100);

            Character archer = battle.Spawn(archerSheet, Team.Heroes, 3, 4);
            Character minion = battle.Spawn(minionSheet, Team.Enemies, 3, 5);

            GridPosition start = archer.Position;

            Assert.AreEqual(1, GridPosition.Distance(start, minion.Position),
                "The archer has to start inside its own minimum range for this to prove anything.");

            CharacterMover mover = new CharacterMover(archer, battle.Grid);
            mover.Tick(BattleDirector.FixedStep, new[] { minion });

            // The destination cell is claimed as soon as the step begins, so the new position is
            // already visible even though the character is still walking there.
            Assert.AreNotEqual(start, archer.Position, "The archer stayed where it could not shoot.");
            Assert.GreaterOrEqual(GridPosition.Distance(archer.Position, minion.Position), 2,
                "The archer moved, but not to a cell it can shoot from.");
        }

        /// <summary>
        /// The opposite case, so the test above cannot pass for the wrong reason: a character with
        /// no minimum range is happy where it stands.
        /// </summary>
        [Test]
        public void AMeleeCharacterDoesNotBackAway()
        {
            CharacterDefinition heroSheet = battle.Sheet("hero", CharacterKind.Hero, power: 0, constitution: 50);
            CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 0, constitution: 100);

            Character hero = battle.Spawn(heroSheet, Team.Heroes, 3, 4);
            Character minion = battle.Spawn(minionSheet, Team.Enemies, 3, 5);

            GridPosition start = hero.Position;

            BattleDirector director = battle.Direct(new BattleRandom(1), hero, minion);
            TestBattle.RunUntilOver(director, 10f);

            Assert.AreEqual(start, hero.Position, "A melee character with a target in range should stay put.");
        }

        /// <summary>
        /// A ranged character given room and speed does get its shots off.
        ///
        /// The speed matters and the test says so on purpose. A mover claims its destination cell
        /// the moment it starts walking, so a chaser is already adjacent from the target
        /// selection's point of view for the whole of its step. An archer that walks at the same
        /// pace as its pursuer therefore finds itself at distance 1 every time it stops, and never
        /// gets a tick where it is both still and in range. Six cells per second against two is
        /// enough to break out of that.
        /// </summary>
        [Test]
        public void AFastRangedCharacterOutrunsAChaserAndShoots()
        {
            CharacterDefinition archerSheet = battle.Sheet(
                "archer", CharacterKind.Hero, power: 4, agility: 400, constitution: 50, minRange: 2, maxRange: 4);
            CharacterDefinition minionSheet = battle.Sheet("minion", CharacterKind.Minion, power: 0, constitution: 500);

            // Row 7 leaves the archer the whole board to retreat across.
            Character archer = battle.Spawn(archerSheet, Team.Heroes, 3, 7);
            Character minion = battle.Spawn(minionSheet, Team.Enemies, 3, 8);

            int fullHealth = minion.CurrentHealth;

            BattleDirector director = battle.Direct(new BattleRandom(1), archer, minion);
            TestBattle.RunUntilOver(director, 20f);

            Assert.Less(minion.CurrentHealth, fullHealth, "The archer never managed to shoot.");
        }
    }
}
