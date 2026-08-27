# Atributos

## Objetivo

Neste arquivo é possível encontrar detalhes de cada atributo presente no jogo.

## Health Point (HP)

- HP indica a vida atual e máxima do personagem.
- A vida atual nunca pode ultrapassar a vida máxima.
- A vida mínima é sempre 0.
    - Dito isso, um personagem pode acabar recebendo dano acima da vida atual, mas o valor sempre se resultará em 0.
- Enquanto a vida atual estiver acima de 0, o personagem é considerado vivo.
- Enquanto a vida atual estiver em 0, o personagem é considerado morto.
- É possível burlar a morte através de habilidades ou buffs.

## Atributos principais

- Os atributos principais estão presentes em todos os personagens.
- Cada herói possui uma base de atributos principais.
- Os jogadores podem aumentar os atributos principais através de outros meios (itens, árvore de habilidades, árvore de progresso).
- O mínimo de pontos de um atributo é 0.
- O máximo de pontos de um atributo é 500, porém o uso de buffs pode fazer com que o valor ultrapasse esse limite.

### Distribuição dos pontos de nível

- Todo personagem recebe 5 pontos de atributos principais a cada nível, conforme `progress.md`.
- No perfil de cada herói o jogador vê os atributos principais e secundários, e tem três controles:
    - **Colocar pontos à mão**, um a um, nos atributos que quiser.
    - **Um interruptor de distribuição automática**, por herói.
    - **Um botão de resetar**, que devolve todos os pontos daquele herói.
- Com o automático **ligado**, os pontos são distribuídos sozinhos seguindo a distribuição declarada na ficha, no instante em que o nível sobe. Nunca sobra ponto parado. Passar a valer é outra coisa, e está logo abaixo.
    - É o estado inicial de todo herói. Em um jogo idle, voltar de uma ausência longa e encontrar o time do mesmo tamanho de antes seria o oposto do que a progressão offline promete.
- Com o automático **desligado**, os pontos se acumulam como disponíveis e esperam o jogador.
- **Ligar o automático gasta apenas os pontos que estavam disponíveis.** Ele não redistribui o que o jogador colocou à mão, pois desfazer uma build inteira deve exigir o botão de resetar, e não um clique em um interruptor.
- **Resetar devolve todos os pontos.** Com o automático ligado eles voltam na mesma hora pela distribuição da ficha, o que equivale a voltar para a build recomendada. Com ele desligado, tudo fica disponível para ser redistribuído.
- Redefinir os pontos é livre e pode ser feito a qualquer momento, sem custo.
- Lacaios e vilões são personagens com o automático permanentemente ligado. Não existe ninguém distribuindo pontos por eles.
- Em qualquer momento vale a regra: **pontos colocados à mão + pontos colocados automaticamente + pontos disponíveis = 5 × (nível − 1)**.

### Um ponto colocado só passa a valer na próxima fase

Esta é a regra mais importante desta seção, e ela **não tem exceção**.

- **Colocar um ponto e o ponto fazer efeito são dois momentos diferentes.** O ponto é colocado na hora, e aparece no perfil na hora. O que ele faz nos atributos só entra em vigor quando a próxima fase começa.
- Vale para **todas** as formas de colocar ponto, sem distinguir quem colocou:
    - O automático distribuindo os pontos de um nível que acabou de subir.
    - O jogador colocando pontos à mão.
    - O botão de resetar.
    - Ligar ou desligar o interruptor do automático.
- **O automático não é exceção.** Ele decide _onde_ o ponto vai sem decidir _quando_ ele vale, exatamente como o jogador.
- É a mesma regra que item, equipe, ordem e formação seguem, descrita em "O grupo é montado no começo de cada fase" em `gameplay.md`.
- **A vida máxima, portanto, nunca muda no meio de uma fase**, já que ela vem apenas de CON.

#### O que continua valendo na hora

