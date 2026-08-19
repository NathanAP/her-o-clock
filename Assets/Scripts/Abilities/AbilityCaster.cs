using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using UnityEngine;

namespace HerOClock.Abilities
{
    /// <summary>
    /// One character's abilities: their cooldowns, and the phase it is currently in.
    ///
    /// The four phases of abilities.md live here. Preparation, casting and recovery are **one
    /// occupied block**: from the first frame of the wind up to the last of the recovery the
    /// character only does that ability. The cooldown is the odd one out — it starts the moment
    /// the casting ends, so the recovery delays the next ability without delaying this one's
    /// recharge.
    ///
    /// Not a MonoBehaviour, for the same reason as the mover and the attacker: the BattleDirector
    /// calls Tick, so there is a single update loop running in a predictable order.
    /// </summary>
    public class AbilityCaster
    {
        private enum Phase
        {
            Idle,
            Preparing,
            Casting,
            Recoiling
        }

        /// <summary>
        /// <summary>
        /// The rank an ability is used at, which is the highest one its schedule has opened for
        /// this character's level.
        ///
        /// While the skill tree does not exist, the level is the only gate: `rankAvailability` on
        /// the sheet says when each step arrives, per ability. A sheet with no schedule stays at
        /// rank 1, which is what every ability written before the field meant.
        ///
        /// This is what makes a low rank 1 safe to write. Without it the damage of an ability has
        /// to be gentle from end to end, because nothing would stop a character reaching the top
        /// rank early.
        /// </summary>
        private int RankOf(AbilityDefinition ability)
        {
            int rank = ability.RankAt(character.Level);
            return rank < 1 ? 1 : rank;
        }

        private readonly Character character;
        private readonly BattleGrid grid;
        private readonly BattleRandom random;
        private readonly IReadOnlyList<AbilityDefinition> abilities;
        private readonly float[] cooldowns;

        private Phase phase = Phase.Idle;
        private float remaining;
        private int castingIndex = -1;
        private List<Character> targets;

        public AbilityCaster(Character character, BattleGrid grid, BattleRandom random)
        {
            this.character = character;
            this.grid = grid;
            this.random = random;

            abilities = character.Definition.Abilities;
            cooldowns = new float[abilities != null ? abilities.Count : 0];
        }

        /// <summary>Raised for every blow an ability lands, so the view can react.</summary>
        public event AbilityResolver.DamageDealt Damaged;

        /// <summary>
        /// True while the character is inside an ability and cannot move or attack.
        ///
        /// The attacker's timer keeps running regardless, so the basic attack becomes available
        /// during the wind up and lands in the first gap afterwards, exactly as the spec describes.
        /// </summary>
        public bool IsBusy
        {
            get { return phase != Phase.Idle; }
        }

        public void Reset()
        {
            phase = Phase.Idle;
            remaining = 0f;
            castingIndex = -1;
            targets = null;

            for (int i = 0; i < cooldowns.Length; i++)
            {
                cooldowns[i] = 0f;
            }
        }

        /// <summary>
        /// Advances every cooldown and whatever phase is under way.
        ///
        /// Called before the mover and the attacker so that <see cref="IsBusy"/> is already correct
        /// for this step when they are asked whether they may act.
        /// </summary>
        public void Tick(float step, IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            if (!character.IsAlive)
            {
                Reset();
                return;
            }

            AdvanceCooldowns(step);

            // Silence cancels a preparation, and abilities.md sends the cancelled one to half of
            // its cooldown rather than to none of it.
            if (phase == Phase.Preparing && character.Statuses.Has(StatusKind.Silenced))
            {
                CancelPreparation();
                return;
            }

            if (phase != Phase.Idle)
            {
                AdvancePhase(step, allies, enemies);
            }
        }

