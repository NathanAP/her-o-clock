# Personagens

## Objetivo

Especificar todos os detalhes gerais sobre os personagens presentes em Her-o-clock.

## Heróis

- Os heróis são os personagens usados pelo jogador.
- A principal característica de design é que todos eles são robôs, mesmo que possuam traços humanos (movimentação, expressões).
- Os heróis podem ser equipados por itens conforme o jogador escolher.
- Cada herói possui suas particularidades que estão descritas em suas fichas presente na pasta `.claude/specs/characters/heroes`.

### Grupo de heróis

- Um grupo de heróis é composto de até 4 personagens e podem ser escolhidos pelo jogador a qualquer momento.
- Um grupo de heróis pode ser montado do jeito que o jogador preferir.
- Durante a escolha do grupo, o jogador pode também alterar a formação em campo de batalha, dispondo de 6 posições (3 à frente e 3 atrás).

## Lacaios

- Os lacaios são personagens que aparecem nas fases antes e/ou junto com os vilões.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades criadas pelos vilões.
- Os lacaios aparecem repetidamente em grupos pré definidos nas fases.
- Cada lacaio possui suas particularidades que estão descritas em suas fichas presente na pasta `.claude/specs/characters/minions`.

## Vilões

- Os vilões são personagens que aparecem ao final de cada fase sozinhos ou junto de seus lacaios.
- Vilões podem ser também chamados e considerados os "bosses" ou "chefões" do jogo.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades.
- Os vilões podem aplicar buffs aos lacaios ou debuffs aos jogadores conforme suas características.
- Cada lacaio possui suas particularidades que estão descritas em suas fichas presente na pasta `.claude/specs/characters/villains`.

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
    - 1 ponto para a árvore de progresso.

## Atributos principais

- Os atributos principais estão presentes em todos os personagens.
- Cada herói possui uma base de atributos principais.
- Os jogadores escolhem quais atributos principais desejam aumentar cada vez que um herói sobe de nível.
    - Cabe ao jogador decidir como ele prefere fazer a distribuição.
    - Os pontos de atributos ganhos nos níveis podem ser redefinidos a qualquer momento.
- Os jogadores podem aumentar as atributos principais através de outros meios (itens, habilidades, árvore de progresso).

### Atributo principal: Health Point (HP)

- HP indica a vida máxima do personagem.
    - Para cada 1 ponto de HP, o personagem ganha 10 pontos de vida máxima.
- A vida atual nunca pode ultrapassar a vida máxima.
- A vida mínima é sempre 0.
- Enquanto a vida atual estiver acima de 0, o personagem é considerado vivo.
- Enquanto a vida atual estiver em 0, o personagem é considerado morto.
- É possível burlar a morte através de habilidades ou buffs.

### Atributo principal: Poder (POW)

- POW aumenta ataques físicos e vida máxima do personagem.
    - Para cada 1 ponto de POW, o personagem ganha 1 ponto de dano físico.
    - Para cada 1 ponto de POW, o personagem ganha 5 pontos de vida máxima.
    - Para cada 1 ponto de POW, o personagem ganha 0.5% de velocidade de regeneração de vida.
- Personagens com mais POW são capazes de utilizar equipamentos e armaduras mais pesados.

### Atributo principal: Agilidade (AGI)

- AGI aumenta evasão e velocidade de ataque básico do personagem
    - Para cada 1 ponto de AGI, o personagem ganha 1% de velocidade de ataque quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 1% de chance de evasão quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 0.2% de velocidade de ataque quando estiver utilizando equipamentos pesados.
    - Para cada 1 ponto de AGI, o personagem ganha 0.2% de chance de evasão quando estiver utilizando equipamentos pesados.
- Personagens com mais AGI são capazes de utilizar equipamentos e armaduras mais ágeis.

### Atributo principal: Especialidade (SPE)

- INT aumenta ataques elementais e redução de recarga do personagem.
    - Para cada 1 ponto de INT, o personagem ganha 1 ponto de dano elemental.
    - Para cada 1 ponto de INT, o personagem ganha 1% de redução de recarga enquanto estiver utilizando equipamentos leves.
    - Para cada 1 ponto de INT, o personagem ganha 0.2% de redução de recarga enquanto estiver utilizando equipamentos pesados.
- Personagens com mais INT são capazes de utilizar equipamentos e armaduras especiais.

## Atributos secundários

- Os atributos secundários estão presentes em alguns personagens, mas não necessariamente em todos eles.
- Os atributos secundários não podem ser aumentados diretamente com pontos de atributos.
- Os jogadores podem aumentar as atributos secundários através de outros meios (itens, habilidades, árvore de progresso).

### Dano físico

### Dano elemental

### Regeneração de vida

### Evasão

### Velocidade de ataque

### Redução de recarga

## Buffs

## Debuffs