**O nível em si, e tudo que cresce por nível sem passar por atributo**: a armadura física e as resistências elementais, descritas em "Atributos limitados crescem com o nível", e o dano base por nível. Subir de nível no meio de uma fase deixa o personagem mais resistente na hora; o que espera é a distribuição de pontos.

#### Exemplos

**Ganhando um nível no automático.** Um herói com distribuição de 100% em CON está no nível 4, com 15 de CON, portanto 150 de vida máxima. No meio de uma fase, com 120 de vida atual, ele sobe para o nível 5:

- Os 5 pontos do nível são creditados e distribuídos na mesma hora: no perfil, o CON já lê 20.
- Durante o resto da fase ele continua com **150 de máxima e 120 de atual**. Nada mudou nos atributos.
- A armadura, que cresce por nível, sobe imediatamente.
- Na fase seguinte os pontos entram em vigor: 20 de CON, **200 de máxima**, e como toda fase começa com o grupo inteiro, ele entra com 200/200.

**Redefinindo no meio da fase.** O mesmo herói, com o automático desligado e 20 pontos disponíveis, coloca todos em CON durante uma fase. A vida máxima continua onde estava até aquela fase terminar. Os pontos não foram perdidos e não voltam atrás — eles só ainda não valem.

### Poder (POW)

- POW aumenta o dano dos ataques físicos do personagem.
    - **A cada 10 pontos de POW, o dano físico aumenta 1%.**
    - Ele **multiplica** o dano base e nunca soma nada a ele. O dano base vem do conteúdo: a arma equipada, ou o dano próprio do personagem enquanto ele não tiver uma.
- POW não dá vida, não dá regeneração e não dá nada além de dano. Quem quer vida investe em CON.
- Personagens com mais POW são capazes de utilizar equipamentos e armaduras mais pesados.

### Agilidade (AGI)

- AGI aumenta evasão, velocidade de ataque básico e velocidade de movimento do personagem.
    - Para cada 1 ponto de AGI, o personagem ganha 1% de velocidade de ataque quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 0.5% de velocidade de ataque quando estiver utilizando equipamentos especiais.
    - Para cada 1 ponto de AGI, o personagem ganha 0.2% de velocidade de ataque quando estiver utilizando equipamentos pesados.
    - Para cada 1 ponto de AGI, o personagem ganha 1% de velocidade de movimento quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 0.75% de velocidade de movimento quando estiver utilizando equipamentos especiais.
    - Para cada 1 ponto de AGI, o personagem ganha 0.5% de velocidade de movimento quando estiver utilizando equipamentos pesados.
    - AGI é a fonte de evasão, mas a conversão em chance de evasão é feita por rendimento decrescente e está descrita em "Evasão".
- Personagens com mais AGI são capazes de utilizar equipamentos e armaduras mais leves.

### Especialidade (SPE)

- SPE aumenta ataques elementais e redução de recarga do personagem.
    - SPE aumenta o dano das habilidades do personagem. **Quanto ele aumenta ainda não está definido**, e é decidido junto com os itens.
        - A promessa foi retirada em vez de implementada porque o modelo inteiro muda com os itens: dano de atributo vai deixar de ser um valor somado e passar a ser uma porcentagem sobre a base que o conteúdo fornece — a arma para o ataque básico, o rank para a habilidade. Implementar o valor antigo agora seria construir algo para apagar em seguida.
    - SPE é a fonte de redução de recarga, mas a conversão em porcentagem é feita por rendimento decrescente e está descrita em "Redução de recarga".
- Personagens com mais SPE são capazes de utilizar equipamentos e armaduras especiais.

### Constituição (CON)

- CON é a **única** fonte de vida máxima que um atributo oferece.
    - Para cada 1 ponto de CON, o personagem ganha 10 pontos de vida máxima.
    - Para cada 1 ponto de CON, o personagem ganha 0.5% de velocidade de regeneração de vida.
- Concentrar a vida em um atributo só é proposital. Enquanto o POW dava vida **e** dano, ele era o único dos quatro que pagava dos dois lados da luta, e nenhuma build que o ignorasse era viável.

