# Battle

## Objetivo

Único GameObject que precisa existir na cena além da câmera e da luz. Ele monta o campo de batalha inteiro em tempo de execução.

A cena guarda apenas este objeto de propósito. Todo o resto (tabuleiro, personagens, diretor do combate) é criado por código, o que mantém o arquivo `.unity` pequeno e praticamente livre de conflito no git.

## Onde fica

Na raiz da hierarquia da cena. Precisa ser criado na mão (`GameObject` → `Create Empty`) e renomeado para `Battle`.

## Transform

Todos os valores padrão. Position `(0, 0, 0)`, sem rotação, escala `1`.

Os objetos criados em tempo de execução entram como filhos dele, então mover o `Battle` move o campo de batalha inteiro.

## BattleBootstrap

Componente que precisa ser adicionado na mão (`Add Component` → `Battle Bootstrap`).

### Associações obrigatórias

- Grid Config: o asset `BattleGridConfig`.
- Hero Formation: o asset `BattleFormation` dos heróis.
- Enemy Formation: o asset `BattleFormation` dos inimigos.

Sem esses três assets nada aparece. Os assets estão descritos em `assets-de-configuracao.md`.

### Mensagens de validação

O componente confere a configuração ao iniciar e escreve no Console quando encontra problema:

- Falta um dos três assets.
- Uma formação aponta para uma casa fora do tabuleiro.
- Dois personagens disputam a mesma casa.
- Um dos lados ficou sem ninguém, quase sempre porque as duas formações ficaram com `Team` igual a `Heroes`.
- Algum personagem tem vida máxima 0, ou seja, nasce morto e nunca age.

Quando está tudo certo, ele escreve quantos heróis enfrentam quantos inimigos. Essas validações existem porque os dois últimos casos não geram erro nenhum da Unity: o jogo simplesmente fica parado com o Console limpo.

### Valores recomendados

- Character Scale: `0.8`. Deixa o quadrado do personagem um pouco menor que a casa, para o tabuleiro aparecer por baixo e as casas continuarem visíveis.
- Target Frame Rate: `30`. O jogo fica aberto o dia inteiro em um cantinho da tela, então não faz sentido gastar GPU à toa.

## BattleDirector

Adicionado automaticamente pelo `BattleBootstrap` em tempo de execução. **Não deve ser adicionado na mão.**

Ele é o único laço de atualização do combate. Os personagens não possuem `Update` próprio de propósito: com um laço só, em ordem fixa, o combate roda sempre igual para o mesmo estado inicial. Isso é o que vai permitir simular a progressão offline mais para frente sem reescrever nada.

## O que é criado em tempo de execução

Ao entrar em Play, a hierarquia abaixo do `Battle` fica assim:

- `Tabuleiro` — um filho por casa, cada um com um `SpriteRenderer` verde na sorting layer `Background`. Tons alternados deixam as casas visíveis e a área dos lacaios fica mais escura que a dos heróis.
- Um filho por personagem, nomeado com o `DisplayName` da ficha, com `SpriteRenderer` na sorting layer `Characters` e o componente `Character`.

## Sorting layers necessárias

Já configuradas em `ProjectSettings/TagManager.asset`: `Default`, `Background`, `Ground`, `Characters`, `Projectiles`, `VFX`, `UI`.

O código usa `Background` e `Characters` pelo nome. Renomear qualquer uma dessas duas quebra a renderização em silêncio.
