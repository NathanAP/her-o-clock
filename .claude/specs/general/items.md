# Itens

## Objetivo

Especificar todos os detalhes gerais sobre os itens presentes em Her-o-clock.

## Onde os números moram

- Este arquivo descreve as regras. Os números que as regras usam ficam em `Assets/Items/`, um arquivo por assunto:
    - `slots.json` traz os slots, o peso defensivo de cada um, o que cada classe faz com a defesa base e o requerimento de atributo.
    - `subtypes.json` traz os subtipos de arma e de mão secundária defensiva, com mãos, alcance, velocidade e faixa de dano.
    - `modifiers.json` traz todos os modificadores que um item não-único pode sortear.
    - `tiers.json` traz a escada de camadas e a distribuição de camadas por nível do item.
    - `uniques.json` traz as regras que valem para todos os itens únicos, incluindo a tabela de grau de poder.
    - `Uniques/` traz um arquivo por item único.
    - `itemStrings.json` traz todo o texto que o jogador lê, no mesmo formato que as fases já usam.
- **Cada número mora em exatamente um arquivo, e este `.md` não repete nenhum deles.** Uma tabela dentro de um `.md` não é conferível por teste e envelhece em silêncio; uma segunda cópia do arquivo envelhece do mesmo jeito, e foi o que aconteceu com as fases antes desta regra existir.
- Ids, chaves e valores internos são escritos em `camelCase`, conforme "## Como um JSON é escrito" em `file-system.md`. A regra vale para o projeto inteiro e não só para os itens.

## Equipamentos

- Os equipamentos são itens que os heróis obtêm e podem ser usados quando os requerimentos forem atingidos.
    - Dito isso, se o herói não atender mais o requerimento de um item já equipado, aquele item é desconsiderado. Visualmente falando ele fica com uma borda vermelha para indicar que o item está inativo.

#### Quais equipamentos contam

- **Um item inativo é desconsiderado por inteiro.** Ele não dá defesa, não dá atributo e não conta na mistura de classe do personagem. Não existe meio ativo.
- A ativação é resolvida assim, e a regra vale para o conjunto todo de uma vez:
    1. Comece com **nenhum** item ativo.
    2. Ative todo item vestido cujo requerimento seja atendido pelos atributos do herói **mais os itens já ativos**.
    3. Repita até que uma passada não ative mais nada.
- **Um item já ativo pode pagar o requerimento de outro.** É isso que permite montar um conjunto que sozinho o herói não sustentaria.
- **Dois itens que só se sustentam mutuamente ficam os dois inativos**, porque não existe um primeiro para vestir. Reorganizar os atributos para conseguir vestir os dois é trabalho do jogador.
- A regra responde uma pergunta só: *daria para vestir estes itens um de cada vez, em alguma ordem?* É exatamente o que o jogador faz, e por isso o resultado nunca depende da ordem em que os slots são olhados.
- A conta é feita quando a fase começa, junto de todo o resto que constrói quem luta.
- Apenas heróis usam equipamentos.
- Os equipamentos não alteram sprites, skins, animações ou efeitos do herói.

### Tipo de equipamento

- Cada tipo de equipamento corresponde a um slot de equipamento que o herói pode utilizar.
- Cada equipamento possui características básicas em comum.
    - Todos os equipamentos possuem:
        - Id gerado automaticamente.
        - Nome gerado automaticamente.
        - Nível de equipamento entre 1 e 100.
        - Classe entre POW, AGI e/ou SPE.
        - Subtipo.
        - Tecnologia.
        - Modificadores conforme a tecnologia.
    - Os equipamentos defensivos possuem em comum:
        - Armadura (POW) / Evasão (AGI) / Resistência elemental (SPE) base escalada com o nível do item.
        - **As três são pontos na mesma escala**, porque `attributes.md` passa as três pela mesma curva contra a mesma constante. O orçamento de evasão é maior que o de armadura em `slots.json`.
            - Dessa forma, a armadura corta 75% no teto e a evasão média corta 50.5%, então pontos iguais valeriam menos. Os orçamentos são escolhidos para que a redução média das duas seja equivalente, e o que separa uma da outra é a variância e não a força.
    - Os equipamentos ofensivos possuem em comum:
        - Faixa de dano escalada com o nível do item.
        - Velocidade de ataque, que substitui a do personagem enquanto a arma estiver equipada.
        - Alcance mínimo e máximo, que substitui o do personagem enquanto a arma estiver equipada.

