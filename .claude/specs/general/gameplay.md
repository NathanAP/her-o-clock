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

## Fases

- Uma fase é uma sequência de combates contra grupos de lacaios, terminando sempre em um combate contra um vilão.
- Cada grupo de lacaios é chamado de onda.
- Lacaios e vilões possuem o nível definido pela fase em que aparecem, e não pela ficha deles. É isso que permite reaproveitar o mesmo lacaio em atos diferentes com forças diferentes.
- Cada fase possui a sua própria pequena história, contada ao jogador quando ela começa.

### Desgaste

- Os heróis **não** recuperam vida entre uma onda e outra. A vida perdida em uma onda segue para a próxima.
- Um herói que cai permanece caído até o fim da fase, pois não existe forma de revivê-lo durante o combate a não ser por habilidade.
- É o desgaste que dá sentido à regeneração de vida e ao roubo de vida. Sem ele os dois atributos seriam decorativos, pois cada combate começaria com todo mundo inteiro.
- É também o desgaste que cria a necessidade de farmar. Chega um ponto em que a fase não é vencível com o time atual, e o jogador precisa subir de nível ou melhorar os itens antes de avançar.
- O desgaste vale **dentro de uma fase**, e não além dela. Uma fase recomeçada, seja por derrota ou por fechar e reabrir o jogo, começa com todo mundo inteiro. O save guarda em que fase o jogador está, e nunca em que ponto dela — o porquê está em `save.md`.

### Entre uma onda e outra

- Os heróis vivos **caminham de volta** às suas posições iniciais, e só depois que todos chegam o grupo segue para o próximo confronto.
    - Eles caminham em vez de simplesmente reaparecerem na formação porque a volta faz parte do combate terminando. O grupo se reagrupa, e aí avança.
- Os heróis caídos são carregados de volta junto com o grupo.
    - Eles voltam para que a área dos inimigos fique livre para a próxima onda, e para que fiquem no lugar certo caso sejam revividos.
- Terminado o reagrupamento, o cenário rola por alguns segundos, representando o grupo avançando pela cidade até o próximo confronto.
- A transição inteira é intencionalmente longa. Ela é o respiro entre dois combates, e um auto-scroller sem respiro vira uma sequência de brigas sem lugar nenhum.

### Vitória e derrota

- A fase é vencida quando o vilão é derrotado.
- A fase é perdida quando todos os heróis caem.
- Ao perder, a fase reinicia do começo, com todos os heróis vivos e com vida cheia.

## Combate

- O combate sempre ocorre automaticamente e não pode ser manipulado pelo jogador.
- O combate começa imediatamente quando os heróis encontram um inimigo.
- O combate termina quando a equipe de heróis, lacaios ou vilões ficar sem membros vivos.
- Ao vencer um combate contra lacaios, os heróis se movem de volta às suas posições originais e continuam até o próximo combate.
- Ao vencer um combate contra vilões, os heróis comemoram sua vitória.

## Campo de batalha

- O campo de batalha é um tabuleiro único de 6 colunas por 8 fileiras, totalizando 48 casas, sempre visto de baixo para cima.
    - As fileiras de 1 a 4 formam a área dos heróis, com 24 casas no total.
    - As fileiras de 5 a 8 formam a área dos lacaios e vilões, com 24 casas no total.
- As duas áreas possuem o mesmo tamanho de propósito. Se em algum momento ficar claro que os heróis precisam de mais ou de menos espaço que os lacaios, essa é a primeira coisa a ser ajustada.
- As duas áreas definem apenas onde cada lado começa a batalha. Elas não limitam para onde os personagens podem ir depois que a batalha começa.
- Os personagens se dispõem no campo de batalha conforme a ordem definida pelo jogador.
    - Geralmente os personagens com mais defesa e vida ficam na frente enquanto personagens mais frágeis ficam atrás, mas isso varia de acordo com cada estratégia (no caso dos heróis) ou fase (no caso dos lacaios).
- A partir do momento que a batalha começa, os personagens se movimentam automaticamente para atacar o seu adversário alvo, mesmo que signifique invadir o espaço adversário.
- Heróis e lacaios geralmente ocupam uma casa, mas habilidades, buffs ou debuffs podem mudar isso.
- Vilões acabam tendo mais variações de ocupação de casas. Por exemplo, um vilão pode ocupar uma área de 2x2 casas.
- Um personagem que ocupa mais de uma casa continua sendo um único personagem, e isso vale para todas as regras de combate.
    - A distância até ele é medida a partir da casa ocupada mais próxima.
    - Uma habilidade em área que alcança várias casas ocupadas por ele o atinge uma única vez.
    - Ele pode ser cercado normalmente, e é comum que vilões possuam habilidades para afastar os heróis quando isso acontece.

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
- Certas armas dependem exclusivamente de uma distância mínima (por exemplo, um arco ou uma sniper). Nesse caso o personagem precisa se movimentar para conseguir realizar seu ataque, conforme descrito em "Movimentação".

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

