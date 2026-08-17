using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HerOClock.Persistence
{
    /// <summary>
    /// The folder of save files: writing one, finding the newest usable one, and clearing out the
    /// old ones.
    ///
    /// This is the only part of persistence that touches a disk. The format, the signature, the
    /// naming and the retention rule are all pure code elsewhere, so what is left here is genuinely
    /// about files.
    ///
    /// The folder is handed in rather than looked up, which is what lets a test point it at a
    /// throwaway directory and exercise the real reading and writing.
    /// </summary>
    public class SaveStore
    {
        private readonly string folder;

        /// <summary>
        /// Files that failed their signature during this session.
        ///
        /// They are never pruned. A save that does not match its contents is the evidence in a bug
        /// report, and deleting it would throw away the only copy of whatever went wrong.
        /// </summary>
        private readonly HashSet<string> quarantined = new HashSet<string>(StringComparer.Ordinal);

        public SaveStore(string folder)
        {
            this.folder = folder;
        }

        /// <summary>The folder the game uses, which the operating system keeps outside the install.</summary>
        public static string DefaultFolder()
        {
            return Path.Combine(Application.persistentDataPath, "Saves");
        }

        /// <summary>Save file names in the folder, newest first. Anything else is left out.</summary>
        public List<string> FileNames()
        {
            List<KeyValuePair<DateTime, string>> found = new List<KeyValuePair<DateTime, string>>();

            if (!Directory.Exists(folder))
            {
                return new List<string>();
            }

            string[] files = Directory.GetFiles(folder);

            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                DateTime utc;

                if (SaveFileName.TryParse(name, out utc))
                {
                    found.Add(new KeyValuePair<DateTime, string>(utc, name));
                }
            }

            found.Sort((a, b) =>
            {
                int byTime = b.Key.CompareTo(a.Key);
                return byTime != 0 ? byTime : string.CompareOrdinal(b.Value, a.Value);
            });

            List<string> names = new List<string>(found.Count);

            for (int i = 0; i < found.Count; i++)
            {
                names.Add(found[i].Value);
            }

            return names;
        }

        /// <summary>
        /// Writes a save and returns the name it was given.
        ///
        /// The write goes to a temporary file and is moved into place at the end. Since every save
        /// is a new file, no good save is ever overwritten, and the move is what stops a half
        /// written file from being mistaken for the newest one.
        /// </summary>
        public string Write(SavePayload payload, DateTime nowUtc)
        {
            Directory.CreateDirectory(folder);

            payload.version = SavePayload.CurrentVersion;
            payload.savedAtUtc = nowUtc.ToUniversalTime().ToString("o");

            string name = FreeName(nowUtc);
            string finalPath = Path.Combine(folder, name);
            string pendingPath = finalPath + SaveFileName.PendingExtension;

            string text = SaveEnvelope.Wrap(JsonUtility.ToJson(payload, true));

            File.WriteAllText(pendingPath, text);

            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);
            }

            File.Move(pendingPath, finalPath);

            return name;
        }

        /// <summary>
        /// The newest save that can actually be used.
        ///
        /// Files are tried from the newest down. One that is not an envelope, not valid JSON, or
        /// written by a newer format is reported and skipped, and the one before it gets a turn.
        /// A signature that does not match does **not** cause a skip: that save is used and marked.
        /// </summary>
        public SaveReadResult Load()
        {
            List<string> names = FileNames();

            for (int i = 0; i < names.Count; i++)
            {
                SaveReadResult result;

                if (TryRead(names[i], out result))
                {
                    return result;
                }
            }

            return new SaveReadResult();
        }

        private bool TryRead(string name, out SaveReadResult result)
        {
            result = new SaveReadResult();

            string path = Path.Combine(folder, name);
            string text;

            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Save '" + name + "' could not be read: " + exception.Message);
                return false;
            }

            string payloadText;
            string signature;
            string problem;

            if (!SaveEnvelope.TryUnwrap(text, out payloadText, out signature, out problem))
            {
                Debug.LogWarning("Save '" + name + "' is not a save file: " + problem);
                return false;
            }

            SavePayload payload;

            try
            {
                payload = JsonUtility.FromJson<SavePayload>(payloadText);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Save '" + name + "' holds no readable progress: " + exception.Message);
                return false;
            }

            if (!SaveMigration.TryUpgrade(payload, out problem))
            {
                Debug.LogWarning("Save '" + name + "' was skipped: " + problem);
                return false;
            }

            result.Found = true;
            result.Payload = payload;
            result.FileName = name;
            result.SignatureMatched = SaveSignature.Matches(payloadText, signature);

            if (!result.SignatureMatched)
            {
                quarantined.Add(name);
                payload.integrity = SavePayload.IntegrityBroken;

                Debug.LogWarning("Save '" + name + "' does not match its signature, so it has been"
                    + " edited or damaged. It is being loaded anyway and marked, and the file is"
                    + " left untouched.");
            }

            return true;
        }

        /// <summary>
        /// Removes the saves the retention ladder does not keep, and any leftover temporary file.
        ///
        /// Only ever called after a new save is safely on disk, so the newest file is always one
        /// that exists. Files that failed their signature are kept regardless.
        /// </summary>
        public void Prune()
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            List<string> deleting = SaveRetention.ToDelete(FileNames());

            for (int i = 0; i < deleting.Count; i++)
            {
                if (quarantined.Contains(deleting[i]))
                {
                    continue;
                }

                Delete(deleting[i]);
            }

            // A write that was interrupted leaves one of these behind. It is never a save, since
            // a finished one has already been moved into place.
            string[] files = Directory.GetFiles(folder, "*" + SaveFileName.PendingExtension);

            for (int i = 0; i < files.Length; i++)
            {
                Delete(Path.GetFileName(files[i]));
            }
        }

        private void Delete(string name)
        {
            try
            {
                File.Delete(Path.Combine(folder, name));
            }
            catch (Exception exception)
            {
                // Not being able to clear an old save is not worth interrupting anyone over. The
                // folder grows by a few kilobytes and the next attempt will try again.
                Debug.LogWarning("Save '" + name + "' could not be removed: " + exception.Message);
            }
        }

        /// <summary>
        /// A name nothing is using yet.
        ///
        /// Two saves inside the same millisecond are not expected, but a name collision would
        /// silently overwrite the file that is supposed to be the previous generation.
        /// </summary>
        private string FreeName(DateTime nowUtc)
        {
            DateTime stamp = nowUtc;

            for (int attempt = 0; attempt < 1000; attempt++)
            {
                string name = SaveFileName.For(stamp);

                if (!File.Exists(Path.Combine(folder, name)))
                {
                    return name;
                }

                stamp = stamp.AddMilliseconds(1);
            }

            return SaveFileName.For(stamp);
        }
    }
}
