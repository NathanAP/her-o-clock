using System.Collections.Generic;

namespace HerOClock.Abilities
{
    /// <summary>
    /// Checks that an ability on a sheet says something the game can actually carry out.
    ///
    /// This is the battery that runs per ability, and it exists because every mistake it catches
    /// is **silent**. A scaling array one entry short makes the last rank read a value that does
    /// not exist; a priority on a shape that chooses nobody simply does nothing; a chain with no
    /// jump range reaches one target and looks like a balance problem. None of them throws, and
    /// none of them shows up in the Console.
    ///
    /// It reports every problem it finds rather than stopping at the first, the same way stage
    /// files and the strings file are handled, and it has no Unity in it so it can be checked by
    /// being handed objects.
    /// </summary>
    public static class AbilityValidator
    {
        public static List<string> Validate(AbilityDefinition ability)
        {
            List<string> problems = new List<string>();

            if (ability == null)
            {
                problems.Add("The ability is missing entirely.");
                return problems;
            }

            string name = string.IsNullOrWhiteSpace(ability.Id) ? "<no id>" : ability.Id;

            if (string.IsNullOrWhiteSpace(ability.Id))
            {
                problems.Add("An ability has no id, so nothing can refer to it.");
            }

            if (ability.Ranks < 1)
            {
                problems.Add(name + ": it declares " + ability.Ranks + " ranks, and every ability has at least one.");
            }

            CheckRanked(name, "preparation", ability.Preparation, ability.Ranks, true, problems);
            CheckRanked(name, "casting", ability.Casting, ability.Ranks, true, problems);
            CheckRanked(name, "recoil", ability.Recoil, ability.Ranks, true, problems);
            CheckRanked(name, "cooldown", ability.Cooldown, ability.Ranks, true, problems);

            CheckTargeting(name, ability, problems);
            CheckEffects(name, ability, problems);

            return problems;
        }

        /// <summary>Every ability of every sheet, with the character's id in front of each problem.</summary>
        public static List<string> Validate(IReadOnlyList<AbilityDefinition> abilities, string ownerId)
        {
            List<string> problems = new List<string>();

            if (abilities == null)
            {
                return problems;
            }

            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < abilities.Count; i++)
            {
                AbilityDefinition ability = abilities[i];

                if (ability != null && !string.IsNullOrWhiteSpace(ability.Id) && !seen.Add(ability.Id))
                {
                    problems.Add(ownerId + ": the ability '" + ability.Id + "' appears more than once.");
                }

                List<string> found = Validate(ability);

                for (int p = 0; p < found.Count; p++)
                {
                    problems.Add(ownerId + ": " + found[p]);
                }
            }

            return problems;
        }

        /// <summary>
        /// The rule abilities.md names its own silent failure for: an array is either one entry,
        /// meaning constant, or exactly as many entries as the ability has ranks.
        /// </summary>
        private static void CheckRanked(
            string name, string field, RankedValue value, int ranks, bool required, List<string> problems)
        {
            if (value.IsEmpty)
            {
                if (required)
                {
                    problems.Add(name + ": '" + field + "' has no value at all.");
                }

                return;
            }

            if (value.Length != 1 && value.Length != ranks)
            {
                problems.Add(name + ": '" + field + "' has " + value.Length + " entries, and the ability has "
                    + ranks + " ranks. It has to be one entry, for a constant, or exactly one per rank.");
            }
        }

        private static void CheckTargeting(string name, AbilityDefinition ability, List<string> problems)
        {
            AbilityTargeting targeting = ability.Targeting;

            if (targeting == null)
            {
                problems.Add(name + ": it has no targeting block, so nobody knows who it is for.");
                return;
            }

            // A priority only means something for the shapes that pick somebody. On the others it
            // would be read without error and do nothing at all.
            if (!targeting.ChoosesATarget && targeting.Priority != TargetPriority.Nearest)
            {
                problems.Add(name + ": it declares the priority '" + targeting.Priority + "' on a '"
                    + targeting.Shape + "' shape, which chooses nobody. Remove it or change the shape.");
            }

            if (targeting.Shape == AbilityShape.Area)
            {
                if (targeting.AreaColumns < 1 || targeting.AreaRows < 1)
                {
                    problems.Add(name + ": an area needs both of its sides, and it declares "
                        + targeting.AreaColumns + " by " + targeting.AreaRows + ".");
                }
            }

            if (targeting.Shape == AbilityShape.Chain)
            {
                CheckRanked(name, "maxTargets", targeting.MaxTargets, ability.Ranks, true, problems);

                if (targeting.JumpRange < 1)
                {
                    problems.Add(name + ": a chain needs a jump range of at least 1, or it can never "
                        + "reach a second target.");
                }
            }

            if (targeting.Shape != AbilityShape.Self && targeting.Range < 1)
            {
                problems.Add(name + ": it has a range of " + targeting.Range
                    + ", so it can never reach anybody.");
            }

            if (targeting.Who == AbilityWho.Self && targeting.Shape != AbilityShape.Self)
            {
                problems.Add(name + ": it targets 'self' with the shape '" + targeting.Shape
                    + "'. Only the 'self' shape can do that.");
            }
        }

        private static void CheckEffects(string name, AbilityDefinition ability, List<string> problems)
        {
            if (ability.Effects == null || ability.Effects.Length == 0)
            {
                problems.Add(name + ": it has no effects, so using it would do nothing.");
                return;
            }

            for (int i = 0; i < ability.Effects.Length; i++)
            {
                AbilityEffect effect = ability.Effects[i];
                string where = name + ", effect " + (i + 1);

                if (effect == null)
                {
                    problems.Add(where + ": it is empty.");
                    continue;
                }

                switch (effect.Type)
                {
                    case EffectType.ModifyStat:
                        CheckRanked(where, "value", effect.Value, ability.Ranks, true, problems);
                        CheckRanked(where, "duration", effect.Duration, ability.Ranks, true, problems);
                        break;

                    case EffectType.DealDamage:
                        CheckRanked(where, "base", effect.Base, ability.Ranks, true, problems);

                        if (effect.Scaling == null)
                        {
                            problems.Add(where + ": damage with no scaling block.");
                        }

                        break;

                    case EffectType.ApplyStatus:
                        CheckRanked(where, "duration", effect.Duration, ability.Ranks, true, problems);

                        if (effect.Status == StatusKind.Blinded)
                        {
                            CheckRanked(where, "value", effect.Value, ability.Ranks, true, problems);
                        }

                        break;

                    case EffectType.MoveTo:
                        if (ability.Targeting != null && ability.Targeting.Shape == AbilityShape.Self)
                        {
                            problems.Add(where + ": it moves somebody next to the last target, and a "
                                + "'self' shape never has one.");
                        }

                        break;
                }
            }
        }
    }
}