        /// <summary>
        /// Starts an ability if one is ready and has somebody to hit.
        ///
        /// Called after the attacker on purpose. That ordering is what gives the basic attack
        /// priority: when both are available on the same step, the blow lands first.
        /// </summary>
        public void TryStart(IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            if (!character.IsAlive || phase != Phase.Idle || abilities == null)
            {
                return;
            }

            if (character.Statuses.Has(StatusKind.Silenced))
            {
                return;
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                AbilityDefinition ability = abilities[i];

                if (ability == null || cooldowns[i] > 0f)
                {
                    continue;
                }

                // Blindness stops single target abilities and leaves areas alone.
                if (character.Statuses.Has(StatusKind.Blinded)
                    && ability.Targeting.Shape == AbilityShape.Single)
                {
                    continue;
                }

                List<Character> found = AbilityTargetResolver.Resolve(
                    character, ability.Targeting, RankOf(ability), allies, enemies);

                if (found.Count == 0)
                {
                    // A ready ability with nowhere to go holds its charge and tries again next
                    // step. It is never fired into empty space.
                    continue;
                }

                Begin(i, ability, found);
                return;
            }
        }

        private void Begin(int index, AbilityDefinition ability, List<Character> found)
        {
            castingIndex = index;
            targets = found;

            float preparation = ability.Preparation.At(RankOf(ability));

            if (preparation > 0f)
            {
                phase = Phase.Preparing;
                remaining = preparation;
                return;
            }

            // An instant ability skips straight to the casting phase, which resolves on the next
            // step through the same path every other ability takes.
            phase = Phase.Casting;
            remaining = ability.Casting.At(RankOf(ability));
        }

        private void AdvancePhase(float step, IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            remaining -= step;

            if (remaining > 0f)
            {
                return;
            }

            AbilityDefinition ability = abilities[castingIndex];

            // Whatever the phase overshot by is carried into the next one, for the same reason the
            // attack timer carries its remainder: rounding each phase up to whole steps would make
            // every ability slower than its sheet says.
            float overshoot = -remaining;

            if (phase == Phase.Preparing)
            {
                phase = Phase.Casting;
                remaining = ability.Casting.At(RankOf(ability)) - overshoot;
                overshoot = 0f;
            }

            if (phase == Phase.Casting && remaining <= 0f)
            {
                overshoot = -remaining;

                Fire(ability, allies, enemies);

                phase = Phase.Recoiling;
                remaining = ability.Recoil.At(RankOf(ability)) - overshoot;
            }

            if (phase == Phase.Recoiling && remaining <= 0f)
            {
                GoIdle();
            }
        }

        private void GoIdle()
        {
            phase = Phase.Idle;
            castingIndex = -1;
            targets = null;
            remaining = 0f;
        }

        /// <summary>
        /// The ability happens. Targets are chosen again here, because the ones picked when it
        /// started may have died during the wind up, and abilities.md says the target dying does
        /// not stop anything: the ability comes out and looks again.
        /// </summary>
        private void Fire(AbilityDefinition ability, IReadOnlyList<Character> allies, IReadOnlyList<Character> enemies)
        {
            List<Character> current = AbilityTargetResolver.Resolve(
                character, ability.Targeting, RankOf(ability), allies, enemies);

            if (current.Count > 0)
            {
                targets = current;
            }

            AbilityResolver.Apply(character, ability, RankOf(ability), targets, grid, random, RaiseDamaged);

            // The cooldown starts the moment the casting ended, which is right here, and the
            // recovery that follows is deliberately not part of it.
            cooldowns[castingIndex] = CooldownOf(ability);
        }

        /// <summary>
        /// The wait before this ability can be used again, shortened by the character's cooldown
        /// reduction.
        ///
        /// Floored at zero rather than clamping the stat. An absurd amount of reduction can only
        /// ever mean "ready immediately", never a negative wait, and no cap has to be invented for
        /// a curve that was designed not to need one.
        /// </summary>
        private float CooldownOf(AbilityDefinition ability)
        {
            float cooldown = ability.Cooldown.At(RankOf(ability));
            float reduction = character.Stats.CooldownReduction;

            return Mathf.Max(0f, cooldown * (1f - reduction / 100f));
        }

        private void CancelPreparation()
        {
            // Half the cooldown, so being silenced mid wind up costs something without costing the
            // whole recharge.
            cooldowns[castingIndex] = CooldownOf(abilities[castingIndex]) * 0.5f;

            GoIdle();
        }

        private void AdvanceCooldowns(float step)
        {
            for (int i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i] > 0f)
                {
                    cooldowns[i] -= step;
                }
            }
        }

        private void RaiseDamaged(Character user, Character target, DamageResult result)
        {
            Damaged?.Invoke(user, target, result);
        }
    }
}
