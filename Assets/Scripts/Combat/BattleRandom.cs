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
            // Xorshift gets stuck on zero, so a seed of 0 becomes an arbitrary fixed value.
            state = seed == 0 ? 2463534242u : unchecked((uint)seed);
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