#### Os oito slots

- Os nomes que o jogador lê são em inglês, conforme a regra de idioma do projeto, e os nomes em português existem para esta pasta de specs.

| Id              | Nome do jogador | Nome na spec       | Papel                 |
| --------------- | --------------- | ------------------ | --------------------- |
| `cranialCasing` | Cranial Casing  | Casco craniano     | Defensivo, cabeça     |
| `chassis`       | Chassis         | Chassi             | Defensivo, corpo      |
| `armServos`     | Arm Servos      | Servos braçais     | Defensivo, braços     |
| `tractionUnits` | Traction Units  | Unidades de tração | Defensivo, pernas     |
| `mainHand`      | Main Hand       | Mão primária       | Ofensivo              |
| `offHand`       | Off Hand        | Mão secundária     | Ofensivo ou defensivo |
| `controller`    | Controller      | Controlador        | Utilitário            |
| `firmware`      | Firmware        | Firmware           | Utilitário            |

- **"Casco" continua sendo o nome da categoria**, e não o nome de nenhum slot. Os quatro defensivos são os cascos quando falamos deles em conjunto, e cada um tem nome próprio na tela.
    - A nomenclatura anterior separava os cascos por posição relativa, e "superior" disputava a mesma região mental que "corporal". Quando um item cai no chão o jogador precisa saber o slot em menos de meio segundo, e anatomia entrega isso enquanto posição relativa não entrega.
- **Mão primária e secundária continuam sendo mãos.** Robôs têm mãos, e trocar isso por um nome técnico seria inventar dificuldade em cima do vocabulário que todo jogador de RPG já lê sem pensar.

#### Casco craniano

- Equipamento defensivo instalado na cabeça do herói.

#### Chassi

- Equipamento defensivo instalado no corpo do herói.
- É o slot de maior peso defensivo, conforme `slots.json`.

#### Servos braçais

- Equipamento defensivo instalado nos braços do herói.
- É o slot de menor peso defensivo, conforme `slots.json`.

#### Unidades de tração

- Equipamento defensivo instalado nas pernas do herói.

#### Mão primária

- Corresponde a quais equipamentos ofensivos o herói usará para combater seus inimigos.
- É a principal fonte de dano do herói.
- O jogador pode escolher entre usar armas de uma ou duas mãos.
    - No primeiro caso, o jogador pode utilizar duas armas de uma mão ou uma arma e mais um equipamento defensivo na mão secundária.
    - No segundo caso, o jogador não poderá equipar itens na mão secundária.
- **A arma substitui três coisas da ficha do personagem enquanto estiver equipada**: o dano base, a velocidade de ataque e o alcance.
    - Ela substitui o **valor base** de cada uma, e nunca o resultado. A velocidade continua sendo `arma × (1 + AGI × taxa da classe)`, então quem investe em AGI continua ganhando com isso — e sem essa parte a classe leve perderia metade do sentido.
        - Exemplo: uma heroína de classe leve com 20 de AGI segurando uma Claw ataca `1.7 × (1 + 20 × 0.01) = 2.04` vezes por segundo. A mesma heroína de mãos vazias ataca `1 × 1.2 = 1.2` vezes.
        - O mesmo vale para o dano: a faixa da arma é multiplicada por POW igual a faixa da ficha seria. O que troca é de onde a faixa vem.
    - A faixa de dano de uma arma cresce com o **nível do item**, e não com o nível de quem a segura. Uma heroína de nível 1 com uma arma de nível 60 bate como uma arma de nível 60.
    - A velocidade e o alcance passam a ser assim porque sem isso o subtipo não significa nada. Uma Claw e um Piledriver que atacam na mesma velocidade e à mesma distância são a mesma arma com números diferentes.
    - Sem arma, o herói usa os três valores da própria ficha: o soco dele, a velocidade 1 e o alcance declarado.
    - **A mão primária vazia significa não ter arma nenhuma**, mesmo com uma arma na secundária. A velocidade e o alcance saem da primária, então não há de onde tirá-los, e o herói soca com a ficha.
