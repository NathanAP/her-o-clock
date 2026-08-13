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
    }
}
