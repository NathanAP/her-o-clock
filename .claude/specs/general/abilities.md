# Habilidades

## Objetivo

Descrever como as habilidades são lidas nas fichas de personagens e trazer mais detalhes específicos sobre elas.

Tudo sobre habilidade mora aqui. O `gameplay.md` descreve o campo de batalha e o ataque básico, e aponta para esta página quando o assunto é habilidade.

## Personagem cobaia

Nesta página utilizaremos um personagem fictício para demonstrar exemplos. Seguem alguns detalhes:

- Desfere 1 ataque básico por segundo.
- Habilidade A: 1 segundo para ser preparada, 0 segundos de tempo de uso, 0 segundos de recuo.
- Habilidade B: 2 segundos para ser preparada, 2 segundos de tempo de uso, 0 segundos de recuo.
- Habilidade C: instantânea, 1 segundo de tempo de uso, 1 segundo de recuo.

## Habilidades nas fichas

- Toda habilidade é descrita na ficha do personagem que a possui.
- Todas as habilidades possuem `ranks`. Quanto mais alto o `rank` de uma habilidade, mais forte ela será.
- Uma habilidade nunca é escrita como um comportamento próprio e exclusivo daquele personagem. Ela é sempre a **combinação** de peças reaproveitáveis, e o que pertence ao personagem são os números e a escolha das peças.
    - Isso vale inclusive para habilidades muito características. O que faz uma habilidade ser marcante é a combinação, não uma regra que só ela usa.
- As habilidades descritas nas fichas possuem níveis, então certos valores podem acabar mudando (o valor do dano pode aumentar a cada nível investido na habilidade). Nesses casos:
    - Valores descritos em `arrays` indicam a escalabilidade conforme o nível (index 0 = nível 1, index 1 = nível 2 e assim por diante).
        - Por exemplo: `{ ..., "duration": [10, 20, 30, 40, 50] }` indica que aquela habilidade possui uma escala conforme seu atual nível.
    - Valores descritos diretamente em numéricos indicam a escalabilidade constante, ou seja, todos os níveis daquela habilidade usam o mesmo valor.
        - Por exemplo: `{ ..., "duration": 20 }` indica que aquela habilidade possui a mesma duração em todos os níveis.
    - Toda habilidade declara quantos níveis ela possui no campo `ranks`, e todo `array` dentro dela precisa ter exatamente esse tamanho.
        - Sem essa regra, um `array` com um valor a menos faria o último nível da habilidade ler um valor que não existe, e isso não daria erro nenhum. Seria um problema silencioso.
- Tudo relacionado a tempo na ficha (tempo de duração, tempo de recarga, etc) está indicado em segundos.
- Enquanto a árvore de habilidades não existir, **toda habilidade é usada no rank 1**. Os demais ranks já ficam escritos na ficha, e é a árvore que vai destravá-los.
    - Escrever os cinco ranks desde já não é trabalho perdido: é o que faz o validador ter o que conferir, e é onde a intenção de escala da habilidade fica registrada enquanto ela é desenhada.

## Quando uma habilidade é usada

- Uma habilidade é avaliada **apenas pelas suas próprias regras**, e nunca pelo alcance do ataque básico de quem a usa.
    - Um personagem com alcance 2 no ataque básico pode ter uma habilidade que atinge todos os inimigos em linha reta até o fim do campo de batalha. Havendo um inimigo nessa linha, ela é usada imediatamente, mesmo que ele esteja muito além das 2 casas.
    - Ignorar isso transformaria a habilidade em desvantagem, pois o personagem ficaria esperando o inimigo chegar perto para usar algo que já podia ter usado.
- Uma habilidade pronta que não encontra nenhum alvo válido pelas suas próprias regras **segura a carga** e tenta de novo no instante seguinte. Ela nunca é usada no vazio.
- Um personagem **nunca se movimenta apenas para conseguir usar uma habilidade**. A movimentação é decidida sempre pelo ataque básico, conforme "### Movimentação" em `gameplay.md`.
    - Habilidades que dependem de reposicionamento resolvem isso sozinhas, movendo o personagem como parte do próprio efeito.
- Uma habilidade **não protege quem está por perto**. Ela atinge todo mundo que estiver na área dela, aliado ou inimigo, conforme o `who` descrever.
    - Uma habilidade que "ataca TODOS em uma área 3x3" usada em um lugar com aliados também acerta os aliados.
    - Uma habilidade que "cura TODOS em uma área 3x3" usada em um lugar com inimigos também cura os inimigos.
- Um herói caído não é alvo válido para nada e não conta para habilidades em área, conforme "### Morte no campo de batalha" em `gameplay.md`.

## Fases de uma habilidade

### Preparo

- Aqui o personagem está preparando sua habilidade para ser usada.
- O preparo mantém o personagem parado no lugar e não pode realizar ações.
- Pode ser considerado também como o tempo de animação **antes** da habilidade.
- Descrito na ficha como `preparation`.

