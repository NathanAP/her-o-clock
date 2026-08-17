using System;

namespace HerOClock.Persistence
{
    /// <summary>
    /// The ten minute buckets that measure the last hour of open game.
    ///
    /// They are in the save because a player who plays in short bursts through the day would
    /// otherwise lose the reference every time the game closed, and be punished precisely for
    /// playing a little at a time. See "Como a referência da última hora é medida" in progress.md.
    /// </summary>
    [Serializable]
    public class ActivitySave
    {
        /// <summary>Oldest first, so the newest bucket is the last one.</summary>
        public ActivityBucketSave[] buckets = new ActivityBucketSave[0];
    }

    /// <summary>
    /// One bucket. It carries its own length because the newest one is always partial, and the
    /// rate is the sum divided by the time the buckets really cover, never by a fixed hour.
    /// </summary>
    [Serializable]
    public class ActivityBucketSave
    {
        public float seconds;
        public long experience;
        public long money;
        public long enemiesDefeated;
        public long damageDealt;
        public long damageTaken;
        public long healing;
    }
}
