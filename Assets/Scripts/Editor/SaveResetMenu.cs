using System.Collections.Generic;
using System.IO;
using HerOClock.Persistence;
using UnityEditor;
using UnityEngine;

namespace HerOClock.EditorTools
{
    /// <summary>
    /// Deletes the save files, so development can start again from a clean slate.
    ///
    /// ## Why this is a development tool and not a game option
    ///
    /// The game has no menu of its own yet, and when it does, "start over" there is a player
    /// facing feature with its own wording and its own consequences. This one serves whoever is
    /// changing the numbers: a rebalance is much easier to read from level 1 with nobody carrying
    /// progress made under the old ones.
    ///
    /// It lives in the editor assembly, so **it does not exist in a build**.
    ///
    /// ## Why the dialog says how much it is about to delete
    ///
    /// Every save is a new file and the older ones are the backup, which save.md relies on to let
    /// a player go back to yesterday. So "reset" throws away a history rather than a file, and a
    /// confirmation that does not say how big that history is would be a misclick waiting to
    /// happen.
    ///
    /// ## What it refuses to touch
    ///
    /// Only files whose names <see cref="SaveFileName"/> understands. That is not caution for its
    /// own sake, it is the rule save.md states: a file whose name cannot be read is left alone,
    /// because there is no way to tell what it is.
    /// </summary>
    public static class SaveResetMenu
    {
        private const string MenuPath = "Her-o-clock/Delete saved games";

        /// <summary>
        /// Greyed out while the game is running.
        ///
        /// Deleting the files mid session does not reset anything: the roster lives in memory and
        /// the next end of wave simply writes a new file. Disabling it is clearer than letting it
        /// appear to do nothing.
        /// </summary>
        [MenuItem(MenuPath, true)]
        private static bool CanDelete()
        {
            return !Application.isPlaying;
        }

        [MenuItem(MenuPath)]
        private static void Delete()
        {
            string folder = SaveStore.DefaultFolder();
            List<string> names = new SaveStore(folder).FileNames();

            if (names.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Nothing to delete",
                    "There is no saved game in\n\n" + folder,
                    "OK");

                return;
            }

            string message =
                "This deletes " + names.Count + " saved " + (names.Count == 1 ? "game" : "games")
                + " from\n\n" + folder
                + "\n\nThe newest is " + names[0]
                + "\n\nEvery save is a separate file and the older ones are the backup, so this "
                + "throws away the whole history and cannot be undone. Files that are not saved "
                + "games are left alone.";

            if (!EditorUtility.DisplayDialog("Delete saved games?", message, "Delete", "Cancel"))
            {
                return;
            }

            int deleted = 0;

            for (int i = 0; i < names.Count; i++)
            {
                string path = Path.Combine(folder, names[i]);

                try
                {
                    File.Delete(path);
                    deleted++;
                }
                catch (IOException error)
                {
                    // Reported rather than swallowed: a file the tool could not remove is one the
                    // next run will load, and silently leaving it would look like the reset failed
                    // to take effect for no reason.
                    Debug.LogError("SaveResetMenu: could not delete " + names[i] + ". " + error.Message);
                }
            }

            Debug.Log("SaveResetMenu: deleted " + deleted + " of " + names.Count
                + " saved games from " + folder + ".");
        }
    }
}
