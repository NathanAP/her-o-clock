using System;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// Converte os atributos principais nos atributos secundarios, seguindo attributes.md.
    /// Toda porcentagem e devolvida na escala de 0 a 100, igual a spec.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        [Header("Atributos principais")]
        [Min(0)] public int Power;
        [Min(0)] public int Agility;
        [Min(0)] public int Specialty;
        [Min(0)] public int Constitution;

        [Header("Equipamento")]
        public EquipmentClass Equipment = EquipmentClass.Light;

        [Header("Bases")]
        [Tooltip("Ataques por segundo antes do bonus de AGI. A spec ainda nao define esse valor, entao ele e por personagem.")]
        [Min(0f)] public float BaseAttacksPerSecond = 1f;

        [Tooltip("Armadura fisica vinda de equipamento. Ainda nao existem itens, entao e preenchida na mao.")]
        [Min(0)] public int PhysicalArmor;

        /// <summary>Velocidade de movimento base de todo personagem, em casas por segundo.</summary>
        public const float BaseCellsPerSecond = 2f;

        // --- Atributos secundarios lineares ---

        public int MaxHealth
        {
            get { return Power * 5 + Constitution * 10; }
        }

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

        // --- Atributos secundarios com rendimento decrescente ---

        /// <summary>Chance de evasao, de 0 a 100. Nunca alcanca 100.</summary>
        public float EvasionChance
        {
            get { return DiminishingReturns(100f, Agility, EvasionConstant); }
        }

        /// <summary>Reducao de recarga, de 0 a 60. Nunca alcanca 60.</summary>
        public float CooldownReduction
        {
            get { return DiminishingReturns(60f, Specialty, CooldownConstant); }
        }

        /// <summary>
        /// Mitigacao fisica, de 0 a 75. A constante cresce com o nivel de quem ataca,
        /// entao a mesma armadura vale menos contra inimigos mais fortes.
        /// </summary>
        public float PhysicalMitigationAgainst(int attackerLevel)
        {
            return DiminishingReturns(75f, PhysicalArmor, 50f * Mathf.Max(1, attackerLevel));
        }

        /// <summary>
        /// Curva de rendimento decrescente da spec: Teto x Pontos / (Pontos + Constante).
        /// O teto nunca e alcancado, apenas aproximado, entao nao existe trava manual.
        /// </summary>
        public static float DiminishingReturns(float cap, float points, float constant)
        {
            if (points <= 0f || constant <= 0f)
            {
                return 0f;
            }

            return cap * points / (points + constant);
        }

        // --- Tabelas por classe de equipamento ---

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
