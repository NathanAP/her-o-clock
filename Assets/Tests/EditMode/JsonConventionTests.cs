using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Every JSON file in the project writes its keys, its ids and its internal values in
    /// `camelCase`.
    ///
    /// ## Why this is a test and not a note in a document
    ///
    /// The rule already existed for the item files, written down in `items.md` in 0.11.0.0, and the
    /// character sheets broke it anyway: `deal_damage` and `modify_stat` sat in `snake_case` right
    /// next to `eachTarget` and `lastTargetAnySide`, in the same file. A convention nobody checks
    /// is a convention the next file quietly ignores.
    ///
    /// It is also not cosmetic. `JsonUtility` matches a key to a C# field letter by letter, so a
    /// key that is not written the way the field is cannot be read at all — which is exactly the
    /// wall 0.11.0.0 hit and paid for.
    ///
    /// ## What counts as an internal value
    ///
    /// A value that the game compares against something: an id, the name of an enum, the slot a
    /// modifier may fall on. Not text a player reads, and not the prose the design sheets carry —
    /// the `info` block of a character is documentation, so its words are left alone.
    /// </summary>
    public class JsonConventionTests
    {
        /// <summary>Lowercase first letter, then letters and digits. Nothing else.</summary>
        private static readonly Regex Camel = new Regex(@"^[a-z][A-Za-z0-9]*$");

        /// <summary>
        /// The keys whose value is an identifier rather than something to read.
        ///
        /// A whitelist and not a guess: a value is only checked when we know what it means. The
        /// alternative would be checking every string in the project, which would fail on every
        /// name, every description and every comment.
        /// </summary>
        private static readonly HashSet<string> IdentifierKeys = new HashSet<string>
        {
            "id", "key", "character", "unlocksCharacter",
            "kind", "type", "equipment", "class", "itemClass",
            "who", "shape", "priority", "anchor", "target",
            "stat", "mode", "damageType", "status",
            "family", "naturalClass", "grants", "slot", "slots", "subtype", "technology"
        };

        private static readonly Regex KeyPattern = new Regex("\"([^\"]+)\"\\s*:");
        private static readonly Regex PairPattern = new Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"");

        /// <summary>
        /// Every JSON the project owns: the data the game loads and the design sheets beside it.
        ///
        /// Packages and imported assets are somebody else's files and follow somebody else's rules.
        /// </summary>
        private static List<string> Files()
        {
            List<string> files = new List<string>();

            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            Collect(Path.Combine(root, "Assets"), files);
            Collect(Path.Combine(root, ".claude", "specs"), files);

            return files;
        }

        private static void Collect(string folder, List<string> into)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            string[] found = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);

            for (int i = 0; i < found.Length; i++)
            {
                if (!found[i].Replace('\\', '/').Contains("/TextMesh Pro/"))
                {
                    into.Add(found[i]);
                }
            }
        }

        [Test]
        public void EveryJsonFileWritesItsKeysInCamelCase()
        {
            List<string> problems = new List<string>();

            foreach (string path in Files())
            {
                string text = File.ReadAllText(path);

                foreach (Match match in KeyPattern.Matches(text))
                {
                    string key = match.Groups[1].Value;

                    if (!Camel.IsMatch(key))
                    {
                        problems.Add(Name(path) + ": the key \"" + key + "\" is not camelCase.");
                    }
                }
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        /// <summary>
        /// The same rule for the values that name something.
        ///
        /// A string table key is a path of segments joined by dots, and each segment is checked on
        /// its own: `stage.act1Stage1.name` is three names, not one.
        /// </summary>
        [Test]
        public void EveryIdentifierValueIsWrittenInCamelCase()
        {
            List<string> problems = new List<string>();

            foreach (string path in Files())
            {
                string text = WithoutProse(File.ReadAllText(path));

                foreach (Match match in PairPattern.Matches(text))
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;

                    if (!IdentifierKeys.Contains(key) || value.Length == 0)
                    {
                        continue;
                    }

                    string[] segments = value.Split('.');

                    for (int i = 0; i < segments.Length; i++)
                    {
                        if (!Camel.IsMatch(segments[i]))
                        {
                            problems.Add(Name(path) + ": \"" + key + "\" is \"" + value
                                + "\", which is not camelCase.");
                            break;
                        }
                    }
                }
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        /// <summary>
        /// A file with nothing in it would pass both tests above and mean nothing, which is the
        /// failure this project has already been bitten by once.
        /// </summary>
        [Test]
        public void ThereAreFilesToCheck()
        {
            Assert.Greater(Files().Count, 10, "Almost no JSON was found, so the convention is checking nothing.");
        }

        /// <summary>
        /// Blanks out the `info` block of a design sheet, which is prose: a character's name, the
        /// noun describing what it is, its lore and the colours of its costume. None of that is an
        /// identifier, and `"type": "Robot"` sitting inside it is a word rather than an enum.
        ///
        /// Blanked rather than removed, so everything after it stays where it was.
        /// </summary>
        private static string WithoutProse(string text)
        {
            int start = text.IndexOf("\"info\"");

            if (start < 0)
            {
                return text;
            }

            int open = text.IndexOf('{', start);

            if (open < 0)
            {
                return text;
            }

            int depth = 0;
            char[] blanked = text.ToCharArray();

            for (int i = open; i < blanked.Length; i++)
            {
                if (blanked[i] == '{')
                {
                    depth++;
                }
                else if (blanked[i] == '}')
                {
                    depth--;
                }

                bool inside = blanked[i] != '\n';

                if (inside)
                {
                    blanked[i] = ' ';
                }

                if (depth == 0)
                {
                    break;
                }
            }

            return new string(blanked);
        }

        private static string Name(string path)
        {
            return Path.GetFileName(path);
        }
    }
}
