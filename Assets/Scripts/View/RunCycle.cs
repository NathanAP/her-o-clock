namespace HerOClock.View
{
    /// <summary>
    /// Which drawing of a running cycle belongs to a character that has walked a given distance.
    ///
    /// Driven by ground covered and never by elapsed time, which is the whole point. A cycle on a
    /// timer runs at its own pace while the character moves at whatever its agility says, and the
    /// two drift apart until the feet are sliding over the floor. Tied to distance, a foot lands
    /// at the same point of the journey no matter how fast the journey is.
    ///
    /// It also means the cycle does not care about the speed the game is being run at. Position is
    /// position.
    /// </summary>
    public static class RunCycle
    {
        /// <summary>
        /// The frame for a distance, in cells, given how much ground one full cycle covers.
        ///
        /// Walking backwards is not a thing here: distance only ever grows, so the index only ever
        /// moves forward and wraps.
        /// </summary>
        public static int FrameAt(float cellsTravelled, float cellsPerCycle, int frameCount)
        {
            if (frameCount <= 0)
            {
                return 0;
            }

            if (cellsPerCycle <= 0f || cellsTravelled <= 0f)
            {
                return 0;
            }

            float turns = cellsTravelled / cellsPerCycle;
            int frame = (int)(turns * frameCount) % frameCount;

            return frame < 0 ? frame + frameCount : frame;
        }
    }
}
