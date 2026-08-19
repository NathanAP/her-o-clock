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
  "enemyLevel": 1,
  "allies": [
    { "character": "gadrat-npc", "column": 4, "row": 3 }
  ],
  "waves": [
    {
      "placements": [
        { "character": "discarded-prototype", "column": 3, "row": 6 }
      ]
    }
  ],
  "villainWave": {
    "placements": [
      { "character": "exposed-prototype", "column": 4, "row": 8 }
    ]
  }
}
```

### Campos

- `id` — identificador estável da fase. Os saves vão se referir a ele, então não deve mudar depois de existir progresso salvo.
- `heroLimit` — quantos heróis esta fase aceita, contados a partir do começo da equipe escalada.
    - É regra **da fase**, não do time: repetir uma fase antiga com a equipe cheia continua entrando com o limite dela.
    - Omitir significa sem limite, e a equipe inteira entra.
- `firstClear` — o que a fase entrega **na primeira vez** que é completada, e nunca de novo.
    - `unlocksCharacter` — o id de um herói que passa a ser possuído pelo jogador.
    - `grantsTeamSlots` — quantas posições novas a equipe ganha.
    - Os dois são campos separados de propósito: herói liberado e slot liberado costumam acontecer juntos e **não são a mesma coisa**.
- `allies` — opcional, os NPCs que lutam ao lado do grupo nesta fase.
    - Eles são posicionados uma vez, no começo, e **ficam durante todas as ondas**, como os heróis. Por isso não moram dentro de uma onda.
    - Precisam ser do tipo `npc` e começar nas fileiras dos heróis. O validador recusa qualquer outra coisa.
    - Um NPC nunca conta para a derrota: a fase é perdida quando o grupo do jogador cai.
- O nome e a pequena história da fase **não ficam neste arquivo**. Eles moram no arquivo de strings, em `stage.{id}.name` e `stage.{id}.lore`.
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
- `level` — opcional, o nível daquele inimigo específico, ignorando o `enemyLevel` da fase.
    - Serve para um inimigo que precisa ser mais fraco ou mais forte que o resto da onda por motivo de história, e não de balanceamento fino.
    - Omitir faz ele usar o `enemyLevel` da fase, que é o caso normal.
    - Ele é diferente do `multiplier`: o nível muda a mitigação, a experiência concedida e o crescimento inteiro do personagem, enquanto o multiplicador só escala os atributos já calculados.
- `startingHealthPercent` — opcional, com quanto de vida aquele personagem entra em campo, em porcentagem da vida máxima dele.
    - Serve para um personagem que chega ferido, sem que a vida máxima dele mude. É diferente de reduzir a vida máxima: ele continua sendo quem é, ele só já apanhou.
    - Omitir equivale a `100`.
    - O validador reclama de valor fora da faixa de 1 a 100. Zero seria um personagem que nasce morto, e isso é sempre erro de digitação.

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

**As duas fases que existem hoje são casos de teste, não conteúdo.** Elas foram escritas na 0.3.0.0 para exercitar o sistema de fases, e nenhum número delas é alvo de balanceamento. O conteúdo de verdade do ato 1 nasce na 0.9.0.0.

- `act1-stage1.json` — vencível com as fichas atuais. Três ondas de nível 1 e um vilão acompanhado. Serve como o caso feliz: uma fase que o time limpa do começo ao fim.
- `act1-stage2.json` — deliberadamente muito acima do nível do time, com inimigos de nível 12. Serve para exercitar **herói caindo em combate e avanço bloqueado**, que é o único jeito de testar esses caminhos enquanto não existe conteúdo real.
    - O `enemyLevel: 12` é extremo de propósito. Ele **não** é uma estimativa do que a segunda fase de um ato deveria pedir.
    - Por isso o snapshot mostra uma parede enorme entre as duas. É o valor esperado deste par de fixtures, e não um problema a resolver.
