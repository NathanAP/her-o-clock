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

Ficha de um personagem. Enquanto não existem heróis, lacaios e vilões de verdade, servem para montar um elenco de teste.

As cores seguem o roadmap: azul para heróis, rosa para lacaios, vermelho para vilões.

### Heroi Tanque

- Display Name: `Heroi Tanque`
- Kind: `Hero`
- Color: `#3B6FD4`
- Level: `1`
- Min Range: `1`
- Max Range: `1`
- Equipment: `Heavy`
- Power: `20`, Agility: `5`, Specialty: `0`, Constitution: `30`
- Base Attacks Per Second: `0.8`
- Physical Armor: `100`

### Heroi Arqueiro

- Display Name: `Heroi Arqueiro`
- Kind: `Hero`
- Color: `#6FA8F0`
- Level: `1`
- Min Range: `2`
- Max Range: `4`
- Equipment: `Light`
- Power: `10`, Agility: `25`, Specialty: `0`, Constitution: `10`
- Base Attacks Per Second: `1.2`
- Physical Armor: `10`

O alcance mínimo `2` é o que faz este personagem recuar quando um inimigo encosta nele. É o melhor jeito de ver a regra de recuo funcionando.

### Lacaio

- Display Name: `Lacaio`
- Kind: `Minion`
- Color: `#E86FA8`
- Level: `1`
- Min Range: `1`
- Max Range: `1`
- Equipment: `Light`
- Power: `8`, Agility: `5`, Specialty: `0`, Constitution: `8`
- Base Attacks Per Second: `1`
- Physical Armor: `0`

### Vilao

- Display Name: `Vilao`
- Kind: `Villain`
- Color: `#C22B2B`
- Level: `1`
- Min Range: `1`
- Max Range: `2`
- Equipment: `Heavy`
- Power: `25`, Agility: `10`, Specialty: `15`, Constitution: `40`
- Base Attacks Per Second: `0.7`
- Physical Armor: `150`

## BattleFormation

Quem começa a batalha em qual casa. São dois assets.

O campo `Team` nasce como `Heroes` nos dois. **Uma das formações precisa ser trocada para `Enemies` na mão.** Se as duas ficarem em `Heroes`, ninguém tem inimigo, ninguém tem alvo e ninguém se movimenta. O `BattleBootstrap` detecta isso e escreve no Console.

### Formacao Herois

- Team: `Heroes`
- Placements:
    - `Heroi Tanque`, Column `3`, Row `1`
    - `Heroi Tanque`, Column `4`, Row `1`
    - `Heroi Arqueiro`, Column `2`, Row `2`
    - `Heroi Arqueiro`, Column `5`, Row `2`

O mesmo `CharacterDefinition` pode aparecer várias vezes. Cada linha cria um personagem separado.

### Formacao Inimigos

- Team: `Enemies`
- Placements:
    - `Lacaio`, Column `2`, Row `6`
    - `Lacaio`, Column `3`, Row `6`
    - `Lacaio`, Column `5`, Row `7`
    - `Vilao`, Column `4`, Row `8`

## O que essa formação demonstra

Ela foi montada de propósito para deixar visível cada regra implementada:

- Os dois tanques começam na fileira 1 com alcance 1, então precisam atravessar meio tabuleiro andando até encostar em alguém.
- Os arqueiros começam na fileira 2 com alcance de 2 a 4 casas, então param muito antes dos tanques. Isso mostra que alcance realmente muda o comportamento.
- Os lacaios vêm na direção contrária ao mesmo tempo, então os dois lados se encontram no meio.
- Quando um lacaio encosta em um arqueiro, o arqueiro recua, porque a distância ficou abaixo do alcance mínimo dele.
- Se o arqueiro ficar sem casa vazia para recuar, ele fica parado. É o personagem encurralado da spec.
