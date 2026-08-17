using System;

namespace HerOClock.Progression
{
    /// <summary>
    /// The money the player has.
    ///
    /// There is nothing to spend it on yet, and it exists this early because the reward an enemy
    /// gives is one thing, not two. Splitting money out later would mean touching the same reward
    /// code twice.
    ///
    /// Being a named type also gives the save system an obvious thing to serialise.
    /// </summary>
    public class PlayerWallet
    {
        public long Money { get; private set; }

        public event Action Changed;

        public void Add(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Money += amount;
            Changed?.Invoke();
        }

        /// <summary>
        /// Sets the money to what a save held, rather than adding to it.
        ///
        /// Separate from <see cref="Add"/> on purpose. Loading is not earning: adding would double
        /// whatever the wallet already had, and a negative value in a file that somebody edited
        /// would leave the player in debt.
        /// </summary>
        public void Restore(long amount)
        {
            Money = Math.Max(0L, amount);
            Changed?.Invoke();
        }
    }
}
