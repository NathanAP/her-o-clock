using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The battery that runs per ability, checking that a sheet says something the game can carry
    /// out.
    ///
    /// Every mistake here is **silent** in the game: a short scaling array makes the last rank read
    /// a value that does not exist, a priority on a shape that chooses nobody does nothing at all,
    /// and a chain with no jump range reaches one target and looks like a balance problem. None of
    /// them throws, and none of them appears in the Console.
    ///
    /// This is also the layer that does not age. It checks whether an ability is **writable**, not
    /// whether its numbers are good, so a balance pass never breaks it.
    /// </summary>
    public class AbilityValidatorTests
    {
        private static AbilityDefinition Sound()
        {
            return TestAbility.Timed("sound", 0f, 0.5f, 0f, 10f).WithDamage(20f);
        }

        [Test]
        public void AWellWrittenAbilityHasNothingToReport()
        {
            Assert.IsEmpty(AbilityValidator.Validate(Sound()));
        }

        // --- The rule the spec names its own failure for ---

        /// <summary>
        /// abilities.md says every array has to be exactly as long as the ability has ranks, and
        /// spells out why: one entry short and the last rank reads a value that does not exist,
        /// with no error anywhere.
        /// </summary>
        [Test]
        public void AnArrayOneEntryShortIsRefused()
        {
            AbilityDefinition ability = Sound();
            ability.Ranks = 5;
            ability.Cooldown = new RankedValue(30f, 28f, 25f, 22f);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void OneEntryPerRankIsAccepted()
        {
            AbilityDefinition ability = TestAbility.Timed("scaling", 0f, 0.5f, 0f, 10f);
            ability.Ranks = 5;
            ability.Cooldown = new RankedValue(30f, 28f, 25f, 22f, 20f);
            ability.WithDamage(0f);
            ability.Effects[0].Base = new RankedValue(50f, 60f, 70f, 80f, 90f);

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void ASingleEntryIsAcceptedAtAnyNumberOfRanks()
        {
            AbilityDefinition ability = Sound();
            ability.Ranks = 5;

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AMissingTimeIsRefused()
        {
            AbilityDefinition ability = Sound();
            ability.Cooldown = new RankedValue();

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        // --- Fields that do not fit the shape ---

        /// <summary>
        /// A priority on a shape that chooses nobody would be read without error and do nothing.
        /// That is exactly the silence abilities.md asked to be refused rather than ignored.
        /// </summary>
        [Test]
        public void APriorityOnAShapeThatChoosesNobodyIsRefused()
        {
            AbilityDefinition ability = Sound().Shaped(AbilityShape.Area);
            ability.Targeting.Priority = TargetPriority.Farthest;
            ability.Targeting.AreaColumns = 3;
            ability.Targeting.AreaRows = 3;

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void APriorityOnAShapeThatDoesChooseIsAccepted()
        {
            AbilityDefinition ability = Sound();
            ability.Targeting.Priority = TargetPriority.LowestHealthPercent;

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AChainWithoutAJumpRangeIsRefused()
        {
            AbilityDefinition ability = Sound().Shaped(AbilityShape.Chain);
            ability.Targeting.MaxTargets = RankedValue.Constant(3f);
            ability.Targeting.JumpRange = 0;

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AChainWithoutAMaximumNumberOfTargetsIsRefused()
        {
            AbilityDefinition ability = Sound().Shaped(AbilityShape.Chain);
            ability.Targeting.JumpRange = 2;
            ability.Targeting.MaxTargets = new RankedValue();

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AnAreaWithoutBothSidesIsRefused()
        {
            AbilityDefinition ability = Sound().Shaped(AbilityShape.Area);
            ability.Targeting.AreaColumns = 0;
            ability.Targeting.AreaRows = 3;

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void ARangeOfZeroIsRefusedForAnythingButSelf()
        {
            AbilityDefinition ability = Sound();
            ability.Targeting.Range = 0;

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void TargetingSelfWithAnotherShapeIsRefused()
        {
            AbilityDefinition ability = Sound().Shaped(AbilityShape.Single, AbilityWho.Self);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>A blink anchored on the last target needs there to be one.</summary>
        [Test]
        public void RepositioningOnASelfShapedAbilityIsRefused()
        {
            AbilityDefinition ability = TestAbility.Timed("blink", 0f, 0f, 0f, 5f)
                .Shaped(AbilityShape.Self, AbilityWho.Self)
                .WithMove();

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        // --- Basics ---

        [Test]
        public void AnAbilityWithNoEffectsIsRefused()
        {
            AbilityDefinition ability = TestAbility.Timed("empty", 0f, 0f, 0f, 5f);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AnAbilityWithNoIdIsRefused()
        {
            AbilityDefinition ability = Sound();
            ability.Id = "";

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void ABuffWithNoDurationIsRefused()
        {
            AbilityDefinition ability = TestAbility.Timed("haste", 0f, 0f, 0f, 5f)
                .WithBuff(ModifiableStat.AttackSpeed, 20f, 0f);

            ability.Effects[0].Duration = new RankedValue();

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void BlindnessWithoutAMissChanceIsRefused()
        {
            AbilityDefinition ability = TestAbility.Timed("blind", 0f, 0f, 0f, 5f)
                .WithStatus(StatusKind.Blinded, 3f);

            ability.Effects[0].Value = new RankedValue();

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        // --- A whole sheet ---

        [Test]
        public void TwoAbilitiesWithTheSameIdOnOneSheetAreRefused()
        {
            List<AbilityDefinition> abilities = new List<AbilityDefinition> { Sound(), Sound() };

            Assert.IsNotEmpty(AbilityValidator.Validate(abilities, "some-hero"));
        }

        [Test]
        public void EveryProblemNamesTheCharacterItCameFrom()
        {
            AbilityDefinition broken = Sound();
            broken.Targeting.Range = 0;

            List<string> problems = AbilityValidator.Validate(
                new List<AbilityDefinition> { broken }, "some-hero");

            Assert.IsNotEmpty(problems);
            StringAssert.StartsWith("some-hero", problems[0]);
        }

        // --- The falloff ---

        [Test]
        public void AFalloffOnAShapeThatReachesSeveralDistancesIsAccepted()
        {
            AbilityDefinition ability = TestAbility.Timed("breath", 1f, 1f, 0.5f, 30f)
                .Shaped(AbilityShape.Line)
                .WithDamage(100f, falloff: 0.15f);

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>
        /// On these two shapes every target sits at the same distance, so a falloff would be read
        /// without any error and change nothing at all. Same silent failure as a priority on a
        /// shape that chooses nobody, and it gets the same treatment.
        /// </summary>
        [TestCase(AbilityShape.Self)]
        [TestCase(AbilityShape.Single)]
        public void AFalloffOnAShapeWithOnlyOneDistanceIsRefused(AbilityShape shape)
        {
            AbilityDefinition ability = TestAbility.Timed("breath", 0f, 1f, 0f, 30f)
                .Shaped(shape, shape == AbilityShape.Self ? AbilityWho.Self : AbilityWho.Enemies)
                .WithDamage(100f, falloff: 0.15f);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>An explicit zero is not a falloff, so it must not be flagged on any shape.</summary>
        [Test]
        public void AZeroFalloffIsAcceptedEverywhere()
        {
            AbilityDefinition ability = TestAbility.Timed("jab", 0f, 1f, 0f, 30f)
                .Shaped(AbilityShape.Single)
                .WithDamage(100f, falloff: 0f);

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>
        /// It is a fraction of the damage lost per cell. A full 1 would leave every cell but the
        /// adjacent one at zero, which is a range of 1 written the hard way.
        /// </summary>
        [TestCase(-0.1f)]
        [TestCase(1f)]
        [TestCase(1.5f)]
        public void AFalloffOutsideZeroToOneIsRefused(float falloff)
        {
            AbilityDefinition ability = TestAbility.Timed("breath", 0f, 1f, 0f, 30f)
                .Shaped(AbilityShape.Line)
                .WithDamage(100f, falloff: falloff);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>
        /// The falloff scales with the rank like every other number, so its array answers to the
        /// same rule: one entry for a constant, or exactly one per rank.
        /// </summary>
        [Test]
        public void AFalloffArrayOneEntryShortIsRefused()
        {
            AbilityDefinition ability = TestAbility.Timed("breath", 0f, 1f, 0f, 30f)
                .Shaped(AbilityShape.Line)
                .WithDamage(100f);

            ability.Ranks = 5;
            ability.Cooldown = new RankedValue(35f, 30f, 25f, 20f, 15f);
            ability.Effects[0].Base = new RankedValue(60f, 75f, 90f, 105f, 120f);
            ability.Effects[0].Falloff = new RankedValue(0.15f, 0.14f, 0.13f, 0.12f);

            Assert.IsNotEmpty(AbilityValidator.Validate(ability));
        }

        [Test]
        public void AFalloffWithOneEntryPerRankIsAccepted()
        {
            AbilityDefinition ability = TestAbility.Timed("breath", 0f, 1f, 0f, 30f)
                .Shaped(AbilityShape.Line)
                .WithDamage(100f);

            ability.Ranks = 5;
            ability.Cooldown = new RankedValue(35f, 30f, 25f, 20f, 15f);
            ability.Effects[0].Base = new RankedValue(60f, 75f, 90f, 105f, 120f);
            ability.Effects[0].Falloff = new RankedValue(0.15f, 0.14f, 0.13f, 0.12f, 0.11f);

            Assert.IsEmpty(AbilityValidator.Validate(ability));
        }

        /// <summary>
        /// Every sheet the game actually ships with has to pass. This is the test that turns the
        /// validator from a library into a guarantee, and it is the one that will catch a real
        /// ability being written wrong in 0.9.0.0.
        /// </summary>
        [Test]
        public void EveryAbilityOnEveryRealSheetIsWritable()
        {
            List<string> problems = new List<string>();

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterDefinition");

            for (int i = 0; i < guids.Length; i++)
            {
                CharacterDefinition sheet = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));

                if (sheet != null)
                {
                    problems.AddRange(AbilityValidator.Validate(sheet.Abilities, sheet.Id));
                }
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
