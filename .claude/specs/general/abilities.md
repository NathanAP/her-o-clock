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
- Uma habilidade nunca é escrita como um comportamento próprio e exclusivo daquele personagem. Ela é sempre a **combinação** de peças reaproveitáveis, e o que pertence ao personagem são os números e a escolha das peças.
    - Isso vale inclusive para habilidades muito características. O que faz uma habilidade ser marcante é a combinação, não uma regra que só ela usa.

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
        - Se o personagem lançar a habilidade C primeiro, após 2 segundo (0 pelo preparo + 1 pelo tempo de uso + 1 pelo tempo de recuo) ele poderá lançar a próxima habilidade.
    - Se em qualquer situação o ataque básico está ou ficou disponível, **o ataque básico pode ganhar prioridade**.
        - Se o personagem estava preparando uma habilidade e o ataque básico ficou disponível nesse tempo, o ataque básico é lançado antes da próxima habilidade.
- A recarga de uma habilidade é acionada a partir do momento que **seu tempo de uso terminou**. Por exemplo:
    - Se a habilidade A for usada, ela entra em recarga após 1 segundo (1 pelo preparo + 0 pelo tempo de uso).
    - Se a habilidade B for usada, ela entra em recarga após 4 segundos (2 pelo preparo + 2 pelo tempo de uso).
    - Se a habilidade C for usada, ela entra em recarga após 2 segundos (0 pelo preparo + 1 pelo tempo de uso).
- O tempo que uma habilidade demora para chegar ao inimigo **não interfere** na recarga da habilidade.
- Parar o preparo de uma habilidade colocar ela em recarga **pela metade do tempo**.

### Escolha de alvo

- O bloco `targeting` responde **quem** a habilidade quer atingir e **em que formato**, e é separado dos efeitos de propósito: o alvo decide onde a habilidade acontece, os efeitos decidem o que acontece ali.
- `who` indica o destinatário pretendido: `self`, `allies` ou `enemies`.
    - É este campo que a pontuação de posicionamento descrita em `gameplay.md` usa para saber quem soma e quem diminui ponto.
- `shape` indica o formato:
    - `self` — apenas quem usou.
    - `single` — um único alvo, escolhido pela ordem de prioridade padrão.
    - `area` — todos dentro de um retângulo de `areaColumns` por `areaRows`.
    - `chain` — uma sequência de alvos, cada um próximo do anterior.
    - `line` — todos em linha reta a partir de quem usou.
- `anchor` indica onde a forma é posicionada:
    - `self` — centrada em quem usou a habilidade.
    - `lastTargetAnySide` — em uma posição ao lado do último alvo da habilidade.
- `range` é a distância máxima, em casas, entre quem usa e o alvo. Ele não tem relação nenhuma com o alcance do ataque básico do personagem.
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
    - `move_to` — reposiciona alguém. Recebe `anchor`, como `behindLastTarget`.
- Um estado nomeado só existe quando ele faz algo que o jogo ainda não sabe fazer. Estados que são apenas números não precisam existir, pois `modify_stat` já dá conta deles.
