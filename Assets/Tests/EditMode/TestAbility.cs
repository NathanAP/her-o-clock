using HerOClock.Abilities;
using HerOClock.Characters;
using HerOClock.Combat;

namespace HerOClock.Tests
{
    /// <summary>
    /// Abilities built in code, so a test can state exactly the shape it needs instead of
    /// depending on whatever the real sheets happen to hold today.
    ///
    /// The same reasoning as <see cref="TestBattle.Sheet"/>: a rule test written against real
    /// content breaks on every balance pass, and then it is protecting a number rather than a rule.
    /// </summary>
    public static class TestAbility
    {
        /// <summary>An ability with no timing at all, ready to have its targeting and effects set.</summary>
        public static AbilityDefinition Instant(string id, int ranks = 1)
        {
            return new AbilityDefinition
            {
                Id = id,
                Ranks = ranks,
                Preparation = RankedValue.Constant(0f),
                Casting = RankedValue.Constant(0f),
                Recoil = RankedValue.Constant(0f),
                Cooldown = RankedValue.Constant(0f),
                Targeting = new AbilityTargeting
                {
                    Who = AbilityWho.Enemies,
                    Shape = AbilityShape.Single,
                    Range = 8
                },
                Effects = new AbilityEffect[0]
            };
        }

        /// <summary>The test character of abilities.md: A is 1/0/0, B is 2/2/0 and C is 0/1/1.</summary>
        public static AbilityDefinition Timed(string id, float preparation, float casting, float recoil, float cooldown = 0f)
        {
            AbilityDefinition ability = Instant(id);

            ability.Preparation = RankedValue.Constant(preparation);
            ability.Casting = RankedValue.Constant(casting);
            ability.Recoil = RankedValue.Constant(recoil);
            ability.Cooldown = RankedValue.Constant(cooldown);

            return ability;
        }

        public static AbilityDefinition WithDamage(
            this AbilityDefinition ability,
            float baseDamage,
            float powerScaling = 0f,
            float falloff = 0f,
            EffectTarget target = EffectTarget.EachTarget)
        {
            return ability.With(new AbilityEffect
            {
                Type = EffectType.DealDamage,
                Target = target,
                DamageType = DamageType.Physical,
                Base = RankedValue.Constant(baseDamage),
                Scaling = new AbilityScaling { Power = powerScaling },
                Falloff = RankedValue.Constant(falloff)
            });
        }

        public static AbilityDefinition WithBuff(
            this AbilityDefinition ability,
            ModifiableStat stat,
            float value,
            float duration,
            EffectTarget target = EffectTarget.EachTarget,
            StatModifierMode mode = StatModifierMode.Percent)
        {
            return ability.With(new AbilityEffect
            {
                Type = EffectType.ModifyStat,
                Target = target,
                Stat = stat,
                Mode = mode,
                Value = RankedValue.Constant(value),
                Duration = RankedValue.Constant(duration)
            });
        }

        public static AbilityDefinition WithStatus(
            this AbilityDefinition ability,
            StatusKind status,
            float duration,
            EffectTarget target = EffectTarget.EachTarget,
            float value = 0f)
        {
            return ability.With(new AbilityEffect
            {
                Type = EffectType.ApplyStatus,
                Target = target,
                Status = status,
                Duration = RankedValue.Constant(duration),
                Value = RankedValue.Constant(value)
            });
        }

        public static AbilityDefinition WithMove(this AbilityDefinition ability)
        {
            return ability.With(new AbilityEffect
            {
                Type = EffectType.MoveTo,
                Target = EffectTarget.Self,
                Anchor = MoveAnchor.LastTargetAnySide
            });
        }

        public static AbilityDefinition With(this AbilityDefinition ability, AbilityEffect effect)
        {
            AbilityEffect[] grown = new AbilityEffect[ability.Effects.Length + 1];
            ability.Effects.CopyTo(grown, 0);
            grown[ability.Effects.Length] = effect;
            ability.Effects = grown;

            return ability;
        }

        public static AbilityDefinition Shaped(
            this AbilityDefinition ability,
            AbilityShape shape,
            AbilityWho who = AbilityWho.Enemies,
            int range = 8)
        {
            ability.Targeting.Shape = shape;
            ability.Targeting.Who = who;
            ability.Targeting.Range = range;

            return ability;
        }

        /// <summary>Gives a sheet an ability, so the character built from it can use it.</summary>
        public static CharacterDefinition Give(this CharacterDefinition sheet, AbilityDefinition ability)
        {
            sheet.Abilities.Add(ability);
            return sheet;
        }
    }
}