- **O ataque à distância desenha um projétil apenas nas famílias à distância**, conforme `subtypes.json`. Uma arma corpo a corpo que alcança duas ou três casas continua sendo alguém golpeando de mais longe, e não um tiro: quem decide alcance são os dois números do alcance, e o desenho é só desenho.

#### Mão secundária

- Corresponde a equipamentos ofensivos ou defensivos que o herói usa, porém fica vazio caso o herói use uma arma de duas mãos na mão primária.
- Quando o herói usa duas armas de uma mão, a velocidade e o alcance efetivos são os da mão primária.
- **Os golpes alternam as mãos**: o primeiro sai da primária, o segundo da secundária, o terceiro da primária, e assim por diante. Cada golpe usa a faixa de dano da arma que golpeou.
- **Os modificadores das duas armas valem sempre**, e não apenas no golpe da mão que os carrega. Roubo de vida, dano elemental e espinhos são do herói, e o que pertence à mão é só o dano.
    - Fazer o contrário significaria que os atributos do herói mudam de um golpe para o outro, o que é um jogo diferente do que este arquivo descreve.
- **Equipar uma arma de duas mãos desequipa as duas mãos**, tanto a arma que estava na primária quanto o que estava na secundária.
    - A regra vale nos dois sentidos: equipar qualquer coisa na secundária enquanto uma arma de duas mãos está na primária tira aquela arma. A secundária estar vazia com uma arma de duas mãos é uma invariante, e uma invariante que só vale por um lado não é invariante.
    - Enquanto não existe inventário, o que sai não tem para onde ir.

#### Controlador

- Corresponde a equipamentos utilitários do herói. É um slot que pode ser usado para ajudar nos ganhos ofensivos e defensivos pro herói.
- É equivalente a um amuleto em jogos de RPG tradicionais.
- Não possui defesa base. Ele existe exclusivamente para carregar modificadores.

#### Firmware

- Tem um papel parecido com o do controlador de fornecer um slot para ajudar a ganhar números ofensivos e defensivos.
- Também não possui defesa base.

### Subtipos

- O subtipo é o que diferencia dois itens do mesmo slot e da mesma classe.
- **Nas armas o subtipo é mecânico. Nos cascos ele é apenas nominal.**
    - Nos cascos, a classe já ocupa o eixo mecânico: pesado dá armadura, leve dá evasão, especial dá resistência elemental. Um segundo eixo faria os dois disputarem o mesmo espaço, e um dos dois viraria decoração.
    - O subtipo de um casco é, então, o substantivo que entra no nome dele, e é derivado da classe. A tabela está em `slots.json`, no bloco `armourNaming`.

#### Subtipos de arma

- Cada subtipo carrega quatro números, e é a diferença entre eles que faz build existir: quantas mãos ocupa, qual o alcance, qual a velocidade de ataque e qual a faixa de dano.
- Os números estão em `subtypes.json`. Esta é a leitura deles:

