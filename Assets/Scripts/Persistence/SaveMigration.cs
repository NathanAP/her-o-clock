namespace HerOClock.Persistence
{
    /// <summary>
    /// Brings a payload read from disk up to the version the game speaks.
    ///
    /// There are no conversion steps yet, because version 1 is the first version there has ever
    /// been. The class exists anyway, and it is not scaffolding: it already carries the two rules
    /// that decide whether a save survives a game update, and both are checked by tests.
    ///
    /// - **A file from the future is refused.** Reading a newer format by guessing what its fields
    ///   mean is the easiest way to destroy the progress of somebody who tried a newer build.
    /// - **A missing field becomes what a game that never had it would hold**, never an invented
    ///   value. That is what lets a new field be added without a conversion step at all, and it is
    ///   why almost every future change here will be no change.
    ///
    /// When a real step is needed, it goes in <see cref="Upgrade"/>, oldest first, each one moving
    /// the payload exactly one version forward.
    /// </summary>
    public static class SaveMigration
    {
        /// <summary>
        /// Prepares a payload for use, or explains why it cannot be used at all.
        ///
        /// Returning false means the file has to be skipped and the next one tried. It is a
        /// stronger reaction than a failed signature, which loads anyway: a signature says the
        /// numbers may have been changed, while a version from the future says we cannot even tell
        /// which number is which.
        /// </summary>
        public static bool TryUpgrade(SavePayload payload, out string problem)
        {
            if (payload == null)
            {
                problem = "the payload could not be read.";
                return false;
            }

            if (payload.version < 1)
            {
                problem = "it declares no format version, so it is not a save file.";
                return false;
            }

            if (payload.version > SavePayload.CurrentVersion)
            {
                problem = "it was written by version " + payload.version + " of the format and this game reads "
                    + SavePayload.CurrentVersion + ", so it comes from a newer build.";
                return false;
            }

            Upgrade(payload);
            FillWhatIsMissing(payload);

            payload.version = SavePayload.CurrentVersion;

            problem = null;
            return true;
        }

        /// <summary>
        /// Version to version conversions, oldest first. Empty while version 1 is the only one.
        /// </summary>
        private static void Upgrade(SavePayload payload)
        {
        }

        /// <summary>
        /// Gives a missing section the shape an empty one has.
        ///
        /// A save written before a section existed simply has no such field, and the serialiser
        /// leaves it null. Every reader downstream would then have to check for null on its own,
        /// and the one that forgot would be the one that crashed on somebody's old save.
        /// </summary>
        private static void FillWhatIsMissing(SavePayload payload)
        {
            if (payload.stage == null)
            {
                payload.stage = new StageSave();
            }

            if (payload.heroes == null)
            {
                payload.heroes = new HeroSave[0];
            }

            if (payload.activity == null)
            {
                payload.activity = new ActivitySave();
            }

            if (payload.activity.buckets == null)
            {
                payload.activity.buckets = new ActivityBucketSave[0];
            }

            if (string.IsNullOrEmpty(payload.integrity))
            {
                payload.integrity = SavePayload.IntegrityOk;
            }

            for (int i = 0; i < payload.heroes.Length; i++)
            {
                HeroSave hero = payload.heroes[i];

                if (hero == null)
                {
                    payload.heroes[i] = new HeroSave();
                    continue;
                }

                if (hero.manualPoints == null || hero.manualPoints.Length != 4)
                {
                    int[] points = new int[4];

                    for (int p = 0; hero.manualPoints != null && p < hero.manualPoints.Length && p < 4; p++)
                    {
                        points[p] = hero.manualPoints[p];
                    }

                    hero.manualPoints = points;
                }
            }
        }
    }
}
