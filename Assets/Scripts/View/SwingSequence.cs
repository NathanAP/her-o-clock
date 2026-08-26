namespace HerOClock.View
{
    /// <summary>
    /// Which drawing of a swing belongs to a blow that started a given time ago.
    ///
    /// Unlike the running cycle this plays once and stops. A swing has a beginning and an end —
    /// wind up, strike, follow through — and looping it would turn a single blow into a windmill.
    ///
    /// A blow landing while another is still playing restarts from the first drawing rather than
    /// queueing behind it. That is what keeps the picture honest at speed: what is shown is
    /// always the most recent blow, never a backlog of old ones being worked through.
    /// </summary>
    public static class SwingSequence
    {
        /// <summary>The frame to show, or -1 once the swing is over.</summary>
        public static int FrameAt(float elapsed, float duration, int frameCount)
        {
            if (frameCount <= 0 || duration <= 0f || elapsed < 0f)
            {
                return -1;
            }

            if (elapsed >= duration)
            {
                return -1;
            }

            int frame = (int)(elapsed / duration * frameCount);

            // Guards the last instant, where floating point can land exactly on the count.
            return frame >= frameCount ? frameCount - 1 : frame;
        }
    }
}
