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

### Heroi Tanque

- Id: `hero-tank`
- Display Name: `Heroi Tanque`
- Kind: `Hero`
- Color: `#3B6FD4`
- Level: `1`
- Min Range: `1`
- Max Range: `1`
- Equipment: `Heavy`
- Power: `20`, Agility: `5`, Specialty: `0`, Constitution: `30`
- Physical Armor: `100`

### Heroi Arqueiro

- Id: `hero-archer`
- Display Name: `Heroi Arqueiro`
- Kind: `Hero`
- Color: `#6FA8F0`
- Level: `1`
- Min Range: `2`
- Max Range: `4`
- Equipment: `Light`
- Power: `10`, Agility: `25`, Specialty: `0`, Constitution: `10`
- Physical Armor: `10`

O alcance mínimo `2` é o que faz este personagem recuar quando um inimigo encosta nele. É o melhor jeito de ver a regra de recuo funcionando.

### Lacaio

- Id: `minion-standard`
- Display Name: `Lacaio`
- Kind: `Minion`
- Color: `#E86FA8`
- Min Range: `1`
- Max Range: `1`
- Equipment: `Light`
- Power: `8`, Agility: `5`, Specialty: `0`, Constitution: `8`
- Physical Armor: `0`

O campo `Level` da ficha é ignorado para lacaios e vilões: quem define o nível deles é a fase em que aparecem.

### Vilao

- Id: `villain-boss`
- Display Name: `Vilao`
- Kind: `Villain`
- Color: `#C22B2B`
- Min Range: `1`
- Max Range: `2`
- Equipment: `Heavy`
- Power: `25`, Agility: `10`, Specialty: `15`, Constitution: `40`
- Physical Armor: `150`

### Campos de defesa e ofensiva

Além dos acima, toda ficha possui campos que ainda não têm fonte no jogo, pois dependem de itens e habilidades. Podem ficar em zero, ou ser preenchidos na mão para experimentar:

- Fire Resistance, Water Resistance, Electric Resistance: pontos de resistência a cada elemento. Passam pela mesma curva da armadura física.
- Thorns Percent: dano físico devolvido a quem ataca, de 0 a 100.
- Life Steal Percent: vida recuperada ao causar dano físico, de 0 a 100.

Como o ataque básico ainda é sempre físico, as três resistências elementais não têm efeito nenhum por enquanto. Espinhos e roubo de vida funcionam desde já.

## CharacterDatabase

Um único asset chamado `CharacterDatabase`, com a lista de todas as fichas.

Existe porque os arquivos de fase são JSON e não conseguem guardar referência de asset. Eles nomeiam o personagem por `Id` e este banco resolve. Os saves vão precisar exatamente da mesma coisa.

Arraste as quatro fichas para a lista `Characters`. O banco reclama no Console se alguma ficha estiver sem `Id` ou se dois `Id` forem iguais.

## StageDatabase

Um único asset chamado `StageDatabase`, com a lista dos arquivos `.json` das fases, na ordem em que devem ser jogadas.

Arraste `act1-stage1.json` e `act1-stage2.json` de `Assets/Stages/` para a lista `Stages`.

Os arquivos são referenciados como `TextAsset`, e não carregados por nome de uma pasta `Resources`, para que renomear ou mover uma fase nunca quebre a referência.

## BattleFormation

O time do jogador e onde ele começa no tabuleiro. Agora existe **apenas a formação dos heróis** — os inimigos vêm das fases.

- Team: `Heroes`
- Placements:
    - `Heroi Tanque`, Column `3`, Row `1`
    - `Heroi Tanque`, Column `4`, Row `1`
    - `Heroi Arqueiro`, Column `2`, Row `2`
    - `Heroi Arqueiro`, Column `5`, Row `2`

A mesma ficha pode aparecer várias vezes. Cada linha cria um personagem separado, com atributos próprios.

A antiga `Formacao Inimigos` deixou de ser usada e pode ser apagada. O conteúdo dela virou a primeira onda de `act1-stage1.json`.
