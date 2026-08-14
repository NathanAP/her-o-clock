# Fases

## Objetivo

Descrever o formato dos arquivos de fase. Eles não são GameObjects nem ScriptableObjects: são arquivos `.json` em `Assets/Stages/`, importados pela Unity como `TextAsset` e apontados pelo `StageDatabase`.

## Por que JSON e não asset

Uma fase em asset é trabalho manual no editor, e cada ajuste de balanceamento é uma nova rodada de cliques. Em JSON, um ato inteiro é escrito de uma vez e revisado como diff no git.

O preço é que uma referência vira texto e um erro de digitação só apareceria rodando. Por isso todo arquivo passa pelo `StageValidator` antes da fase começar, e ele reporta todos os problemas de uma vez no Console.

## Formato

```json
{
  "id": "act1-stage1",
  "name": "Avenida das Turbinas",
  "lore": "A pequena historia desta fase.",
  "enemyLevel": 1,
  "waves": [
    {
      "placements": [
        { "character": "minion-standard", "column": 3, "row": 6 }
      ]
    }
  ],
  "villainWave": {
    "placements": [
      { "character": "villain-boss", "column": 4, "row": 8 }
    ]
  }
}
```

### Campos

- `id` — identificador estável da fase. Os saves vão se referir a ele, então não deve mudar depois de existir progresso salvo.
- `name` — nome mostrado ao jogador.
- `lore` — a pequena história da fase, contada quando ela começa. Pode ser curta.
- `enemyLevel` — nível de todos os lacaios e vilões da fase, ignorando o `Level` das fichas. É o que permite reaproveitar o mesmo lacaio em atos diferentes com forças diferentes.
- `waves` — os grupos de lacaios, enfrentados na ordem em que aparecem.
- `villainWave` — o combate final. Fica em campo próprio, e não como última entrada de `waves`, para que a estrutura obrigue a regra de que toda fase termina contra um vilão. O validador reclama se não houver ninguém do tipo `Villain` ali.

### Placements

- `character` — o `Id` da ficha, resolvido pelo `CharacterDatabase`.
- `column` e `row` — a casa onde ele começa, contando de 1.
- `multiplier` — opcional, multiplica os atributos daquele inimigo específico.
    - Vale para os atributos principais **e para as defesas** (armadura física e as três resistências), depois do crescimento por nível ter sido aplicado.
    - Omitir equivale a `1`.
    - É o ajuste fino: a ficha diz **quem** o personagem é e o nível diz **quão forte**, mas às vezes um inimigo precisa aparecer enfraquecido ou reforçado só naquela fase.
    - Serve, por exemplo, para um vilão que invoca lacaios do primeiro ato: mesma ficha, nível baixo e `"multiplier": 0.5`.
    - O validador reclama de valor negativo.

Lacaios e vilões devem ficar nas fileiras de 5 a 8, que é a área deles. A área só define onde a onda começa, os personagens se movimentam livremente depois.

## O que o validador confere

- A fase tem `id` e `enemyLevel` válido.
- Existe pelo menos uma onda de lacaios, e nenhuma onda está vazia.
- Existe uma onda de vilão, e ela contém pelo menos um personagem do tipo `Villain`.
- Todo `character` existe no `CharacterDatabase`.
- Nenhum herói foi colocado como inimigo.
- Toda casa está dentro do tabuleiro.
- Duas entradas da mesma onda não disputam a mesma casa.

A mesma casa em ondas diferentes é permitida, pois as ondas nunca coexistem.

## Fases existentes

- `act1-stage1.json` — vencível com as fichas atuais. Três ondas de nível 1 e um vilão acompanhado.
- `act1-stage2.json` — deliberadamente acima do nível do time, com inimigos de nível 12. Serve para testar a parede que justifica farmar, e vira o caso de teste da progressão.
