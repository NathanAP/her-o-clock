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
        public string DisplayName = "Unnamed";
        public CharacterKind Kind = CharacterKind.Hero;

        [Tooltip("Placeholder colour. Blue for heroes, pink for minions, red for villains.")]
        public Color Color = Color.white;

        [Header("Level")]
        [Tooltip("Used by the mitigation of whoever receives this character's attacks.")]
        [Min(1)] public int Level = 1;

        [Header("Basic attack range")]
        [Tooltip("Shortest distance, in cells, the character can reach. 1 means there is no minimum range.")]
        [Min(1)] public int MinRange = 1;

        [Tooltip("Longest distance, in cells, the character can reach. Melee uses 1.")]
        [Min(1)] public int MaxRange = 1;

        [Header("Attributes")]
        public CharacterStats Stats = new CharacterStats();

        private void OnValidate()
        {
            // A minimum range above the maximum would leave the character with no valid
            // distance at all, so it could never attack anyone.
            if (MinRange > MaxRange)
            {
                MinRange = MaxRange;
            }
        }
    }
}
