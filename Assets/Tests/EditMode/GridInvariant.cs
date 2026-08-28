using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// What has to be true about the board at every single simulation step.
    ///
    /// The board keeps occupancy in two places at once: each <see cref="Character"/> holds its own
    /// `Position`, and <see cref="BattleGrid"/> holds a table of who is in which cell. Nothing
    /// rebuilds one from the other, so **the moment they disagree, they disagree forever** — and
    /// the symptom is `IsFree` lying, which lets two characters be drawn on top of each other.
    ///
    /// This is a diagnostic and not a rule of the game, which is why it lives with the tests. It
    /// exists to find the step where the disagreement starts, rather than have somebody guess
    /// which of several plausible causes is the real one.
    /// </summary>
    public static class GridInvariant
    {
        /// <summary>
        /// Returns null when everything agrees, or the first disagreement found.
        ///
        /// Characters that are not on the board are skipped: a fallen minion released its cell
        /// when it died, so its stale `Position` means nothing. A fallen **hero** is not skipped,
        /// because it stays lying on its cell and keeps holding it so it can be revived in place.
        /// </summary>
        public static string Check(
            BattleGrid grid, BattleGridConfig config, IReadOnlyList<Character> characters)
        {
            if (grid == null || config == null || characters == null)
            {
                return null;
            }

            // The one a player actually sees: two bodies in the same square.
            for (int i = 0; i < characters.Count; i++)
            {
                if (!IsOnBoard(characters[i]))
                {
                    continue;
                }

                for (int j = i + 1; j < characters.Count; j++)
                {
                    if (!IsOnBoard(characters[j]))
                    {
                        continue;
                    }

                    if (characters[i].Position.Equals(characters[j].Position))
                    {
                        return Name(characters[i]) + " and " + Name(characters[j])
                            + " are both standing on " + characters[i].Position + ".";
                    }
                }
            }

            // Every character is the occupant of its own cell.
            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                if (!IsOnBoard(character))
                {
                    continue;
                }

                IGridOccupant occupant = grid.OccupantAt(character.Position);

                if (ReferenceEquals(occupant, character))
                {
                    continue;
                }

                return Name(character) + " thinks it is on " + character.Position
                    + ", but the grid says that cell holds " + Describe(occupant) + ".";
            }

            // And nothing occupies a cell it is not standing on.
            for (int column = 1; column <= config.Columns; column++)
            {
                for (int row = 1; row <= config.Rows; row++)
                {
                    GridPosition cell = new GridPosition(column, row);
                    IGridOccupant occupant = grid.OccupantAt(cell);

                    if (occupant == null)
                    {
                        continue;
                    }

                    Character character = occupant as Character;

                    if (character == null)
                    {
                        return "The grid holds something that is not a character on " + cell + ".";
                    }

                    if (!character.Position.Equals(cell))
                    {
                        return "The grid says " + Name(character) + " holds " + cell
                            + ", but that character thinks it is on " + character.Position + ".";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The body has to be within one cell of the cell it logically occupies.
        ///
        /// While a character walks, its `Position` is already the **destination** — the cell is
        /// claimed the moment the step starts — and the body is drawn interpolating towards it.
        /// So the gap between the two shrinks from one cell to zero and never exceeds one.
        ///
        /// A larger gap means the body is travelling towards a cell that is no longer the
        /// character's, which is the state a blink leaves behind if it relocates somebody in the
        /// middle of a step: the mover keeps dragging the body to the old destination, and the
        /// cell it released can be taken by somebody else in the meantime.
        ///
        /// **This is the check the logical one cannot make.** The grid and the positions can agree
        /// perfectly while two bodies sit in the same square on screen.
        /// </summary>
        public static string CheckBodies(
            BattleGrid grid, IReadOnlyList<Character> characters, float cellSize)
        {
            if (grid == null || characters == null || cellSize <= 0f)
            {
                return null;
            }

            // A diagonal step is one cell by the board's own reckoning and sqrt(2) cells across
            // the floor, so the bound has to allow for it.
            float allowed = cellSize * 1.45f;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                if (!IsOnBoard(character))
                {
                    continue;
                }

                Vector3 where = grid.WorldPositionOf(character.Position);
                float gap = Vector3.Distance(character.transform.position, where);

                if (gap > allowed)
                {
                    return Name(character) + " is drawn " + (gap / cellSize).ToString("F2")
                        + " cells away from " + character.Position + ", the cell it holds.";
                }
            }

            return null;
        }

        /// <summary>
        /// A character the board is still accounting for.
        ///
        /// A destroyed object compares equal to null through Unity's overloaded operator, which is
        /// why this is not a plain null check.
        /// </summary>
        private static bool IsOnBoard(Character character)
        {
            return character != null && (character.IsAlive || character.Kind == CharacterKind.Hero);
        }

        private static string Name(Character character)
        {
            return character.Definition != null ? character.Definition.Id : "a character";
        }

        private static string Describe(IGridOccupant occupant)
        {
            if (occupant == null)
            {
                return "nobody";
            }

            Character character = occupant as Character;
            return character != null ? Name(character) : "something that is not a character";
        }
    }
}