## Rendimento decrescente

- Alguns atributos do jogo são limitados por natureza, pois representam uma chance ou uma porcentagem de mitigação. Esses atributos nunca podem passar de 100% e por isso possuem um teto.
- Se esses atributos crescessem de forma linear, o jogador atingiria o teto muito cedo e todo ponto investido depois disso seria desperdiçado.
- Para evitar isso, todo atributo limitado é calculado através da fórmula de rendimento decrescente:
    - `Valor final = Teto × Pontos ÷ (Pontos + Constante)`
    - `Teto` é o valor máximo teórico daquele atributo.
    - `Pontos` é o total acumulado vindo de todas as fontes (atributos principais, itens, árvore de habilidades, árvore de progresso).
    - `Constante` define a velocidade da curva. Quanto maior a constante, mais devagar o atributo cresce.
- O `Teto` nunca é alcançado, o atributo apenas se aproxima dele infinitamente. Isso garante que:
    - Cada ponto investido sempre tem algum efeito, do nível 1 até o nível 100.
    - Nenhum personagem se torna imune a nada.
    - Não é necessário travar o valor manualmente, pois a própria curva impede que o teto seja ultrapassado.
- Atributos ilimitados por natureza continuam crescendo de forma linear, pois não podem saturar. São eles:
    - Dano físico e dano elemental.
    - Vida máxima.
    - Velocidade de ataque.
    - Velocidade de movimento.
    - Regeneração de vida.

## Atributos secundários

- Os atributos secundários estão presentes em alguns personagens, mas não necessariamente em todos eles.
- Os atributos secundários não podem ser aumentados diretamente com pontos de atributos, apenas através de outros meios (itens, árvore de habilidades, entre outros).

### De onde eles vêm depende do tipo de personagem

- Para heróis, os secundários chegam passivamente: parte deles é convertida dos atributos principais, e o resto vem do equipamento, da árvore de habilidades e dos buffs. O jogador nunca digita "armadura", ele veste uma armadura.
- Para lacaios e vilões, os secundários são escritos na ficha, um por um. Eles não carregam equipamento, então não existe de onde a armadura deles aparecer sozinha.
    - É por isso que a ficha de um inimigo costuma declarar valores defensivos e a de um herói costuma deixá-los em zero.
    - Um lacaio blindado é blindado porque a ficha dele diz isso, e não porque ele achou uma armadura.

### Atributos limitados crescem com o nível

- Os atributos defensivos que passam pela curva de rendimento decrescente (armadura física e resistência elemental) precisam crescer conforme o personagem sobe de nível.
- O motivo é que a constante da curva é `50 × nível do atacante`. Um valor parado vale cada vez menos, e um personagem sem fonte nova de armadura simplesmente apodrece.
- Por isso a ficha declara, além do valor inicial, quanto aquele atributo ganha por nível:
    - `Valor = (Valor inicial + Ganho por nível × (nível − 1)) × Multiplicador da fase`
- Quando o ganho por nível é igual ao valor inicial, a mitigação contra um atacante do mesmo nível fica constante durante o jogo inteiro. É o caso mais comum, porque a armadura e a constante da curva passam a crescer juntas e se cancelam.
    - Por exemplo, um personagem com 100 de armadura inicial e 100 por nível mitiga 50% no nível 1, 50% no nível 12 e 50% no nível 50.
    - Um valor inicial de 150 com ganho de 150 por nível mitiga 56.25% em qualquer nível.
- Ganhos por nível menores que o valor inicial fazem a defesa perder força devagar, e maiores fazem ela ganhar. As duas coisas são escolhas válidas de ficha.
- Atributos secundários que são porcentagens do dano, como espinhos e roubo de vida, não precisam de ganho por nível. Eles não decaem, pois acompanham o dano por definição.

### Dano físico

