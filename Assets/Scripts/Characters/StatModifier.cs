using HerOClock.Abilities;

namespace HerOClock.Characters
{
    /// <summary>
    /// One buff or debuff currently on a character.
    ///
    /// Buff and debuff are the same thing here, separated only by the sign of the value, exactly
    /// as buffs-and-debuffs.md describes.
    ///
    /// The source id is what makes "the same buff arriving twice" answerable: two applications of
    /// the same ability share it, while two different abilities touching the same stat do not.
    /// </summary>
    public class StatModifier
    {
        /// <summary>Id of the ability that granted it. Two of these are "the same buff".</summary>
        public string SourceId;

        public ModifiableStat Stat;
        public StatModifierMode Mode;
        public float Value;

        /// <summary>Seconds left. Counted down by the character on every simulation step.</summary>
        public float Remaining;
    }
}
