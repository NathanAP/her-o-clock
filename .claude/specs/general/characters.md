# Personagens

## Objetivo

Especificar todos os detalhes gerais sobre os personagens presentes em Her-o-clock.

## Fichas

- As fichas de cada personagem do jogo estão presentes na pasta `.claude/specs/characters`.
- O desenvolvimento do jogo deve espelhar as fichas dos personagens.
- As fichas também indicam valores de dano, redução de recarga, escala para buffs e debuffs, entre outros.
- As habilidades descritas nas fichas possuem níveis, então certos valores podem acabar mudando (o valor do dano pode aumentar a cada nível investido na habilidade). Nesses casos:
    - Valores descritos em `arrays` indicam a escalabilidade conforme o nível (index 0 = nível 1, index 1 = nível 2 e assim por diante).
        - Por exemplo: `{ ..., "duration": [10, 20, 30, 40, 50] }` indica que aquela habilidade possui uma escala conforme seu atual nível.
    - Valores descritos diretamente em numéricos indicam a escalabilidade constante, ou seja, todos os níveis daquela habilidade usam o mesmo valor.
        - Por exemplo: `{ ..., "duration": 20 }` indica que aquela habilidade possui a mesma duração em todos os níveis.
    - Toda habilidade declara quantos níveis ela possui no campo `ranks`, e todo `array` dentro dela precisa ter exatamente esse tamanho.
        - Sem essa regra, um `array` com um valor a menos faria o último nível da habilidade ler um valor que não existe, e isso não daria erro nenhum. Seria um problema silencioso.
- Tudo relacionado a tempo na ficha (tempo de duração, tempo de recarga, etc) está indicado em segundos.

### Estrutura de uma ficha

- `id` — identificador estável em texto. É por ele que fases, saves e outras fichas apontam para este personagem, então ele nunca muda depois que existe conteúdo o referenciando.
- `displayName` e `kind` — o nome mostrado e o tipo (`hero`, `minion` ou `villain`).
- `initialLevel` e `maxLevel` — a faixa de níveis que aquele personagem alcança.
- `info` — tudo que o jogador pode ver: descrição, história, características físicas e visual.
- `hidden` — a verdade por trás do personagem, que o jogador só descobre jogando.
    - Este bloco existe **apenas como documento de design**. Ele nunca deve ir para o dado que o jogo lê, senão qualquer pessoa abre o arquivo do jogo e encontra a revelação antes de merecê-la.
- `baseAttributes` — os atributos principais no nível inicial, antes de qualquer outra fonte.
- `attributeGrowth` — como o personagem distribui os 5 pontos que recebe a cada nível, em porcentagens. Descrito em `progress.md`.
- `abilities` — as habilidades do personagem, descritas abaixo.

## Heróis

- Os heróis são os personagens usados pelo jogador.
- A principal característica de design é que todos eles são robôs, mesmo que possuam traços humanos (movimentação, expressões).
- Os heróis podem ser equipados por itens conforme o jogador escolher.

### Grupo de heróis

- Um grupo de heróis é composto de até 4 personagens e pode ser escolhido pelo jogador a qualquer momento.
- Um grupo de heróis pode ser montado do jeito que o jogador preferir.
- Durante a escolha do grupo, o jogador pode também alterar a formação inicial em campo de batalha, dispondo das 24 casas da área dos heróis (6 colunas por 4 fileiras).
    - Como o grupo possui no máximo 4 heróis, a maior parte da área fica vazia. Isso é intencional e é o que permite ao jogador escolher entre concentrar o grupo ou espalhá-lo contra ataques em área.
    - A formação define apenas onde a batalha começa. A partir dali os personagens se movimentam sozinhos, conforme descrito em `gameplay.md`.

## Lacaios

- Os lacaios são personagens que aparecem nas fases antes e/ou junto com os vilões.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades criadas pelos vilões.
- Os lacaios aparecem repetidamente em grupos pré definidos nas fases.

## Vilões

- Os vilões são personagens que aparecem ao final de cada fase sozinhos ou junto de seus lacaios.
- Vilões podem ser também chamados e considerados os "bosses" ou "chefões" do jogo.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades.
- Os vilões podem aplicar buffs aos lacaios ou debuffs aos jogadores conforme suas características.

## Nível de personagem

- Personagens possuem níveis que vão de 1 até 100.
- Quanto mais alto o nível de um personagem:
    - Mais atributos ele terá.
    - Mais habilidades ele irá possuir.
- Para subir o nível de seus heróis, o jogador deve enfrentar lacaios e vilões.
    - Muitas vezes o jogador precisará repetir a mesma fase inúmeras vezes para conseguir ficar mais forte e enfrentar a próxima fase.
- Lacaios e vilões possuem níveis de acordo com a fase em que estão presentes, e não de acordo com a ficha deles.
- Tudo que diz respeito a ganhar experiência, subir de nível e distribuir os pontos recebidos está descrito em `progress.md`.

## Habilidades

- Toda habilidade é descrita na ficha do personagem que a possui.
- Uma habilidade nunca é escrita como um comportamento próprio e exclusivo daquele personagem. Ela é sempre a **combinação** de peças reaproveitáveis, e o que pertence ao personagem são os números e a escolha das peças.
    - Isso vale inclusive para habilidades muito características. O que faz uma habilidade ser marcante é a combinação, não uma regra que só ela usa.

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
    - `bestPlacement` — no melhor lugar possível, conforme a pontuação descrita em `gameplay.md`.
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

## Buffs

- Os buffs serão descritos futuramente.

## Debuffs

- Os debuffs serão descritos futuramente.
