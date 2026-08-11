# Jogabilidade

## Objetivo

Explicar como a jogabilidade de Her-o-clock funciona.

## Ideia principal

- Her-o-clock é um jogo auto-scroller minúsculo, isso quer dizer que o tamanho da tela é intencionalmente pequeno para poder caber em um cantinho da tela sem atrapalhar outras tarefas do usuário.
- O jogador escolhe seus heróis e escolhe uma fase para ser iniciada.
- A partir de agora é tudo automático:
    - Os heróis andam de baixo para cima infinitamente até encontrar lacaios inimigos.
    - Os inimigos aparecem e um combate é iniciado imediatamente.
    - A batalha continua até que um dos lados seja derrotado.
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

- O combate sempre ocorre automaticamente e não pode ser manipulado pelo jogador.
- O combate começa imediatamente quando os heróis encontram um inimigo.
- O combate termina quando a equipe de heróis, lacaios ou vilões ficar sem membros vivos.
- Ao vencer um combate contra lacaios, os heróis se movem de volta às suas posições originais e continuam até o próximo combate.
- Ao vencer um combate contra vilões, os heróis comemoram sua vitória.

## Campo de batalha

- O campo de batalha é um tabuleiro único de 6 colunas por 12 fileiras, totalizando 72 casas, sempre visto de baixo para cima.
    - As fileiras de 1 a 6 formam a área dos heróis, com 36 casas no total.
    - As fileiras de 7 a 12 formam a área dos lacaios e vilões, com 36 casas no total.
- As duas áreas definem apenas onde cada lado começa a batalha. Elas não limitam para onde os personagens podem ir depois que a batalha começa.
- Os personagens se dispõem no campo de batalha conforme a ordem definida pelo jogador.
    - Geralmente os personagens com mais defesa e vida ficam na frente enquanto personagens mais frágeis ficam atrás, mas isso varia de acordo com cada estratégia (no caso dos heróis) ou fase (no caso dos lacaios).
- A partir do momento que a batalha começa, os personagens se movimentam automaticamente para atacar o seu adversário alvo, mesmo que signifique invadir o espaço adversário.
- Heróis e lacaios geralmente ocupam uma casa, mas habilidades, buffs ou debuffs podem mudar isso.
- Vilões acabam tendo mais variações de ocupação de casas.

### Distância

- A distância entre duas casas é contada em casas, como em um tabuleiro de xadrez.
- A contagem considera as oito casas ao redor como estando à distância 1, incluindo as diagonais.
    - Ou seja, a distância entre duas casas é a maior diferença entre as fileiras e as colunas delas.
    - Por exemplo, a distância entre a casa da fileira 3 coluna 2 e a casa da fileira 4 coluna 1 é 1.
- Contar a diagonal como distância 1 faz com que um personagem na fileira da frente alcance as três casas inimigas à sua frente. Sem isso, cada coluna viraria um corredor isolado e a formação perderia quase todo o significado.

### Alcance

- Alcance é a faixa de distância, em casas, que um personagem consegue atingir com seu ataque básico.
    - O alcance máximo é a maior distância que o personagem consegue atingir e todo personagem possui um.
    - O alcance mínimo é a menor distância que o personagem consegue atingir e a maior parte dos personagens não possui um.
    - Um inimigo só está dentro do alcance quando a distância até ele respeita as duas pontas ao mesmo tempo.
- Alcance é uma característica do personagem e pode ser alterada por outros meios (itens, árvore de habilidades, habilidades, buffs e debuffs).
- Personagens corpo a corpo possuem alcance máximo 1 e só atingem inimigos nas casas vizinhas.
- Personagens à distância possuem alcance máximo maior e conseguem atingir inimigos por cima das fileiras da frente.
- Certas armas dependem exclusivamente de uma distância mínima (por exemplo, um arco ou uma sniper), nesse caso, o personagem precisa se movimentar para conseguir realizar seu ataque.
    - Perceba que um personagem pode acabar encurralado pelo seu alvo. Nesse caso, o ideal é ele buscar um meio viável para poder voltar a atacar.
    - Nessa mesma situação, o personagem encurralado pode estar atacando outro alvo. Nesse caso, o ataque dele continua normalmente.
- Se um personagem não possui nenhum alvo válido dentro do seu alcance, ele avança uma casa na direção do inimigo mais próximo e tenta novamente.
    - O avanço só acontece para casas vazias.
    - Avançar custa tempo e portanto custa dano, então uma formação mal montada é punida naturalmente.
    - Essa regra também é o que permite que personagens corpo a corpo alcancem o vilão depois que os lacaios da frente morrem.
- Se um personagem não possui alvo válido e também não consegue avançar, ele apenas aguarda.

### Escolhendo alvos

- Os personagens usam ataques básicos para atacar o inimigo que estiver mais próximo, entretanto outras fontes podem afetar essa decisão (itens, árvore de habilidades, entre outros).
- A escolha de alvo é refeita a cada ataque básico, nunca fica guardada.
    - Por exemplo, se um arqueiro estava atacando um inimigo a 2 casas de distância e um outro inimigo chega a 1 casa de distância, o alvo muda no próximo ataque.
