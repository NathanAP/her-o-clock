# Progresso

## Objetivo

Especificar como funciona o progresso de Her-o-clock: experiência, nível, dinheiro, o que acontece enquanto o jogador está fora, e as árvores que ele compra com o que juntou.

## As três passadas

- O jogo é jogado três vezes, sempre pelos mesmos atos, com dificuldade crescente a cada volta.
- Terminar a história uma vez não termina o jogo. É a segunda e a terceira volta que carregam a maior parte da progressão.
- Os níveis aproximados em que cada passada termina são:
    - **Primeira passada:** o jogador chega ao fim do último ato por volta do nível 35.
    - **Segunda passada:** por volta do nível 85.
    - **Terceira passada:** é onde o nível deixa de ser o que importa e a otimização de build e de itens assume.
- A terceira passada sobrar pouco nível é intencional. Se ela ainda dependesse de subir de nível, competiria com o próprio conteúdo que ela deveria apresentar.
- O que decide onde cada passada termina **não é a curva de experiência, é a quantidade de fases em cada ato**. Mais fases não deixam o jogo mais rápido, deixam o mesmo tempo mais variado. É esse o parâmetro a ajustar quando os atos existirem.

## Experiência e nível

### As duas fórmulas

```
xpParaSubir(nível do herói)   = 50 × nível^3.55
xpPorLacaio(nível do lacaio)  = 10 × nível^2
```

- Um vilão vale **10 lacaios** do mesmo nível, tanto em experiência quanto em dinheiro.
    - Com isso o vilão sozinho responde por quase metade da experiência de uma fase, e completar a fase paga muito melhor do que ficar repetindo as ondas.
    - A mesma regra vale para os dois recursos de propósito, para não existirem duas regras a calibrar depois.
- Cada lacaio concede **1 de dinheiro**, independente do nível.
    - Esse valor é provisório. O número certo só existe quando a árvore de progresso existir, pois é a curva de custo dela que decide quanto dinheiro precisa entrar no jogo.
- A experiência de um inimigo depende do **nível dele**, nunca do nível de quem o derrotou.
    - É isso que impede o jogador de farmar eternamente no ato 1 e o obriga a avançar para continuar evoluindo.

### Por que existe um expoente (e por que é 3.55)

- O que controla o ritmo do jogo inteiro não são as constantes, é a **diferença entre os dois expoentes**. Ela define quantos inimigos do próprio nível são necessários para subir um nível, e essa quantidade cresce conforme o jogador avança.
- O valor foi calibrado contra o Task Bar Hero numa progressão gratuita, que é a referência mais próxima do gênero: lá, 250 horas de jogo levam ao nível 70 de 80, ou seja **87% do máximo**. Com 3.55, as mesmas 250 horas levam ao nível 86 de 100.
- Com esse expoente, os últimos 14 níveis custam mais de 100 horas sozinhos. O nível 100 existe, mas ninguém precisa alcançá-lo, que é o comportamento desejado.

### Quem ganha experiência

- Todos os heróis do grupo ativo ganham a experiência dos inimigos derrotados.
- **Heróis caídos continuam ganhando experiência** até o fim da fase.
    - Sem essa regra, um herói fraco carregado por um grupo forte não ganharia quase nada, pois morreria logo na primeira onda.
- **Heróis fora do grupo não ganham experiência.**
- Todo herói recém-liberado começa no **nível 1**, independente do nível dos demais.

### O custo de trocar de herói, e o item que o resolve

- As duas regras acima significam que colocar um herói novo no grupo custa caro. Subir do nível 1 ao 100 sendo carregado por um grupo que farma no nível máximo leva cerca de 200 horas, contra 366 horas no ritmo normal.
    - Ser carregado só compensa quando o grupo farma perto do nível máximo. Em fases de nível intermediário, o herói novo recebe menos experiência por inimigo do que receberia subindo no próprio ritmo.
- Esse custo é resolvido por **itens que concedem experiência ao banco**.
- O item ocupa um espaço de equipamento em um **herói ativo** e concede experiência aos heróis fora do grupo.
    - Se ele ficasse equipado no próprio herói do banco não disputaria espaço com nada, e a resposta certa seria sempre equipá-lo. Não haveria escolha.
    - Ocupando um espaço ativo, ele troca poder de combate agora por variedade de time depois. Perto de uma parede o jogador tira o item, farmando confortável ele põe.

