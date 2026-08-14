using System;
using HerOClock.Progression;
using UnityEngine;
using UnityEngine.Serialization;

namespace HerOClock.Characters
{
    /// <summary>
    /// Turns primary attributes into secondary ones, following attributes.md.
    /// Every percentage is returned on a 0 to 100 scale, matching the spec.
    ///
    /// The serialized fields are the **base** values of the sheet. What the rest of the game
    /// reads are the totals, which today are the base plus the points earned by levelling.
    /// Nothing outside this class should ever read a base value directly: when items, skill
    /// trees and buffs arrive, they become new sources inside <see cref="TotalOf"/> and no
    /// other file has to change.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        [Header("Primary attributes (base values of the sheet)")]
        [Min(0)] public int BasePower;
        [Min(0)] public int BaseAgility;
        [Min(0)] public int BaseSpecialty;
        [Min(0)] public int BaseConstitution;

        [Header("Equipment")]
        public EquipmentClass Equipment = EquipmentClass.Light;

        [Header("Defence (base values of the sheet)")]
        [Tooltip("Physical armour at the character's starting level.")]
        [FormerlySerializedAs("PhysicalArmor")]
        [Min(0)] public int BasePhysicalArmor;

        [Tooltip("Physical armour gained on every level. Setting it equal to the base keeps mitigation "
            + "against a same level attacker constant for the whole game.")]
        [Min(0)] public int PhysicalArmorPerLevel;

