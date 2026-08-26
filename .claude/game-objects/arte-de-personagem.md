# Arte de personagem

## Objetivo

Descrever como um personagem deixa de ser um retângulo colorido e passa a ter desenho, e quais números precisam continuar batendo para que ele apareça do tamanho certo e no lugar certo.

## Onde os arquivos ficam

- `Assets/Art/Characters/` guarda um `.png` por pose, um arquivo por desenho.
    - `tempo-down.png` — virado para baixo, o jogador vê a frente.
    - `tempo-up.png` — virado para cima, o jogador vê as costas.
    - `tempo-side.png` — de perfil, olhando para a **direita**. A esquerda é este mesmo desenho espelhado.
    - `tempo-dead.png` — caído.
    - `tempo-attack-melee.png` — golpeando com lâmina.
    - `tempo-attack-ranged.png` — disparando.
    - `tempo-run-0.png` a `tempo-run-7.png` — o ciclo de corrida, em ordem.
- Não existe desenho para a esquerda de propósito. Ele é o de perfil com `flipX`, o que custa um campo em vez de mais um desenho por personagem.

## Tamanho e ajustes de importação

- Todo desenho é de **24 × 32 pixels**, com o personagem ocupando cerca de 16 a 21 de largura.
- Os pés ficam **2 pixels acima da borda de baixo** do quadro, e é por isso que o pivô é `(0.5, 0.0625)` e não o canto inferior. Com o pivô no canto, o personagem flutuaria dois pixels acima do chão da casa — coisa que não aparece no Inspector e aparece na tela.
- **Os ajustes de importação são aplicados por código**, não à mão. Quem faz isso é o `PixelArtTextureImporter`, um `AssetPostprocessor` que pega tudo que entra em `Assets/Art/`.
    - Escrever o `.meta` na mão foi descartado: o formato dele muda entre versões da Unity, e apagar um `.meta` desfaria os ajustes em silêncio.
    - São seis ajustes, e cada um deles falha calado quando esquecido: filtro bilinear borra a arte, compressão inventa cor que não estava na paleta, e os 100 pixels por unidade padrão fazem um sprite de 24×32 nascer com um terço do tamanho da casa.

| Ajuste | Valor | Por quê |
| --- | --- | --- |
| Texture Type | Sprite | |
| Sprite Mode | Single | Um arquivo por desenho |
| Pixels Per Unit | 30 | O mesmo número do `Pixel Perfect Camera`, que é o que faz uma casa medir 30×30 |
| Filter Mode | Point | Qualquer outro borra |
| Compression | Uncompressed | Compressão cria cores fora da paleta |
| Generate Mip Maps | desligado | Não existe câmera se afastando |
| Pivot | Custom `(0.5, 0.0625)` | Põe os pés no chão da casa |

- **O `30` aqui e o `Assets Pixels Per Unit` do `main-camera.md` são o mesmo número.** Se um mudar, o outro muda junto, e o tamanho do sprite muda com eles.

## A pose de ataque

- Vale para as duas mãos: `AttackMelee` é usada quando o ataque básico do personagem é corpo a corpo e `AttackRanged` quando é à distância. Quem escolhe é o `AutoAttack` da ficha, e quando os itens chegarem é a arma que passa a mandar nesse valor — sem que nenhum dos dois sprites precise mudar.
- **A pose é um estado, nunca uma sequência.** Cada ataque básico liga a pose e arma um cronômetro; um ataque novo reinicia o cronômetro e nunca entra numa fila.
    - O motivo é a velocidade. Com velocidade de ataque somada aos 8x do jogo, os golpes chegam mais rápido que os quadros na tela. Uma fila ficaria correndo atrás da luta e acabaria tocando golpes que aconteceram segundos antes.
    - A degradação em velocidade alta sai correta de graça: a pose simplesmente não desce mais, que é como alguém atacando sem parar deve parecer.
