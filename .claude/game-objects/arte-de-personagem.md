# Arte de personagem

## Objetivo

Descrever como um personagem deixa de ser um retângulo colorido e passa a ter desenho, e quais números precisam continuar batendo para que ele apareça do tamanho certo e no lugar certo.

## Onde os arquivos ficam

- `Assets/Art/Characters/` guarda um `.png` por direção, um arquivo por desenho.
    - `tempo-down.png` — virado para baixo, o jogador vê a frente.
    - `tempo-up.png` — virado para cima, o jogador vê as costas.
    - `tempo-side.png` — de perfil, olhando para a **direita**. A esquerda é este mesmo desenho espelhado.
    - `tempo-dead.png` — caído.
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
    - Um passo que não sai do lugar mantém a direção atual, para quem parou de andar continuar olhando para quem estava perseguindo.
- **A direção é lida no `Update` e não no evento `Changed`.** Andar não dispara `Changed`, e dispará-lo a cada passo reconstruiria vida e atributos de quem só virou de lado. O custo de ler no `Update` é uma comparação de enum por quadro.

## Valores no `CharacterViewSettings` que mudaram junto

- `SpriteOffsetY` nasceu em `-0.5`, que é meia casa para baixo, ou seja o chão.
- `HealthBarOffsetY` subiu de `0.4` para `0.55`. Um sprite de 32 pixels é bem mais alto que o retângulo de `0.7` casa que existia antes, e a barra ficava em cima da cabeça.
- `SpriteFlashColor` nasceu separado do `FlashColor`. A piscada de dano do retângulo é para o branco, e **o branco não serve para sprite**: a cor do `SpriteRenderer` multiplica, então branco é exatamente a cor que não muda nada. Sprite pisca para um vermelho claro.

## A folha de origem

- Os quatro desenhos foram recortados de `.claude/specs/characters/heroes/tempo sprites.png`, uma folha de 192×256 com 64 quadros de 24×32 gerada por ferramenta.
- **Aquela folha não é uma animação.** A diferença entre quadros vizinhos fica entre 34% e 50% dos pixels em todas as oito linhas; um ciclo de caminhada de verdade muda de 10% a 15%. Tocar aqueles quadros em sequência seria tremida, não movimento.
- Por isso o que entrou foram quatro poses paradas. Animação depende de uma folha feita para isso, e é problema de outra versão.
- A paleta dos quatro foi unificada em 15 cores. A folha original tinha 253 cores e **nenhuma delas aparecia nos 64 quadros**, ou seja, cada quadro tinha tons próprios e a Tempo mudaria de cor ao virar.
