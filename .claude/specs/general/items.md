# Itens

## Objetivo

Especificar todos os detalhes gerais sobre os itens presentes em Her-o-clock.

## Equipamentos

- Os equipamentos são itens que os heróis obtém e podem ser usados quando os requerimentos forem atingidos.
    - Dito isso, se o herói não atender mais o requerimento de um item já equipado, aquele item é desconsiderado. Visualmente falando ele fica com uma borda vermelha para indicar que o item está inativo.
- Apenas heróis usam equipamentos.
- Os equipamentos não alteram sprites, skins, animações ou efeitos do herói.

### Tipo de equipamento

- Cada tipo de equipamento corresponde a um slot de equipamento que o herói pode utilizar.

#### Casco superior

- Correspondente à equipamentos defensivos da parte superior do corpo do herói.

#### Casco inferior

- Correspondente à equipamentos defensivos da parte inferior do corpo do herói.

#### Cascos braçais

- Correspondente à equipamentos defensivos dos braços do herói.

#### Casco corporal

- Correspondente à equipamentos defensivos do corpo do herói.

#### Mão primária

- Corresponde a quais equipamentos ofensivos o herói usará para combater seus inimigos.
- É a principal fonte de dano do herói.
- O jogador pode escolher entre usar armas de uma ou duas mãos.
    - No primeiro caso, o jogador pode utilizar duas armas de uma mão ou uma arma e mais um equipamento defensivo na mão secundária.
    - No segundo caso, o jogador não poderá equipar itens na mão secundária.

#### Mão secundária

- Corresponde à equipamentos ofensivos, defensivos o herói usa, porém fica vazio caso o herói use uma arma de duas mãos na mão primária.

#### Controlador

- Corresponde a equipamentos utilitários do herói. É um slot que pode ser usado para ajudar nos ganhos ofensivos e defensivos pro herói.
- É equivalente a um amuleto em jogos de RPG tradicionais.

#### Firmware

- Tem um papel parecido com o do controlador de fornecer um slot para ajudar a ganhar números ofensivos e defensivos.

### Classes

- Todo equipamento pertence a uma das classes abaixo.
- A classe do equipamento define como os atributos principais do herói são convertidos em atributos secundários, conforme descrito em `attributes.md`.
- Cada classe é liberada por um atributo principal diferente, então a escolha do equipamento acompanha naturalmente a build do herói.

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
- Apesar do nome, todo itens especiais continua sendo tecnologia, assim como todo o resto do universo de Unia.

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
- A tecnologia indica o quão modificada aquele equipamento é.
- Tecnologia é diferente de raridade: conseguir itens mais tecnológicos é normal, porém conseguir os modificadores corretos que fazem a raridade ser elevada.
- Itens com menor tecnologia não necessariamente são piores que outro equipamento de alta tecnologia. Por exemplo:
    - O herói pode usar um equipamento de alta tecnologia de 10 de armadura, mas acaba de pegar um equipamento sem tecnologia com 200 de armadura.
    - O herói pode usar um equipamento de alta tecnologia que aumenta suas habilidades em 20% mas acaba de pegar um equipamento de tecnologia básica que aumenta suas habilidades em 80%.
- Futuramente teremos opções para permitir a junção de 3 itens da mesma tecnologia para se obter um novo equipamento aleatório de até um nível de tecnologia acima.

#### Sem tecnologia

- Itens que não trazem modificadores.
- O nome e no ícone do inventário e baú possui a coloração transparente.

#### Rudimentar

- Itens que trazem até 1 modificador.
- O nome e no ícone do inventário e baú possui a coloração cinza metálico.
- Podem ser obtidos a partir da primeira passada no ato 1 e fase 1.

#### Convencional

- Itens que trazem até 2 modificadores.
- O nome e no ícone do inventário e baú possui a coloração verde.
- Podem ser obtidos a partir da primeira passada no ato 1 e fase 1.

#### Otimizada

- Itens que trazem até 3 modificadores.
- O nome e no ícone do inventário e baú possui a coloração azul elétrico.
- Podem ser obtidos a partir da primeira passada no ato 2 e fase 1.