| Subtipo    | Mãos | Alcance       | Velocidade   | Faixa de dano  | Classe natural |
| ---------- | ---- | ------------- | ------------ | -------------- | -------------- |
| Blade      | 1    | Corpo a corpo | Rápida       | Estreita       | Leve           |
| Claw       | 1    | Corpo a corpo | Muito rápida | Muito estreita | Leve           |
| Ram        | 1    | Corpo a corpo | Lenta        | Larga          | Pesado         |
| Prod       | 1    | Corpo a corpo | Média        | Média          | Médio          |
| Catalyst   | 1    | Curto         | Média        | Estreita       | Especial       |
| Emitter    | 1    | Longo         | Rápida       | Estreita       | Leve especial  |
| Greatblade | 2    | Corpo a corpo | Média        | Larga          | Médio          |
| Piledriver | 2    | Corpo a corpo | Muito lenta  | Muito larga    | Pesado         |
| Lance      | 2    | Curto         | Lenta        | Média          | Pesado         |
| Rifle      | 2    | Longo         | Média        | Média          | Leve           |
| Cannon     | 2    | Muito longo   | Muito lenta  | Muito larga    | Pesado         |
| Reactor    | 2    | Curto         | Lenta        | Média          | Especial       |

- **A faixa de dano de cada subtipo é derivada de um orçamento de dano por segundo**, e não escolhida solta.
    - `Dano médio × Velocidade de ataque = Orçamento da família`.
    - As famílias e os orçamentos estão em `subtypes.json`, no bloco `damageBudget`.
    - É isso que permite Piledriver e Claw serem escolhas de verdade, e não uma delas ser estritamente melhor. Quem escolhe a arma lenta compra picos grandes e sofre mais com evasão; quem escolhe a rápida compra consistência e se beneficia mais de roubo de vida.
- **Arma de duas mãos rende mais dano por segundo que uma de uma mão**, e o que ela paga é a mão secundária inteira.
- **Arma à distância rende menos dano por segundo que a corpo a corpo**, e o que ela compra é não precisar atravessar o tabuleiro.
- **Uma arma corpo a corpo que alcança mais de uma casa paga um desconto por isso**, porque ela ataca de longe sem abrir mão de atacar colado, e isso não tem contrapartida nenhuma. São Lance, Catalyst e Reactor, e o desconto se acumula com o das especiais.
- **Catalyst e Reactor rendem menos que o resto**, porque quem carrega eles não luta com ataque básico. O retorno deles vem da classe especial e dos modificadores de habilidade.
- **A classe natural é uma tendência e não uma trava.** Um Cannon leve é possível e deve ser possível, porque é exatamente esse tipo de item fora do padrão que torna uma build fora do meta viável.

#### Subtipos defensivos de mão secundária

- Existem três, e cada um entrega uma das três defesas do jogo:
    - **Bulwark** entrega armadura.
    - **Deflector** entrega evasão.
    - **Battery** entrega resistência elemental.

### Classes

- Todo equipamento pertence a uma das classes abaixo.
- A classe do equipamento define como os atributos principais do herói são convertidos em atributos secundários, conforme descrito em `attributes.md`.
- **Nenhum equipamento decide isso sozinho.** Cada um entrega uma fatia, e a classe do herói é a mistura das fatias de tudo que ele veste — um herói de quatro cascos pesados e uma arma leve fica entre pesado e leve, e não em nenhum dos dois. A regra e o exemplo numérico estão em "A classe do personagem", em `attributes.md`.
    - As três classes híbridas entram lá como meia fatia para cada lado, e é por isso que `attributes.md` continua conhecendo apenas três classes enquanto os equipamentos têm seis.
- Cada classe é liberada por um atributo principal diferente, então a escolha do equipamento acompanha naturalmente a build do herói.
- **O requerimento de atributo cresce com o nível do item**, e a fórmula está em `slots.json`, no bloco `requirement`.
    - Classes híbridas exigem os dois atributos, cada um a uma fração do valor cheio. Vestir híbrido custa mais no total e menos em cada lado, e é isso que faz a escolha existir.

#### Pesado

- Liberado por POW.
- Características voltadas à armadura física, fazendo o herói absorver parcialmente os ataques.
- Torna o herói menos eficaz em AGI e SPE.

#### Leve

- Liberado por AGI.
- Características voltadas à agilidade, fazendo o herói ter velocidade de ataque, velocidade de movimento e evasão.
- Torna o herói menos eficaz em POW e SPE.

