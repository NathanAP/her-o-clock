# Atributos

## Objetivo

Neste arquivo é possível encontrar detalhes de cada atributo presente no jogo.

## Atributos principais

- Os atributos principais estão presentes em todos os personagens.
- Cada herói possui uma base de atributos principais.
- Os jogadores escolhem quais atributos principais desejam aumentar cada vez que um herói sobe de nível.
    - Cabe ao jogador decidir como ele prefere fazer a distribuição.
    - Os pontos de atributos ganhos nos níveis podem ser redefinidos a qualquer momento.
- Os jogadores podem aumentar as atributos principais através de outros meios (itens, árvore de habilidades, árvore de progresso).

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
- Os atributos secundários não podem ser aumentados diretamente com pontos de atributos, apenas através de outros meios (itens, árvore de habilidades, entre outros).

### Dano físico

- É o dano causado através de ataques físicos e através do elemento terra.

### Dano elemental

- É o dano causado através dos elementos de fogo, água e elétrico.
- Nenhum elemento possui vantagem contra outros elementos.

### Regeneração de vida

- É a quantidade de pontos de vida que um personagem regenera por segundo.
- Regeneração de vida não é considerada uma cura.
- Qualquer valor abaixo de regeneração de vida abaixo de 0 é considerado como sendo 0.

### Evasão

- Atributo defensivo capaz de fazer com que o personagem desvie parcialmente um ataque, seja ele físico ou elemental.
- Ao fazer um teste de evasão 30% do valor de evasão se torna chance de evasão perfeita. Por exemplo:
    - Se um personagem possui 10% de evasão, há 3% de chance dessa evasão ser perfeita.
    - Se um personagem possui 20% de evasão, há 6% de chance dessa evasão ser perfeita.
- Uma evasão perfeita elimina 75% do dano que seria causado ao personagem.
    - Esse valor pode ser potencializado por outros meios (itens, árvore de habilidades, entre outros).
- Uma evasão normal elimina 40% do dano que seria causado ao personagem.
    - Esse valor pode ser potencializado por outros meios (itens, árvore de habilidades, entre outros).
- Se um personagem possuir mais de 100% de evasão, a sua evasão sempre vai ser considerada como 100%.
- Qualquer valor abaixo de evasão abaixo de 0 é considerado como sendo 0.

### Velocidade de ataque

- Atributo ofensivo que indica quantos ataques básicos por segundo um personagem faz.
- Esse atributo se diz respeito exclusivamente aos ataques básicos dos personagens.
- O valor mínimo de velocidade de ataque é 0, que indicaria que o personagem perdeu a habilidade de fazer ataques básicos.

### Redução de recarga

- Atributo ofensivo que indica o quão rápido um personagem utiliza suas habilidades.
- A redução de recarga é calculada junto com o valor de recarga de uma habilidade. Por exemplo:
    - Se uma habilidade possui 10 segundos de recarga e o personagem possui 50% de redução de recarga, aquele personagem pode usar essa habilidade a cada 5 segundos.
    - Se uma habilidade possui 1 minuto de recarga e o personagem possui 50% de redução de recarga, aquele personagem pode usar essa habilidade a cada 30 segundos.
- Se um personagem possuir mais de 60% de redução de recarga, a sua redução de recarga sempre vai ser considerada como 60%.
- Qualquer valor abaixo de redução de recarga abaixo de 0 é considerado como sendo 0.

### Armadura física

- Atributo defensivo capaz de fazer com que o personagem evite parcialmente um ataque físico.
- Diferente da evasão, a armadura física evita diretamente o ataque de acordo com a potência da armadura física. Por exemplo:
    - Se um personagem possui 100 de armadura física e recebe 300 de dano físico, apenas 200 de dano é recebido.
    - Se um personagem possui 500 de armadura física e recebe 1000 de dano físico, apenas 500 de dano é recebido.
    - Se um personagem possui 1000 de armadura física e recebe 200 de dano físico, nenhum dano é recebido.
- A armadura física escala infinitamente mas não pode ficar abaixo de 0.

### Resistência elemental

- Atributo defensivo capaz de fazer com que o personagem evite parcialmente um ataque elemental.
- Personagens possuem resistência elemental para cada elemento individualmente.
    - Ou seja, a resistência ao elemento A não mitiga dano de elemento B e nem C.
- Diferente da armadura física, a resistência elemental mitiga o dano em % de acodo com a potência da resistência. Por exemplo:
    - Se um personagem possui 30% do elemento A e recebe 1000 de dano de elemento A, apenas 700 de dano é recebido.
    - Se um personagem possui 30% do elemento B e recebe 1000 de dano do elemento C, o dano completo é recebido.
- A resistência elemental escala infinitamente para positivo ou negativo. Ou seja:
    - Se o personagem possuir 200% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai curar 1000 pontos de vida.
    - Se o personagem possuir 150% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai curar 500 pontos de vida.
    - Se o personagem possuir -100% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai receber 2000 pontos de dano.
    - Se o personagem possuir -50% de resistência ao elemento A e recebe 1000 de dano do elemento A, ele vai receber 1500 pontos de dano.
- Receber vida por possuir mais de 100% de resistência elemental é considerada uma cura.

### Roubo de vida

- Atributo ofensivo que faz com que o personagem se cure ao desferir um ataque físico.
- O roubo de vida também é calculado em habilidades.
- O roubo de vida deve sempre ser arredondado para o valor inteiro mai próximo. Por exemplo:
    - Valores abaixo de 0.5 se tornam 0 e igual ou acima se tornam 1.
    - Valores abaixo de 1.5 se tornam 1 e igual ou acima se tornam 2.
- O cálculo de roubo de vida é feito sempre em cima do dano final causado. Por exemplo:
    - Se um personagem causa 100 de dano físico e possui 3% de roubo de vida, ele vai curar 3 pontos de vida.
    - Se um personagem tenta causar 100 de dano ao adversário e o dano for mitigado para apenas 10 (por conta de armadura ou evasão), ele não vai receber cura (cálculo terminado em 0.3 e arredondado para 0).
    - Se um personagem causa 100 de dano elemental e possui 3% de roubo de vida, ele não recebe cura.
- O total roubado não pode ser abaixo de 0.
- Receber vida por roubo de vida é considerada uma cura.