- É o dano causado através de ataques físicos e através do elemento terra.
- `Dano do ataque básico = Dano base × (1 + POW × 0.001)`
- O **dano base** vem do conteúdo, e nunca do atributo:
    - Enquanto o personagem não tem arma, ele é o valor declarado na ficha — o soco do próprio personagem. É baixo de propósito.
    - Com item, a arma equipada substitui esse valor.
- Assim como a armadura, o dano base declara **quanto ganha por nível**, e pelo mesmo motivo descrito em "Atributos limitados crescem com o nível": um valor parado envelhece.
    - Heróis deixam esse ganho em zero, pois quem os faz bater mais forte é o equipamento.
    - Lacaios e vilões usam o ganho, pois não existe outra coisa que os faça bater mais forte em um ato posterior.

### Dano elemental

- É o dano causado através dos elementos de fogo, água e elétrico.
- Nenhum elemento possui vantagem contra outros elementos.

### Regeneração de vida

- É a quantidade de pontos de vida que um personagem regenera por segundo.
- Regeneração de vida não é considerada uma cura.
- Qualquer valor de regeneração de vida abaixo de 0 é considerado como sendo 0.
- O valor base é 0 para todo personagem. Nenhum personagem regenera vida por existir.
    - A regeneração chega por outros meios: itens, árvore de habilidades e buffs. Para lacaios e vilões, ela é escrita na ficha, como todo secundário deles.
- CON não concede regeneração, ele multiplica a que existir. É isso que os 0.5% por ponto significam:
    - `Regeneração por segundo = Regeneração base × (1 + CON × 0.005)`
    - Um personagem com 0 de regeneração base continua com 0, por mais CON que tenha.
    - Um personagem com 10 de regeneração base e 40 de CON regenera 12 pontos por segundo.
- A regeneração continua correndo entre uma onda e outra, pois é medida por segundo e a transição leva vários segundos.
    - Isso não contradiz o desgaste descrito em `gameplay.md`. Ninguém volta com a vida cheia: cada personagem recupera apenas o que a própria taxa render naquele tempo.
    - É exatamente aí que um item de regeneração se paga, e é o que dá sentido ao atributo existir.

### Evasão

- Atributo defensivo capaz de fazer com que o personagem desvie parcialmente um ataque, seja ele físico ou elemental.
- A chance de evasão é calculada por rendimento decrescente sobre o total de AGI do personagem:
    - `Chance de evasão = 100 × AGI ÷ (AGI + Constante)`
    - A constante é 100 enquanto o personagem estiver utilizando equipamentos leves.
    - A constante é 200 enquanto o personagem estiver utilizando equipamentos especiais.
    - A constante é 500 enquanto o personagem estiver utilizando equipamentos pesados.
- Por exemplo, utilizando equipamentos leves:
    - Um personagem com 25 de AGI possui 20% de evasão.
    - Um personagem com 100 de AGI possui 50% de evasão.
    - Um personagem com 300 de AGI possui 75% de evasão.
    - Um personagem com 900 de AGI possui 90% de evasão.
- Por exemplo, utilizando equipamentos especiais:
    - Um personagem com 100 de AGI possui 33.3% de evasão.
    - Um personagem com 200 de AGI possui 50% de evasão.
    - Um personagem com 600 de AGI possui 75% de evasão.
- Por exemplo, utilizando equipamentos pesados:
    - Um personagem com 100 de AGI possui 16.7% de evasão.
    - Um personagem com 500 de AGI possui 50% de evasão.
    - Um personagem com 1500 de AGI possui 75% de evasão.
- Ao fazer um teste de evasão 30% do valor de evasão se torna chance de evasão perfeita. Por exemplo:
    - Se um personagem possui 10% de evasão, há 3% de chance dessa evasão ser perfeita.
    - Se um personagem possui 20% de evasão, há 6% de chance dessa evasão ser perfeita.
- Uma evasão perfeita elimina 75% do dano que seria causado ao personagem.
    - Esse valor pode ser potencializado por outros meios (itens, árvore de habilidades, entre outros).
- Uma evasão normal elimina 40% do dano que seria causado ao personagem.
    - Esse valor pode ser potencializado por outros meios (itens, árvore de habilidades, entre outros).
