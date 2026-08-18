using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Targeting;

namespace HerOClock.Abilities
{
    /// <summary>
    /// Works out who an ability hits, following "## Escolha de alvo" in abilities.md.
    ///
    /// An ability is judged only by its own rules and never by the basic attack's range, so a
    /// character with reach 2 can hold an ability that fires down the whole board and use it the
    /// moment somebody stands in the line.
    ///
    /// Everything here compares integers or picks by a fixed order. Nothing is drawn at random,
    /// because a battle has to end the same way every time it is replayed from a seed.
    /// </summary>
    public static class AbilityTargetResolver
    {
        /// <summary>
        /// The characters an ability would hit right now, or an empty list when it has no valid
        /// target. An empty result is what makes a ready ability hold its charge rather than fire
        /// into nothing.
        /// </summary>
        public static List<Character> Resolve(
            Character user,
            AbilityTargeting targeting,
            int rank,
            IReadOnlyList<Character> allies,
            IReadOnlyList<Character> enemies)
        {
            List<Character> found = new List<Character>();

            if (user == null || !user.IsAlive || targeting == null)
            {
                return found;
            }

            if (targeting.Shape == AbilityShape.Self)
            {
                found.Add(user);
                return found;
            }

            IReadOnlyList<Character> pool = PoolFor(targeting.Who, user, allies, enemies);

            switch (targeting.Shape)
            {
                case AbilityShape.Single:
                    AddSingle(user, targeting, pool, found);
                    break;

                case AbilityShape.Area:
                    AddArea(user, targeting, allies, enemies, found);
                    break;

                case AbilityShape.Chain:
                    AddChain(user, targeting, rank, pool, found);
                    break;

                case AbilityShape.Line:
                    AddLine(user, targeting, pool, found);
                    break;
            }

            return found;
        }

        private static IReadOnlyList<Character> PoolFor(
            AbilityWho who, Character user, IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            if (who == AbilityWho.Self)
            {
                return new[] { user };
            }

            return who == AbilityWho.Allies ? allies : enemies;
        }

        // --- The shapes ---

        private static void AddSingle(
            Character user, AbilityTargeting targeting, IReadOnlyList<Character> pool, List<Character> found)
        {
            Character best = Best(user, targeting.Priority, pool, targeting.Range, null);

            if (best != null)
            {
                found.Add(best);
            }
        }

        /// <summary>
        /// An area is always centred on whoever used it, and catches **everyone** inside it,
        /// friend and foe alike. `who` says who the ability was looking for; it protects nobody
        /// from standing in the way.
        /// </summary>
        private static void AddArea(
            Character user,
            AbilityTargeting targeting,
            IReadOnlyList<Character> allies,
            IReadOnlyList<Character> enemies,
            List<Character> found)
        {
            int halfColumns = targeting.AreaColumns / 2;
            int halfRows = targeting.AreaRows / 2;

            AddInsideArea(user, allies, halfColumns, halfRows, found);
            AddInsideArea(user, enemies, halfColumns, halfRows, found);
        }

        private static void AddInsideArea(
            Character user, IReadOnlyList<Character> pool, int halfColumns, int halfRows, List<Character> found)
        {
            if (pool == null)
            {
                return;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                Character candidate = pool[i];

                if (candidate == null || !candidate.IsAlive || found.Contains(candidate))
                {
                    continue;
                }

                int columns = System.Math.Abs(candidate.Position.Column - user.Position.Column);
                int rows = System.Math.Abs(candidate.Position.Row - user.Position.Row);

                if (columns <= halfColumns && rows <= halfRows)
                {
                    found.Add(candidate);
                }
            }
        }

        /// <summary>
        /// The first link is chosen by the priority; every jump after it goes to whoever is nearest
        /// the previous target and has not been hit yet. The priority rules the start of the
        /// sequence only.
        /// </summary>
        private static void AddChain(
            Character user, AbilityTargeting targeting, int rank, IReadOnlyList<Character> pool, List<Character> found)
        {
            Character first = Best(user, targeting.Priority, pool, targeting.Range, null);

            if (first == null)
            {
                return;
            }

            found.Add(first);

            int maxTargets = targeting.MaxTargets.IsEmpty ? 1 : targeting.MaxTargets.IntAt(rank);

            while (found.Count < maxTargets)
            {
                Character previous = found[found.Count - 1];
                Character next = Best(previous, TargetPriority.Nearest, pool, targeting.JumpRange, found);

                if (next == null)
                {
                    return;
                }

                found.Add(next);
            }
        }

        /// <summary>
        /// Only characters standing on one of the eight straight directions can be picked, and the
        /// priority chooses among those. Filtering before choosing is what guarantees the target is
        /// actually hit: choosing first and then looking for the closest matching direction would
        /// leave the ability aimed at somebody the line misses.
        /// </summary>
        private static void AddLine(
            Character user, AbilityTargeting targeting, IReadOnlyList<Character> pool, List<Character> found)
        {
            List<Character> aligned = new List<Character>();

            if (pool != null)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    Character candidate = pool[i];

                    if (candidate != null && candidate.IsAlive && IsAligned(user.Position, candidate.Position))
                    {
                        aligned.Add(candidate);
                    }
                }
            }