        [Tooltip("Resistance points against fire at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("FireResistance")]
        [Min(0)] public int BaseFireResistance;

        [Min(0)] public int FireResistancePerLevel;

        [Tooltip("Resistance points against water at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("WaterResistance")]
        [Min(0)] public int BaseWaterResistance;

        [Min(0)] public int WaterResistancePerLevel;

        [Tooltip("Resistance points against electricity at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("ElectricResistance")]
        [Min(0)] public int BaseElectricResistance;

        [Min(0)] public int ElectricResistancePerLevel;

        [Tooltip("Physical damage reflected back at the attacker, from 0 to 100. A share of the damage, "
            + "so it does not decay with level and needs no growth of its own.")]
        [Range(0f, 100f)] public float ThornsPercent;

        [Header("Offence")]
        [Tooltip("Health recovered when dealing physical damage, from 0 to 100.")]
        [Range(0f, 100f)] public float LifeStealPercent;

        [Tooltip("Health recovered per second before POW. Zero on every sheet today: regeneration is "
            + "meant to arrive with items and skills. POW multiplies whatever ends up here.")]
        [Min(0f)] public float BaseHealthRegen;

        // --- Per instance state, never part of the sheet ---

        [NonSerialized] private int level = 1;
        [NonSerialized] private AttributeGrowth growth;
        [NonSerialized] private float multiplier = 1f;
        [NonSerialized] private int[] levelPoints = new int[4];

        /// <summary>Base attack speed of every character, in attacks per second.</summary>
        public const float BaseAttacksPerSecond = 1f;

        /// <summary>Base movement speed of every character, in cells per second.</summary>
        public const float BaseCellsPerSecond = 2f;

        /// <summary>
        /// A copy of these stats, so a character instance can be levelled up and buffed
        /// without touching the shared definition asset.
        ///
        /// The level points array is rebuilt on purpose. A shallow copy would hand both
        /// characters the same array, and levelling one would silently level the other.
        /// </summary>
        public CharacterStats Clone()
        {
            CharacterStats copy = (CharacterStats)MemberwiseClone();
            copy.levelPoints = new int[4];
            Array.Copy(levelPoints, copy.levelPoints, 4);
            return copy;
        }

        /// <summary>
        /// Sets everything that belongs to this instance rather than to the sheet.
        ///
        /// The points earned by levelling are computed straight from the level instead of being
        /// accumulated one level at a time. That keeps the result free of rounding drift and lets
        /// a level 40 minion be created without walking through 39 level ups.
        /// </summary>
        public void ApplyInstance(int level, AttributeGrowth growth, float multiplier)
        {
            this.level = Mathf.Max(1, level);
            this.growth = growth;
            this.multiplier = Mathf.Max(0f, multiplier);

            AttributeGrowth.Distribute(LevelProgress.PointsAtLevel(this.level), growth, levelPoints);
        }

        /// <summary>
        /// The value of an attribute counting every source, which today means the sheet plus
        /// the points earned by levelling, scaled by the stage multiplier.
        /// </summary>
        public int TotalOf(Attribute attribute)
        {
            int total = BaseOf(attribute) + levelPoints[(int)attribute];

            if (Mathf.Approximately(multiplier, 1f))
            {
                return total;
            }

            return Mathf.Max(0, Mathf.RoundToInt(total * multiplier));
        }

        public int Power { get { return TotalOf(Attribute.Power); } }
        public int Agility { get { return TotalOf(Attribute.Agility); } }
        public int Specialty { get { return TotalOf(Attribute.Specialty); } }
        public int Constitution { get { return TotalOf(Attribute.Constitution); } }

        private int BaseOf(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.Power: return BasePower;
                case Attribute.Agility: return BaseAgility;
                case Attribute.Specialty: return BaseSpecialty;
                default: return BaseConstitution;
            }
        }

        // --- Defence ---

        /// <summary>
        /// A defensive value counting every source, the same way <see cref="TotalOf"/> does for the
        /// primary attributes. Nothing outside this class reads the base fields.
        ///
        /// Defence has to grow with the level because the mitigation curve's constant grows with
        /// the attacker's level. Left flat, the same armour is worth less every level, and a
        /// character with no source of new armour simply rots: the villain used to fall from 56%
        /// mitigation at level 1 to 4% at level 50 without anything being done to it.
        ///
        /// Growth equal to the base is the shape that holds mitigation still, because both the
        /// armour and the curve's constant then scale with the level and cancel out.
        /// </summary>
        private int DefenceOf(int baseValue, int perLevel)
        {
            int total = baseValue + perLevel * (level - 1);

            if (Mathf.Approximately(multiplier, 1f))
            {
                return Mathf.Max(0, total);
            }

            return Mathf.Max(0, Mathf.RoundToInt(total * multiplier));
        }

        public int PhysicalArmor
        {
            get { return DefenceOf(BasePhysicalArmor, PhysicalArmorPerLevel); }
        }

        public int FireResistance
        {
            get { return DefenceOf(BaseFireResistance, FireResistancePerLevel); }
        }

        public int WaterResistance
        {
            get { return DefenceOf(BaseWaterResistance, WaterResistancePerLevel); }
        }

        public int ElectricResistance
        {
            get { return DefenceOf(BaseElectricResistance, ElectricResistancePerLevel); }
        }

        // --- Linear secondary attributes ---

        public int MaxHealth
        {
            get { return Power * 5 + Constitution * 10; }
        }

        /// <summary>
        /// Health recovered per second.
        ///
        /// POW does not grant regeneration, it multiplies it, which is what "0.5% of regeneration
        /// speed per point" in attributes.md means. With nothing granting a base, this is zero for
        /// everyone today, exactly like thorns and life steal. It becomes real when items arrive.
        ///
        /// It still scales with the level, because POW does.
        /// </summary>
        public float HealthPerSecond
        {
            get { return BaseHealthRegen * (1f + Power * HealthRegenPerPower); }
        }

        /// <summary>Share of regeneration speed granted by each point of POW.</summary>
        public const float HealthRegenPerPower = 0.005f;

        public int PhysicalDamage
        {
            get { return Power; }
        }

        public int ElementalDamage
        {
            get { return Specialty; }
        }

        public float AttacksPerSecond
        {
            get { return BaseAttacksPerSecond * (1f + Agility * AttackSpeedPerAgility); }
        }

        public float CellsPerSecond
        {
            get { return BaseCellsPerSecond * (1f + Agility * MoveSpeedPerAgility); }
        }

        // --- Secondary attributes with diminishing returns ---

        /// <summary>Evasion chance, from 0 to 100. Never reaches 100.</summary>
        public float EvasionChance
        {
            get { return (float)DiminishingReturns(100.0, Agility, EvasionConstant); }
        }

        /// <summary>Cooldown reduction, from 0 to 60. Never reaches 60.</summary>
        public float CooldownReduction
        {
            get { return (float)DiminishingReturns(60.0, Specialty, CooldownConstant); }
        }

        /// <summary>
        /// Physical mitigation, from 0 to 75. The constant grows with the attacker's level,
        /// so the same armour is worth less against stronger enemies.
        /// </summary>
        public float PhysicalMitigationAgainst(int attackerLevel)
        {
            return (float)DiminishingReturns(75.0, PhysicalArmor, 50.0 * Mathf.Max(1, attackerLevel));
        }

        /// <summary>
        /// The diminishing returns curve from the spec: Cap x Points / (Points + Constant).
        /// The cap is never reached, only approached, so no manual clamp is needed anywhere.
        ///
        /// It computes in double so the damage calculation can use it without going back through
        /// float, where the runtime is free to round intermediates differently and change a
        /// result that lands on a half. This is the only place the curve is written down.
        /// </summary>
        public static double DiminishingReturns(double cap, double points, double constant)
        {
            if (points <= 0.0 || constant <= 0.0)
            {
                return 0.0;
            }

            return cap * points / (points + constant);
        }

        // --- Per equipment class tables ---

        private float AttackSpeedPerAgility
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 0.01f;
                    case EquipmentClass.Magic: return 0.005f;
                    default: return 0.002f;
                }
            }
        }

        private float MoveSpeedPerAgility
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 0.01f;
                    case EquipmentClass.Magic: return 0.0075f;
                    default: return 0.005f;
                }
            }
        }

        private float EvasionConstant
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 100f;
                    case EquipmentClass.Magic: return 200f;
                    default: return 500f;
                }
            }
        }

        private float CooldownConstant
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 60f;
                    case EquipmentClass.Magic: return 40f;
                    default: return 300f;
                }
            }
        }
    }
}