#### Especial

- Liberado por SPE.
- Características voltadas às habilidades e resistência elemental, fazendo o herói desferir habilidades mais rapidamente e ser mais resistente a outros ataques elementais.
- Torna o herói menos eficaz em POW e AGI.
- Apesar do nome, todo item especial continua sendo tecnologia, assim como todo o resto do universo de Unia.

#### Médio

- Liberado por POW / AGI.
- Classe híbrida que possui parcialmente características de ambas.

#### Leve especial

- Liberado por AGI / SPE.
- Classe híbrida que possui parcialmente características de ambas.

#### Pesado especial

- Liberado por POW / SPE.
- Classe híbrida que possui parcialmente características de ambas.

### Tecnologia

- Todo equipamento pertence a uma das tecnologias abaixo.
- A tecnologia indica o quão modificado aquele equipamento é.
- Tecnologia é diferente de raridade: conseguir itens mais tecnológicos é normal, porém conseguir os modificadores corretos que fazem a raridade ser elevada.
- **A tecnologia decide apenas a quantidade de modificadores, nunca a qualidade deles.** Quem decide a qualidade é a camada, e a camada depende do nível do item.
    - É por isso que um item Convencional com dois modificadores de camada alta pode valer mais que um Quântico com seis de camada baixa.
- Itens com menor tecnologia não necessariamente são piores que outro equipamento de alta tecnologia. Por exemplo:
    - O herói pode usar um equipamento de alta tecnologia com pouca armadura, mas acaba de pegar um equipamento sem tecnologia com muito mais.
    - O herói pode usar um equipamento de alta tecnologia que aumenta pouco suas habilidades mas acaba de pegar um de tecnologia rudimentar que aumenta muito mais.
- **A quantidade sorteada é o máximo da tecnologia ou o máximo menos um**, conforme `tiers.json`. Sem isso, dois itens da mesma tecnologia teriam sempre o mesmo tamanho.
- Futuramente teremos opções para permitir a junção de 3 itens da mesma tecnologia para se obter um novo equipamento aleatório de até um nível de tecnologia acima.

#### Sem tecnologia

- Itens que não trazem modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração transparente.

#### Rudimentar

- Itens que trazem até 1 modificador.
- O nome e o ícone no inventário e no baú possuem a coloração cinza metálico.
- Podem ser obtidos a partir da primeira passada no ato 1 e fase 1.

#### Convencional

- Itens que trazem até 2 modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração verde.
- Podem ser obtidos a partir da primeira passada no ato 1 e fase 1.

#### Otimizada

- Itens que trazem até 3 modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração azul elétrico.
- Podem ser obtidos a partir da primeira passada no ato 2 e fase 1.

#### Avançada

- Itens que trazem até 4 modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração violeta.
- Podem ser obtidos a partir da segunda passada no ato 1 e fase 1.

#### Nano

- Itens que trazem até 5 modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração ciano.
- Podem ser obtidos a partir da segunda passada no ato 4 e fase 1.

#### Quântica

- Itens que trazem até 6 modificadores.
- O nome e o ícone no inventário e no baú possuem a coloração rosa energético.
- Podem ser obtidos a partir da terceira passada no ato 1 e fase 1.

#### Única

- O nome e o ícone no inventário e no baú possuem a coloração dourada.
- Itens que possuem modificadores completamente únicos e exclusivos.
- A principal ideia é que esses itens tragam algo verdadeiramente único ao herói.
- As principais referências de itens únicos são o Path of Exile e o Diablo.
- Itens melhores são mais raros.
- Podem variar na quantidade total de modificadores.
- **Um item único não sorteia quais modificadores tem, apenas os valores dentro de cada faixa.** Ele não passa pelo gerador em nenhum momento, e é por isso que ele aparece nesta lista sem uma quantidade máxima de modificadores.
    - A rigor, "Única" não é um degrau da mesma escada que as outras: as outras dizem quantos modificadores aleatórios o item recebe, e esta diz que ele não recebe nenhum. Ela fica aqui porque é assim que o jogador lê a informação, e a coloração é o que ele usa para diferenciar.

