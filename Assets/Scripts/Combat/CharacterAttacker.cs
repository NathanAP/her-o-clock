using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Targeting;

namespace HerOClock.Combat
{
    /// <summary>
    /// One character's attack timer.
    ///
    /// The timer always runs, including while the character walks. A blow lands when the timer
    /// is ready, a target is in range and the character is not mid step. Since the timer stops
    /// at zero and never banks time, nobody can charge up an attack by stepping in and out of range.
    ///
    /// Not a MonoBehaviour on purpose, for the same reason as CharacterMover: the BattleDirector
    /// calls Tick, so there is a single update loop running in a predictable order.
    /// </summary>
    public class CharacterAttacker
    {
        private readonly Character character;
        private readonly BattleRandom random;
        private float cooldown;

        public CharacterAttacker(Character character, BattleRandom random)
        {
            this.character = character;
            this.random = random;

            // Every character has to wind up before landing its first blow.
            cooldown = AttackInterval;
        }

        /// <summary>Raised on every blow landed, so the view can react.</summary>
        public event System.Action<Character, Character, DamageResult> Attacked;

        private float AttackInterval
        {
            get
            {
                float attacksPerSecond = character.Stats.AttacksPerSecond;
                return attacksPerSecond > 0f ? 1f / attacksPerSecond : float.MaxValue;
            }
        }

        public void Reset()
        {
            cooldown = AttackInterval;
        }

        public void Tick(float deltaTime, IReadOnlyList<Character> enemies, bool isMoving)
        {
            if (!character.IsAlive)
            {
                return;
            }

            if (cooldown > 0f)
            {
                cooldown -= deltaTime;

                if (cooldown > 0f)
                {
                    return;
                }

                // The timer stops at zero instead of going negative. Otherwise a character that
                // walked for a long time would bank several blows to land all at once.
                cooldown = 0f;
            }

            if (isMoving)
            {
                return;
            }

            Character target = TargetSelector.Select(character, enemies, true);
            if (target == null)
            {
                return;
            }

            Attack(target);
            cooldown = AttackInterval;
        }

        private void Attack(Character target)
        {
            DamageResult result = Resolve(character, target, character.Stats.PhysicalDamage, DamageType.Physical, true);

            target.TakeDamage(result.Damage);
            target.Heal(result.Healing);
            character.Heal(result.LifeStolen);

            Attacked?.Invoke(character, target, result);

            if (result.Thorns > 0)
            {
                // The reflection is an independent physical attack against the attacker, and it
                // goes through the attacker's own mitigation. Thorns never react to thorns.
                DamageResult thorns = Resolve(target, character, result.Thorns, DamageType.Physical, false);
                character.TakeDamage(thorns.Damage);

                Attacked?.Invoke(target, character, thorns);
            }
        }

        private DamageResult Resolve(Character attacker, Character defender, int baseDamage, DamageType type, bool canTriggerThorns)
        {
            DamageInput input = new DamageInput
            {
                BaseDamage = baseDamage,
                Type = type,
                AttackerLevel = attacker.Level,
                AttackerLifeStealPercent = attacker.Stats.LifeStealPercent,
                TargetEvasionChance = defender.Stats.EvasionChance,
                TargetMitigationPoints = MitigationPointsOf(defender.Stats, type),
                TargetResistanceBonus = 0f,
                TargetThornsPercent = defender.Stats.ThornsPercent,
                CanTriggerThorns = canTriggerThorns
            };

            return DamageCalculator.Resolve(input, random);
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