## Atributos por nível

- Ao subir de nível, todo personagem recebe **5 pontos de atributos principais** e **1 ponto para a árvore de habilidades**.
- Cada ficha declara como aquele personagem distribui os 5 pontos, em porcentagens:

```json
"attributeGrowth": { "pow": 40, "agi": 20, "spe": 0, "con": 40 }
```

- Para **heróis**, essa é a distribuição padrão. Ela é aplicada sozinha enquanto o herói estiver com a distribuição automática ligada, e o jogador pode desligá-la para colocar os pontos à mão. Os controles estão descritos em `attributes.md`.
- Para **lacaios e vilões**, essa é a única forma que eles têm de ficar mais fortes, pois não existe ninguém distribuindo pontos por eles. Eles são personagens com o automático permanentemente ligado.
- São quatro números por ficha, e não uma tabela de valores por nível. O nível faz o resto.
- A parte automática é sempre recalculada a partir do total, e nunca somada de cinco em cinco.
    - O arredondamento das porcentagens é resolvido por maior resto, então distribuir 5 pontos trinta e nove vezes não dá o mesmo resultado que distribuir 195 de uma vez.
    - É isso que garante que um lacaio criado direto no nível 40 tenha exatamente os mesmos atributos de um que subiu do 1 até lá, o que a reprodutibilidade das batalhas depende.

### A ficha diz quem o personagem é, a fase diz quão forte ele está

- A **ficha** define a identidade: os atributos base e a distribuição por nível. Se um lacaio é resistente e lento, ele é assim em todos os atos.
- A **fase** define a instância: o nível daquele inimigo e, quando necessário, um multiplicador de ajuste fino.
- Essa divisão é o que permite reaproveitar o mesmo personagem sem duplicar dados:
    - Um vilão que invoca lacaios fracos do primeiro ato usa a mesma ficha com nível baixo.
    - Vilões que voltam no ato final em dupla ou trio usam a mesma ficha com nível alto, posicionados mais de uma vez.
    - Nerfar ou reforçar uma fase específica é mexer no multiplicador dela, sem afetar nenhuma outra.
- Se a fase declarasse os atributos inteiros, o mesmo bloco de números estaria copiado em centenas de arquivos, e a identidade do personagem deixaria de existir em algum lugar.

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
- O tempo ausente é contado a partir do **último save gravado**, e nunca de um horário de saída.
    - É o único instante em que se pode confiar. Um processo morto não avisa que está fechando, então um horário de saída simplesmente não existiria nesse caso.
    - Isso liga a frequência do save à precisão do cálculo: o jogador perde no máximo o intervalo entre dois saves. Os momentos em que o jogo grava estão em `save.md`.

### A progressão offline é um cálculo, nunca uma simulação

- O jogo **não reproduz combates** para descobrir o que aconteceu enquanto o jogador esteve fora. Ele multiplica o ritmo da última hora pelo tempo de ausência e aplica os tetos.
- As três proibições desta seção são justamente o que torna isso possível, e cada uma remove uma pergunta que só um combate de verdade saberia responder:
    - Sem queda de itens, não é preciso sortear nada.
    - Sem avanço de fase, não é preciso saber se o grupo venceria.
    - Com teto de 1 nível, o ritmo não muda no meio do cálculo.
- Permitir qualquer uma dessas três coisas offline significaria escrever um simulador de combate. É uma decisão bem maior do que parece, e por isso está registrada aqui.

### Como a referência da última hora é medida

- O jogo guarda o que foi ganho em **baldes de 10 minutos**, mantendo os seis últimos, que somam uma hora.
- Cada balde guarda tudo que a progressão offline e as estatísticas precisam: experiência, dinheiro, inimigos derrotados, dano causado, dano recebido e cura.
- A taxa é a soma dos baldes dividida pelo tempo que eles cobrem, e não por uma hora fixa.
    - Se o jogador só tem dois baldes, a taxa é a soma deles dividida por 20 minutos. Assim uma sessão curta não é lida como uma hora fraca.
