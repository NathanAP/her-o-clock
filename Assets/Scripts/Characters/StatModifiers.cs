using System;
using System.Collections.Generic;
using HerOClock.Abilities;

namespace HerOClock.Characters
{
    /// <summary>
    /// The buffs and debuffs sitting on one character.
    ///
    /// This is the seam architecture.md promised: items, skill trees and buffs become further
    /// sources inside the stats rather than a second calculation beside them. It reaches the
    /// derived values too, and not only the four primaries, because content buffs attack speed and
    /// cooldown reduction long before it buffs an attribute.
    ///
    /// Pure C# with no Unity in it, so the stacking rule can be checked by handing it numbers.
    /// </summary>
    public class StatModifiers
    {
        private readonly List<StatModifier> active = new List<StatModifier>();

        /// <summary>Raised whenever the set changes, so the character can rebuild what it caches.</summary>
        public event Action Changed;

        public int Count
        {
            get { return active.Count; }
        }

        public IReadOnlyList<StatModifier> Active
        {
            get { return active; }
        }

        /// <summary>
        /// Adds a buff or debuff, applying the rule from buffs-and-debuffs.md: the same one arriving
        /// again **keeps the stronger value and restarts the duration**. It never stacks.
        ///
        /// Stacking was rejected because it would need a ceiling per buff, or six heroes carrying
        /// the same slow would pin an enemy at zero. "Same" means same source and same stat: two
        /// different abilities touching attack speed still add up normally.
        ///
        /// The duration is always the new one, even when the value that survives is the old one.
        /// Refreshing a weak effect must not be worse than doing nothing.
        /// </summary>
        public void Apply(string sourceId, ModifiableStat stat, StatModifierMode mode, float value, float duration)
        {
            if (duration <= 0f || value == 0f)
            {
                return;
            }

            for (int i = 0; i < active.Count; i++)
            {
                StatModifier existing = active[i];

                if (existing.SourceId != sourceId || existing.Stat != stat || existing.Mode != mode)
                {
                    continue;
                }

                // Stronger in the direction of the effect: for a buff the larger number, for a
                // debuff the more negative one.
                if (Math.Abs(value) > Math.Abs(existing.Value) && Math.Sign(value) == Math.Sign(existing.Value))
                {
                    existing.Value = value;
                }
                else if (Math.Sign(value) != Math.Sign(existing.Value))
                {
                    // A buff replacing a debuff on the same source, or the other way round. There
                    // is no "stronger" across the sign, so the newest one wins outright.
                    existing.Value = value;
                }

                existing.Remaining = duration;
                Raise();
                return;
            }

            active.Add(new StatModifier
            {
                SourceId = sourceId,
                Stat = stat,
                Mode = mode,
                Value = value,
                Remaining = duration
            });

            Raise();
        }

        /// <summary>Counts down every duration and drops what expired.</summary>
        public void Tick(float step)
        {
            if (step <= 0f || active.Count == 0)
            {
                return;
            }

            bool removed = false;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                active[i].Remaining -= step;

                if (active[i].Remaining <= 0f)
                {
                    active.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                Raise();
            }
        }

        public void Clear()
        {
            if (active.Count == 0)
            {
                return;
            }

            active.Clear();
            Raise();
        }

        /// <summary>
        /// Applies everything touching a stat to a value that was computed without them.
        ///
        /// Flat comes first and percent second, so a percentage always reads as a share of the
        /// whole rather than of whatever happened to be applied before it. Two orders would give
        /// two different numbers for the same pair of buffs, and nobody could predict which.
        ///
        /// Percentages from different sources add up before being applied, for the same reason:
        /// two buffs of 20% give 40%, not 44%.
        /// </summary>
        public float Apply(ModifiableStat stat, float value)
        {
            if (active.Count == 0)
            {
                return value;
            }

            float flat = 0f;
            float percent = 0f;

            for (int i = 0; i < active.Count; i++)
            {
                StatModifier modifier = active[i];

                if (modifier.Stat != stat)
                {
                    continue;
                }

                if (modifier.Mode == StatModifierMode.Flat)
                {
                    flat += modifier.Value;
                }
                else
                {
                    percent += modifier.Value;
                }
            }

            if (flat == 0f && percent == 0f)
            {
                return value;
            }

            return (value + flat) * (1f + percent / 100f);
        }

        private void Raise()
        {
            Changed?.Invoke();
        }
    }
}
