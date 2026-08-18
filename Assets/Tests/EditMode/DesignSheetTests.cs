using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The character sheets in `.claude/specs/characters/`, which are design documents the game
    /// never loads.
    ///
    /// Nothing read them until now, and that is exactly why two mistakes lived in Gadrat's sheet
    /// without anybody noticing: it had `//` comments, which are not JSON at all, and its growth
    /// added up to 125 instead of 100. A document nobody parses is a document where a typo is
    /// invisible until a person happens to read that line.
    ///
    /// This is deliberately shallow. It checks the handful of things that are mechanically wrong
    /// rather than the design behind them, because a sheet is a place for opinions and only the
    /// opinions have to survive a balance pass. The deeper check — that the Unity asset agrees
    /// with the design document it mirrors — belongs to 0.9.0.0, when the two sides finally meet.
    /// </summary>
    public class DesignSheetTests
    {
        /// <summary>
        /// Field names are lowercase because JsonUtility matches them against the JSON keys, the
        /// same way the stage files are read. Keys the sheet has and this class does not are
        /// simply ignored, which is what keeps this from being a second copy of the whole sheet.
        /// </summary>
        [Serializable]
        private class DesignSheet
        {
            public string id;
            public string kind;
            public int initialLevel;
            public DesignGrowth attributeGrowth;
        }

        [Serializable]
        private class DesignGrowth
        {
            public int pow;
            public int agi;
            public int spe;
            public int con;

            public int Total
            {
                get { return pow + agi + spe + con; }
            }
        }

        private static string SheetsFolder
        {
            get
            {
                return Path.GetFullPath(
                    Path.Combine(Application.dataPath, "..", ".claude", "specs", "characters"));
            }
        }

        private static IEnumerable<string> SheetPaths()
        {
            string folder = SheetsFolder;

            if (!Directory.Exists(folder))
            {
                return new string[0];
            }

            return Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
        }

        private static DesignSheet Parse(string path)
        {
            return JsonUtility.FromJson<DesignSheet>(File.ReadAllText(path));
        }

        [Test]
        public void ThereAreDesignSheetsToCheck()
        {
            CollectionAssert.IsNotEmpty(new List<string>(SheetPaths()),
                "No design sheet was found, so every other test here would pass by saying nothing.");
        }

        /// <summary>
        /// A sheet that is not JSON cannot be read by anything, including the person writing the
        /// next one who copies it as a template.
        /// </summary>
        [Test]
        public void EveryDesignSheetIsValidJson()
        {
            foreach (string path in SheetPaths())
            {
                Assert.DoesNotThrow(() => Parse(path), Path.GetFileName(path) + " is not valid JSON.");
            }
        }

        [Test]
        public void EveryDesignSheetHasAnId()
        {
            foreach (string path in SheetPaths())
            {
                DesignSheet sheet = Parse(path);

                Assert.IsFalse(string.IsNullOrWhiteSpace(sheet.id),
                    Path.GetFileName(path) + " has no id, so nothing can refer to it.");
            }
        }

        /// <summary>
        /// The rule from "## Atributos por nível" in progress.md. The distribution divides by the
        /// declared total, so a sheet adding up to 125 keeps working and simply means something
        /// other than what whoever wrote it intended. Nothing breaks, which is the whole problem.
        /// </summary>
        [Test]
        public void EveryDesignSheetSplitsExactlyOneHundredPercent()
        {
            foreach (string path in SheetPaths())
            {
                DesignSheet sheet = Parse(path);

                Assert.IsNotNull(sheet.attributeGrowth, Path.GetFileName(path) + " declares no growth.");
                Assert.AreEqual(100, sheet.attributeGrowth.Total,
                    Path.GetFileName(path) + " splits " + sheet.attributeGrowth.Total + " points instead of 100.");
            }
        }

        /// <summary>
        /// From "### Estrutura de uma ficha" in characters.md: a hero enters the team at level 1,
        /// and a minion or villain takes its level from the stage it appears in. A starting level
        /// written on an enemy sheet is a number nothing reads.
        /// </summary>
        [Test]
        public void OnlyHeroesDeclareAStartingLevel()
        {
            foreach (string path in SheetPaths())
            {
                DesignSheet sheet = Parse(path);
                string name = Path.GetFileName(path);

                if (sheet.kind == "hero")
                {
                    Assert.AreEqual(1, sheet.initialLevel, name + " is a hero and has to start at level 1.");
                }
                else
                {
                    Assert.AreEqual(0, sheet.initialLevel,
                        name + " is a " + sheet.kind + ", and its level comes from the stage, not from the sheet.");
                }
            }
        }
    }
}
