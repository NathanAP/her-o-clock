namespace HerOClock.Combat
{
    /// <summary>
    /// The battle's random number source.
    ///
    /// It deliberately does not use UnityEngine.Random. That generator is static and global,
    /// so any other system drawing a number would change the outcome of the fight by
    /// accident. Here the sequence belongs to the battle and depends only on the seed, which
    /// keeps the promise that the same starting state always produces the same combat.
    ///
    /// The algorithm is a 32 bit xorshift, chosen because it is short and produces the same
    /// sequence on every platform. System.Random makes no such guarantee across runtime versions.
    /// </summary>
    public sealed class BattleRandom
    {
        private uint state;

        public BattleRandom(int seed)
        {
            Seed = seed;
            state = Scramble(seed);
        }

        /// <summary>
        /// Turns a seed into a starting state, spreading it across all 32 bits first.
        ///
        /// Feeding the seed straight in does not work. Xorshift diffuses slowly, so small seeds
        /// produce first draws that are almost a straight line: seeds 1 to 16 all came out below
        /// 0.0161 x seed, which meant every one of them rolled a perfect evasion on the first
        /// try. Small seeds are exactly what a developer types by hand to reproduce a fight, so
        /// the one case that had to be trustworthy was the broken one.
        ///
        /// The mixer below is the usual 32 bit avalanche: multiply, shift, xor, repeat. It is
        /// plain unchecked integer arithmetic, so the sequence stays identical on every platform,
        /// which is the reason this generator exists in the first place.
        /// </summary>
        private static uint Scramble(int seed)
        {
            unchecked
            {
                uint value = (uint)seed;

                value ^= 2747636419u;
                value *= 2654435769u;
                value ^= value >> 16;
                value *= 2654435769u;
                value ^= value >> 16;
                value *= 2654435769u;

                // Xorshift gets stuck on zero, so that one state becomes an arbitrary fixed value.
                return value == 0u ? 2463534242u : value;
            }
        }

        public int Seed { get; }

        /// <summary>Returns a value from 0 inclusive to 1 exclusive.</summary>
        public float NextFloat01()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;

            return (state & 0x00FFFFFFu) / 16777216f;
        }

        /// <summary>Rolls a chance written from 0 to 100, matching the spec's percentages.</summary>
        public bool Roll(float percentChance)
        {
            if (percentChance <= 0f)
            {
                return false;
            }

            if (percentChance >= 100f)
            {
                return true;
            }

            return NextFloat01() * 100f < percentChance;
        }
    }
}
