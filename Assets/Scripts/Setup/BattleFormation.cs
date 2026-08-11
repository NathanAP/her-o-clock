using System;
using System.Collections.Generic;
using HerOClock.Characters;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Quem comeca a batalha em qual casa.
    ///
    /// Para os herois isso e a formacao escolhida pelo jogador. Para os lacaios e
    /// viloes e o grupo pre definido da fase. A formacao define apenas onde a batalha
    /// comeca, nao limita a movimentacao depois.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleFormation", menuName = "Her-o-clock/Battle Formation")]
    public class BattleFormation : ScriptableObject
    {
        [Serializable]
        public struct Placement
        {
            public CharacterDefinition Character;

            [Min(1)] public int Column;
            [Min(1)] public int Row;
        }

        [Tooltip("Lado do combate. Herois de um lado, lacaios e viloes do outro.")]
        public Team Team = Team.Heroes;

        public List<Placement> Placements = new List<Placement>();
    }
}
