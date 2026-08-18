namespace HerOClock.Abilities
{
    /// <summary>
    /// Which rule heads an ability's target choice, from the `priority` field in abilities.md.
    ///
    /// It **replaces the first rule** of the default chain in gameplay.md, and the rest of that
    /// chain carries on underneath as the tie-break, in its own order, ending on the positional
    /// rule that can never tie.
    ///
    /// That is what answers "and if several tie?". There is no "any of them": a battle has to end
    /// the same way every time it is replayed from a seed, and a tie broken at random destroys
    /// that. Reusing the existing chain also avoids inventing a second tie-break system just for
    /// abilities.
    /// </summary>
    public enum TargetPriority
    {
        /// <summary>The closest. The default, and the head of the standard chain already.</summary>
        Nearest = 0,

        Farthest = 1,

        LowestHealthPercent = 2,

        LowestMaxHealth = 3,

        LowestPhysicalArmor = 4
    }
}
