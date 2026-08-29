using System;
using System.Collections.Generic;

namespace HerOClock.Text
{
    /// <summary>
    /// Every piece of text the player reads, looked up by key.
    ///
    /// Nothing the player sees lives in an asset any more. A display name inside a
    /// ScriptableObject sits in the same file as the balance numbers, so a translation pass and a
    /// balance pass end up fighting over the same diff, and adding a second language would mean
    /// duplicating every sheet rather than adding one file.
    ///
    /// The keys are built from the id the asset already carries, so there is nothing to keep in
    /// sync: a character with the id <c>gadrat</c> reads <c>character.gadrat.name</c>, and
    /// renaming an id is the same act as renaming its text.
    ///
    /// It knows nothing about Unity, so the whole thing can be checked outside the editor.
    /// </summary>
    public class StringTable
    {
        private readonly Dictionary<string, string> entries;

        private StringTable(Dictionary<string, string> entries)
        {
            this.entries = entries;
        }

        public string Language { get; private set; }

        public int Count
        {
            get { return entries.Count; }
        }

        public IEnumerable<string> Keys
        {
            get { return entries.Keys; }
        }

        // --- Key shapes ---

        public static string CharacterName(string id)
        {
            return "character." + id + ".name";
        }

        public static string StageName(string id)
        {
            return "stage." + id + ".name";
        }

        public static string StageLore(string id)
        {
            return "stage." + id + ".lore";
        }

        // --- Reading ---

        /// <summary>
        /// The text for a key.
        ///
        /// A missing key comes back as the key itself between hashes rather than as an empty
        /// string. Empty text is invisible, and a label that silently disappears is the kind of
        /// problem that ships. <c>#stage.act1Stage1.name#</c> on screen is impossible to miss.
        /// </summary>
        public string Get(string key)
        {
            string value;
            return entries.TryGetValue(key, out value) ? value : "#" + key + "#";
        }

        public bool Has(string key)
        {
            return entries.ContainsKey(key);
        }

        // --- Building ---

        /// <summary>
        /// Builds a table from the contents of a strings file, reporting every problem it found
        /// rather than stopping at the first, the same way stage files are handled.
        /// </summary>
        public static StringTable From(StringTableData data, List<string> problems)
        {
            Dictionary<string, string> entries = new Dictionary<string, string>(StringComparer.Ordinal);

            if (data == null)
            {
                problems.Add("The strings file could not be read at all.");
                return new StringTable(entries);
            }

            if (string.IsNullOrWhiteSpace(data.language))
            {
                problems.Add("The strings file does not say which language it is.");
            }

            if (data.entries == null || data.entries.Length == 0)
            {
                problems.Add("The strings file has no entries.");
                return new StringTable(entries) { Language = data.language };
            }

            for (int i = 0; i < data.entries.Length; i++)
            {
                StringEntry entry = data.entries[i];

                if (string.IsNullOrWhiteSpace(entry.key))
                {
                    problems.Add("Entry " + (i + 1) + " has no key.");
                    continue;
                }

                if (string.IsNullOrEmpty(entry.value))
                {
                    problems.Add("The key '" + entry.key + "' has no text.");
                    continue;
                }

                if (entries.ContainsKey(entry.key))
                {
                    problems.Add("The key '" + entry.key + "' appears more than once.");
                    continue;
                }

                entries.Add(entry.key, entry.value);
            }

            return new StringTable(entries) { Language = data.language };
        }
    }
}
