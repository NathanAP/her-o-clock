using UnityEngine;

namespace HerOClock.View
{
    /// <summary>
    /// Slides the board downwards between waves, so the party reads as advancing through the
    /// city while the grid itself never moves.
    ///
    /// The grid is only a readable stand-in for positions. What the fiction says is happening
    /// is that the heroes are walking, so the ground is what moves.
    ///
    /// The board always slides by an even number of rows. Since the checkerboard repeats every
    /// two rows, snapping back to the start at the end of the slide is invisible.
    /// </summary>
    public class BoardScroller
    {
        /// <summary>
        /// Rows travelled during one transition.
        ///
        /// It has to be even: the checkerboard repeats every two rows, so an even slide is the
        /// only kind that can snap back to the start without anyone noticing.
        ///
        /// Six rows over three seconds matches the base movement speed of two cells per second,
        /// so the ground appears to move at walking pace.
        /// </summary>
        public const int RowsPerTransition = 6;

        private readonly Transform board;
        private readonly Vector3 origin;
        private readonly float distance;

        public BoardScroller(Transform board, float cellSize)
        {
            this.board = board;
            origin = board.localPosition;
            distance = RowsPerTransition * cellSize;
        }

        /// <summary>Places the board according to how far along the transition is, from 0 to 1.</summary>
        public void SetProgress(float progress)
        {
            float clamped = Mathf.Clamp01(progress);
            board.localPosition = origin + new Vector3(0f, -distance * clamped, 0f);
        }

        public void ResetPosition()
        {
            board.localPosition = origin;
        }
    }
}