- A evasão nunca alcança 100%, pois a curva de rendimento decrescente apenas se aproxima desse valor.
- Qualquer valor de evasão abaixo de 0 é considerado como sendo 0.

### Velocidade de ataque

- Atributo ofensivo que indica quantos ataques básicos por segundo um personagem faz.
- Esse atributo se diz respeito exclusivamente aos ataques básicos dos personagens.
- O valor mínimo de velocidade de ataque é 0, que indicaria que o personagem perdeu a habilidade de fazer ataques básicos.
- Por padrão a velocidade de ataque de um personagem é 1.

### Velocidade de movimento

- Atributo que indica o quão rápido um personagem se movimenta pelo campo de batalha.
- É medido em casas por segundo, da mesma forma que a velocidade de ataque é medida em ataques por segundo.
- Todo personagem possui 2 casas por segundo de velocidade de movimento base, e os ganhos vindos de AGI e de outras fontes são somados por cima desse valor.
    - Com esse valor, atravessar o campo de batalha inteiro leva 3.5 segundos e reposicionar uma casa leva meio segundo.
    - A travessia mais longa do tabuleiro de 8 fileiras é de 7 casas, da fileira 1 até a fileira 8.
- Um personagem só se movimenta quando não possui nenhum alvo válido dentro do seu alcance, conforme descrito em `gameplay.md`.
- O valor mínimo de velocidade de movimento é 0, que indicaria que o personagem perdeu a habilidade de se movimentar.

### Redução de recarga

- Atributo ofensivo que indica o quão rápido um personagem utiliza suas habilidades.
- A redução de recarga é calculada por rendimento decrescente sobre o total de SPE do personagem:
    - `Redução de recarga = 60 × SPE ÷ (SPE + Constante)`
    - A constante é 60 enquanto o personagem estiver utilizando equipamentos leves.
    - A constante é 40 enquanto o personagem estiver utilizando equipamentos especiais.
    - A constante é 300 enquanto o personagem estiver utilizando equipamentos pesados.
- Por exemplo, utilizando equipamentos especiais:
    - Um personagem com 20 de SPE possui 20% de redução de recarga.
    - Um personagem com 40 de SPE possui 30% de redução de recarga.
    - Um personagem com 120 de SPE possui 45% de redução de recarga.
    - Um personagem com 360 de SPE possui 54% de redução de recarga.
- Por exemplo, utilizando equipamentos leves:
    - Um personagem com 20 de SPE possui 15% de redução de recarga.
    - Um personagem com 60 de SPE possui 30% de redução de recarga.
    - Um personagem com 180 de SPE possui 45% de redução de recarga.
    - Um personagem com 540 de SPE possui 54% de redução de recarga.
- Por exemplo, utilizando equipamentos pesados:
    - Um personagem com 100 de SPE possui 15% de redução de recarga.
    - Um personagem com 300 de SPE possui 30% de redução de recarga.
    - Um personagem com 900 de SPE possui 45% de redução de recarga.
- A redução de recarga é aplicada junto com o valor de recarga de uma habilidade. Por exemplo:
    - Se uma habilidade possui 10 segundos de recarga e o personagem possui 50% de redução de recarga, aquele personagem pode usar essa habilidade a cada 5 segundos.
    - Se uma habilidade possui 1 minuto de recarga e o personagem possui 50% de redução de recarga, aquele personagem pode usar essa habilidade a cada 30 segundos.
- A redução de recarga nunca alcança 60%, pois a curva de rendimento decrescente apenas se aproxima desse valor.
- Qualquer valor de redução de recarga abaixo de 0 é considerado como sendo 0.

### Armadura física

- Atributo defensivo capaz de fazer com que o personagem mitigue parcialmente um ataque físico.
- A armadura física nunca elimina um ataque por completo, ela sempre mitiga apenas uma porcentagem dele.
- A mitigação é calculada por rendimento decrescente sobre o total de armadura física do personagem:
    - `Mitigação física = 75 × Armadura ÷ (Armadura + Constante)`
    - A constante é `50 × nível do atacante`.