- **Os dois cronômetros de apresentação correm em tempo não escalado**, tanto a pose quanto a piscada de dano. `Time.deltaTime` encolhe junto com a velocidade do jogo, então uma pose medida em tempo de jogo duraria dois centésimos de segundo real em 8x e nenhum quadro chegaria a desenhá-la.
    - Isso era um defeito já existente na piscada de dano, corrigido junto na 0.10.2.0.
- A pose olha para a direita, como o perfil, e é mostrada com o personagem virado para qualquer lado. Desenhar ataque para as quatro direções custaria quatro vezes a arte para corrigir algo que o jogador lê como golpe de qualquer jeito.

## O ciclo de corrida

- São oito desenhos numerados a partir de zero. O linker lê até achar um buraco, então um ciclo pode ter qualquer tamanho e um entregue pela metade toca até onde vai.
- **O ciclo é dirigido por distância percorrida, nunca por tempo.** Um ciclo no relógio anda no próprio ritmo enquanto a personagem anda no ritmo da AGI dela, e os dois se afastam até os pés patinarem no chão. Amarrado à distância, o pé encosta sempre no mesmo ponto do percurso.
    - `RunCycleCells` diz quanto chão um ciclo inteiro cobre. Um significa um ciclo por casa.
    - Como é posição e não relógio, o ciclo também não se importa com a velocidade em que o jogo está rodando.
- A distância é medida no `CharacterView` comparando o `transform` entre quadros, e não perguntada ao `CharacterMover`. A view continua espectadora e a simulação segue sem saber que desenhos existem.
- **Um salto maior que meia casa num quadro só não conta.** Isso não é andar, é ser colocado em algum lugar: reinício de batalha, entrada de fase, ou o chão deslizando entre ondas. Contar esses giraria as pernas por uma viagem que ninguém fez.
- A prioridade entre desenhos é **golpe, depois corrida, depois parado**. Quem parou para bater deve ser visto batendo, e não pego no meio da passada.
- O ciclo é de perfil, como o `Side`, e é usado mesmo subindo ou descendo o tabuleiro — a mesma concessão já aceita para a pose de ataque.

## Como o desenho chega no personagem

- `CharacterDefinition` tem um campo `Sprites`, do tipo `CharacterSprites`, com um sprite por direção.
- **Deixar vazio mantém o retângulo colorido.** É isso que permite a Tempo ter desenho enquanto o Gadrat e os lacaios continuam quadrados, sem que nenhum dos dois precise saber do outro.
- `CharacterView` decide uma vez, no `Build`, se desenha sprite ou retângulo:
    - Com sprite, a escala do objeto fica em **1** e a cor em **branco**. O sprite já sabe o próprio tamanho pelos pixels por unidade, e escalar brigaria com a grade de pixels e faria a arte cintilar ao andar.
    - Sem sprite, continua tudo como era: quadrado branco escalado e tingido pela cor da ficha.

## Direção

- A direção mora em `Character.Facing` e é **só apresentação**. Nenhuma regra de combate a lê, então atacar alguém pelas costas é igual a atacar de frente.
- Ela é decidida pelo `FacingResolver`, que recebe as duas casas de um passo e devolve para onde virar:
    - O herói começa virado para cima e o inimigo para baixo, pois um ocupa a metade de baixo do tabuleiro e o outro a de cima.
    - Num passo na diagonal, a fileira ganha da coluna. O tabuleiro é lido de baixo para cima e atravessar fileira é o que o movimento significa.
    - Um passo que não sai do lugar mantém a direção atual.
- **Ao chegar na casa, a direção volta para a padrão do time.** É o `SettleFacing`, chamado pelo `CharacterMover` quando o passo termina.
    - Sem isso um passo lateral deixava o personagem parado de perfil, lutando de lado contra alguém que está acima dele. Estar de perfil é estado de trânsito, não de descanso.
    - Como as duas metades do tabuleiro são fixas, voltar para cima ou para baixo acerta praticamente sempre, sem a view precisar saber quem é o alvo.