            Character chosen = Best(user, targeting.Priority, aligned, targeting.Range, null);

            if (chosen == null)
            {
                return;
            }

            int stepColumn = System.Math.Sign(chosen.Position.Column - user.Position.Column);
            int stepRow = System.Math.Sign(chosen.Position.Row - user.Position.Row);

            // Everyone the line crosses, all the way to the edge of the board, and not only up to
            // the character that decided the direction.
            for (int i = 0; i < aligned.Count; i++)
            {
                Character candidate = aligned[i];

                if (IsOnRay(user.Position, candidate.Position, stepColumn, stepRow))
                {
                    found.Add(candidate);
                }
            }
        }

        private static bool IsAligned(GridPosition from, GridPosition to)
        {
            int columns = to.Column - from.Column;
            int rows = to.Row - from.Row;

            if (columns == 0 && rows == 0)
            {
                return false;
            }

            return columns == 0 || rows == 0 || System.Math.Abs(columns) == System.Math.Abs(rows);
        }

        private static bool IsOnRay(GridPosition from, GridPosition to, int stepColumn, int stepRow)
        {
            int columns = to.Column - from.Column;
            int rows = to.Row - from.Row;

            return System.Math.Sign(columns) == stepColumn
                && System.Math.Sign(rows) == stepRow
                && IsAligned(from, to);
        }

        // --- Choosing one ---

        /// <summary>
        /// The best candidate by the given priority, with the standard chain of gameplay.md
        /// breaking every tie underneath it.
        ///
        /// A taunt still comes first. Being forced to attack whoever taunted you is above every
        /// other targeting rule, and an ability declaring its own priority does not escape that.
        /// </summary>
        private static Character Best(
            Character from,
            TargetPriority priority,
            IReadOnlyList<Character> pool,
            int range,
            List<Character> alreadyChosen)
        {
            if (pool == null)
            {
                return null;
            }

            if (from.TauntedBy != null && from.TauntedBy.IsAlive && !from.TauntedBy.IsUntargetable)
            {
                if (Contains(pool, from.TauntedBy) && Reaches(from, from.TauntedBy, range)
                    && (alreadyChosen == null || !alreadyChosen.Contains(from.TauntedBy)))
                {
                    return from.TauntedBy;
                }
            }

            Character best = null;

            for (int i = 0; i < pool.Count; i++)
            {
                Character candidate = pool[i];

                if (candidate == null || !candidate.IsAlive || candidate.IsUntargetable)
                {
                    continue;
                }

                if (!Reaches(from, candidate, range))
                {
                    continue;
                }

                if (alreadyChosen != null && alreadyChosen.Contains(candidate))
                {
                    continue;
                }

                if (best == null || IsBetter(from, priority, candidate, best))
                {
                    best = candidate;
                }
            }

            return best;
        }

        private static bool Reaches(Character from, Character candidate, int range)
        {
            return GridPosition.Distance(from.Position, candidate.Position) <= range;
        }

        private static bool Contains(IReadOnlyList<Character> pool, Character character)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (ReferenceEquals(pool[i], character))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The declared priority replaces the head of the chain; everything below it stays exactly
        /// as gameplay.md orders it, ending on the positional rule that can never tie.
        /// </summary>
        private static bool IsBetter(Character from, TargetPriority priority, Character candidate, Character current)
        {
            switch (priority)
            {
                case TargetPriority.Farthest:
                {
                    int candidateDistance = GridPosition.Distance(from.Position, candidate.Position);
                    int currentDistance = GridPosition.Distance(from.Position, current.Position);

                    if (candidateDistance != currentDistance)
                    {
                        return candidateDistance > currentDistance;
                    }

                    break;
                }

                case TargetPriority.LowestHealthPercent:
                {
                    long candidateShare = (long)candidate.CurrentHealth * current.Stats.MaxHealth;
                    long currentShare = (long)current.CurrentHealth * candidate.Stats.MaxHealth;

                    if (candidateShare != currentShare)
                    {
                        return candidateShare < currentShare;
                    }

                    break;
                }

                case TargetPriority.LowestMaxHealth:
                {
                    if (candidate.Stats.MaxHealth != current.Stats.MaxHealth)
                    {
                        return candidate.Stats.MaxHealth < current.Stats.MaxHealth;
                    }

                    break;
                }

                case TargetPriority.LowestPhysicalArmor:
                {
                    if (candidate.Stats.PhysicalArmor != current.Stats.PhysicalArmor)
                    {
                        return candidate.Stats.PhysicalArmor < current.Stats.PhysicalArmor;
                    }

                    break;
                }
            }

            return TargetSelector.IsBetterByStandardChain(from, candidate, current);
        }
    }
}