- A constante cresce junto com o nível do atacante para que a armadura precise ser melhorada durante todo o jogo. Uma armadura excelente na primeira fase se torna fraca contra inimigos muito mais fortes.
- Por exemplo, contra um atacante de nível 10 (constante 500):
    - Um personagem com 500 de armadura física mitiga 37.5% do dano físico recebido.
    - Um personagem com 1500 de armadura física mitiga 56.2% do dano físico recebido.
- Por exemplo, contra um atacante de nível 50 (constante 2500):
    - Um personagem com 500 de armadura física mitiga 12.5% do dano físico recebido.
    - Um personagem com 2500 de armadura física mitiga 37.5% do dano físico recebido.
- A mitigação nunca alcança 75%, pois a curva de rendimento decrescente apenas se aproxima desse valor.
- A armadura física escala infinitamente mas não pode ficar abaixo de 0.

### Resistência elemental

- Atributo defensivo capaz de fazer com que o personagem mitigue parcialmente um ataque elemental.
- Personagens possuem resistência elemental para cada elemento individualmente.
    - Ou seja, a resistência ao elemento A não mitiga dano de elemento B e nem C.
- A resistência elemental é calculada exatamente como a armadura física, mas separada por elemento:
    - `Mitigação elemental = 75 × Resistência ÷ (Resistência + Constante)`
    - A constante é `50 × nível do atacante`.
- Por exemplo, contra um atacante de nível 10 (constante 500):
    - Um personagem com 500 de resistência ao elemento A mitiga 37.5% do dano do elemento A.
    - Esse mesmo personagem não mitiga nada do dano do elemento B e nem do elemento C.
- Como a resistência é dividida entre três elementos, cobrir todos eles custa muito mais do que cobrir o dano físico. Essa é a principal diferença entre os dois atributos defensivos.
- Fontes comuns de resistência (itens comuns, nós comuns da árvore de habilidades) nunca ultrapassam os 75% da curva.
- Fontes especiais (itens únicos, nós específicos da árvore de habilidades, buffs) são somadas depois da curva e são as únicas capazes de levar a resistência acima de 75%.
    - Ultrapassar 100% de resistência a um elemento deve ser um objetivo de build, nunca um acidente.
- Quando a resistência final ultrapassa 100%, o dano recebido se transforma em cura. Por exemplo:
    - Se o personagem possuir 200% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai curar 1000 pontos de vida.
    - Se o personagem possuir 150% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai curar 500 pontos de vida.
- Debuffs podem levar a resistência para valores negativos, aumentando o dano recebido. Por exemplo:
    - Se o personagem possuir -50% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai receber 1500 pontos de dano.
    - Se o personagem possuir -100% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai receber 2000 pontos de dano.
- A resistência elemental nunca fica abaixo de -100%, para que o acúmulo de debuffs não gere dano infinito.
- Receber vida por possuir mais de 100% de resistência elemental é considerada uma cura.

### Roubo de vida

- Atributo ofensivo que faz com que o personagem se cure ao desferir um ataque físico.
- O roubo de vida também é calculado em habilidades.
- O roubo de vida é arredondado conforme descrito em "Arredondamento". Por exemplo:
    - 0.4 se torna 0 e 0.6 se torna 1.
    - 1.4 se torna 1 e 1.6 se torna 2.
- O cálculo de roubo de vida é feito sempre em cima do dano final causado. Por exemplo:
    - Se um personagem causa 100 de dano físico e possui 3% de roubo de vida, ele vai curar 3 pontos de vida.
    - Se um personagem tenta causar 100 de dano ao adversário e o dano for mitigado para apenas 10 (por conta de armadura ou evasão), ele não vai receber cura (cálculo terminado em 0.3 e arredondado para 0).
    - Se um personagem causa 100 de dano elemental e possui 3% de roubo de vida, ele não recebe cura.
