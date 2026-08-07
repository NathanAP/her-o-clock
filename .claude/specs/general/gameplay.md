# Jogabilidade

## Objetivo

Explicar como a jogabilidade de Her-o-clock funciona.

## Ideia principal

- Her-o-clock é um jogo auto scroller minúsculo, isso quer dizer que o tamanho da tela é intencionalmente pequeno para poder caber em um cantinho da tela sem atrapalhar outras tarefas do usuário.
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

- Só podem agir enquanto estiverem vivos.
- Possuem suas próprias características, história, estatísticas e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Andam de baixo para cima até encontrar o próximo inimigo.
    - Ao encontrar, uma batalha é iniciada.
- Enquanto em batalha:
    - Atacam seus inimigos com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.

## Vilões

- Estão sempre presentes no fim das fases.
- Possuem suas próprias características, história, estatísticas e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Potencialmente podem fortalecer seus lacaios ou aplicar penalidades aos heróis.
    - Aguardam a chegada dos heróis.
- Enquanto em batalha:
    - Atacam os heróis com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.

## Lacaios

- Estão espalhados em grupos e em pontos estratégicos nas fases.
- Possuem suas próprias características, história, estatísticas e habilidades.

### Ações automáticas

- Enquanto fora de batalha:
    - Aguardam a chegada dos heróis.
- Enquanto em batalha:
    - Atacam os heróis com seu ataque básico.
    - Quando disponíveis usam suas habilidades automaticamente.
