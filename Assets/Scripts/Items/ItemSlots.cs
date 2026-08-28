using System;

namespace HerOClock.Items
{
    /// <summary>
    /// `slots.json`: the eight equipment slots, the defence budget they share out, what each class
    /// does with that budget, and the attribute a piece demands to stay active.
    ///
    /// Field names are lowercase because JsonUtility matches them against the JSON keys character
    /// for character. These classes are a wire format, not normal C# types — the same rule
    /// <see cref="Stages.StageData"/> follows.
    /// </summary>
    [Serializable]
    public class ItemSlots
    {
        public ItemSlot[] slots;
        public DefenceBudget defenceBudget;
        public ClassDefence[] classDefence;
        public SlotRequirement requirement;
        public ArmourNaming armourNaming;
    }

    [Serializable]
    public class ItemSlot
    {
        public string id;

        /// <summary>`defensive`, `weapon`, `weaponOrDefensive` or `utility`.</summary>
        public string kind;

        /// <summary>Share of the defence budget this slot carries. Zero for the utility slots.</summary>
        public float defenceWeight;

        /// <summary>The word this slot puts into an item's name, shorter than the slot's own name.</summary>
        public string namingNoun;

        /// <summary>
        /// The two things the off hand can be, and the only slot that has any.
        ///
        /// A modifier that may only fall on a weapon, or only on defensive gear, has to tell the
        /// two apart, and these are the ids `modifiers.json` uses to do it.
        /// </summary>
        public string[] roles;
    }

    [Serializable]
    public class DefenceBudget
    {
        public ScaledValue armour;
        public ScaledValue evasion;
        public ScaledValue elementalResistance;
    }

    /// <summary>What one equipment class turns the defence budget into. Hybrids take from two.</summary>
    [Serializable]
    public class ClassDefence
    {
        public string id;
        public float armour;
        public float evasion;
        public float elementalResistance;
    }

    [Serializable]
    public class SlotRequirement
    {
        /// <summary>Multiplied by the item's level and rounded up.</summary>
        public float perItemLevel;

        /// <summary>What each side of a hybrid demands, as a share of the full value.</summary>
        public float hybridShare;

        public ClassAttributes[] attributeByClass;
    }

    [Serializable]
    public class ClassAttributes
    {
        public string id;
        public string[] attributes;
    }

    [Serializable]
    public class ArmourNaming
    {
        public ClassNouns[] classNouns;
    }

    [Serializable]
    public class ClassNouns
    {
        public string id;
        public LevelledNoun[] nouns;
    }

    [Serializable]
    public class LevelledNoun
    {
        public int minItemLevel;
        public string noun;
    }
}
