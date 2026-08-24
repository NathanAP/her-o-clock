using HerOClock.Battle;
using HerOClock.Characters;

namespace HerOClock.View
{
    /// <summary>
    /// Turns a step across the board into a direction to face.
    ///
    /// Plain arithmetic on two cells, with no Unity in sight, so the rule can be checked by a
    /// test instead of by looking at the screen. Which sprite that direction then picks is the
    /// view's problem, not this one's.
    /// </summary>
    public static class FacingResolver
    {
        /// <summary>
        /// Which way a character starts a battle turned.
        ///
        /// Heroes occupy the bottom half of the board and everyone else the top half, so a
        /// hero begins turned up and an enemy begins turned down. That way both sides are
        /// looking at each other before anybody has moved.
        /// </summary>
        public static Facing Default(Team team)
        {
            return team == Team.Heroes ? Facing.Up : Facing.Down;
        }

        /// <summary>
        /// The direction of a step from one cell to another.
        ///
        /// A step that goes nowhere keeps the current direction rather than snapping to a
        /// default: a character that stops to attack should keep looking at whoever it was
        /// walking towards.
        ///
        /// On a diagonal the row wins over the column. The board is read from the bottom up and
        /// advancing is what the movement means, so a character crossing a row while drifting
        /// one column sideways still reads as going forward.
        /// </summary>
        public static Facing FromStep(GridPosition from, GridPosition to, Facing current)
        {
            int rows = to.Row - from.Row;
            int columns = to.Column - from.Column;

            if (rows == 0 && columns == 0)
            {
                return current;
            }

            if (Abs(rows) >= Abs(columns))
            {
                return rows > 0 ? Facing.Up : Facing.Down;
            }

            return columns > 0 ? Facing.Right : Facing.Left;
        }

        private static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
    }
}
