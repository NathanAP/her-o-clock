# Jogabilidade

## Objetivo

Explicar como a jogabilidade de Her-o-clock funciona.

## Ideia principal

- Her-o-clock é um jogo auto-scroller minúsculo, isso quer dizer que o tamanho da tela é intencionalmente pequeno para poder caber em um cantinho da tela sem atrapalhar outras tarefas do usuário.
- O jogador escolhe seus heróis e escolhe uma fase para ser iniciada.
- A partir de agora é tudo automático:
    - Os heróis andam de baixo para cima infinitamente até encontrar lacaios inimigos.
    - Os inimigos aparecem e um combate é iniciado imediatamente.
    - A batalha continuam até que um dos lados seja derrotado.
- Derrotar inimigos concede dinheiro, experiência e itens de artesanato.
- Em caso de derrota dos heróis, a fase é reiniciada automaticamente.
- A última batalha da fase é sempre contra um vilão inimigo.

## Fluxo de jogabilidade: primeira vez

Este é o fluxo quando o jogo é aberto pela primeira vez.

0 - O jogo é aberto pela primeira vez.
1 - O cenário de liberação de heróis abre.
2 - O jogador escolhe um entre os quatro heróis principais para começar.
3 - Segue a partir do fluxo principal.

## Fluxo de jogabilidade: principal

Este é o fluxo principal do jogo.

0 - O jogo está aberto.
1 - O cenário principal é carregado.
2 - As ações automáticas ocorrem em loop.
3 - A fase termina.
4 - A próxima fase começa de acordo com as atuais preferências do jogador.

## Heróis

- Jogadores podem escolher até quatro heróis por vez.
- Só podem agir enquanto estiverem vivos.
- Possuem suas próprias características, história, atributos e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Andam de baixo para cima até encontrar o próximo inimigo.
    - Ao encontrar, uma batalha é iniciada.
- Enquanto em batalha:
    - Atacam seus inimigos com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.

## Vilões

- Estão sempre presentes no fim das fases.
- Possuem suas próprias características, história, atributos e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Potencialmente podem fortalecer seus lacaios ou aplicar penalidades aos heróis.
    - Aguardam a chegada dos heróis.
- Enquanto em batalha:
    - Atacam os heróis com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.

## Lacaios

- Estão espalhados em grupos e em pontos estratégicos nas fases.
- Possuem suas próprias características, história, atributos e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Aguardam a chegada dos heróis.
- Enquanto em batalha:
    - Atacam os heróis com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.

## Combate

- O combate sempre ocorre automaticamente e não podem ser manipulados pelo jogador.

## Campo de batalha

- Os personagens se dispõe no campo de batalha conforme a ordem definida pelo jogador.
- Para os heróis o campo de batalha dispõe de: 3 posições na frente e 3 posições atrás.
    - Geralmente os personagens com mais defesa e vida ficam na frente enquanto personagens mais frágeis ficam atrás, mas isso varia de acordo com cada estratégia (no caso dos heróis) ou fase (no caso dos lacaios).
- Para os lacaios o campo de batalha dispõe de: 3 posições na frente, 3 posições no meio e 3 posições atrás.
- Vilões sempre estão no fundo da fase em uma área especial para eles e não ocupam nenhum dos 9 espaços disponíveis para lacaios.

### Escolhendo alvos

- Os personagens usam ataques básicos para atacar o inimigo que estiver mais próximo, entretanto outras fontes podem afetar essa decisão (itens, árvore de habilidades, entre outros).
- Habilidades são usadas conforme suas regras. Por exemplo:
    - Se a habilidade descrever que ela "ataca o inimigo mais distante".
    - Se a habilidade descrever que ela "ataca TODOS em uma área 2x2".
- Habilidades podem afetar aliados e inimigos conforme sua descrição. Por exemplo:
    - Se a habilidade descrever que ela "ataca TODOS em uma área 2x2" e for usada em um local com aliados, os aliados também sofrerão aquele ataque.
    - Se a habilidade descrever que ela "cura TODOS em uma área 2x2" e for usada em um local com inimigos, os inimigos também sofrerão aquela cura.
- Habilidades em área tentam afetar positivamente o máximo de aliados e o mínimo de inimigos possível.
- Habilidades em área tentam afetar negativamente o máximo de inimigos e o mínimo de aliados possível.
