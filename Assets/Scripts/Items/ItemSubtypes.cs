using System;

namespace HerOClock.Items
{
    /// <summary>
    /// `subtypes.json`: the weapon subtypes and the three defensive off hands.
    ///
    /// Armour does not appear here. A casing's subtype is only a noun and comes from its class,
    /// because the class already occupies the mechanical axis: a second one would make the two
    /// fight over the same space and turn one of them into decoration.
    /// </summary>
    [Serializable]
    public class ItemSubtypes
    {
        public DamageBudget damageBudget;
        public WeaponSubtype[] weapons;
        public OffHandDefensive[] offHandDefensive;
    }

    /// <summary>
    /// The damage per second each weapon family is allowed, and the discounts some of them pay.
    ///
    /// A subtype's damage range is **derived** from this rather than chosen loose: average damage
    /// times attack speed equals the family's budget. That is what lets a Piledriver and a Claw
    /// both be choices instead of one being strictly better.
    ///
    /// The game does not read this. It exists so a test can check the derivation.
    /// </summary>
    [Serializable]
    public class DamageBudget
    {
        public FamilyBudget[] families;

        /// <summary>The discount Catalyst and Reactor pay for belonging to ability builds.</summary>
        public float specialMultiplier;

        /// <summary>The discount a melee family weapon pays for reaching more than one cell.</summary>
        public float reachMultiplier;

        /// <summary>How far a subtype may sit from its budget before the test complains.</summary>
        public float tolerance;
    }

    [Serializable]
    public class FamilyBudget
    {
        public string id;
        public float @base;
        public float perLevel;

        public float At(int level)
        {
            int steps = level < 1 ? 0 : level - 1;
            return @base + perLevel * steps;
        }
    }

    [Serializable]
    public class WeaponSubtype
    {
        public string id;
        public string family;
        public int hands;
        public int minRange;
        public int maxRange;
        public float attackSpeed;
        public ScaledRange damage;

        /// <summary>A tendency and never a lock: a light Cannon is possible and should be.</summary>
        public string naturalClass;

        public LevelledBase[] bases;
    }

    [Serializable]
    public class OffHandDefensive
    {
        public string id;

        /// <summary>`armour`, `evasion` or `elementalResistance`.</summary>
        public string grants;

        public string naturalClass;
        public LevelledBase[] bases;
    }

    /// <summary>One of the three or four name variants a subtype unlocks as item level rises.</summary>
    [Serializable]
    public class LevelledBase
    {
        public int minItemLevel;
        public string id;
    }
}