### Itens únicos

- Cada item único mora em um arquivo próprio em `.claude/specs/items/uniques/`.
- **Itens não-únicos não têm lista e nunca vão ter.** Escrever um arquivo por item comum seria trabalho manual infinito, e cada modificador novo obrigaria a revisitar tudo. O que existe é a lista de bases e a lista de modificadores, e o gerador combina as duas.
- **Itens únicos são a única exceção**, porque a graça deles é justamente serem escritos à mão, um a um.
- Um item único declara:
    - O slot e o subtipo dos quais ele parte. Ele herda a base do subtipo e sobrescreve apenas o que precisa.
    - A classe e a faixa de nível em que ele pode aparecer.
    - A lista fixa de modificadores, cada um com a própria faixa de valor. A escada de camadas não se aplica: o único já traz a faixa dele pronta.
- **Um modificador exclusivo mora dentro do arquivo do próprio item único.**
    - Um modificador que existe em exatamente um item não tem o que fazer em uma tabela compartilhada, e mantê-lo lá dentro deixa o item legível como um documento só.
- Nomes e textos de itens únicos ficam no arquivo de strings, como todo texto que o jogador lê.
- O arquivo `uniques/catharsis-core.json` existe como exemplo do formato.

#### Um item único novo entra sozinho

- **Adicionar um item único é escrever o arquivo na pasta, e mais nada.** Não existe lista para editar à mão, e nenhum código muda.
- A pasta é `Assets/Items/Uniques/`. Todo `.json` que estiver lá é um item único, e todo item único está lá.
- O catálogo continua sendo um ScriptableObject com referências de `TextAsset`, como o `StageDatabase` já faz, porque referência de asset sobrevive a renomear e a mover arquivo. A diferença é que a lista de únicos é preenchida sozinha sempre que um arquivo entra, sai ou muda de lugar na pasta.
    - A ordem não importa, ao contrário das fases. Quem decide a chance de cada um é a raridade, e não a posição na lista.
- **Um teste confere que a pasta e o catálogo batem**, que nenhuma entrada está vazia e que nenhum id se repete.
    - É ele que sustenta a promessa. Se o preenchimento automático falhar algum dia, o teste quebra, em vez de o item sumir do jogo em silêncio.
- O passo a passo completo de adicionar um único é: escrever o `.json` na pasta, escrever o nome e a lore no arquivo de strings, e rodar os testes.

#### Raridade e nível mínimo

- **Os itens únicos mais fortes são os mais raros e só começam a cair mais tarde**, e isso é uma regra que a spec obriga, e não uma intenção que depende de alguém lembrar.
- Cada item único declara um **grau de poder** de 1 a 5, e é ele que decide as duas coisas ao mesmo tempo: o peso de sorteio e o nível mínimo do item.
    - A tabela está em `uniques.json`.
    - Escolher os dois separadamente permitiria escrever, sem querer, um item que decide uma build inteira e cai no nível 1. Amarrando os dois ao mesmo grau, isso deixa de ser possível.
    - É o mesmo raciocínio da escada de camadas: uma tabela só, e o jogo inteiro se move de forma coerente.
- Os cinco graus vão de "curiosidade que muda como uma habilidade se comporta" até "objetivo de fim de jogo cujo nome o jogador conhece antes de ver um".
- Um item único pode declarar um nível mínimo **acima** do piso do próprio grau, e nunca abaixo dele. Subir é decisão de conteúdo válida; descer quebraria a promessa.
- **Não existe um campo dizendo "cai a partir da fase X-Y".**
    - O nível do item sai do nível de quem dropou ele, então o piso de nível já é um gate de fase por consequência.
    - Ter os dois seriam dois eixos medindo a mesma coisa, e eles divergiriam na primeira vez que uma fase mudasse de nível.
