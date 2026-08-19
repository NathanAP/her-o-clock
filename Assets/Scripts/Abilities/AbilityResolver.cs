using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// Applies an ability's effects to the targets it found, in the order they are written.
    ///
    /// The order is load-bearing and not a detail: an ability that makes its user intangible has
    /// to apply that before dealing the damage that would otherwise come back at it.
    ///
    /// Effects are independent. One that cannot happen — a reposition with nowhere to go — does
    /// not cancel the ones around it.
    /// </summary>
    public static class AbilityResolver
    {
        /// <summary>Raised for every blow an ability lands, so the view can show it.</summary>
        public delegate void DamageDealt(Character user, Character target, DamageResult result);

        public static void Apply(
            Character user,
            AbilityDefinition ability,
            int rank,
            IReadOnlyList<Character> targets,
            BattleGrid grid,
            BattleRandom random,
            DamageDealt onDamage)
        {
            if (user == null || ability == null || ability.Effects == null)
            {
                return;
            }

            for (int i = 0; i < ability.Effects.Length; i++)
            {
                AbilityEffect effect = ability.Effects[i];

                if (effect == null)
                {
                    continue;
                }

                ApplyTo(user, ability, rank, effect, Receivers(user, effect, targets), targets, grid, random, onDamage);
            }
        }

        /// <summary>Who one effect lands on, which is declared per effect and not per ability.</summary>
        private static List<Character> Receivers(Character user, AbilityEffect effect, IReadOnlyList<Character> targets)
        {
            List<Character> receivers = new List<Character>();

            switch (effect.Target)
            {
                case EffectTarget.Self:
                    receivers.Add(user);
                    break;

                case EffectTarget.FirstTarget:
                    if (targets != null && targets.Count > 0)
                    {
                        receivers.Add(targets[0]);
                    }
                    break;

                default:
                    if (targets != null)
                    {
                        receivers.AddRange(targets);
                    }
                    break;
            }

            return receivers;
        }

        private static void ApplyTo(
            Character user,
            AbilityDefinition ability,
            int rank,
            AbilityEffect effect,
            List<Character> receivers,
            IReadOnlyList<Character> targets,
            BattleGrid grid,
            BattleRandom random,
            DamageDealt onDamage)
        {
            float duration = effect.Duration.At(rank);
            float value = effect.Value.At(rank);

            for (int i = 0; i < receivers.Count; i++)
            {
                Character receiver = receivers[i];

                if (receiver == null || !receiver.IsAlive)
                {
                    continue;
                }

                switch (effect.Type)
                {
                    case EffectType.ModifyStat:
                        receiver.Modifiers.Apply(ability.Id, effect.Stat, effect.Mode, value, duration);
                        break;

                    case EffectType.DealDamage:
                        Damage(user, receiver, ability, rank, effect, random, onDamage);
                        break;

                    case EffectType.ApplyStatus:
                        Status(user, receiver, effect, duration, value);
                        break;

                    case EffectType.MoveTo:
                        // Anchored on the ability's own targets, not on this effect's receivers.
                        // A blink is written as "move me", so the receiver is the user and the
                        // place to land is decided by where the ability went.
                        Move(receiver, effect, targets, grid);
                        break;
                }
            }
        }

        private static void Damage(
            Character user,
            Character target,
            AbilityDefinition ability,
            int rank,
            AbilityEffect effect,
            BattleRandom random,
            DamageDealt onDamage)
        {
            // The rank's own number, raised by the share of the user's attributes it scales with.
            // The attribute multiplies rather than adds, exactly as a weapon and POW do: otherwise
            // enough points would make the rank stop mattering.
            float raw = effect.Base.At(rank) * effect.Scaling.MultiplierFor(user.Stats);
            int baseDamage = Mathf.Max(0, Mathf.RoundToInt(raw * FalloffFactor(user, target, effect, rank)));

            DamageInput input = new DamageInput
            {
                BaseDamage = baseDamage,
                Type = effect.DamageType,
                AttackerLevel = user.Level,
                AttackerLifeStealPercent = user.Stats.LifeStealPercent,
                TargetEvasionChance = target.Stats.EvasionChance,
                TargetMitigationPoints = MitigationPointsOf(target.Stats, effect.DamageType),
                TargetResistanceBonus = 0f,
                TargetThornsPercent = target.Stats.ThornsPercent,

                // An ability is not a basic attack, and thorns reflect blows. Letting an area
                // ability wake up every thorns in the rectangle at once would make one piece of
                // equipment define the whole fight.
                CanTriggerThorns = false
            };

            DamageResult result = DamageCalculator.Resolve(input, random);

            // An ability never kills the character that used it. The cost of a powerful ability is
            // meant to hurt, and the promise that it will not finish the job has to be absolute:
            // checking before the cast would look at a number the preparation time can outdate, and
            // would disarm the character exactly when the battle got hard.
            //
            // Only the ability's own damage is held back. Thorns, an ally's area and anybody's
            // basic attack still kill normally, or a character with a costly ability would become
            // immortal by accident.
            if (ReferenceEquals(target, user))
            {
                result.Damage = Mathf.Min(result.Damage, Mathf.Max(0, target.CurrentHealth - 1));
            }

            target.TakeDamage(result.Damage);
            target.Heal(result.Healing);
            user.Heal(result.LifeStolen);

            if (onDamage != null)
            {
                onDamage(user, target, result);
            }
        }

        private static void Status(Character user, Character receiver, AbilityEffect effect, float duration, float value)
        {
            if (effect.Status == StatusKind.Taunted)
            {
                // A taunt needs to know who is doing it, which is the one named state that is a
                // reference and not just a flag.
                receiver.ApplyTaunt(user, duration);
                return;
            }

            receiver.Statuses.Apply(effect.Status, duration, value);
        }

        /// <summary>
        /// Puts a character on one of the four cells around the last target: the first one free and
        /// on the board, ordered by lowest row and then lowest column.
        ///
        /// The order is fixed rather than "nearest" or "most convenient", because the same battle
        /// from the same seed has to end the same way. With nowhere to go, the character simply
        /// stays put and the rest of the ability carries on.
        /// </summary>
        private static void Move(Character moving, AbilityEffect effect, IReadOnlyList<Character> targets, BattleGrid grid)
        {
            if (grid == null || effect.Anchor != MoveAnchor.LastTargetAnySide)
            {
                return;
            }

            Character last = LastOther(moving, targets);

            if (last == null)
            {
                return;
            }

            GridPosition around = last.Position;

            GridPosition[] sides =
            {
                new GridPosition(around.Column, around.Row - 1),
                new GridPosition(around.Column - 1, around.Row),
                new GridPosition(around.Column + 1, around.Row),
                new GridPosition(around.Column, around.Row + 1)
            };

            for (int i = 0; i < sides.Length; i++)
            {
                if (grid.IsInside(sides[i]) && grid.IsFree(sides[i]))
                {
                    moving.MoveTo(sides[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// The last target of the ability, skipping the character being moved. A blink that lands
        /// on the caster's own square would be no blink at all.
        /// </summary>
        private static Character LastOther(Character moving, IReadOnlyList<Character> targets)
        {
            if (targets == null)
            {
                return null;
            }

            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i] != null && !ReferenceEquals(targets[i], moving))
                {
                    return targets[i];
                }
            }

            return null;
        }

        /// <summary>
        /// How much of the base damage survives the distance between the user and the target.
        ///
        /// Multiplicative rather than subtractive, for the same reason every other reduction in the
        /// game is: it falls off but never reaches zero, so being in the line always counts for
        /// something and being close always counts for more. A subtractive curve would zero out
        /// past some distance and would need a floor written by hand on every ability.
        ///
        /// The exponent is the distance minus one, which is what makes the target standing right
        /// next to the user take the full number written on the sheet.
        /// </summary>
        private static float FalloffFactor(Character user, Character target, AbilityEffect effect, int rank)
        {
            float falloff = effect.Falloff.At(rank);

            if (falloff <= 0f)
            {
                return 1f;
            }

            int cells = GridPosition.Distance(user.Position, target.Position);
            return Mathf.Pow(1f - falloff, Mathf.Max(0, cells - 1));
        }

        private static int MitigationPointsOf(CharacterStats stats, DamageType type)
        {
            switch (type)
            {
                case DamageType.Fire: return stats.FireResistance;
                case DamageType.Water: return stats.WaterResistance;
                case DamageType.Electric: return stats.ElectricResistance;
                default: return stats.PhysicalArmor;
            }
        }
    }
}
