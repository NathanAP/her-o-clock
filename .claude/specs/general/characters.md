# Personagens

## Objetivo

Especificar todos os detalhes gerais sobre os personagens presentes em Her-o-clock.

## Fichas

- As fichas de cada personagem do jogo estão presentes na pasta `.claude/specs/characters`.
- O desenvolvimento do jogo deve espelhar as fichas dos personagens.
- As fichas também indicam valores de dano, redução de recarga, escala para buffs e debuffs, entre outros.
- As habilidades descritas nas fichas possuem níveis, então certos valores podem acabar mudando (o valor do dano pode aumentar a cada nível investido na habilidade). Nesses casos:
    - Valores descritos em `arrays` indicam a escalabilidade conforme o nível (index 0 = nível 1, index 1 = nível 2 e assim por diante).
        - Por exemplo: `{ ..., "duration": [10, 20, 30, 40, 50] }` indica que aquela habilidade possui uma escala conforme seu atual nível.
    - Valores descritos diretamente em numéricos indicam a escalabilidade constante, ou seja, todos os níveis daquela habilidade usam o mesmo valor.
        - Por exemplo: `{ ..., "duration": 20 }` indica que aquela habilidade possui a mesma duração em todos os níveis.
- Tudo relacionado a tempo na ficha (tempo de duração, tempo de recarga, etc) está indicado em segundos.

## Heróis

- Os heróis são os personagens usados pelo jogador.
- A principal característica de design é que todos eles são robôs, mesmo que possuam traços humanos (movimentação, expressões).
- Os heróis podem ser equipados por itens conforme o jogador escolher.

### Grupo de heróis

- Um grupo de heróis é composto de até 4 personagens e pode ser escolhido pelo jogador a qualquer momento.
- Um grupo de heróis pode ser montado do jeito que o jogador preferir.
- Durante a escolha do grupo, o jogador pode também alterar a formação inicial em campo de batalha, dispondo das 24 casas da área dos heróis (6 colunas por 4 fileiras).
    - Como o grupo possui no máximo 4 heróis, a maior parte da área fica vazia. Isso é intencional e é o que permite ao jogador escolher entre concentrar o grupo ou espalhá-lo contra ataques em área.
    - A formação define apenas onde a batalha começa. A partir dali os personagens se movimentam sozinhos, conforme descrito em `gameplay.md`.

## Lacaios

- Os lacaios são personagens que aparecem nas fases antes e/ou junto com os vilões.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades criadas pelos vilões.
- Os lacaios aparecem repetidamente em grupos pré definidos nas fases.

## Vilões

- Os vilões são personagens que aparecem ao final de cada fase sozinhos ou junto de seus lacaios.
- Vilões podem ser também chamados e considerados os "bosses" ou "chefões" do jogo.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades.
- Os vilões podem aplicar buffs aos lacaios ou debuffs aos jogadores conforme suas características.

## Nível de personagem

- Personagens possuem níveis que vão de 1 até 100.
- Quanto mais alto o nível de um personagem:
    - Mais atributos ele terá.
    - Mais habilidades ele irá possuir.
- Para subir o nível de seus heróis, o jogador deve enfrentar lacaios e vilões.
    - Muitas vezes o jogador precisará repetir a mesma fase inúmeras vezes para conseguir ficar mais forte e enfrentar a próxima fase.
- Lacaios e vilões possuem níveis de acordo com a fase em que estão presentes.
- Ao subir de nível, o jogador recebe os seguintes benefícios:
    - 5 pontos de atributos principais.
    - 1 ponto para a árvore de habilidades.

## Buffs

- Os buffs serão descritos futuramente.

## Debuffs

- Os debuffs serão descritos futuramente.