### Ativação

- Aqui o personagem está usando sua habilidade.
- Pode ser considerado também como o tempo de animação **durante** a habilidade.
- Descrito na ficha como `casting`.

### Recuo

- Aqui o personagem terminou de usar sua habilidade e está sofrendo as consequências (caso haja uma).
- O recuo mantém o personagem parado no lugar e não pode realizar ações.
- Pode ser considerado também como o tempo de animação **após** a habilidade.
- Descrito na ficha como `recoil`.

### Recarga

- As habilidades dos personagens podem ser usadas mesmo se o seu ataque básico está em recarga.
- Quando uma habilidade está disponível junto do ataque básico ou de outras habilidades, temos que resolver alguns conflitos:
    - A que for lançada por último deverá esperar o **preparo**, **tempo de uso** e **recuo** da anterior. Ou seja:
        - Se o personagem lançar a habilidade A primeiro, após 1 segundo (1 pelo preparo + 0 pelo tempo de uso + 0 pelo tempo de recuo) ele poderá lançar a próxima habilidade.
        - Se o personagem lançar a habilidade B primeiro, após 4 segundos (2 pelo preparo + 2 pelo tempo de uso + 0 pelo tempo de recuo) ele poderá lançar a próxima habilidade.
        - Se o personagem lançar a habilidade C primeiro, após 2 segundos (0 pelo preparo + 1 pelo tempo de uso + 1 pelo tempo de recuo) ele poderá lançar a próxima habilidade.
    - Se em qualquer situação o ataque básico está ou ficou disponível, **o ataque básico ganha prioridade**.
        - Se o personagem estava preparando uma habilidade e o ataque básico ficou disponível nesse tempo, o ataque básico é lançado antes da próxima habilidade.
        - Ele nunca interrompe o que já está em andamento. Ele entra na primeira brecha, ou seja, depois que o preparo, o tempo de uso e o recuo da habilidade atual terminarem.
- A recarga de uma habilidade é acionada a partir do momento que **seu tempo de uso terminou**. Por exemplo:
    - Se a habilidade A for usada, ela entra em recarga após 1 segundo (1 pelo preparo + 0 pelo tempo de uso).
    - Se a habilidade B for usada, ela entra em recarga após 4 segundos (2 pelo preparo + 2 pelo tempo de uso).
    - Se a habilidade C for usada, ela entra em recarga após 1 segundo (0 pelo preparo + 1 pelo tempo de uso).
    - Repare que o recuo fica de fora desta conta, e é só isso que separa esta lista da anterior. A habilidade C libera a próxima habilidade em 2 segundos, mas já começou a recarregar em 1.
- O tempo que uma habilidade demora para chegar ao inimigo **não interfere** na recarga da habilidade.
- Parar o preparo de uma habilidade coloca ela em recarga **pela metade do tempo**.
    - Um preparo é parado pelo debuff `Silenciado` ou pela morte de quem estava preparando, e por nada além disso.
    - O alvo morrer durante o preparo não para nada. A habilidade sai normalmente e escolhe alvo de novo no instante em que sai, conforme as regras dela.

## Escolha de alvo

- O bloco `targeting` responde **quem** a habilidade quer atingir, **em que formato** e **com qual critério**, e é separado dos efeitos de propósito: o alvo decide onde a habilidade acontece, os efeitos decidem o que acontece ali.
- `who` indica o destinatário pretendido: `self`, `allies` ou `enemies`.
    - É ele que decide entre quem a habilidade procura o alvo dela.
    - Ele não protege ninguém de estar no caminho, conforme "## Quando uma habilidade é usada".
- `shape` indica o formato:
    - `self` — apenas quem usou.
    - `single` — um único alvo.
    - `area` — todos dentro de um retângulo de `areaColumns` por `areaRows`.
    - `chain` — uma sequência de alvos, cada um próximo do anterior.
    - `line` — todos em linha reta a partir de quem usou, na direção do alvo escolhido.
- `anchor` indica onde a forma é posicionada:
    - `self` — centrada em quem usou a habilidade.
- `range` é a distância máxima, em casas, entre quem usa e o alvo. Ele não tem relação nenhuma com o alcance do ataque básico do personagem.

### Prioridade

- `priority` diz qual regra **encabeça** a escolha do alvo, e é opcional.
- A cadeia de prioridade padrão do jogo está em "### Escolhendo alvos" em `gameplay.md`. Uma habilidade sem `priority` usa aquela cadeia inteira, na ordem dela.
- Declarando um `priority`, a regra escolhida vira a primeira, e **o resto da cadeia padrão continua embaixo dela como desempate**, na mesma ordem, terminando na regra posicional que nunca empata.
    - É isso que responde "e se tiver mais de um?". Não existe "qualquer um": uma batalha precisa terminar igual toda vez que for repetida com a mesma semente, e um empate resolvido ao acaso quebra isso.
    - Reaproveitar a cadeia como desempate também evita inventar um segundo sistema de desempate só para habilidade.
