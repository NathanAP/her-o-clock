using System.Collections.Generic;

namespace HerOClock.Characters
{
    /// <summary>
    /// What a hero's hands are holding, reduced to the numbers a fight needs.
    ///
    /// It is the offensive twin of <see cref="EquipmentTotals"/>, and it lives here for the same
    /// reason: `Items` already depends on this namespace, so the bag sits on this side and
    /// `HerOClock.Items.WeaponBuilder` is what fills it. The arrow only ever points one way.
    ///
    /// **A weapon replaces the base of three things, never the result.** The damage range, the
    /// attack speed and the reach come from here instead of from the sheet, and then POW multiplies
    /// the damage and AGI multiplies the speed exactly as they always did. Replacing the result
    /// would make AGI stop paying for anyone holding a weapon, and the light class would lose half
    /// its point.
    /// </summary>
    public class WeaponLoadout
    {
        private readonly List<Hand> hands = new List<Hand>();

        /// <summary>Attacks per second before AGI, which is the `1` a bare character swings at.</summary>
        public float AttackSpeed { get; private set; }

        public int MinRange { get; private set; }

        public int MaxRange { get; private set; }

        /// <summary>Whether the swing draws a bolt. Visual only, exactly as on a sheet.</summary>
        public AutoAttackType Attack { get; private set; }

        /// <summary>
        /// How many hands take turns swinging: one, or two when a second weapon is held.
        ///
        /// Never zero. A loadout only exists when there is a weapon in the main hand, and something
        /// with nothing in it is not a loadout at all — it is a null, and the character falls back
        /// to the punch on its sheet.
        /// </summary>
        public int HandCount
        {
            get { return hands.Count; }
        }

        /// <summary>
        /// Sets what the main hand decides for the whole character: speed, reach and how the swing
        /// is drawn. The off hand never gets a say in any of the three.
        /// </summary>
        public void UseMainHand(float attackSpeed, int minRange, int maxRange, AutoAttackType attack)
        {
            AttackSpeed = attackSpeed;
            MinRange = minRange;
            MaxRange = maxRange;
            Attack = attack;
        }

        /// <summary>Adds one hand's damage range, in the order the hands take turns.</summary>
        public void AddHand(float minDamage, float maxDamage)
        {
            hands.Add(new Hand { Min = minDamage, Max = maxDamage });
        }

        public float MinDamageOf(int hand)
        {
            return hands[Index(hand)].Min;
        }

        public float MaxDamageOf(int hand)
        {
            return hands[Index(hand)].Max;
        }

        /// <summary>
        /// Wraps rather than throwing, so a caller counting swings never has to know how many hands
        /// there are. A character that loses a weapon mid thought keeps swinging with what is left.
        /// </summary>
        private int Index(int hand)
        {
            if (hands.Count == 0)
            {
                return 0;
            }

            int wrapped = hand % hands.Count;
            return wrapped < 0 ? wrapped + hands.Count : wrapped;
        }

        private struct Hand
        {
            public float Min;
            public float Max;
        }
    }
}
