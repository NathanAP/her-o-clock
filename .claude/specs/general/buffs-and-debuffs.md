# Buffs e debuffs

## Objetivo

- Detalhar cada buff e debuff do jogo.

## Sobre buffs e debuffs

- Os buffs são efeitos positivos aplicados de diversas formas aos personagens.
- Os debuffs são efeitos negativos aplicados de diversas formas aos personagens.
- As habilidades descrevem individualmente quais buffs e debuffs são aplicados aos seus alvos.
- Os valores que cada buff e debuff aplica são sempre descritos nas fichas.
- Buff e debuff são a mesma coisa por dentro. O que separa os dois é o sinal do valor, conforme o efeito `modify_stat` em `abilities.md`.

## O mesmo buff chegando duas vezes

- Quando um personagem recebe um buff ou debuff que ele já carrega, **vence o de maior valor e a duração é reiniciada**.
    - "Maior valor" é o mais forte na direção do efeito: entre dois buffs, o que mais aumenta; entre dois debuffs, o que mais diminui.
    - A duração é sempre a da aplicação nova, mesmo quando o valor que fica é o antigo. Renovar um efeito fraco não pode ser pior do que não fazer nada.
- **Eles não somam.** Dois heróis com a mesma habilidade de lentidão não deixam o inimigo duas vezes mais lento.
    - Sem essa regra, cada buff precisaria de um teto próprio, senão o grupo inteiro usando a mesma peça travaria um inimigo em zero. Seria uma constante a mais para calibrar em cada buff do jogo, e nenhuma delas seria discutível sozinha.
    - O custo é que uma build de acúmulo não existe. É uma perda aceitável perto de um sistema em que a conta do jogador depende de quantos aliados dele carregam a mesma peça.
- Efeitos **diferentes** sobre o mesmo atributo continuam se somando normalmente. A regra acima fala da mesma fonte chegando de novo, e não do atributo.
- Cada aplicação conta a sua própria duração a partir do instante em que entrou.

## Tipos

### De atributo primário

- Um buff é capaz de aumentar o total de atributos até 1000 pontos, ultrapassando o limite geral de 500.
- Um debuff é capaz de diminuir o total de atributos até 0 pontos.

### De atributo secundário

- Principais alvos dos buffs e debuffs, há muitos tipos de buffs e debuffs nessa categoria.

### Intangível

- Buff que permite que o portador se torne intangível por um tempo.
- Enquanto dura, o portador **não sofre dano de nenhuma fonte**.

### Inalvejável

- Buff que impede que o portador seja escolhido como alvo por um tempo.
- Ele deixa de ser candidato do ataque básico e das habilidades de alvo único, de cadeia e de linha.
- Uma habilidade em área **continua atingindo** o portador, pois ela não escolhe ninguém: ela cobre um retângulo e pega quem estiver dentro.
- Os dois existem separados porque resolvem coisas diferentes, e por isso é comum aparecerem juntos. Inalvejável tira o portador da mira; intangível tira o dano que veio de uma área ou que já estava a caminho.

### Cego

- Debuff que faz com que o portador erre uma porcentagem dos ataques básicos e não consiga usar habilidades de alvos únicos.
- Não afeta habilidades em área.

### Silenciado

- Debuff que faz com que o portador não possa usar habilidades por um tempo.
- Cancela automaticamente qualquer preparação sendo feita, aplicando quaisquer regra de cancelamento necessária.
