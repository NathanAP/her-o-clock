using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// Ficha de um personagem. Enquanto nao existem herois, lacaios e viloes de verdade,
    /// serve para montar personagens de teste sem escrever codigo.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Her-o-clock/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identidade")]
        public string DisplayName = "Sem nome";
        public CharacterKind Kind = CharacterKind.Hero;

        [Tooltip("Cor provisoria. Azul para herois, rosa para lacaios, vermelho para viloes.")]
        public Color Color = Color.white;

        [Header("Nivel")]
        [Tooltip("Usado pela mitigacao de quem recebe o ataque deste personagem.")]
        [Min(1)] public int Level = 1;

        [Header("Alcance do ataque basico")]
        [Tooltip("Menor distancia, em casas, que o personagem consegue atingir. 1 significa que nao existe alcance minimo.")]
        [Min(1)] public int MinRange = 1;

        [Tooltip("Maior distancia, em casas, que o personagem consegue atingir. Corpo a corpo usa 1.")]
        [Min(1)] public int MaxRange = 1;

        [Header("Atributos")]
        public CharacterStats Stats = new CharacterStats();

        private void OnValidate()
        {
            // Um alcance minimo maior que o maximo deixaria o personagem sem nenhuma
            // distancia valida, entao ele nunca conseguiria atacar.
            if (MinRange > MaxRange)
            {
                MinRange = MaxRange;
            }
        }
    }
}