#### Avançada

- Itens que trazem até 4 modificadores.
- O nome e no ícone do inventário e baú possui a coloração violeta.
- Podem ser obtidos a partir da segunda passada no ato 1 e fase 1.

#### Nano

- Itens que trazem até 5 modificadores.
- O nome e no ícone do inventário e baú possui a coloração ciano.
- Podem ser obtidos a partir da segunda passada no ato 4 e fase 1.

#### Quântica

- Itens que trazem até 6 modificadores.
- O nome e no ícone do inventário e baú possui a coloração rosa energético.
- Podem ser obtidos a partir da terceira passada no ato 1 e fase 1.

#### Única

- O nome e no ícone do inventário e baú possui a coloração dourada.
- Itens que possuem modificadores completamente únicos e exclusivos.
- A principal ideia é que esses itens tragam algo verdadeiramente único ao herói.
- As principais referências de itens únicos é o Path of Exile e Diablo.
- Itens melhores são mais raros.
- Podem variar na quantidade total de modificadores.
- O nome e no ícone do inventário e baú possui a coloração laranja.

### Modificadores

- Os modificadores aparecem a partir da tecnologia básica.
- Os modificadores são sub-divididos em hardware e software.
    - A melhor forma de entender essa subdivisão é comparar com os prefixos e sufixos dos itens de Path of Exile.
- Cada equipamento pode possui de 0 até 3 hardwares e de 0 até 3 softwares. Por exemplo:
    - Se um equipamento possui 3 modificadores totais, ele pode possuir:
        - 2 hardwares e 1 software.
        - 1 hardware e 2 softwares.
        - 3 hardwares e 0 softwares.
        - 0 hardwares e 3 softwares.
- Modificadores possuem 5 camadas que fazem com que o item se torne mais sucinto a possuir melhores números.
- Futuramente teremos modificadores exclusivos para cada tipo de equipamento.

#### Modificadores de hardware

- São os modificadores atreladas à armadura, resistências e outras características defensivas.
- Esta é a lista de modificadores de hardwares disponíveis:
    - +X de CON.
    - +X à vida total.
    - +X% de resistência a fogo.
    - +X% de resistência a água.
    - +X% de resistência elétrica.
    - +X% de resistência a todos os elementos.
    - +X de armadura neste equipamento.
    - +X de evasão neste equipamento.
    - +X ao dano devolvido como dano de mesmo tipo ao atacante.

#### Modificadores de software

- São modificadores atreladas a atributos, dano e outras características ofensivas.
- Está é a lista de modificadores de softwares disponíveis:
    - +X de POW.
    - +X de AGI.
    - +X de SPE.
    - +X - +Y ao dano base da arma.
    - +X% ao dano de habilidades de fogo.
    - +X% ao dano de habilidades de água.
    - +X% ao dano de habilidades elétricas.
    - +X% ao dano de habilidades físicas.
    - +X% de resistência ignorada ao atacar.

#### Camada de modificadores

- As camadas são o que definem a range dos valores dos modificadores.
    - A melhor forma de entender as camadas é comparar com os tiers dos itens de Path of Exile.
- Quanto mais alta a camada, mais baixa a chance dele aparecer.
- O que define a chance de aparecimento de melhores camadas em um equipamento é seu nível e ele cresce exponencialmente.
    - Itens de nível 1:
        - Chance de receber modificadores na camada 1: 70%.
        - Chance de receber modificadores na camada 2: 25%.
        - Chance de receber modificadores na camada 3: 3%.
        - Chance de receber modificadores na camada 4: 1.5%.
        - Chance de receber modificadores na camada 5: 0.5%.
    - Itens de nível 100:
        - Chance de receber modificadores na camada 1: 5%.
        - Chance de receber modificadores na camada 2: 20%.
        - Chance de receber modificadores na camada 3: 40%.
        - Chance de receber modificadores na camada 4: 25%.
        - Chance de receber modificadores na camada 5: 10%.