### Movimentação

- Um personagem só se movimenta quando não consegue atacar seu alvo de onde está. Enquanto conseguir atacar, ele permanece parado.
- O alvo que define a movimentação é escolhido pela mesma ordem de prioridade do ataque básico, mas ignorando o filtro de alcance.
    - Ou seja, quando ninguém está dentro do alcance, a cadeia é consultada novamente sobre todos os inimigos vivos, e o personagem se movimenta em direção a quem ela escolher.
    - É por isso que uma provocação continua funcionando mesmo quando quem provocou está longe demais para ser atacado.
- Se o alvo está mais distante que o alcance máximo, o personagem avança uma casa na direção dele e tenta atacar novamente.
- Se o alvo está mais próximo que o alcance mínimo, o personagem recua uma casa para longe dele e tenta atacar novamente.
    - O recuo é o que permite que armas como arco e sniper voltem a atacar depois que um inimigo encosta nelas.
- O personagem sempre se movimenta para a casa vazia que mais o aproxima de conseguir atacar.
- Movimentar custa tempo e portanto custa dano, então uma formação mal montada é punida naturalmente.
- Essa regra também é o que permite que personagens corpo a corpo alcancem o vilão depois que os lacaios da frente morrem.
- A movimentação é decidida sempre pelo ataque básico. Um personagem nunca se movimenta apenas para conseguir usar uma habilidade.
    - Habilidades que dependem de reposicionamento resolvem isso sozinhas, movendo o personagem como parte do próprio efeito.

#### Personagem encurralado

- Um personagem está encurralado quando não consegue atacar seu alvo e também não possui nenhuma casa vazia para onde se movimentar.
- Um personagem encurralado escolhe o próximo alvo válido da cadeia de prioridade e ataca normalmente, se houver algum dentro do seu alcance.
- Se não houver nenhum alvo válido, ele apenas aguarda até que a situação mude.
- Ficar encurralado é a principal fraqueza das armas que dependem de distância mínima, e é intencional que seja assim.

### Habilidades

Habilidades são descritas por inteiro em `abilities.md`. O que vale registrar aqui é onde as duas coisas se encostam:

- Uma habilidade tem **alcance e formato próprios**, sem nenhuma relação com o alcance do ataque básico de quem a usa.
- Uma habilidade escolhe alvo pelas regras dela, mas **a cadeia de prioridade desta página é a base**: uma habilidade sem critério declarado usa a cadeia inteira, e uma que declara um critério apenas troca a primeira regra e mantém o resto como desempate.
- A **provocação continua acima de tudo**, inclusive de uma habilidade que declare o próprio critério.
- Uma habilidade em área é **sempre centrada em quem a usou**, e não procura o melhor lugar do tabuleiro.
    - O motivo é a proposta do jogo: ele é minúsculo e vive num cantinho da tela, então o jogador precisa conseguir prever a briga olhando de relance. Uma área aparecendo onde uma conta invisível decidiu destrói essa leitura, pelo mesmo motivo que não existe atributo de aggro.
    - O efeito colateral é que posicionar o herói vira parte da decisão, em vez de uma otimização que o jogo faz sozinho por trás.
    - Uma versão anterior desta página descrevia uma pontuação que procurava o melhor local do tabuleiro. Ela foi removida na 0.6.0.3, junto com a âncora `bestPlacement` que a selecionava.

### Morte no campo de batalha

- Lacaios e vilões mortos desaparecem do campo de batalha e liberam a casa que ocupavam.
- Heróis mortos permanecem caídos na casa que ocupavam e podem ser revividos ali mesmo.
- Um herói caído não é um alvo válido para nada e não conta para habilidades em área.
- Um herói caído não protege ninguém. Ele não impede que os inimigos avancem e nem que eles ataquem quem está atrás dele.
- Personagens vivos nunca param na casa de um herói caído, para que ele possa ser revivido no mesmo lugar.
    - Como a diagonal conta como distância 1, contornar essa casa quase nunca custa movimento adicional.