- O total roubado não pode ser abaixo de 0.
- Receber vida por roubo de vida é considerada uma cura.

### Espinhos

- Atributo defensivo que faz com que o personagem devolva parte do dano físico recebido ao seu atacante.
- Espinhos só reage a ataques físicos.
- O cálculo é feito sobre o dano físico que o personagem receberia antes de qualquer mitigação, para que personagens muito defensivos continuem devolvendo um valor relevante.
- O dano devolvido é sempre físico e é mitigado normalmente pela armadura física do atacante.
- Espinhos nunca reage a um dano vindo de outro espinhos, para evitar devoluções infinitas entre dois personagens.
- O total devolvido não pode ser abaixo de 0.
- **O dano devolvido concede roubo de vida a quem devolveu**, calculado sobre o dano final da devolução, como em qualquer outro dano físico.
    - É o que torna possível o personagem que se cura apanhando, combinando espinhos com roubo de vida.
    - Sem isso, as duas fontes de cura defensiva do jogo nunca conversariam entre si, e a build de espinhos seria estritamente pior que a de roubo de vida comum.

### Ocupação

- Atributo responsável por indicar quantas casas aquele personagem ocupa.

## Ordem do cálculo de dano

- Todo ataque, seja básico ou de habilidade, é resolvido sempre nesta ordem:

1. **Dano base.** É o dano físico ou elemental de quem ataca. Um mesmo ataque nunca é físico e elemental ao mesmo tempo.
2. **Espinhos.** Calculado sobre o dano base, antes de qualquer mitigação. O valor devolvido é resolvido como um ataque físico independente contra quem atacou, passando pela mitigação dele normalmente.
3. **Mitigação.** Armadura física para dano físico, resistência do elemento correspondente para dano elemental.
4. **Evasão.** Sorteada uma única vez por ataque. Uma evasão normal elimina 40% do dano e uma evasão perfeita elimina 75%.
5. **Dano final.** Arredondado para o valor inteiro mais próximo e subtraído da vida atual.
6. **Roubo de vida.** Calculado sobre o dano final e apenas em ataques físicos.

### As reduções sempre multiplicam entre si

- Evasão e mitigação nunca são somadas, sempre multiplicadas. Por exemplo:
    - Um personagem com 75% de mitigação que sofre uma evasão normal de 40% recebe `100% × 60% × 25% = 15%` do dano.
    - Se as reduções fossem somadas, esse mesmo personagem receberia `100% - 40% - 75%`, o que resultaria em dano negativo.
- Somar reduções permitiria que um personagem alcançasse 100% e se tornasse imune, o que contraria diretamente a regra descrita em "Rendimento decrescente".

### Quando o ataque vira cura

- Quando a resistência ao elemento do ataque ultrapassa 100%, o ataque deixa de causar dano e passa a curar, conforme descrito em "Resistência elemental".
- Nesse caso a evasão não é sorteada, pois não existe dano para ser evitado.
- Espinhos e roubo de vida também não acontecem, pois nenhum dano foi causado.

### Arredondamento

- Todo valor de dano, cura, roubo de vida e espinhos é arredondado para o valor inteiro mais próximo. Por exemplo:
    - 0.4 se torna 0 e 0.6 se torna 1.
    - 12.3 se torna 12 e 12.7 se torna 13.
- Quando o valor cai exatamente no meio, ele é arredondado para o inteiro par mais próximo. Por exemplo:
    - 0.5 se torna 0 e 1.5 se torna 2.
    - 2.5 se torna 2 e 3.5 se torna 4.
- Esse desempate pelo par é o arredondamento padrão da linguagem em que o jogo é feito, e não uma escolha de design.
    - Ele também possui uma vantagem prática: sempre arredondar o meio para cima acumularia um viés para cima ao longo de milhares de golpes, e o desempate pelo par não acumula viés nenhum.
- O arredondamento acontece apenas no final de cada cálculo, nunca nos passos intermediários.
