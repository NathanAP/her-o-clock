namespace HerOClock.Progression
{
    /// <summary>
    /// What an absence was worth.
    ///
    /// Every number here comes from the same <see cref="EffectiveHours"/> multiplied by the same
    /// buckets, which is what progress.md demands: the statistics shown on return cannot be reached
    /// by a second path, or the screen ends up contradicting the credit.
    ///
    /// The experience is what the absence **offers**. Each hero's own ceiling of one level is
    /// applied per hero afterwards, since it depends on where that hero was.
    /// </summary>
    public struct OfflineCredit
    {
        /// <summary>
        /// Hours of online-equivalent play the absence is worth, after the rate and the money
        /// ceiling. This is the single factor every number below is built from.
        /// </summary>
        public double EffectiveHours;

        public long Money;
        public long ExperienceOffered;
        public long EnemiesDefeated;
        public long DamageDealt;
        public long DamageTaken;
        public long Healing;
    }
}
