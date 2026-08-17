namespace HerOClock.Progression
{
    /// <summary>
    /// What was earned in one slice of open game.
    ///
    /// It carries its own length because the newest bucket is always partial. The rate is the sum
    /// of the buckets divided by the time they really cover, never by a fixed hour, so that a short
    /// session is not read as a weak hour.
    ///
    /// Everything the offline progression and the statistics need lives here together, and on
    /// purpose: progress.md requires the statistics shown on return to come from the same
    /// multiplication as the experience and the money. Two sources would eventually disagree, and
    /// a screen claiming 1,432 enemies next to the experience of 800 is a bug the player can see.
    /// </summary>
    public class ActivityBucket
    {
        public float Seconds;
        public long Experience;
        public long Money;
        public long EnemiesDefeated;
        public long DamageDealt;
        public long DamageTaken;
        public long Healing;
    }
}
