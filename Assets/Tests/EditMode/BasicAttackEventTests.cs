using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The signal that says "somebody swung a basic attack", which the view layer uses to draw the
    /// blow itself.
    ///
    /// It exists apart from `Attacked` because that one cannot answer the question. `Attacked`
    /// fires for the blow, fires **again** for the thorns coming back at the attacker, and the
    /// director raises it a third way for every blow an ability lands. Drawing a projectile off it
    /// would send a bolt flying backwards out of whoever was hit.
    ///
    /// The ability case is not tested here because it cannot happen: `AbilityCaster` has no such
    /// event to raise. Only `CharacterAttacker` does, and only from a basic attack.
    ///
    /// This is deliberately about the event and not about the projectile. The bolt is decoration
    /// and pays no test cost; this is the combat layer promising something the decoration leans on.
    /// </summary>
    public class BasicAttackEventTests
    {
        /// <summary>
        /// One second, because the base rate is one attack per second and the cooldown starts full.
        /// Long enough for the shooter's first blow, short enough that the second never comes.
        /// </summary>
        private const float OneBlow = 1.1f;

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

        /// <summary>
        /// A shooter that reaches four cells and a target that only reaches one, standing four
        /// cells apart.
        ///
        /// The separation is what makes these tests readable: with both characters adjacent they
        /// both swing, even the one with no power, and no assertion can tell whose event is whose.
        /// Out of the target's reach, exactly one character is swinging.
        /// </summary>
        private void Separated(out Character shooter, out Character target, float thornsPercent = 0f)
        {
            CharacterDefinition shooterSheet = battle.Sheet(
                "shooter", CharacterKind.Minion, power: 100, constitution: 500, minRange: 1, maxRange: 4);

            CharacterDefinition targetSheet = battle.Sheet(
                "target", CharacterKind.Hero, power: 100, constitution: 500, minRange: 1, maxRange: 1);

            targetSheet.Stats.BaseThornsPercent = thornsPercent;

            target = battle.Spawn(targetSheet, Team.Heroes, 3, 4);
            shooter = battle.Spawn(shooterSheet, Team.Enemies, 3, 8);
        }

        /// <summary>
        /// The case that made the event necessary. A thorny defender makes one blow produce two
        /// `Attacked`, and only one of them is somebody swinging.
        /// </summary>
        [Test]
        public void ThornsRaiseAnotherBlowButNotAnotherSwing()
        {
            Character shooter;
            Character target;
            Separated(out shooter, out target, thornsPercent: 50f);

            BattleDirector director = battle.Direct(new BattleRandom(1), target, shooter);

            int blows = 0;
            int swings = 0;

            director.Attacked += (a, t, r) => blows++;
            director.BasicAttackLanded += (a, t) => swings++;

            TestBattle.RunUntilOver(director, OneBlow);

            Assert.AreEqual(2, blows, "Expected the blow and the thorns coming back.");
            Assert.AreEqual(1, swings, "Only one character actually swung.");
        }

        /// <summary>
        /// The order of the two characters matters: a projectile is drawn from the first towards
        /// the second. Swapped, every shot would fly out of the target and into whoever fired it.
        /// </summary>
        [Test]
        public void TheSwingNamesWhoSwungFirstAndWhoWasHitSecond()
        {
            Character shooter;
            Character target;
            Separated(out shooter, out target);

            BattleDirector director = battle.Direct(new BattleRandom(1), target, shooter);

            Character swung = null;
            Character hit = null;

            director.BasicAttackLanded += (a, t) =>
            {
                if (swung == null)
                {
                    swung = a;
                    hit = t;
                }
            };

            TestBattle.RunUntilOver(director, OneBlow);

            Assert.AreSame(shooter, swung, "The first argument has to be whoever swung.");
            Assert.AreSame(target, hit, "The second argument has to be whoever was hit.");
        }

        /// <summary>
        /// The swing is announced before the damage is resolved, so whatever draws it sees the
        /// board as it was when the blow was thrown. A killing shot would otherwise be drawn out of
        /// a fight the target had already left.
        /// </summary>
        [Test]
        public void TheSwingIsAnnouncedWhileTheTargetIsStillStanding()
        {
            CharacterDefinition shooterSheet = battle.Sheet(
                "shooter", CharacterKind.Minion, power: 10000, constitution: 500, minRange: 1, maxRange: 4);

            CharacterDefinition targetSheet = battle.Sheet(
                "target", CharacterKind.Hero, power: 0, constitution: 1, minRange: 1, maxRange: 1);

            Character target = battle.Spawn(targetSheet, Team.Heroes, 3, 4);
            Character shooter = battle.Spawn(shooterSheet, Team.Enemies, 3, 8);

            BattleDirector director = battle.Direct(new BattleRandom(1), target, shooter);

            bool aliveWhenAnnounced = false;

            director.BasicAttackLanded += (a, t) => aliveWhenAnnounced = t.IsAlive;

            TestBattle.RunUntilOver(director, OneBlow);

            Assert.IsFalse(target.IsAlive, "The setup should have killed it in one blow.");
            Assert.IsTrue(aliveWhenAnnounced, "The swing was announced after the target had already died.");
        }
    }
}
