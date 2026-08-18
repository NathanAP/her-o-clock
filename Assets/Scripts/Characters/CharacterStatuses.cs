using System;
using System.Collections.Generic;
using HerOClock.Abilities;

namespace HerOClock.Characters
{
    /// <summary>
    /// The named states currently on one character, described in buffs-and-debuffs.md.
    ///
    /// A named state only exists when it does something the game cannot already express with a
    /// number. Anything that is only a number belongs in <see cref="StatModifiers"/>.
    ///
    /// Like a stat modifier, the same state arriving again refreshes rather than stacks. Two
    /// silences do not silence for twice as long.
    /// </summary>
    public class CharacterStatuses
    {
        private readonly Dictionary<StatusKind, float> remaining = new Dictionary<StatusKind, float>();
        private readonly Dictionary<StatusKind, float> values = new Dictionary<StatusKind, float>();

        /// <summary>Raised when a state arrives or expires.</summary>
        public event Action Changed;

        /// <summary>
        /// Grants a state for a while.
        ///
        /// The value carries whatever the state needs beyond its own existence, which today is only
        /// the miss chance of <see cref="StatusKind.Blinded"/>. It comes from the ability, because
        /// each source of blindness decides how strong it is.
        /// </summary>
        public void Apply(StatusKind kind, float duration, float value = 0f)
        {
            if (duration <= 0f)
            {
                return;
            }

            float current;

            if (remaining.TryGetValue(kind, out current) && current >= duration)
            {
                // Already covered for longer. The value still updates, so a stronger source
                // arriving under a weaker one is not wasted.
                values[kind] = Math.Max(values.TryGetValue(kind, out float existing) ? existing : 0f, value);
                return;
            }

            remaining[kind] = duration;
            values[kind] = value;

            Raise();
        }

        public bool Has(StatusKind kind)
        {
            float left;
            return remaining.TryGetValue(kind, out left) && left > 0f;
        }

        public float ValueOf(StatusKind kind)
        {
            float value;
            return values.TryGetValue(kind, out value) ? value : 0f;
        }

        public void Tick(float step)
        {
            if (step <= 0f || remaining.Count == 0)
            {
                return;
            }

            List<StatusKind> expired = null;

            foreach (StatusKind kind in new List<StatusKind>(remaining.Keys))
            {
                float left = remaining[kind] - step;

                if (left <= 0f)
                {
                    if (expired == null)
                    {
                        expired = new List<StatusKind>();
                    }

                    expired.Add(kind);
                }
                else
                {
                    remaining[kind] = left;
                }
            }

            if (expired == null)
            {
                return;
            }

            for (int i = 0; i < expired.Count; i++)
            {
                remaining.Remove(expired[i]);
                values.Remove(expired[i]);
            }

            Raise();
        }

        public void Clear()
        {
            if (remaining.Count == 0)
            {
                return;
            }

            remaining.Clear();
            values.Clear();
            Raise();
        }

        private void Raise()
        {
            Changed?.Invoke();
        }
    }
}
