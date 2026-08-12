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