- **O que existe é uma restrição de origem, que é outra coisa.** Um item único pode declarar de quais fases ou de quais personagens ele cai, e aí ele cai apenas dali. É assim que um item fica amarrado a um vilão específico.
    - Sem essa declaração, ele cai de qualquer coisa que atenda o nível.
- Os pesos são relativos entre os itens únicos. **A chance de um drop ser único em vez de tecnológico não mora aqui**, ela é assunto do drop.
- Dois exemplares do mesmo item único nunca são idênticos: ele sorteia o próprio nível dentro da faixa dele, e os valores dos modificadores acompanham esse nível.

### Nomenclatura

- **O nome é montado a partir dos dados reais do item, e nunca de um sorteio solto.**
    - `[prefixo] [nome base] [sufixo]`
    - O **nome base** vem do subtipo, e cada subtipo tem de três a quatro variantes liberadas por faixa de nível do item. Um casco monta o nome base como `<substantivo da classe> <substantivo do slot>`.
    - O **prefixo** é a palavra do modificador de hardware de maior camada do item.
    - O **sufixo** é a palavra do modificador de software de maior camada do item, e sempre começa com `of`.
    - Empate de camada é resolvido pela ordem em que os modificadores foram sorteados, para que o nome seja determinístico como todo o resto da batalha.
- Um item sem hardware não ganha prefixo. Um item sem software não ganha sufixo. Um item sem tecnologia é só o nome base.
- **A tecnologia não entra no nome.** A coloração já comunica isso, e uma quarta palavra faria o nome estourar a largura do tooltip.
- O ganho desse modelo é que **o nome vira informação**. O sufixo `of the Furnace` sempre significa dano de fogo, e o jogador aprende o vocabulário em poucas horas e passa a ler o chão em vez de abrir o tooltip de tudo. Um sorteio desconectado dos modificadores nunca ensina nada.
- As palavras estão em `modifiers.json`, cada uma junto do modificador que a produz, e o texto em inglês está no arquivo de strings.

### Modificadores

- Os modificadores aparecem a partir da tecnologia rudimentar.
- Os modificadores são subdivididos em hardware e software.
    - A melhor forma de entender essa subdivisão é comparar com os prefixos e sufixos dos itens de Path of Exile.
- Cada equipamento pode possuir de 0 até 3 hardwares e de 0 até 3 softwares. Por exemplo:
    - Se um equipamento possui 3 modificadores totais, ele pode possuir:
        - 2 hardwares e 1 software.
        - 1 hardware e 2 softwares.
        - 3 hardwares e 0 softwares.
        - 0 hardwares e 3 softwares.
- Modificadores possuem 5 camadas que fazem com que o item se torne mais suscetível a possuir melhores números.
- **Cada modificador declara em quais slots pode cair.**
    - Sem isso, uma Blade poderia sortear armadura e a diferença entre casco e arma começaria a evaporar.
    - Modificadores de armadura e evasão locais caem apenas em equipamentos defensivos. Modificadores de dano de arma caem apenas em armas. O resto cai em qualquer lugar.
- **Cada modificador declara um peso de sorteio.** Modificadores muito fortes, como resistência ignorada, são raros por peso e não por camada.
- Futuramente teremos modificadores exclusivos para cada tipo de equipamento.

#### Modificadores de hardware

- São os modificadores atrelados à armadura, resistências e outras características defensivas.
- Esta é a lista de modificadores de hardware disponíveis:
    - +X de CON.
    - +X à vida total.
    - +X de resistência a fogo.
    - +X de resistência a água.
    - +X de resistência elétrica.
    - +X de resistência a todos os elementos.
    - +X de armadura neste equipamento.
    - +X de evasão neste equipamento.
    - +X% do dano físico recebido devolvido ao atacante.
