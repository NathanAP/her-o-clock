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
- Os jogadores escolhem quais atributos principais desejam aumentar cada vez que um herói sobe de nível.
    - Cabe ao jogador decidir como ele prefere fazer a distribuição.
    - Os pontos de atributos ganhos nos níveis podem ser redefinidos a qualquer momento.
- Os jogadores podem aumentar os atributos principais através de outros meios (itens, árvore de habilidades, árvore de progresso).

### Poder (POW)

- POW aumenta ataques físicos e vida máxima do personagem.
    - Para cada 1 ponto de POW, o personagem ganha 1 ponto de dano físico.
    - Para cada 1 ponto de POW, o personagem ganha 5 pontos de vida máxima.
    - Para cada 1 ponto de POW, o personagem ganha 0.5% de velocidade de regeneração de vida.
- Personagens com mais POW são capazes de utilizar equipamentos e armaduras mais pesados.

### Agilidade (AGI)

- AGI aumenta evasão, velocidade de ataque básico e velocidade de movimento do personagem.
    - Para cada 1 ponto de AGI, o personagem ganha 1% de velocidade de ataque quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 0.5% de velocidade de ataque quando estiver utilizando equipamentos mágicos.
    - Para cada 1 ponto de AGI, o personagem ganha 0.2% de velocidade de ataque quando estiver utilizando equipamentos pesados.
    - Para cada 1 ponto de AGI, o personagem ganha 1% de velocidade de movimento quando estiver utilizando equipamentos leves.
    - Para cada 1 ponto de AGI, o personagem ganha 0.75% de velocidade de movimento quando estiver utilizando equipamentos mágicos.
    - Para cada 1 ponto de AGI, o personagem ganha 0.5% de velocidade de movimento quando estiver utilizando equipamentos pesados.
    - AGI é a fonte principal de evasão, mas a conversão em chance de evasão é feita por rendimento decrescente e está descrita em "Evasão".
- Personagens com mais AGI são capazes de utilizar equipamentos e armaduras mais leves.

### Especialidade (SPE)

- SPE aumenta ataques elementais e redução de recarga do personagem.
    - Para cada 1 ponto de SPE, o personagem ganha 1 ponto de dano elemental.
    - SPE é a fonte principal de redução de recarga, mas a conversão em porcentagem é feita por rendimento decrescente e está descrita em "Redução de recarga".
- Personagens com mais SPE são capazes de utilizar equipamentos e armaduras mágicos.

### Constituição (CON)

- CON ajuda a aumentar a vida máxima do personagem.
    - Para cada 1 ponto de CON, o personagem ganha 10 pontos de vida máxima.

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

### Dano físico

- É o dano causado através de ataques físicos e através do elemento terra.

### Dano elemental

- É o dano causado através dos elementos de fogo, água e elétrico.
- Nenhum elemento possui vantagem contra outros elementos.

### Regeneração de vida

- É a quantidade de pontos de vida que um personagem regenera por segundo.
- Regeneração de vida não é considerada uma cura.
- Qualquer valor de regeneração de vida abaixo de 0 é considerado como sendo 0.

### Evasão

- Atributo defensivo capaz de fazer com que o personagem desvie parcialmente um ataque, seja ele físico ou elemental.
- A chance de evasão é calculada por rendimento decrescente sobre o total de AGI do personagem:
    - `Chance de evasão = 100 × AGI ÷ (AGI + Constante)`
    - A constante é 100 enquanto o personagem estiver utilizando equipamentos leves.
    - A constante é 200 enquanto o personagem estiver utilizando equipamentos mágicos.
    - A constante é 500 enquanto o personagem estiver utilizando equipamentos pesados.
- Por exemplo, utilizando equipamentos leves:
    - Um personagem com 25 de AGI possui 20% de evasão.
    - Um personagem com 100 de AGI possui 50% de evasão.
    - Um personagem com 300 de AGI possui 75% de evasão.
    - Um personagem com 900 de AGI possui 90% de evasão.
- Por exemplo, utilizando equipamentos mágicos:
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

### Velocidade de movimento

- Atributo que indica o quão rápido um personagem se movimenta pelo campo de batalha.
- É medido em casas por segundo, da mesma forma que a velocidade de ataque é medida em ataques por segundo.
- Todo personagem possui 2 casas por segundo de velocidade de movimento base, e os ganhos vindos de AGI e de outras fontes são somados por cima desse valor.
    - Com esse valor, atravessar o campo de batalha inteiro leva 6 segundos e reposicionar uma casa leva meio segundo.
- Um personagem só se movimenta quando não possui nenhum alvo válido dentro do seu alcance, conforme descrito em `gameplay.md`.
- O valor mínimo de velocidade de movimento é 0, que indicaria que o personagem perdeu a habilidade de se movimentar.

### Redução de recarga

- Atributo ofensivo que indica o quão rápido um personagem utiliza suas habilidades.
- A redução de recarga é calculada por rendimento decrescente sobre o total de SPE do personagem:
    - `Redução de recarga = 60 × SPE ÷ (SPE + Constante)`
    - A constante é 60 enquanto o personagem estiver utilizando equipamentos leves.
    - A constante é 40 enquanto o personagem estiver utilizando equipamentos mágicos.
    - A constante é 300 enquanto o personagem estiver utilizando equipamentos pesados.
- Por exemplo, utilizando equipamentos mágicos:
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
- O roubo de vida deve sempre ser arredondado para o valor inteiro mais próximo. Por exemplo:
    - Valores abaixo de 0.5 se tornam 0 e igual ou acima se tornam 1.
    - Valores abaixo de 1.5 se tornam 1 e igual ou acima se tornam 2.
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

### Ocupação

- Atributo responsável por indicar quantas casas aquele personagem ocupa.
