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
    /// When a blow does land, the time the timer overshot by is **carried into the next
    /// interval**. That matters more than it looks: a blow can only land on a simulation step,
    /// so throwing the overshoot away would round every interval up to whole steps, and a
    /// character at 4 attacks per second would really make 3.75 of them. The faster the
    /// character, the more it lost, which punished exactly the builds that pay for speed.
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

        /// <summary>
        /// Raised once per basic attack, naming who swung at whom.
        ///
        /// It exists apart from <see cref="Attacked"/> because that one cannot answer "was this a
        /// basic attack": it also fires for the thorns coming back at the attacker, and the
        /// director raises it again for every blow an ability lands. Anything that wants to draw
        /// the swing itself — a projectile, later an animation — needs the question answered.
        /// </summary>
        public event System.Action<Character, Character> Struck;

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

        /// <summary>
        /// Advances the timer and lands a blow when everything allows it.
        ///
        /// <paramref name="blocked"/> covers every reason the character cannot swing right now:
        /// mid step, or busy with an ability. The timer keeps running either way, which is what
        /// makes the basic attack become available during an ability's wind up and land in the
        /// first gap afterwards, exactly as abilities.md describes.
        /// </summary>
        public void Tick(float step, IReadOnlyList<Character> enemies, bool blocked)
        {
            if (!character.IsAlive)
            {
                return;
            }

            if (cooldown > 0f)
            {
                cooldown -= step;

                if (cooldown > 0f)
                {
                    return;
                }
            }

            // The timer is ready. From here it is held at zero until a blow actually lands, so a
            // character that walked for a long time cannot bank several blows to land at once.
            if (blocked)
            {
                cooldown = 0f;
                return;
            }

            Character target = TargetSelector.Select(character, enemies, true);
            if (target == null)
            {
                cooldown = 0f;
                return;
            }

            // Blindness makes a share of the basic attacks miss. The share comes from whatever
            // applied it, since each source of blindness decides how strong it is.
            if (character.Statuses.Has(Abilities.StatusKind.Blinded)
                && random.Roll(character.Statuses.ValueOf(Abilities.StatusKind.Blinded)))
            {
                cooldown += AttackInterval;
                return;
            }

            Attack(target);

            // Adding the interval rather than assigning it is what carries the overshoot, so the
            // real rate settles on the sheet's value instead of on whole simulation steps.
            cooldown += AttackInterval;

            if (cooldown <= 0f)
            {
                // Only reachable above one blow per simulation step, where the extra blows
                // cannot be landed at all. Letting the debt pile up would leave the character
                // attacking every step long after whatever made it that fast wore off.
                cooldown = AttackInterval;
            }
        }

        private void Attack(Character target)
        {
            // Announced before the damage is resolved, so whatever draws the swing is looking at
            // the board as it was when the blow was thrown, with the target still standing.
            Struck?.Invoke(character, target);

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

                // Thorns is physical damage, and life steal reacts to physical damage, so the one
                // reflecting heals from it. That is what makes a tank who heals by being hit a
                // build somebody can actually put together.
                //
                // The healing field is not read here, and cannot be: it only appears when
                // resistance climbs past 100%, which is elemental only. Physical mitigation is
                // capped at 75% by the curve.
                target.Heal(thorns.LifeStolen);

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