- A provocação continua acima de tudo. Um personagem provocado mira quem o provocou mesmo com um `priority` declarado, conforme "### Provocação" em `gameplay.md`.
- Valores aceitos:
    - `nearest` — o alvo mais próximo. É o padrão quando o campo é omitido.
    - `farthest` — o alvo mais distante.
    - `lowestHealthPercent` — o de menor porcentagem de vida atual.
    - `lowestMaxHealth` — o de menor vida máxima.
    - `lowestPhysicalArmor` — o de menor armadura física.
- Ele só faz sentido para os formatos que escolhem alguém, ou seja `single`, `chain` e `line`. Em `self` e em `area` centrada em quem usa não há alvo a escolher, e o campo é recusado pelo validador.
- **Nenhum valor fora desta lista é aceito.** Uma ficha declarando um `priority` que não existe é recusada em vez de ignorada, senão ela seria lida sem erro nenhum e a habilidade se comportaria de um jeito que a ficha não diz.

### Formatos com regras próprias

- A forma `chain` possui dois campos próprios:
    - `maxTargets` — quantos alvos a sequência alcança no máximo. Ela atinge menos do que isso quando não existem inimigos suficientes.
    - `jumpRange` — a distância máxima entre um alvo e o próximo.
    - O primeiro alvo é escolhido pelo `priority`, e cada alvo seguinte é o inimigo válido **mais próximo do anterior** que ainda não foi atingido. O `priority` manda só no começo da sequência; os saltos são sempre pelo mais próximo.
- A forma `line` só considera alvos que estejam em uma das **oito direções retas** a partir de quem usou: as quatro ortogonais e as quatro diagonais.
    - Entre esses, o `priority` escolhe um. A linha então sai de quem usou, passa por ele e segue até a borda do campo de batalha, atingindo todo mundo que cruzar.
    - Filtrar antes de escolher é o que garante que o alvo escolhido seja de fato atingido. Escolher primeiro e depois procurar a direção mais parecida deixaria a habilidade mirando alguém que a linha não pega.
    - O efeito colateral é que `line` é uma habilidade posicional: ela exige que alguém esteja alinhado. Isso é desejado, e é a mesma leitura de tabuleiro que faz a diagonal contar como distância 1.
    - Se ninguém estiver alinhado, a habilidade não tem alvo válido e segura a carga, como qualquer outra.

## Efeitos

- O bloco `effects` lista o que acontece, e **os efeitos são resolvidos na ordem em que aparecem**. Isso importa: uma habilidade que deixa o personagem intocável precisa aplicar o status antes de causar o dano.
- Todo efeito declara o seu próprio `target`, que pode ser `self`, `eachTarget` ou `firstTarget`.
    - Isso permite que uma mesma habilidade cause dano nos inimigos e aplique um buff em quem a usou.
- Todo efeito declara a sua própria `duration` quando faz sentido ter uma.
    - A duração pertence ao efeito e não à habilidade, para que uma mesma habilidade possa aplicar um buff longo e um atordoamento curto.
- Os tipos de efeito existentes são poucos de propósito, e cada um serve a muitas habilidades diferentes:
    - `modify_stat` — altera um atributo por um tempo. Recebe `stat`, `mode` (`percent` ou `flat`) e `value`.
        - Buff e debuff são o mesmo efeito. O que separa os dois é o sinal do valor.
        - O mesmo efeito chegando duas vezes renova em vez de somar, conforme `buffs-and-debuffs.md`.
    - `deal_damage` — causa dano. Recebe `damageType`, `base` e `scaling`, que é a fração de cada atributo que entra no cálculo.
    - `apply_status` — aplica um estado nomeado, como `untargetable` ou `intangible`. Cada um deles está descrito em `buffs-and-debuffs.md`.
    - `move_to` — reposiciona alguém. Recebe `anchor`, que hoje só tem um valor: `lastTargetAnySide`, uma das casas vizinhas ao último alvo da habilidade.
- Um estado nomeado só existe quando ele faz algo que o jogo ainda não sabe fazer. Estados que são apenas números não precisam existir, pois `modify_stat` já dá conta deles.

### Para onde o `move_to` leva

- `lastTargetAnySide` considera as casas imediatamente acima, abaixo, à esquerda e à direita do último alvo.
- Vale a primeira casa **livre e dentro do tabuleiro**, na ordem de menor fileira e, persistindo o empate, menor coluna. É a mesma ordem de desempate que o resto do jogo já usa.
    - Precisa ser uma ordem fixa, e não a mais próxima ou a mais conveniente, senão a mesma batalha com a mesma semente pode terminar diferente.
- Se nenhuma das quatro servir, **o personagem simplesmente não sai do lugar** e o resto da habilidade acontece normalmente.
    - Uma habilidade nunca falha inteira por causa de um efeito que não coube. Os efeitos são independentes e resolvidos em ordem, conforme esta mesma seção.