- **A direção é lida no `Update` e não no evento `Changed`.** Andar não dispara `Changed`, e dispará-lo a cada passo reconstruiria vida e atributos de quem só virou de lado. O custo de ler no `Update` é uma comparação de enum por quadro.

## Valores no `CharacterViewSettings` que mudaram junto

- `SpriteOffsetY` nasceu em `-0.5`, que é meia casa para baixo, ou seja o chão.
- `HealthBarOffsetY` subiu de `0.4` para `0.55`. Um sprite de 32 pixels é bem mais alto que o retângulo de `0.7` casa que existia antes, e a barra ficava em cima da cabeça.
- `SpriteFlashColor` nasceu separado do `FlashColor`. A piscada de dano do retângulo é para o branco, e **o branco não serve para sprite**: a cor do `SpriteRenderer` multiplica, então branco é exatamente a cor que não muda nada. Sprite pisca para um vermelho claro.

## A paleta

Os cinco valores saem da barra da folha de referência, `.claude/specs/characters/heroes/tempo-reference.png`, medidos pela média de cada quadradinho, já que eles são texturizados e não chapados.

| Papel | Hex | Onde vive |
| --- | --- | --- |
| Anil | `#149ECA` | A cor da Tempo, e de mais ninguém. Peças grandes do corpo |
| Marfim | `#EEE2D0` | Contraste. Peito, coxas, painéis vizinhos ao anil |
| Amarelo | `#F3B738` | Sinalização. Crista, filete, visor. Sempre pouco |
| Grafite | `#2E333C` | Articulação. Só onde o corpo dobra |
| Violeta | `#B33EA3` | O Catarsis. Núcleo e capa. Reservado, não é de ninguém |

- **O anil é exclusivo da Tempo.** A regra de identidade cromática está no `roadmap.md` e vale para o elenco inteiro: cor é do personagem, nunca do time.
- **O campo `Color` da ficha não é decoração.** Ele deixou de pintar o corpo quando o sprite entrou, mas continua colorindo o **projétil do ataque à distância** no `OnBasicAttackLanded`. Ele precisa acompanhar o tom do personagem, senão um herói anil dispara tiros de outra cor.
    - Foi exatamente o que aconteceu entre a 0.10.1.0 e a 0.10.2.2: a Tempo ficou ciano e o campo continuou cobalto, sem nada acusar, porque ela ainda não tinha arma de alcance para revelar.

## A folha de origem

- Os seis desenhos foram recortados de `.claude/specs/characters/heroes/tempo sprites.png`, uma folha de 192×256 com 64 quadros de 24×32 gerada por ferramenta.
- **A linha 1 é um ciclo de corrida utilizável**, e virou os oito quadros de `tempo-run`.
- Isso contraria o que este arquivo dizia até a 0.10.2.3, que era que a folha não servia para animação. Aquela conclusão saiu de duas medições erradas, e vale registrar as duas para não se repetirem:
    - O filtro procurava **tronco parado com pernas alternando**, que é o critério de uma caminhada clássica. Corrida move o corpo inteiro por definição, então o filtro descartava justamente o que servia.
    - O parâmetro citado, de 10% a 15% de diferença entre quadros vizinhos, vale para sprite grande. Num 24×32 a personagem tem umas 300 pixels e mover braços e pernas em 2 pixels muda quase tudo: medindo só onde há personagem, **todas** as oito linhas dão de 87% a 98%.
- **Churn de pixel mede quanto mudou, não se a mudança faz sentido.** O que decide se uma folha anima são outras três coisas, e as três passam aqui: âncora dos pés igual nos 64 quadros, altura variando 1 pixel dentro da linha, e paleta unificada.
- As demais linhas continuam sendo poses avulsas, e delas saíram os seis desenhos parados.
- A paleta dos seis foi unificada em 15 cores. A folha original tinha 253 cores e **nenhuma delas aparecia nos 64 quadros**, ou seja, cada quadro tinha tons próprios e a Tempo mudaria de cor ao virar.
