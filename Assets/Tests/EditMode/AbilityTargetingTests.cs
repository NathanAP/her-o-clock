using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Who an ability hits, against "## Escolha de alvo" in abilities.md.
    ///
    /// The five shapes and the priority, which replaces the head of the standard chain while the
    /// rest of it carries on underneath as the tie-break.
    /// </summary>
    public class AbilityTargetingTests
    {
        private TestBattle battle;
        private CharacterDefinition sheet;

        [SetUp]
        public void SetUp()
        {
            battle = new TestBattle();
            sheet = battle.Sheet("body", CharacterKind.Minion, power: 10, constitution: 10);
        }

        [TearDown]
        public void TearDown()
        {
            battle.Dispose();
        }

        private Character Hero(int column, int row)
        {
            return battle.Spawn(sheet, Team.Heroes, column, row);
        }

        private Character Enemy(int column, int row)
        {
            return battle.Spawn(sheet, Team.Enemies, column, row);
        }

        private static List<Character> Resolve(
            Character user, AbilityTargeting targeting, IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            return AbilityTargetResolver.Resolve(user, targeting, 1, allies, enemies);
        }

        // --- The shapes ---

        [Test]
        public void SelfHitsOnlyTheUser()
        {
            Character user = Hero(3, 1);
            Character enemy = Enemy(3, 2);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Self, AbilityWho.Self).Targeting,
                new[] { user }, new[] { enemy });

            Assert.AreEqual(1, found.Count);
            Assert.AreSame(user, found[0]);
        }

        [Test]
        public void SingleHitsOneTarget()
        {
            Character user = Hero(3, 1);
            Character near = Enemy(3, 2);
            Character far = Enemy(3, 5);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Targeting, new[] { user }, new[] { near, far });

            Assert.AreEqual(1, found.Count, "Single hit more than one character.");
            Assert.AreSame(near, found[0], "Single took somebody other than the nearest.");
        }

        /// <summary>
        /// An area catches **everyone** inside it, ally and enemy alike. `who` says who the ability
        /// was looking for; it protects nobody from standing in the way.
        /// </summary>
        [Test]
        public void AnAreaCatchesAlliesStandingInIt()
        {
            Character user = Hero(3, 3);
            Character friend = Hero(3, 2);
            Character enemy = Enemy(3, 4);
            Character farAway = Enemy(1, 8);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Area).Targeting;
            targeting.AreaColumns = 3;
            targeting.AreaRows = 3;

            List<Character> found = Resolve(user, targeting, new[] { user, friend }, new[] { enemy, farAway });

            Assert.Contains(user, found, "The user is inside its own area.");
            Assert.Contains(friend, found, "An ally in the rectangle was spared, which the spec forbids.");
            Assert.Contains(enemy, found);
            Assert.IsFalse(found.Contains(farAway), "Somebody outside the rectangle was hit.");
        }

        [Test]
        public void AnAreaIsCentredOnTheUserAndNeverMoves()
        {
            Character user = Hero(1, 1);
            Character enemy = Enemy(5, 6);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Area).Targeting;
            targeting.AreaColumns = 3;
            targeting.AreaRows = 3;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { enemy });

            Assert.IsFalse(found.Contains(enemy),
                "The area moved to where the enemies were, which was removed in 0.6.0.3.");
        }

        /// <summary>
        /// The first link is chosen by the priority, and every jump after it goes to whoever is
        /// nearest the previous target.
        /// </summary>
        [Test]
        public void AChainJumpsFromOneTargetToTheNext()
        {
            Character user = Hero(1, 1);
            Character first = Enemy(2, 2);
            Character second = Enemy(3, 3);
            Character third = Enemy(4, 4);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Chain, AbilityWho.Enemies, 2).Targeting;
            targeting.MaxTargets = RankedValue.Constant(3f);
            targeting.JumpRange = 2;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { first, second, third });

            Assert.AreEqual(3, found.Count);
            Assert.AreSame(first, found[0]);
            Assert.AreSame(second, found[1]);
            Assert.AreSame(third, found[2]);
        }

        [Test]
        public void AChainReachesFewerTargetsWhenThereAreNotEnough()
        {
            Character user = Hero(1, 1);
            Character only = Enemy(2, 2);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Chain, AbilityWho.Enemies, 2).Targeting;
            targeting.MaxTargets = RankedValue.Constant(5f);
            targeting.JumpRange = 1;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { only });

            Assert.AreEqual(1, found.Count);
        }

        [Test]
        public void AChainNeverHitsTheSameTargetTwice()
        {
            Character user = Hero(1, 1);
            Character first = Enemy(2, 2);
            Character second = Enemy(3, 3);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Chain, AbilityWho.Enemies, 2).Targeting;
            targeting.MaxTargets = RankedValue.Constant(4f);
            targeting.JumpRange = 3;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { first, second });

            Assert.AreEqual(2, found.Count);
        }

        // --- Line ---

        /// <summary>
        /// A line only considers characters standing on one of the eight straight directions, and
        /// that filter is what guarantees the chosen target is actually hit.
        /// </summary>
        [Test]
        public void ALineOnlyConsidersWhoIsAlignedWithTheUser()
        {
            Character user = Hero(3, 1);
            Character straightAhead = Enemy(3, 4);
            Character offTheLine = Enemy(5, 4);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Line).Targeting,
                new[] { user }, new[] { offTheLine, straightAhead });

            Assert.Contains(straightAhead, found);
            Assert.IsFalse(found.Contains(offTheLine), "Somebody off every straight direction was picked.");
        }

        [Test]
        public void ALineHitsEverybodyItCrossesAndNotOnlyTheFirst()
        {
            Character user = Hero(3, 1);
            Character near = Enemy(3, 3);
            Character far = Enemy(3, 6);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Line).Targeting,
                new[] { user }, new[] { near, far });

            Assert.AreEqual(2, found.Count, "The line stopped at the first target instead of carrying on.");
        }

        [Test]
        public void ALineRunsAlongDiagonalsToo()
        {
            Character user = Hero(1, 1);
            Character diagonal = Enemy(4, 4);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Line).Targeting,
                new[] { user }, new[] { diagonal });

            Assert.Contains(diagonal, found, "The diagonal counts as distance 1, so it is a straight direction.");
        }

        [Test]
        public void ALineWithNobodyAlignedFindsNothing()
        {
            Character user = Hero(1, 1);
            Character offTheLine = Enemy(3, 2);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Line).Targeting,
                new[] { user }, new[] { offTheLine });

            Assert.IsEmpty(found, "With nobody aligned the ability has no target and holds its charge.");
        }

        // --- Priority ---

        [Test]
        public void TheDefaultPriorityIsTheNearest()
        {
            Character user = Hero(3, 1);
            Character near = Enemy(3, 2);
            Character far = Enemy(3, 6);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Targeting, new[] { user }, new[] { far, near });

            Assert.AreSame(near, found[0]);
        }

        [Test]
        public void FarthestTakesTheOneFurthestAway()
        {
            Character user = Hero(3, 1);
            Character near = Enemy(3, 2);
            Character far = Enemy(3, 6);

            AbilityTargeting targeting = TestAbility.Instant("a").Targeting;
            targeting.Priority = TargetPriority.Farthest;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { near, far });

            Assert.AreSame(far, found[0]);
        }

        [Test]
        public void LowestHealthPercentTakesTheMostHurt()
        {
            Character user = Hero(3, 1);
            Character healthy = Enemy(3, 2);
            Character hurt = Enemy(3, 6);

            hurt.TakeDamage(hurt.Stats.MaxHealth - 1);

            AbilityTargeting targeting = TestAbility.Instant("a").Targeting;
            targeting.Priority = TargetPriority.LowestHealthPercent;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { healthy, hurt });

            Assert.AreSame(hurt, found[0], "It took the healthy one, so the priority is being ignored.");
        }

        /// <summary>
        /// The tie-break is the standard chain of gameplay.md, which ends on a positional rule that
        /// can never tie. There is no "any of them": the same battle from the same seed has to end
        /// the same way.
        /// </summary>
        [Test]
        public void ATieIsBrokenByTheStandardChainAndNeverAtRandom()
        {
            Character user = Hero(3, 1);
            Character lowColumn = Enemy(2, 4);
            Character highColumn = Enemy(4, 4);

            AbilityTargeting targeting = TestAbility.Instant("a").Targeting;

            List<Character> first = Resolve(user, targeting, new[] { user }, new[] { lowColumn, highColumn });
            List<Character> second = Resolve(user, targeting, new[] { user }, new[] { highColumn, lowColumn });

            Assert.AreSame(lowColumn, first[0], "The lowest column should win an otherwise exact tie.");
            Assert.AreSame(first[0], second[0], "The order the candidates arrived in changed the answer.");
        }

        // --- Taunt and untargetable ---

        /// <summary>
        /// A taunt sits above everything, including an ability that declares its own priority.
        /// </summary>
        [Test]
        public void ATauntOverridesADeclaredPriority()
        {
            Character user = Hero(3, 1);
            Character near = Enemy(3, 2);
            Character taunter = Enemy(3, 5);

            user.ApplyTaunt(taunter, 5f);

            AbilityTargeting targeting = TestAbility.Instant("a").Targeting;
            targeting.Priority = TargetPriority.Nearest;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { near, taunter });

            Assert.AreSame(taunter, found[0], "The taunt was ignored by the ability.");
        }

        [Test]
        public void AnUntargetableCharacterIsNotChosen()
        {
            Character user = Hero(3, 1);
            Character hidden = Enemy(3, 2);
            Character visible = Enemy(3, 5);

            hidden.Statuses.Apply(StatusKind.Untargetable, 5f);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Targeting, new[] { user }, new[] { hidden, visible });

            Assert.AreSame(visible, found[0], "An untargetable character was picked as a target.");
        }

        /// <summary>
        /// Untargetable takes a character out of the running, but an area chooses nobody: it covers
        /// a rectangle and catches whoever is inside.
        /// </summary>
        [Test]
        public void AnAreaStillCatchesAnUntargetableCharacter()
        {
            Character user = Hero(3, 3);
            Character hidden = Enemy(3, 4);

            hidden.Statuses.Apply(StatusKind.Untargetable, 5f);

            AbilityTargeting targeting = TestAbility.Instant("a").Shaped(AbilityShape.Area).Targeting;
            targeting.AreaColumns = 3;
            targeting.AreaRows = 3;

            List<Character> found = Resolve(user, targeting, new[] { user }, new[] { hidden });

            Assert.Contains(hidden, found);
        }

        // --- Range ---

        [Test]
        public void SomebodyBeyondTheAbilitysRangeIsNotAValidTarget()
        {
            Character user = Hero(3, 1);
            Character far = Enemy(3, 6);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Shaped(AbilityShape.Single, AbilityWho.Enemies, 2).Targeting,
                new[] { user }, new[] { far });

            Assert.IsEmpty(found);
        }

        [Test]
        public void ADeadCharacterIsNeverATarget()
        {
            Character user = Hero(3, 1);
            Character fallen = Enemy(3, 2);

            fallen.TakeDamage(fallen.Stats.MaxHealth);

            List<Character> found = Resolve(user,
                TestAbility.Instant("a").Targeting, new[] { user }, new[] { fallen });

            Assert.IsEmpty(found);
        }
    }
}
