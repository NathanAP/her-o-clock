using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// The list of every character sheet the game knows about.
    ///
    /// It exists because stage files are plain JSON and cannot hold a reference to an asset.
    /// They name a character by id, and the index built from this list resolves it. Save games
    /// will need exactly the same thing later, since a save cannot store an object reference.
    ///
    /// This asset holds no runtime state on purpose. A ScriptableObject stays loaded between
    /// Play sessions, and with Domain Reload disabled nothing clears its fields, so a cached
    /// index built at a bad moment would survive forever. The index is built fresh instead.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Her-o-clock/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [Tooltip("Every character sheet the game knows about.")]
        public List<CharacterDefinition> Characters = new List<CharacterDefinition>();

        /// <summary>
        /// Builds a fresh id to sheet index, reporting every sheet it had to skip.
        /// Call it once when the game starts and pass the result around.
        /// </summary>
        public Dictionary<string, CharacterDefinition> BuildIndex()
        {
            Dictionary<string, CharacterDefinition> index = new Dictionary<string, CharacterDefinition>();

            for (int i = 0; i < Characters.Count; i++)
            {
                CharacterDefinition definition = Characters[i];

                if (definition == null)
                {
                    Debug.LogWarning("CharacterDatabase: entry " + i + " is empty.", this);
                    continue;
                }

                string id = definition.Id == null ? string.Empty : definition.Id.Trim();

                if (id.Length == 0)
                {
                    Debug.LogError("CharacterDatabase: " + definition.DisplayName
                        + " has no Id, so no stage file can refer to it.", definition);
                    continue;
                }

                if (index.ContainsKey(id))
                {
                    Debug.LogError("CharacterDatabase: the id '" + id
                        + "' is used by more than one sheet. Ids have to be unique.", definition);
                    continue;
                }

                index.Add(id, definition);
            }

            return index;
        }

        /// <summary>Id and kind of every known character, for validating stage files.</summary>
        public static Dictionary<string, CharacterKind> KindsOf(IReadOnlyDictionary<string, CharacterDefinition> index)
        {
            Dictionary<string, CharacterKind> kinds = new Dictionary<string, CharacterKind>();

            foreach (KeyValuePair<string, CharacterDefinition> entry in index)
            {
                kinds.Add(entry.Key, entry.Value.Kind);
            }

            return kinds;
        }
    }
}
