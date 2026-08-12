# Progresso

## Objetivo

Especificar como funciona o progresso de Her-o-clock.

## Dinheiro

- Com dinheiro o jogador pode:
    - Investir na árvore de progresso para melhorar seu avanço.
    - Criar itens através do artesanato.
    - Quebrar itens para adquirir itens de artesanato.
- Enquanto o jogo está aberto **não existe teto de dinheiro**. O jogador acumula o quanto conseguir.
    - O jogo foi feito para ficar aberto em um cantinho da tela o dia inteiro. Um teto durante o jogo puniria exatamente o comportamento que o jogo quer incentivar.
- O que mantém o dinheiro escasso não é um teto, é a árvore de progresso ficar cada vez mais cara. Sempre existe um próximo nó valendo mais do que o jogador tem no bolso.
- O único teto de dinheiro é o da progressão offline, descrito na seção seguinte.

## Progressão offline

- Mesmo offline o jogador continua ganhando recursos.
- Para fazer esse cálculo, guardamos quanto de experiência e dinheiro o jogador fez na última hora online.
    - Essa referência se ajusta sozinha conforme o jogador fica mais forte, sem precisar de uma fórmula paralela para manter sincronizada.
- Com esse valor em mãos, o jogador mantém **25% do seu ritmo online** enquanto estiver offline.
- A progressão offline possui três tetos, e todos podem ser aumentados na árvore de progresso:
    - **Ritmo:** os 25% podem subir até 40%.
    - **Experiência:** no máximo 1 nível completo de cada personagem por ausência.
    - **Dinheiro:** no máximo o equivalente a 12 horas do ganho da última hora online.
- São os tetos, e não o ritmo, que limitam de verdade uma ausência longa.
    - Aumentar apenas o ritmo faz o jogador chegar mais cedo no mesmo teto, sem levar nada a mais. Por isso o caminho da árvore vende principalmente teto, e o ritmo é o complemento.
- Não é possível adquirir itens enquanto offline.
- Não é possível passar de fase enquanto offline.

### Exemplos

- Se o jogador saiu faltando 5% para seus personagens subirem ao nível 10 e permaneceu 1 mês fora, ao voltar seus personagens estarão no nível 10 com 95% de progresso.
- Se o jogador ganhava $1.000 por hora e permaneceu 1 mês fora, ao voltar ele terá recebido no máximo $12.000.

## Árvore de progresso

- A árvore de progresso possui uma sequência de caminhos para o jogador escolher comprar através de dinheiro adquirido ao eliminar inimigos.
- A ideia geral dessa árvore é fazer com que o jogador precise investir tempo para conseguir avançar no jogo através do farm.

### Custo dos nós

- O custo de um nó depende de **quantos nós o jogador já comprou**, e não de qual nó ele é.
    - O sexto nó comprado custa sempre o mesmo valor, independente do caminho escolhido. O sétimo custa sempre o mesmo, e assim por diante.
- É isso que torna a escolha honesta. Como todos os nós disponíveis custam igual, nunca existe um "mais barato" para o jogador correr atrás, e a decisão passa a ser exclusivamente qual dificuldade ele quer resolver primeiro.

### O custo cresce mais rápido que a renda

- O custo dos nós cresce mais rápido do que a renda do jogador ao longo do jogo. Ou seja, cada nó custa mais tempo de espera que o anterior.
- Isso é intencional e é o que dá vida longa ao dinheiro. Enquanto existir um próximo nó fora de alcance, o dinheiro tem propósito.
- Em um jogo idle esse custo é pago em **espera**, não em esforço. O jogador não fica moendo inimigos, ele deixa o jogo aberto e vive a vida dele. Por isso um nó caro aqui não é o mesmo castigo que seria em um jogo de farm ativo.
- Para calibrar a árvore, **projete a espera e derive o custo**, nunca o contrário.
    - Decida quanto tempo de jogo aberto cada trecho da árvore deve custar. Por exemplo: os primeiros nós custam alguns minutos, o meio da árvore custa horas, o fim custa dias.
    - O valor em dinheiro sai dessa decisão junto com a renda daquele momento do jogo.
    - Olhando para um custo de "$50.000.000" é impossível saber se o ritmo está bom. Olhando para "um dia de jogo aberto" é imediato.
- O único cenário ruim é o custo crescer **mais devagar** que a renda. Nesse caso o fim da árvore desaba justamente quando ela deveria ser o objetivo de longo prazo.

### Caminhos

- A árvore é distribuída em oito caminhos, e cada um resolve uma dificuldade diferente, que aperta em um momento diferente:
    - **Superior direito** — ganho de experiência dos heróis enquanto online.
    - **Inferior direito** — ganho de dinheiro enquanto online.
    - **Superior esquerdo** — quantidade de espaços no inventário.
    - **Inferior esquerdo** — quantidade de espaços e abas no baú.
    - **Vertical superior** — chance de adquirir melhores itens de artesanato ao eliminar inimigos.
    - **Vertical inferior** — chance de adquirir melhores itens ao eliminar inimigos.
    - **Horizontal direito** — progressão offline: os três tetos e o ritmo.
    - **Horizontal esquerdo** — artesanato: custo e eficiência da criação de itens.
- Nenhum caminho pode resolver a mesma dificuldade que outro. Dois caminhos servindo a mesma dor tornam a escolha entre eles arbitrária.
- Cada caminho precisa ter pelo menos 50 nós.

### Valor dos nós

- Os nós concedem valores baixos, para que nenhum caminho desande sozinho e fique óbvio investir em um só lado antes dos outros. Por exemplo:
    - "+10 de dinheiro ao eliminar um inimigo."
    - "+1% de dinheiro ao eliminar um inimigo."
    - "+10 de experiência ao eliminar um inimigo."
    - "+1% de experiência ao eliminar um inimigo."
    - "+0.2% de ritmo de progressão enquanto offline."
- O que precisa ser uniforme é a **magnitude** dos nós, não a **relevância** dos caminhos.
    - Espaço no baú não vale nada até o baú encher, e a partir dali vale muito. É essa variação que faz a escolha ser real.
    - Se todos os oito caminhos forem igualmente úteis o tempo todo, a decisão volta a ser arbitrária, só que por outro motivo.

## Árvore de habilidades

- Falaremos sobre a árvore de habilidades futuramente.
