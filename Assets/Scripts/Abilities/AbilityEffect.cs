using System;
using HerOClock.Characters;
using HerOClock.Combat;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// One entry of an ability's `effects` list.
    ///
    /// The fields a given effect uses depend on its <see cref="Type"/>, and the rest are ignored.
    /// That is the price of a serialised sheet Unity can draw: a polymorphic hierarchy would need
    /// a custom drawer and would not be visible in the Inspector at all.
    /// <see cref="AbilityValidator"/> is what pays that price back, by refusing an effect whose
    /// fields do not match its type.
    /// </summary>
    [Serializable]
    public class AbilityEffect
    {
        public EffectType Type = EffectType.DealDamage;

        public EffectTarget Target = EffectTarget.EachTarget;

        [Tooltip("How long it lasts, in seconds. Ignored by effects that happen instantly.")]
        public RankedValue Duration;

        [Header("Modify stat")]
        public ModifiableStat Stat = ModifiableStat.Power;

        [Tooltip("Percent multiplies the value the character already has; flat adds points to it.")]
        public StatModifierMode Mode = StatModifierMode.Percent;

        [Tooltip("The amount. Negative makes it a debuff: buff and debuff are the same effect, "
            + "separated only by the sign. Also carries the miss chance of a blinding status.")]
        public RankedValue Value;

        [Header("Deal damage")]
        public DamageType DamageType = DamageType.Physical;

        [Tooltip("Damage before any attribute is counted.")]
        public RankedValue Base;

        [Tooltip("Fraction of each of the user's attributes that is added to the damage.")]
        public AbilityScaling Scaling = new AbilityScaling();

        [Tooltip("How much damage is lost per cell of distance from the user. 0 keeps it the same "
            + "everywhere. Multiplicative, so the damage falls off but never reaches zero.")]
        public RankedValue Falloff;

        [Header("Apply status")]
        public StatusKind Status = StatusKind.Untargetable;

        [Header("Move to")]
        public MoveAnchor Anchor = MoveAnchor.LastTargetAnySide;
    }
}
