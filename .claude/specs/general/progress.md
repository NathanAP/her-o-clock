# Árvore de progresso

## Objetivo

Especificar como funciona o progresso de Her-o-clock.

## Dinheiro

- Com dinheiro o jogador pode:
    - Investir na árvore de progresso para melhorar seu avanço.
    - Criar itens através do artesanato.
    - Quebrar itens para adquirir itens de artesanato.
- O máximo de dinheiro que um jogador pode possuir é 100% em cima do próximo nó na árvore de progresso. Por exemplo:
    - Se o nó mais caro na árvore de progresso custa $1000, o máximo de dinheiro que um jogador pode ter é $2000.
    - Se o nó mais caro na árvore de progresso custa $10000, o máximo de dinheiro que um jogador pode ter é $20000.

## Progressão offline

- Mesmo offline o jogador continua ganhando um pouco de recursos.
- Para fazer esse cálculo, precisamos guardar quanto de experiência e dinheiro um usuário fez na última hora online. Com esse valor em mãos permitimos ele manter 25% do seu ritmo online enquanto estiver offline.
    - A árvore de progresso permite o jogador a subir esse ritmo para até 40%.
- Não é possível ultrapassar o máximo de dinheiro ao voltar a ficar online. Por exemplo:
    - Se o jogador saiu com $500 com seu máximo de dinheiro sendo $3000 e ele permaneceu 1 mês fora, ao voltar é provável que seu dinheiro esteja em $3000.
- Não é possível subir mais de 1 nível completo de cada personagem ao voltar a ficar online. Por exemplo:
    - Se o jogador saiu faltando 5% para seus personagens subir ao nível 10 e ele permaneceu 1 mês fora, ao voltar é provável que seus personagens estejam no nível 10 com 95% de progresso.
- Não é possível adquirir itens enquanto offline.
- Não é possível passar de fase enquanto offline.

## Árvore de progresso

- A árvore de progresso possui uma sequência de caminhos para o jogador escolher comprar através de dinheiro adquirido ao eliminar inimigos.
- A ideia geral dessa árvore é fazer com que o jogador precise investir tempo para conseguir avançar no jogo através do farm
- A árvore é distribuída da seguinte forma:
    - Lado superior direito contém nós relacionados ao ganho de experiência de heróis enquanto online.
    - Lado inferior direito contém nós relacionados ao ganho de dinheiro enquanto online.
    - Lado superior esquerdo contém nós relacionados à quantidade de espaços no inventário.
    - Lado inferior esquerdo contém nós relacionados à quantidade de espaços e abas no baú.
    - Vertical inferior contém chance de adquirir melhores itens ao eliminar inimigos.
    - Vertical superior contém chance de adquirir melhores itens de artesanato ao eliminar inimigos.
- Cada "trecho" da árvore precisa ter pelo menos 50 nós cada.
- Nós devem conceder um valor baixo para que ela não desande e não seja mais viável investir em um só lado antes dos outros. Por exemplo:
    - "+10 de dinheiro ao eliminar um inimigo."
    - "+1% de dinheiro ao eliminar um inimigo."
    - "+10 de experiência ao eliminar um inimigo."
    - "+1% de experiência ao eliminar um inimigo."
    - "+0.2% máximo progressão enquanto offline."

## Árvore de habilidades

- Falaremos sobre a árvore de habilidades futuramente.
