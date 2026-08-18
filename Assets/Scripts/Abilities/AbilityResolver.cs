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
            // The ability's own number, plus the share of the user's attributes it scales with.
            // Without the scaling an ability would be worth the same on a character that built for
            // it and on one that did not.
            float raw = effect.Base.At(rank) + effect.Scaling.AppliedTo(user.Stats);
            int baseDamage = Mathf.Max(0, Mathf.RoundToInt(raw));

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
