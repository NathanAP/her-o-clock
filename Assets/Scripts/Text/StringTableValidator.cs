using System.Collections.Generic;

namespace HerOClock.Text
{
    /// <summary>
    /// Checks that a strings table holds everything the content will ask of it.
    ///
    /// This is the same bargain the StageValidator pays for: keys are text, so a typo is invisible
    /// until the moment something tries to read it. Worse than a stage typo, in fact, because a
    /// missing string does not empty the battlefield — the game runs perfectly and just shows the
    /// wrong words, which is the sort of thing that ships.
    ///
    /// It also reports keys nobody asks for. They are not errors, but a strings file that keeps
    /// entries for content that no longer exists slowly becomes impossible to translate, because
    /// nobody can tell which half still matters.
    ///
    /// No Unity here either, so the whole thing is checked outside the editor.
    /// </summary>
    public static class StringTableValidator
    {
        public static void Validate(
            StringTable strings,
            IEnumerable<string> characterIds,
            IEnumerable<string> stageIds,
            List<string> problems)
        {
            HashSet<string> expected = new HashSet<string>();

            foreach (string id in characterIds)
            {
                Require(strings, StringTable.CharacterName(id), expected, problems);
            }

            foreach (string id in stageIds)
            {
                Require(strings, StringTable.StageName(id), expected, problems);
                Require(strings, StringTable.StageLore(id), expected, problems);
            }

            foreach (string key in strings.Keys)
            {
                if (!expected.Contains(key))
                {
                    problems.Add("The key '" + key + "' is in the file but nothing asks for it.");
                }
            }
        }

        private static void Require(StringTable strings, string key, HashSet<string> expected, List<string> problems)
        {
            expected.Add(key);

            if (!strings.Has(key))
            {
                problems.Add("The key '" + key + "' is missing.");
            }
        }
    }
}
