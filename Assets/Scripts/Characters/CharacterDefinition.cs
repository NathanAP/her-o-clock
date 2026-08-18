using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// A character sheet. While real heroes, minions and villains do not exist, it serves to
    /// build test characters without writing code.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Her-o-clock/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable text id used by stage files, save games and the strings file. "
            + "Never change it once content refers to it.")]
        public string Id = "";

        public CharacterKind Kind = CharacterKind.Hero;

        [Tooltip("Placeholder colour. Blue for heroes, pink for minions, red for villains.")]
        public Color Color = Color.white;

        [Header("Level")]
        [Tooltip("Level a hero starts at. Minions and villains ignore it, since the stage decides theirs.")]
        [Min(1)] public int Level = 1;

        [Tooltip("Highest level this character can reach.")]
        [Min(1)] public int MaxLevel = 100;

        [Tooltip("How the 5 points earned on every level are split, in percentages. Should add up to 100.")]
        public AttributeGrowth Growth = new AttributeGrowth
        {
            Power = 25, Agility = 25, Specialty = 25, Constitution = 25
        };

        [Header("Basic attack range")]
        [Tooltip("Shortest distance, in cells, the character can reach. 1 means there is no minimum range.")]
        [Min(1)] public int MinRange = 1;

        [Tooltip("Longest distance, in cells, the character can reach. Melee uses 1.")]
        [Min(1)] public int MaxRange = 1;

        [Tooltip("Whether the basic attack draws a projectile flying to the target. Purely visual: "
            + "the two ranges above are what decide reach.")]
        public AutoAttackType AutoAttack = AutoAttackType.Melee;

        [Header("Attributes")]
        public CharacterStats Stats = new CharacterStats();

        [Header("Abilities")]
        [Tooltip("Used automatically, in this order, whenever one is off cooldown and has a valid target.")]
        public List<Abilities.AbilityDefinition> Abilities = new List<Abilities.AbilityDefinition>();

        private void OnValidate()
        {
            // A minimum range above the maximum would leave the character with no valid
            // distance at all, so it could never attack anyone.
            if (MinRange > MaxRange)
            {
                MinRange = MaxRange;
            }

            if (Level > MaxLevel)
            {
                Level = MaxLevel;
            }

            // The shares are normalised when the points are handed out, so a total other than
            // 100 still works. It is almost always a typo though, and worth saying out loud.
            if (Growth.Total != 100)
            {
                Debug.LogWarning(Id + ": the attribute growth adds up to " + Growth.Total
                    + " instead of 100. It still works, but the percentages will not read as written.", this);
            }
        }
    }
}
