# Assets de configuração

## Objetivo

Descrever os ScriptableObjects que alimentam o `BattleBootstrap`. Eles não são GameObjects, mas sem eles a cena não monta nada.

Todos são criados pelo menu de contexto da janela Project: `Create` → `Her-o-clock` → nome do asset.

Local recomendado: `Assets/ScriptableObjects/`.

## BattleGridConfig

Dimensões do campo de batalha. Um único asset, chamado `BattleGridConfig`.

- Columns: `6`
- Rows: `8`
- Hero Rows: `4`
- Cell Size: `1`

Fileiras de 1 a 4 são a área dos heróis, de 5 a 8 a área dos lacaios e vilões, conforme `gameplay.md`.

Esses valores ficam em asset justamente para conseguirmos testar outros tamanhos sem mexer em código. Ao mudar as dimensões, os valores da `Main Camera` precisam ser recalculados junto.

## CharacterDefinition

Ficha de um personagem. É o molde imutável: história, design, cor e habilidades. O nível, os atributos e futuramente o equipamento pertencem à **instância** criada em campo, não à ficha.

O campo `Id` é obrigatório e é por ele que os arquivos de fase e, no futuro, os saves apontam para a ficha. **Nunca mude um `Id` depois que existir conteúdo referindo-se a ele.**

As cores seguem o roadmap: azul para heróis, rosa para lacaios, vermelho para vilões.

A ficha **não guarda nenhum texto que o jogador leia**. O nome mostrado vem do arquivo de strings, na chave `character.{id}.name`. Ver "## Arquivo de strings" no fim deste arquivo.

### Heroi Tanque

- Id: `hero-tank`
- Kind: `Hero`
- Color: `#3B6FD4`
- Level: `1`, Max Level: `100`
- Min Range: `1`, Max Range: `1`
- Equipment: `Heavy`
- Base Power: `5`, Base Agility: `2`, Base Specialty: `1`, Base Constitution: `4`
- Base Physical Armor: `100`, Physical Armor Per Level: `100`

### Heroi Arqueiro

- Id: `hero-archer`
- Kind: `Hero`
- Color: `#6FA8F0`
- Level: `1`, Max Level: `100`
- Min Range: `2`, Max Range: `4`
- Equipment: `Light`
- Base Power: `4`, Base Agility: `5`, Base Specialty: `1`, Base Constitution: `2`
- Base Physical Armor: `10`, Physical Armor Per Level: `10`

O alcance mínimo `2` é o que faz este personagem recuar quando um inimigo encosta nele. É o melhor jeito de ver a regra de recuo funcionando.

### Lacaio

- Id: `minion-standard`
- Kind: `Minion`
- Color: `#E86FA8`
- Level: `1`, Max Level: `100`
- Min Range: `1`, Max Range: `1`
- Equipment: `Light`
- Base Power: `2`, Base Agility: `1`, Base Specialty: `0`, Base Constitution: `3`
- Sem armadura nenhuma. É um civil sob controle de alguém, não um soldado.

O campo `Level` da ficha é ignorado para lacaios e vilões: quem define o nível deles é a fase em que aparecem.

### Vilao

- Id: `villain-boss`
- Kind: `Villain`
- Color: `#C22B2B`
- Level: `1`, Max Level: `100`
- Min Range: `1`, Max Range: `2`
- Equipment: `Heavy`
- Base Power: `3`, Base Agility: `2`, Base Specialty: `0`, Base Constitution: `4`
- Base Physical Armor: `150`, Physical Armor Per Level: `150`

### Crescimento de defesa por nível

Toda ficha declara, ao lado do valor inicial de armadura e de cada resistência, **quanto aquele atributo ganha por nível**. Sem isso a defesa apodrece sozinha, porque a constante da curva de mitigação cresce com o nível do atacante.

**Ganho igual ao valor inicial mantém a mitigação parada para sempre**, pois a armadura e a constante passam a crescer juntas e se cancelam. É o que as quatro fichas usam, e é por isso que os números de nível 1 delas continuam valendo em qualquer nível:

| Ficha | Armadura inicial | Ganho por nível | Mitigação em qualquer nível |
|---|---|---|---|
| Heroi Tanque | 100 | 100 | 50,0% |
| Heroi Arqueiro | 10 | 10 | 12,5% |
| Lacaio | 0 | 0 | 0% |
| Vilao | 150 | 150 | 56,3% |

Ganhos menores que a base fazem a defesa perder força devagar, e maiores fazem ganhar. As duas coisas são escolhas válidas.

A armadura nas fichas de **herói** é um substituto de equipamento, já que itens ainda não existem. Quando existirem, ela deve ir a zero e o equipamento assumir.

### Crescimento por nível

Toda ficha precisa declarar como o personagem gasta os **5 pontos que recebe a cada nível**, em porcentagens que somam 100. O campo fica logo abaixo do nível.

Para heróis é a distribuição padrão, que o jogador vai poder substituir quando existir interface. Para lacaios e vilões é a **única** forma que eles têm de ficar mais fortes, já que ninguém distribui pontos por eles.

Sugestões para as fichas atuais:

| Ficha | POW | AGI | SPE | CON |
|---|---|---|---|---|
| Heroi Tanque | 30 | 5 | 0 | 65 |
| Heroi Arqueiro | 35 | 45 | 0 | 20 |
| Lacaio | 40 | 20 | 0 | 40 |
| Vilao | 35 | 15 | 15 | 35 |