- **"Neste equipamento" é soma direta sobre a defesa base daquele item, e nada além disso.** Um casco com 100 de armadura base e o modificador em 20 passa a ter 120, e é esse 120 que entra na soma do personagem.
    - Não existe multiplicador local, ou seja, nenhum modificador multiplica a defesa base de um item. O que "neste equipamento" carrega é a restrição de slot: ele só cai em equipamento defensivo, e é isso que impede uma Blade de sortear armadura.
- **As resistências são pontos e não porcentagem.** `attributes.md` calcula mitigação elemental por rendimento decrescente sobre um total de pontos, e a constante da curva é `50 × nível do atacante`. Um modificador em porcentagem não teria onde entrar nessa conta.
    - Fontes que somam porcentagem depois da curva existem, mas `attributes.md` as reserva a itens únicos e nós específicos, justamente porque são as únicas capazes de passar dos 75%.

#### Modificadores de software

- São modificadores atrelados a atributos, dano e outras características ofensivas.
- Esta é a lista de modificadores de software disponíveis:
    - +X de POW.
    - +X de AGI.
    - +X de SPE.
    - +X - +Y ao dano base da arma.
        - É o único modificador que sorteia uma **faixa** em vez de um valor: o mínimo é somado ao mínimo da arma e o máximo ao máximo dela. Um valor único somado nas duas pontas nunca alargaria a faixa, que é justamente o que este modificador faz.
        - Ele é **local à arma que o carrega**, no mesmo sentido de "neste equipamento": vale para o dano daquela arma e de mais nada. Com duas armas empunhadas, um modificador que valesse para as duas faria a secundária ser sempre melhor que qualquer alternativa, em vez de ser uma escolha.
    - +X% ao dano de habilidades de fogo.
    - +X% ao dano de habilidades de água.
    - +X% ao dano de habilidades elétricas.
    - +X% ao dano de habilidades físicas.
    - +X% de resistência ignorada ao atacar.
- **Nenhum modificador aumenta o rank de uma habilidade.**

#### Camada de modificadores

- As camadas são o que definem a faixa dos valores dos modificadores.
    - A melhor forma de entender as camadas é comparar com os tiers dos itens de Path of Exile.
- Quanto mais alta a camada, mais baixa a chance dela aparecer.
- **A camada é um multiplicador compartilhado por todos os modificadores**, e cada modificador declara apenas a própria faixa de camada 1.
    - A alternativa seria escrever cinco faixas por modificador. Isso daria centenas de números para balancear um a um, e nenhuma garantia de que a camada 4 de um modificador vale o mesmo que a camada 4 de outro.
    - Com a escada, mexer em um número move o jogo inteiro de forma coerente, e continua sendo possível um modificador declarar a própria escada quando precisar fugir da regra.
- A faixa de cada modificador é escrita como valor base mais ganho por nível do item, exatamente a mesma forma que as fichas de personagem já usam para armadura e dano base.
    - `Valor = (Base + Ganho por nível × (nível do item − 1)) × Multiplicador da camada`
    - O motivo é o mesmo descrito em `attributes.md`: contra uma constante que cresce, um valor parado apodrece.
    - Modificadores que são porcentagem do dano têm ganho por nível zero, porque `attributes.md` diz que porcentagens de dano não decaem — elas acompanham o dano por definição.
- O que define a chance de aparecimento de melhores camadas em um equipamento é seu nível, e ela cresce exponencialmente.
    - As pontas e o miolo da tabela estão em `tiers.json`, junto das invariantes que o teste confere.
    - Entre dois pontos da tabela o valor é interpolado linearmente.
- **Nenhuma camada é travada por nível.** Um item de nível 1 pode sortear camada 5, e essa loteria pequena é proposital: é a única coisa capaz de fazer um drop do ato 1 surpreender alguém.
- **A camada de cada modificador é sorteada de forma independente**, então um item de seis modificadores pode ter seis camadas diferentes.

## Perguntas em aberto

- Coisas que a spec ainda não decide e que precisam ser decididas antes de os números virarem definitivos.

### Drop e disponibilidade

- O drop será configurado futuramente.
