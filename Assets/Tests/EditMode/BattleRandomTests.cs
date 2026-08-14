using HerOClock.Combat;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the battle's own random source.
    ///
    /// This generator exists so a fight can be replayed from its seed, so the properties being
    /// checked here are not statistical niceties: they are the promise the developer relies on
    /// when investigating something odd.
    /// </summary>
    public class BattleRandomTests
    {
        [Test]
        public void TheSameSeedProducesTheSameSequence()
        {
            BattleRandom first = new BattleRandom(20260813);
            BattleRandom second = new BattleRandom(20260813);

            for (int i = 0; i < 500; i++)
            {
                Assert.AreEqual(first.NextFloat01(), second.NextFloat01(), "Draw " + i + " diverged.");
            }
        }

        [Test]
        public void DifferentSeedsProduceDifferentSequences()
        {
            BattleRandom first = new BattleRandom(1);
            BattleRandom second = new BattleRandom(2);

            Assert.AreNotEqual(first.NextFloat01(), second.NextFloat01());
        }

        [Test]
        public void EveryDrawStaysBetweenZeroAndOne()
        {
            BattleRandom random = new BattleRandom(7);

            for (int i = 0; i < 100000; i++)
            {
                float value = random.NextFloat01();

                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void TheSequenceIsEvenlySpread()
        {
            BattleRandom random = new BattleRandom(99);
            double total = 0;

            const int draws = 200000;
            for (int i = 0; i < draws; i++)
            {
                total += random.NextFloat01();
            }

            Assert.AreEqual(0.5, total / draws, 0.01);
        }

        // --- Roll ---

        [Test]
        public void RollNeverSucceedsAtZeroPercent()
        {
            BattleRandom random = new BattleRandom(3);

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsFalse(random.Roll(0f));
            }
        }

        [Test]
        public void RollAlwaysSucceedsAtOneHundredPercent()
        {
            BattleRandom random = new BattleRandom(3);

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(random.Roll(100f));
            }
        }

        [Test]
        public void RollLandsCloseToTheRequestedChance()
        {
            BattleRandom random = new BattleRandom(4242);
            int hits = 0;

            const int rolls = 200000;
            for (int i = 0; i < rolls; i++)
            {
                if (random.Roll(25f))
                {
                    hits++;
                }
            }

            Assert.AreEqual(25.0, hits * 100.0 / rolls, 0.5);
        }

        // --- Seeds a developer actually types ---

        /// <summary>
        /// The seed field exists so a developer can reproduce a fight, and the seeds people type
        /// by hand are 1, 2, 3. Before the scrambling was added, xorshift diffused so slowly that
        /// the first draw of those seeds was almost exactly 0.0161 x seed: seeds 1 through 16 all
        /// came out below 0.30, so every hand typed seed rolled a perfect evasion on its first
        /// try. Anyone investigating evasion with a small seed would have been badly misled.
        /// </summary>
        [Test]
        public void SmallSeedsDoNotAllStartLow()
        {
            int low = 0;

            for (int seed = 1; seed <= 16; seed++)
            {
                if (new BattleRandom(seed).NextFloat01() < 0.30f)
                {
                    low++;
                }
            }

            Assert.Less(low, 16, "Every small seed started below 0.30, which is the old correlation bug.");
        }

        [Test]
        public void TheFirstDrawIsNotOrderedByTheSeed()
        {
            float previous = new BattleRandom(1).NextFloat01();
            bool alwaysIncreasing = true;

            for (int seed = 2; seed <= 16; seed++)
            {
                float current = new BattleRandom(seed).NextFloat01();

                if (current <= previous)
                {
                    alwaysIncreasing = false;
                }

                previous = current;
            }

            Assert.IsFalse(alwaysIncreasing, "The first draw rose with every seed, so the seed was barely mixed at all.");
        }

        [Test]
        public void ASeedOfZeroStillProducesAUsableSequence()
        {
            BattleRandom random = new BattleRandom(0);
            double total = 0;

            for (int i = 0; i < 1000; i++)
            {
                total += random.NextFloat01();
            }

            Assert.Greater(total, 0.0, "A stuck generator would return zero forever.");
        }

        [Test]
        public void NegativeSeedsWork()
        {
            BattleRandom random = new BattleRandom(-12345);
            double total = 0;

            for (int i = 0; i < 1000; i++)
            {
                total += random.NextFloat01();
            }

            Assert.AreEqual(0.5, total / 1000.0, 0.05);
        }
    }
}
