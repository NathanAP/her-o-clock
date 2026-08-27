namespace HerOClock.Characters
{
    /// <summary>
    /// Where a character's level points sit at one moment: what the player placed by hand, how
    /// many the sheet's distribution spent, what is still waiting to be placed, and which of the
    /// two is doing the placing.
    ///
    /// It is its own type because a character carries **two** of these at once and they mean
    /// different things — one is what the player has been editing, the other is what the stats
    /// are allowed to read. See <see cref="AttributeAllocation"/> for why.
    ///
    /// It holds no rule of its own. Every rule about what may move and when lives one level up,
    /// which is what keeps this a thing that can be copied wholesale without thinking.
    /// </summary>
    public class AttributeSplit
    {
        /// <summary>Points the player placed by hand, in attribute order.</summary>
        public readonly int[] Manual = new int[4];

        /// <summary>
        /// How many points the sheet's distribution has spent.
        ///
        /// A count and never the split it produces. The split is resolved by largest remainder,
        /// so handing out five points thirty-nine times does not give what handing out 195 once
        /// gives, and only the count survives being written down and read back.
        /// </summary>
        public int AutomaticTotal;

        /// <summary>Points earned and not yet placed. Always zero while on automatic.</summary>
        public int Unspent;

        /// <summary>While true, points are spent by the sheet's distribution the moment they arrive.</summary>
        public bool IsAutomatic = true;

        /// <summary>Everything this character has ever been granted, placed or not.</summary>
        public int Granted
        {
            get { return TotalManual + AutomaticTotal + Unspent; }
        }

        public int TotalManual
        {
            get { return Manual[0] + Manual[1] + Manual[2] + Manual[3]; }
        }

        /// <summary>Becomes an exact copy of another split, sharing nothing with it.</summary>
        public void CopyFrom(AttributeSplit other)
        {
            if (other == null)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                Manual[i] = other.Manual[i];
            }

            AutomaticTotal = other.AutomaticTotal;
            Unspent = other.Unspent;
            IsAutomatic = other.IsAutomatic;
        }
    }
}
