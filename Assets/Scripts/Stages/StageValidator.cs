using System.Collections.Generic;
using HerOClock.Characters;

namespace HerOClock.Stages
{
    /// <summary>
    /// Checks a stage file before the game tries to use it.
    ///
    /// This is the price of holding content in JSON: a reference is a piece of text, so a typo
    /// would otherwise only show up at runtime as an empty battlefield. The validator turns
    /// every such mistake into a clear message, and reports all of them at once rather than
    /// stopping at the first.
    ///
    /// It knows nothing about Unity on purpose, so it can be tested outside the editor.
    /// </summary>
    public static class StageValidator
    {
        public static List<string> Validate(
            StageData stage,
            int columns,
            int rows,
            IReadOnlyDictionary<string, CharacterKind> knownCharacters)
        {
            List<string> problems = new List<string>();

            if (stage == null)
            {
                problems.Add("The stage file could not be read at all.");
                return problems;
            }

            if (string.IsNullOrWhiteSpace(stage.id))
            {
                problems.Add("The stage has no id.");
            }

            if (stage.enemyLevel < 1)
            {
                problems.Add("enemyLevel is " + stage.enemyLevel + ", but the lowest level is 1.");
            }

            if (stage.waves == null || stage.waves.Length == 0)
            {
                problems.Add("The stage has no minion waves.");
            }
            else
            {
                for (int i = 0; i < stage.waves.Length; i++)
                {
                    ValidateWave(stage.waves[i], "wave " + (i + 1), columns, rows, knownCharacters, false, problems);
                }
            }

            if (stage.villainWave == null)
            {
                problems.Add("The stage has no villain wave, and every stage ends against a villain.");
            }
            else
            {
                ValidateWave(stage.villainWave, "the villain wave", columns, rows, knownCharacters, true, problems);
            }

            return problems;
        }

        private static void ValidateWave(
            StageWave wave,
            string label,
            int columns,
            int rows,
            IReadOnlyDictionary<string, CharacterKind> knownCharacters,
            bool requiresVillain,
            List<string> problems)
        {
            if (wave == null || wave.placements == null || wave.placements.Length == 0)
            {
                problems.Add(Capitalise(label) + " has nobody in it.");
                return;
            }

            HashSet<long> usedCells = new HashSet<long>();
            bool hasVillain = false;

            for (int i = 0; i < wave.placements.Length; i++)
            {
                StagePlacement placement = wave.placements[i];
                string where = Capitalise(label) + ", entry " + (i + 1);

                if (string.IsNullOrWhiteSpace(placement.character))
                {
                    problems.Add(where + " has no character id.");
                    continue;
                }

                CharacterKind kind;
                if (knownCharacters == null || !knownCharacters.TryGetValue(placement.character, out kind))
                {
                    problems.Add(where + " refers to '" + placement.character
                        + "', which is not in the character database.");
                    continue;
                }

                if (kind == CharacterKind.Hero)
                {
                    problems.Add(where + " places '" + placement.character
                        + "', which is a hero. Waves are made of minions and villains.");
                }

                if (kind == CharacterKind.Villain)
                {
                    hasVillain = true;
                }

                if (placement.column < 1 || placement.column > columns
                    || placement.row < 1 || placement.row > rows)
                {
                    problems.Add(where + " sits on column " + placement.column + ", row " + placement.row
                        + ", which is outside a board of " + columns + " by " + rows + ".");
                    continue;
                }

                long cell = (long)placement.column * 10000 + placement.row;
                if (!usedCells.Add(cell))
                {
                    problems.Add(where + " wants column " + placement.column + ", row " + placement.row
                        + ", which another entry of the same wave already took.");
                }
            }

            if (requiresVillain && !hasVillain)
            {
                problems.Add(Capitalise(label) + " has no villain in it.");
            }
        }

        private static string Capitalise(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
    }
}