- A escolha de alvo do ataque básico segue esta ordem de prioridade, sempre considerando apenas inimigos vivos dentro do alcance:
    1. O inimigo que estiver provocando o personagem, se houver.
    2. O inimigo mais próximo.
    3. O inimigo com a menor porcentagem de vida atual.
    4. O inimigo com a menor vida máxima.
    5. O inimigo com a menor armadura física.
    6. O inimigo na casa de menor fileira e, em caso de empate, de menor coluna.
- Cada regra só é consultada quando a anterior termina empatada.
- A última regra é posicional justamente porque ela nunca pode empatar. Sem ela, um grupo de lacaios idênticos deixaria o combate imprevisível e impossível de reproduzir.
- As regras 3, 4 e 5 existem para que o personagem sempre ataque o inimigo que morre mais cedo, reduzindo o dano que o grupo ainda vai receber.

### Provocação

- Provocação é o efeito que força um personagem a atacar quem o provocou, ignorando qualquer outra regra de escolha de alvo.
- Provocação sempre possui uma duração e desaparece quando ela termina.
- Provocação não é um atributo e não acumula. Ela é aplicada por habilidades e existe apenas enquanto durar.
    - Se dois inimigos provocarem o mesmo personagem, vale a provocação mais recente.
- A escolha de alvo é toda posicional e visível no tabuleiro justamente para que o jogador consiga prever o combate apenas olhando para a tela. Provocação é a única coisa capaz de quebrar essa leitura, e por isso ela é sempre temporária e sempre vem de uma habilidade.

### Habilidades

- Habilidades são usadas conforme suas regras e podem ignorar completamente a escolha de alvo padrão. Por exemplo:
    - Se a habilidade descrever que ela "ataca o inimigo mais distante".
    - Se a habilidade descrever que ela "ataca o inimigo com a menor porcentagem de vida atual".
    - Se a habilidade descrever que ela "ataca TODOS em uma área 3x3".
- Habilidades também possuem o seu próprio alcance e o seu próprio formato de área, que não precisam ter nenhuma relação com o ataque básico do personagem.
    - Por exemplo, um personagem com alcance 2 no ataque básico pode possuir uma habilidade que atinge todos os inimigos em linha reta até o fim do campo de batalha.
- Uma habilidade só é avaliada pelas suas próprias regras, nunca pelo alcance do ataque básico de quem a usa.
    - No exemplo acima, se existir um inimigo na linha da habilidade, ela é usada imediatamente, mesmo que esse inimigo esteja muito além das 2 casas do ataque básico.
    - Ignorar isso transformaria a habilidade em desvantagem, pois o personagem ficaria esperando o inimigo chegar perto para usar algo que já podia ter usado.
- Uma habilidade pronta que não encontra nenhum alvo válido pelas suas próprias regras segura a carga e tenta novamente no instante seguinte. Ela nunca é usada no vazio.
- Uma habilidade que não descreve nenhuma regra de alvo utiliza a mesma ordem de prioridade do ataque básico.
- Habilidades podem afetar aliados e inimigos conforme sua descrição. Por exemplo:
    - Se a habilidade descrever que ela "ataca TODOS em uma área 3x3" e for usada em um local com aliados, os aliados também sofrerão aquele ataque.
    - Se a habilidade descrever que ela "cura TODOS em uma área 3x3" e for usada em um local com inimigos, os inimigos também sofrerão aquela cura.

### Escolhendo o local de uma habilidade em área

- Uma habilidade em área é sempre posicionada no local que gerar a melhor pontuação, calculada assim:
    - Cada personagem que a habilidade deveria afetar soma 1 ponto.
    - Cada personagem que a habilidade não deveria afetar diminui 1 ponto.
- O local escolhido é o de maior pontuação. Por exemplo, para uma habilidade de cura:
    - Um local que alcança 3 aliados e 2 inimigos pontua 1.
    - Um local que alcança 2 aliados e nenhum inimigo pontua 2.
    - O segundo local é escolhido, pois possui a maior pontuação.
- Em caso de empate na pontuação, vence o local que contém a casa de menor fileira e, persistindo o empate, de menor coluna.
- Habilidades específicas podem alterar o peso dessa conta quando fizer sentido para elas, mas o padrão é sempre 1 ponto para cada lado.

### Morte no campo de batalha

- Lacaios e vilões mortos desaparecem do campo de batalha e liberam a casa que ocupavam.
- Heróis mortos permanecem caídos na casa que ocupavam e podem ser revividos ali mesmo.
- Um herói caído não é um alvo válido para nada e não conta para habilidades em área.
- Um herói caído não protege ninguém. Ele não impede que os inimigos avancem e nem que eles ataquem quem está atrás dele.
- Personagens vivos nunca param na casa de um herói caído, para que ele possa ser revivido no mesmo lugar.
    - Como a diagonal conta como distância 1, contornar essa casa quase nunca custa movimento adicional.
