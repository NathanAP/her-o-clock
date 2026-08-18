namespace HerOClock.Abilities
{
    /// <summary>
    /// The named states an `apply_status` effect can grant, described in buffs-and-debuffs.md.
    ///
    /// A named state only exists when it does something the game cannot already express. Anything
    /// that is only a number belongs in `modify_stat`, which is why there is no "faster" or
    /// "tougher" here.
    /// </summary>
    public enum StatusKind
    {
        /// <summary>Cannot be chosen as a target. An area still catches it, since an area chooses nobody.</summary>
        Untargetable = 0,

        /// <summary>Takes no damage from any source.</summary>
        Intangible = 1,

        /// <summary>Forced to attack whoever applied it, above every other targeting rule.</summary>
        Taunted = 2,

        /// <summary>Cannot use abilities, and any preparation under way is cancelled.</summary>
        Silenced = 3,

        /// <summary>Misses a share of its basic attacks and cannot use single target abilities.</summary>
        Blinded = 4
    }
}
