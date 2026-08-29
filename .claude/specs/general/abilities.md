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

## Em que nível cada rank é liberado

- Toda habilidade declara um `rankAvailability`: **uma entrada por rank**, dizendo o nível de personagem necessário para alcançá-lo.

```json
"ranks": 5,
"rankAvailability": [1, 10, 20, 35, 50]
```

- A primeira entrada é o nível em que a habilidade passa a existir para aquele personagem. As seguintes são os degraus.
- Os valores **nunca diminuem** de um rank para o outro. Um rank 3 liberado antes do rank 2 seria um degrau que ninguém consegue subir na ordem.
- O array segue a mesma regra de todo array de habilidade: exatamente uma entrada por rank, conforme "## Habilidades nas fichas".
- **É por habilidade, e nunca por árvore.** Duas habilidades da mesma árvore podem abrir os ranks em ritmos completamente diferentes, e é isso que permite uma delas ser a que cresce cedo e a outra ser a recompensa tardia.

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
- O tempo de uso mantém o personagem parado no lugar e não pode realizar ações.
- Pode ser considerado também como o tempo de animação **durante** a habilidade.
- Descrito na ficha como `casting`.
- As três fases formam **um bloco ocupado**: do começo do preparo até o fim do recuo o personagem só executa aquela habilidade. É a mesma leitura que a regra de conflito abaixo já usa ao somar as três, e é o que impede um personagem de sair andando no meio do próprio golpe.

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
    - `deal_damage` — causa dano. Recebe `damageType`, `base` e `scaling`, e aceita um `falloff` opcional.
        - O `base` vem do **rank** da habilidade, e é o conteúdo dizendo quanto ela vale.
        - O `scaling` é o **peso** de cada atributo sobre a taxa padrão de `attributes.md`, e nunca dano somado por ponto. Peso `1` significa que o atributo paga os 10 pontos por 1% inteiros; `0.5` paga metade disso.
        - `Dano = base × (1 + Σ(atributo × peso) × 0.001 + Σ(porcentagens de dano de habilidade) ÷ 100)`
        - As porcentagens vindas de equipamento e da árvore entram na **mesma soma** que os atributos, e as aplicáveis são escolhidas pelo `damageType` deste efeito. A regra inteira, com exemplo, está em "### Dano de habilidade" em `attributes.md`.
        - É a mesma leitura que a arma e o POW têm: o conteúdo dá a base, o atributo multiplica. Somar dano por ponto faria o rank parar de importar assim que o personagem tivesse pontos suficientes, que é exatamente o problema que o dano flat cria com a arma.
    - `apply_status` — aplica um estado nomeado, como `untargetable` ou `intangible`. Cada um deles está descrito em `buffs-and-debuffs.md`.
    - `move_to` — reposiciona alguém. Recebe `anchor`, que hoje só tem um valor: `lastTargetAnySide`, uma das casas vizinhas ao último alvo da habilidade.
- Um estado nomeado só existe quando ele faz algo que o jogo ainda não sabe fazer. Estados que são apenas números não precisam existir, pois `modify_stat` já dá conta deles.

### A queda de dano por distância

- O `deal_damage` aceita um campo `falloff`, que é o quanto o dano perde a cada casa de distância entre quem usou a habilidade e quem está recebendo.
- A conta é multiplicativa:
    - `Dano = Dano base × (1 − falloff) ^ (distância − 1)`
    - A `distância` é medida em casas a partir de quem usou, contando a diagonal como 1, do mesmo jeito que o resto do jogo mede.
    - O `− 1` é o que faz o alvo colado receber o dano cheio. Sem ele, nem quem está do lado levaria o valor escrito na ficha, e o número da ficha deixaria de significar alguma coisa.
- Por exemplo, com 100 de dano base e `falloff` de 0.15:
    - A 1 casa, 100 de dano.
    - A 2 casas, 85 de dano.
    - A 4 casas, 61 de dano.
    - A 7 casas, 38 de dano.
    - As 7 casas são a maior distância que o campo de batalha permite, de um canto ao canto oposto, conforme "## Campo de batalha" em `gameplay.md`.
- A queda acontece sobre o **dano base**, ou seja, no passo 1 de "## Ordem do cálculo de dano" em `attributes.md`. Espinhos, mitigação e evasão acontecem normalmente depois dela, já sobre o valor reduzido.
- Ela é multiplicativa e nunca subtrativa, pelo mesmo motivo das reduções descritas em "### As reduções sempre multiplicam entre si" em `attributes.md`: assim o dano **nunca chega a zero**.
    - Uma queda subtrativa zeraria o dano a partir de certa distância, e aí cada habilidade precisaria de um piso escrito à mão. Seria mais uma constante para calibrar, e ela não seria discutível sozinha.
    - Com a queda multiplicativa, estar na linha sempre vale alguma coisa e estar perto sempre vale mais.
- O campo é opcional e vale `0` quando omitido, ou seja, o dano é o mesmo a qualquer distância.
- Ele só é aceito nas formas que alcançam casas a distâncias diferentes, que são `area`, `chain` e `line`.
    - Em `self` e em `single` a distância é sempre a mesma, então a queda seria lida sem erro nenhum e não faria diferença. Uma ficha declarando `falloff` nessas duas formas é recusada, e não ignorada.
- Ele aceita um valor por rank, como qualquer outro número de habilidade. Uma habilidade que perde menos dano por casa conforme evolui é uma evolução legítima.
- O valor fica **entre 0 e 1, sem incluir o 1**. Uma queda de 100% por casa faria todo alvo que não estivesse colado receber zero, e quem quer isso está pedindo uma habilidade de alcance 1.

### Dano em quem usou a habilidade

- Uma habilidade pode causar dano em quem a usou, declarando um `deal_damage` com `target` igual a `self`. É assim que se escreve o custo de uma habilidade poderosa.
- **Esse dano nunca leva a vida abaixo de 1.** Quando não há vida suficiente, ele é reduzido ao que sobra e o personagem continua vivo com 1 ponto.
- A trava existe para que a promessa seja absoluta. A alternativa seria impedir o uso quando a conta mataria, e ela tem dois furos:
    - A habilidade tem preparo. O personagem passaria na conferência, levaria dano durante o preparo e morreria quando ela finalmente saísse. A proibição teria olhado para um número que já não valia mais.
    - Ela desarmaria o personagem justamente com a vida baixa, que é quando ele mais precisa do efeito. Uma habilidade que some quando a batalha aperta é uma habilidade que não existe.
- A trava vale **apenas para o dano que a própria habilidade causa em quem a usou**. Nada mais é travado:
    - Espinhos de um inimigo, dano em área de um aliado e o ataque básico de qualquer um continuam capazes de matar normalmente.
    - Sem esse limite, um personagem com uma habilidade de custo se tornaria imortal por acidente, e a trava deixaria de ser um custo para virar uma defesa.

### Para onde o `move_to` leva

- `lastTargetAnySide` considera as casas imediatamente acima, abaixo, à esquerda e à direita do último alvo.
- Vale a primeira casa **livre e dentro do tabuleiro**, na ordem de menor fileira e, persistindo o empate, menor coluna. É a mesma ordem de desempate que o resto do jogo já usa.
    - Precisa ser uma ordem fixa, e não a mais próxima ou a mais conveniente, senão a mesma batalha com a mesma semente pode terminar diferente.
- Se nenhuma das quatro servir, **o personagem simplesmente não sai do lugar** e o resto da habilidade acontece normalmente.
    - Uma habilidade nunca falha inteira por causa de um efeito que não coube. Os efeitos são independentes e resolvidos em ordem, conforme esta mesma seção.