O campo `Max Level` fica em `100` para heróis. Para lacaios e vilões ele pode ser menor, se você quiser que aquele inimigo não exista acima de certo nível.

O Console avisa se as porcentagens não somarem 100. Elas continuam funcionando (são normalizadas), mas quase sempre é erro de digitação.

### Campos de defesa e ofensiva

Além dos acima, toda ficha possui campos que ainda não têm fonte no jogo, pois dependem de itens e habilidades. Podem ficar em zero, ou ser preenchidos na mão para experimentar:

- Fire Resistance, Water Resistance, Electric Resistance: pontos de resistência a cada elemento. Passam pela mesma curva da armadura física.
- Thorns Percent: dano físico devolvido a quem ataca, de 0 a 100.
- Life Steal Percent: vida recuperada ao causar dano físico, de 0 a 100.

Como o ataque básico ainda é sempre físico, as três resistências elementais não têm efeito nenhum por enquanto. Espinhos e roubo de vida funcionam desde já.

### Tipo de ataque básico

O campo `Auto Attack` diz se o ataque básico desenha um projétil (`Ranged`) ou não (`Melee`). Ele é **puramente visual** e não muda regra nenhuma: quem decide alcance são o `Min Range` e o `Max Range`.

Um personagem de alcance longo com ataque `Melee` bate de longe sem nada voando, e isso é uma combinação válida de se querer. Das quatro fichas de teste, só o `Hero Archer Def` é `Ranged`.

### Lista de habilidades

A lista `Abilities` da ficha está **vazia de propósito nas quatro fichas de teste**. O sistema funciona desde a 0.7.0.0, e ele só aparece no jogo rodando quando a 0.9.0.0 trouxer os personagens de verdade.

Ao preencher uma habilidade à mão no Inspector, dois campos carregam regra e passam despercebidos:

- **Falloff**, dentro de um efeito de dano: o quanto o dano perde a cada casa de distância de quem usou. Fica em `0` quando a habilidade não quer isso, e aí o dano é o mesmo em qualquer distância. Ele é recusado nas formas `Self` e `Single`, onde todo alvo está sempre à mesma distância.
- **Target igual a Self em um efeito de dano**: é assim que se escreve o custo de vida de uma habilidade. Esse dano nunca mata quem a usou, ele para em 1 de vida.

Todo campo numérico é um `RankedValue`, ou seja, uma lista. **Uma entrada** significa constante em todos os ranks, e **uma por rank** significa que o valor escala. Qualquer outro tamanho é recusado pelo validador, e é o erro mais fácil de cometer no Inspector, porque a lista cresce com um clique e ninguém confere o tamanho depois.

## CharacterDatabase

Um único asset chamado `CharacterDatabase`, com a lista de todas as fichas.

Existe porque os arquivos de fase são JSON e não conseguem guardar referência de asset. Eles nomeiam o personagem por `Id` e este banco resolve. Os saves vão precisar exatamente da mesma coisa.

Arraste as quatro fichas para a lista `Characters`. O teste `ContentTests` reprova se alguma ficha do projeto ficar de fora. O banco reclama no Console se alguma ficha estiver sem `Id` ou se dois `Id` forem iguais.

## StageDatabase

Um único asset chamado `StageDatabase`, com a lista dos arquivos `.json` das fases, na ordem em que devem ser jogadas.

Arraste `act1-stage1.json` e `act1-stage2.json` de `Assets/Stages/` para a lista `Stages`.

Os arquivos são referenciados como `TextAsset`, e não carregados por nome de uma pasta `Resources`, para que renomear ou mover uma fase nunca quebre a referência.

## BattleFormation

O time do jogador e onde ele começa no tabuleiro. Agora existe **apenas a formação dos heróis** — os inimigos vêm das fases.

- Team: `Heroes`
- Placements:
    - `Heroi Tanque`, Column `3`, Row `2`
    - `Heroi Tanque`, Column `4`, Row `2`
    - `Heroi Arqueiro`, Column `2`, Row `1`
    - `Heroi Arqueiro`, Column `5`, Row `1`

A fileira 1 é a mais recuada, então os tanques na fileira 2 ficam **à frente** dos arqueiros.

A mesma ficha pode aparecer várias vezes. Cada linha cria um personagem separado, com atributos próprios.

A antiga `Formacao Inimigos` deixou de ser usada e pode ser apagada. O conteúdo dela virou a primeira onda de `act1-stage1.json`.

## Arquivo de strings

`Assets/Strings/en.json`, apontado pelo campo `Strings File` do `BattleBootstrap`.

Ele guarda **todo** texto que o jogador lê, e as chaves são montadas a partir do `Id` que o conteúdo já carrega:

- `character.{id}.name` — o nome mostrado de uma ficha.
- `stage.{id}.name` e `stage.{id}.lore` — o nome e a pequena história de uma fase.

Não existe nada para manter em sincronia: renomear um `Id` é o mesmo ato que renomear o texto dele.

O `BattleBootstrap` confere o arquivo ao iniciar e reclama no Console de chave faltando **e** de texto sobrando, que é o de conteúdo apagado. Uma chave que não existe aparece na tela como `#chave#`, e não como vazio, porque um rótulo que some sem deixar rastro é muito mais difícil de notar.
