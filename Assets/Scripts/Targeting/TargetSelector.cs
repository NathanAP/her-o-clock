using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;

namespace HerOClock.Targeting
{
    /// <summary>
    /// Picks a character's target following the priority order in gameplay.md.
    ///
    /// The chain is used in two ways:
    /// honouring range, to decide who to attack;
    /// and ignoring range, to decide who to walk towards.
    ///
    /// Every comparison uses integers on purpose. With no floats involved, the choice is
    /// always the same for the same board state, which lets us replay a battle and, later
    /// on, simulate offline progression.
    /// </summary>
    public static class TargetSelector
    {
        public static Character Select(Character self, IReadOnlyList<Character> candidates, bool respectRange)
        {
            if (self == null || candidates == null)
            {
                return null;
            }

            // Rule 1: a taunt overrides everything else.
            if (self.TauntedBy != null && self.TauntedBy.IsAlive && !self.TauntedBy.IsUntargetable)
            {
                if (!respectRange || self.CanAttack(self.TauntedBy))
                {
                    return self.TauntedBy;
                }
            }

            Character best = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                Character candidate = candidates[i];

                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                // Untargetable takes a character out of the running entirely. An area still
                // catches it, because an area chooses nobody: it covers a rectangle.
                if (candidate.IsUntargetable)
                {
                    continue;
                }

                if (respectRange && !self.CanAttack(candidate))
                {
                    continue;
                }

                if (best == null || IsBetterByStandardChain(self, candidate, best))
                {
                    best = candidate;
                }
            }

            return best;
        }

        /// <summary>
        /// Rules 2 through 6. Each one is only consulted when the previous one ties.
        /// Rule 6 is positional precisely because it can never tie, so the chain always
        /// ends with a single winner.
        ///
        /// Public because abilities reuse it: a declared `priority` replaces the head of the chain
        /// and everything here carries on underneath as the tie-break. Reusing it is what avoids a
        /// second tie-break system that could disagree with this one.
        /// </summary>
        public static bool IsBetterByStandardChain(Character self, Character candidate, Character current)
        {
            // Rule 2: the closest enemy.
            int candidateDistance = GridPosition.Distance(self.Position, candidate.Position);
            int currentDistance = GridPosition.Distance(self.Position, current.Position);
            if (candidateDistance != currentDistance)
            {
                return candidateDistance < currentDistance;
            }

            // Rule 3: the lowest percentage of current health.
            // Compared by cross multiplication so no division is needed.
            long candidateShare = (long)candidate.CurrentHealth * current.Stats.MaxHealth;
            long currentShare = (long)current.CurrentHealth * candidate.Stats.MaxHealth;
            if (candidateShare != currentShare)
            {
                return candidateShare < currentShare;
            }

            // Rule 4: the lowest maximum health.
            if (candidate.Stats.MaxHealth != current.Stats.MaxHealth)
            {
                return candidate.Stats.MaxHealth < current.Stats.MaxHealth;
            }

            // Rule 5: the lowest physical armour.
            if (candidate.Stats.PhysicalArmor != current.Stats.PhysicalArmor)
            {
                return candidate.Stats.PhysicalArmor < current.Stats.PhysicalArmor;
            }

            // Rule 6: the lowest row and, on a tie, the lowest column.
            if (candidate.Position.Row != current.Position.Row)
            {
                return candidate.Position.Row < current.Position.Row;
            }

            return candidate.Position.Column < current.Position.Column;
        }
    }
}
