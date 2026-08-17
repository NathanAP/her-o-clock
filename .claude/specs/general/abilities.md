# Habilidades

## Objetivo

Descrever como as habilidades são lidas nas fichas de personagens e trazer mais detalhes específicos sobre elas.

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

## Fases de uma habilidade

### Preparo

- Aqui o personagem está preparando sua habilidade para ser usada.
- O preparo mantém o personagem parado no lugar.
- Pode ser considerado também como o tempo de animação **antes** da habilidade.
- Descrito na ficha como `preparation`.

### Ativação

- Aqui o personagem está usando sua habilidade.
- Pode ser considerado também como o tempo de animação **durante** a habilidade.
- Descrito na ficha como `casting`.

### Recuo

- Aqui o personagem terminou de usar sua habilidade e está sofrendo as consequências (caso haja uma).
- O recuo mantém o personagem parado no lugar.
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

### Escolha de alvo

- O bloco `targeting` responde **quem** a habilidade quer atingir e **em que formato**, e é separado dos efeitos de propósito: o alvo decide onde a habilidade acontece, os efeitos decidem o que acontece ali.
- `who` indica o destinatário pretendido: `self`, `allies` ou `enemies`.
    - É ele que decide quem a habilidade procura e quem ela ignora ao escolher alvo.
    - Ele **não** protege quem está por perto. Uma área pega todo mundo que estiver dentro dela, conforme "### Habilidades" em `gameplay.md`.
- `shape` indica o formato:
    - `self` — apenas quem usou.
    - `single` — um único alvo, escolhido pela ordem de prioridade padrão descrita em `gameplay.md`.
    - `area` — todos dentro de um retângulo de `areaColumns` por `areaRows`.
    - `chain` — uma sequência de alvos, cada um próximo do anterior.
    - `line` — todos em linha reta a partir de quem usou.
- `anchor` indica onde a forma é posicionada:
    - `self` — centrada em quem usou a habilidade.
- `range` é a distância máxima, em casas, entre quem usa e o alvo. Ele não tem relação nenhuma com o alcance do ataque básico do personagem.
- **Ainda não existe como declarar uma prioridade de alvo diferente da padrão.** `gameplay.md` cita habilidades que atacam "o inimigo mais distante" ou "o de menor porcentagem de vida atual", e elas são desejáveis, mas nenhum campo desta página as expressa.
    - Enquanto esse campo não existir, `single` usa sempre a cadeia de prioridade padrão, e **nenhuma ficha pode declarar outra coisa**. O validador recusa o que não estiver aqui.
    - Está escrito em vez de ficar subentendido porque o silêncio aqui é do tipo caro: uma ficha declarando `"priority": "lowestHealth"` seria lida sem erro nenhum e simplesmente ignorada.
- A forma `chain` possui dois campos próprios:
    - `maxTargets` — quantos alvos a sequência alcança no máximo. Ela atinge menos do que isso quando não existem inimigos suficientes.
    - `jumpRange` — a distância máxima entre um alvo e o próximo.
    - A sequência começa no inimigo válido mais próximo de quem usou, e cada alvo seguinte é o inimigo válido mais próximo do anterior que ainda não foi atingido.

### Efeitos

- O bloco `effects` lista o que acontece, e **os efeitos são resolvidos na ordem em que aparecem**. Isso importa: uma habilidade que deixa o personagem intocável precisa aplicar o status antes de causar o dano.
- Todo efeito declara o seu próprio `target`, que pode ser `self`, `eachTarget` ou `firstTarget`.
    - Isso permite que uma mesma habilidade cause dano nos inimigos e aplique um buff em quem a usou.
- Todo efeito declara a sua própria `duration` quando faz sentido ter uma.
    - A duração pertence ao efeito e não à habilidade, para que uma mesma habilidade possa aplicar um buff longo e um atordoamento curto.
- Os tipos de efeito existentes são poucos de propósito, e cada um serve a muitas habilidades diferentes:
    - `modify_stat` — altera um atributo por um tempo. Recebe `stat`, `mode` (`percent` ou `flat`) e `value`.
        - Buff e debuff são o mesmo efeito. O que separa os dois é o sinal do valor.
    - `deal_damage` — causa dano. Recebe `damageType`, `base` e `scaling`, que é a fração de cada atributo que entra no cálculo.
    - `apply_status` — aplica um estado nomeado, como `untargetable` ou `intangible`.
    - `move_to` — reposiciona alguém. Recebe `anchor`, que hoje só tem um valor: `lastTargetAnySide`, uma das casas vizinhas ao último alvo da habilidade.
- Um estado nomeado só existe quando ele faz algo que o jogo ainda não sabe fazer. Estados que são apenas números não precisam existir, pois `modify_stat` já dá conta deles.

#### Para onde o `move_to` leva

- `lastTargetAnySide` considera as casas imediatamente acima, abaixo, à esquerda e à direita do último alvo.
- Vale a primeira casa **livre e dentro do tabuleiro**, na ordem de menor fileira e, persistindo o empate, menor coluna. É a mesma ordem de desempate que o resto do jogo já usa.
    - Precisa ser uma ordem fixa, e não a mais próxima ou a mais conveniente, senão a mesma batalha com a mesma semente pode terminar diferente.
- Se nenhuma das quatro servir, **o personagem simplesmente não sai do lugar** e o resto da habilidade acontece normalmente.
    - Uma habilidade nunca falha inteira por causa de um efeito que não coube. Os efeitos são independentes e resolvidos em ordem, conforme esta mesma seção.