- Os baldes são guardados no save.
    - Sem isso, quem joga em sessões de poucos minutos ao longo do dia perderia a referência toda vez que fechasse o jogo, e seria punido justamente por jogar pouco.
- **O ganho offline não entra nos baldes.** Eles medem apenas jogo aberto.
    - Se o crédito de uma ausência alimentasse a referência, ela passaria a se medir por si mesma, e cada ausência inflaria a próxima.
- **Voltar não limpa os baldes.** Eles ficam velhos, mas continuam descrevendo o mesmo time, que não enfraqueceu enquanto o jogo estava fechado.
    - Limpar faria os primeiros minutos depois da volta serem lidos como uma hora fraca, punindo justamente quem faz uma segunda ausência curta em seguida.

### Os tetos

A progressão offline possui três tetos, e todos podem ser aumentados na árvore de progresso:

- **Ritmo:** os 25% podem subir até 40%.
- **Experiência:** cada personagem sobe **no máximo 1 nível** por ausência, mantendo a fração de progresso que tinha dentro do nível.
    - Quem saiu com 95% do caminho para o nível 10 volta no nível 10 com 95% do caminho para o 11, não importa quanto tempo ficou fora.
    - Preservar a fração, em vez de creditar uma quantidade fixa de experiência, é o que faz o teto valer exatamente um nível em qualquer ponto do jogo. Creditar "o custo de um nível" renderia menos de um nível, porque o nível seguinte sempre custa mais que o atual.
- **Dinheiro:** no máximo o equivalente a **12 horas** do ganho da última hora online.
    - O teto é calculado sobre o ritmo **online**, e não sobre os 25% do ritmo offline. Um jogador que fazia $1.000 por hora leva no máximo 12 × $1.000, e não 12 × $250.

São os tetos, e não o ritmo, que limitam de verdade uma ausência longa.

- Aumentar apenas o ritmo faz o jogador chegar mais cedo no mesmo teto, sem levar nada a mais. Por isso o caminho da árvore vende principalmente teto, e o ritmo é o complemento.
- Os tetos são também a única defesa que existe contra o jogador adiantar o relógio do sistema, conforme "## O relógio" em `save.md`. Um mês de ausência rende o mesmo que 12 horas, então adiantar o relógio não leva a lugar nenhum. Quem mexer nos tetos mexe nas duas coisas ao mesmo tempo.

Além dos tetos:

- Não é possível adquirir itens enquanto offline.
- Não é possível passar de fase enquanto offline.

### Estatísticas do período offline

- As estatísticas mostradas ao voltar saem dos mesmos baldes, multiplicadas do mesmo jeito que a experiência e o dinheiro.
- Elas nunca podem ser calculadas por outro caminho. Se a tela disser que 1.432 inimigos foram derrotados mas a experiência creditada corresponder a 800, o jogador percebe a inconsistência.

### Exemplos

- Se o jogador saiu faltando 5% para seus personagens subirem ao nível 10 e permaneceu 1 mês fora, ao voltar seus personagens estarão no nível 10 com 95% de progresso.
- Se o jogador ganhava $1.000 por hora e permaneceu 1 mês fora, ao voltar ele terá recebido no máximo $12.000.
- Se o jogador ganhava $600 por hora e permaneceu 2 horas fora, ele recebe **$300**, pois `600 × 2 × 25% = 300`. O teto daquele save é $7.200, e nem chega perto de ser alcançado.
- Se o jogador tem apenas 2 baldes preenchidos, somando $100, o ritmo dele é de **$300 por hora**, pois os dois baldes cobrem 20 minutos.

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

## Sobre os números desta spec

- As horas citadas aqui foram calculadas em cima das fichas de teste que existem hoje, que rendem cerca de **11 inimigos por minuto** de jogo aberto.
- Elas servem para comparar decisões entre si e para saber se um ritmo é de horas, de dias ou de meses. Não são promessas.
- Quando existirem heróis, lacaios e vilões de verdade, o ritmo de combate muda e todas as estimativas precisam ser refeitas. As fórmulas continuam valendo, os prazos não.
