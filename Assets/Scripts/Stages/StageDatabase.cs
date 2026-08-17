using System.Collections.Generic;
using UnityEngine;

namespace HerOClock.Stages
{
    /// <summary>
    /// Every stage file the game knows about, in play order.
    ///
    /// The files are referenced as TextAssets rather than loaded by name from a Resources
    /// folder, so renaming or moving a stage file never breaks the reference.
    /// </summary>
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "Her-o-clock/Stage Database")]
    public class StageDatabase : ScriptableObject
    {
        [Tooltip("The .json stage files, in the order they are meant to be played.")]
        public List<TextAsset> Stages = new List<TextAsset>();

        /// <summary>
        /// Position of the stage with the given id, or -1 when there is none.
        ///
        /// A save records the stage by id rather than by position, because inserting a stage in the
        /// middle of an act would otherwise move every player who was past it. Finding it means
        /// reading the files, which only happens once at startup.
        /// </summary>
        public int IndexOf(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int i = 0; i < Stages.Count; i++)
            {
                StageData stage = Load(i);

                if (stage != null && stage.id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Reads a stage file into memory. Returns null and logs the reason when it cannot.
        /// </summary>
        public StageData Load(int index)
        {
            if (index < 0 || index >= Stages.Count)
            {
                Debug.LogError("StageDatabase: there is no stage at position " + index + ".", this);
                return null;
            }

            TextAsset file = Stages[index];

            if (file == null)
            {
                Debug.LogError("StageDatabase: the entry at position " + index + " is empty.", this);
                return null;
            }

            try
            {
                return JsonUtility.FromJson<StageData>(file.text);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("StageDatabase: " + file.name + " is not valid JSON. " + exception.Message, file);
                return null;
            }
        }
    }
}
