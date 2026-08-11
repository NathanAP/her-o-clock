using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Common;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Monta a batalha inteira em tempo de execucao a partir dos assets de configuracao.
    ///
    /// A cena guarda apenas este componente. Todo o resto e criado por codigo de proposito,
    /// para que o arquivo da cena fique pequeno e praticamente nunca gere conflito no git.
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [Header("Configuracao")]
        [SerializeField] private BattleGridConfig gridConfig;

        [Header("Formacoes")]
        [SerializeField] private BattleFormation heroFormation;
        [SerializeField] private BattleFormation enemyFormation;

        [Header("Aparencia provisoria")]
        [Tooltip("Tamanho do quadrado do personagem em relacao a casa. Menor que 1 deixa o tabuleiro aparecer por baixo.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float characterScale = 0.8f;

        [Header("Desempenho")]
        [Tooltip("O jogo fica aberto o dia inteiro em um cantinho da tela, entao nao faz sentido gastar GPU a toa.")]
        [Min(10)]
        [SerializeField] private int targetFrameRate = 30;

        private BattleGrid grid;
        private BattleDirector director;

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;

            if (gridConfig == null)
            {
                Debug.LogError("BattleBootstrap: falta associar o Grid Config.", this);
                return;
            }

            grid = new BattleGrid(gridConfig);
            BoardRenderer.Build(grid, transform);

            List<Character> characters = new List<Character>();
            SpawnFormation(heroFormation, characters);
            SpawnFormation(enemyFormation, characters);

            if (characters.Count == 0)
            {
                Debug.LogWarning("BattleBootstrap: nenhum personagem foi criado. Confira as formacoes.", this);
                return;
            }

            if (!IsBattleValid(characters))
            {
                return;
            }

            director = gameObject.AddComponent<BattleDirector>();
            director.Begin(grid, characters);
        }

        /// <summary>
        /// Confere se a batalha consegue acontecer.
        ///
        /// Sem isso os dois erros mais comuns de configuracao nao geram nenhuma mensagem:
        /// um lado sem ninguem faz todo mundo ficar parado por nao ter inimigo, e um
        /// personagem com vida maxima 0 nasce morto e tambem fica parado.
        /// </summary>
        private bool IsBattleValid(List<Character> characters)
        {
            int heroCount = 0;
            int enemyCount = 0;
            bool valid = true;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];

                if (character.Team == Team.Heroes)
                {
                    heroCount++;
                }
                else
                {
                    enemyCount++;
                }

                if (character.Stats.MaxHealth <= 0)
                {
                    Debug.LogError("BattleBootstrap: " + character.Definition.DisplayName
                        + " tem vida maxima 0, entao nasce morto e nunca age. "
                        + "Vida maxima e Power x 5 mais Constitution x 10, e os dois estao zerados na ficha.",
                        character.Definition);
                    valid = false;
                }
            }

            if (heroCount == 0 || enemyCount == 0)
            {
                Debug.LogError("BattleBootstrap: a batalha tem " + heroCount + " heroi(s) e " + enemyCount
                    + " inimigo(s). Os dois lados precisam ter alguem, senao ninguem tem alvo e ninguem se movimenta. "
                    + "Confira o campo Team das duas formacoes: uma precisa estar em Heroes e a outra em Enemies.", this);
                valid = false;
            }

            if (valid)
            {
                Debug.Log("BattleBootstrap: batalha iniciada com " + heroCount + " heroi(s) contra " + enemyCount + " inimigo(s).", this);
            }

            return valid;
        }

        private void SpawnFormation(BattleFormation formation, List<Character> output)
        {
            if (formation == null)
            {
                Debug.LogWarning("BattleBootstrap: uma das formacoes nao foi associada.", this);
                return;
            }

            for (int i = 0; i < formation.Placements.Count; i++)
            {
                BattleFormation.Placement placement = formation.Placements[i];

                if (placement.Character == null)
                {
                    Debug.LogWarning("Formacao " + formation.name + ": a posicao " + i + " esta sem personagem.", formation);
                    continue;
                }

                GridPosition position = new GridPosition(placement.Column, placement.Row);

                if (!grid.IsInside(position))
                {
                    Debug.LogWarning("Formacao " + formation.name + ": " + placement.Character.DisplayName
                        + " esta na casa " + position + ", que fica fora do tabuleiro.", formation);
                    continue;
                }

                if (!grid.IsFree(position))
                {
                    Debug.LogWarning("Formacao " + formation.name + ": " + placement.Character.DisplayName
                        + " quer a casa " + position + ", que ja esta ocupada.", formation);
                    continue;
                }

                output.Add(CreateCharacter(placement.Character, formation.Team, position));
            }
        }

        private Character CreateCharacter(CharacterDefinition definition, Team team, GridPosition position)
        {
            GameObject instance = new GameObject(definition.DisplayName);
            instance.transform.SetParent(transform, false);

            float size = gridConfig.CellSize * characterScale;
            instance.transform.localScale = new Vector3(size, size, 1f);

            SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite.Get();
            renderer.color = definition.Color;
            renderer.sortingLayerName = "Characters";

            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, position, grid);

            return character;
        }
    }
}
