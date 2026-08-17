# Main Camera

## Objetivo

Câmera única da cena de batalha. Ela é responsável por manter o jogo em pixel art nítido dentro da janela pequena descrita em `gameplay.md`.

## Onde fica

Na raiz da hierarquia da cena. Já existe por padrão na `SampleScene`.

## Transform

- Position: `X 0`, `Y 0`, `Z -10`.
- Rotation e Scale: valores padrão.

O tabuleiro é desenhado centralizado na origem, então a câmera em `(0, 0)` já enquadra tudo.

## Camera

- Projection: `Orthographic`.
- Size: `5.33`.
- Background Type: `Solid Color`.
- Background: um cinza bem escuro, por exemplo `#141418`.

O valor `5.33` vem da conta `320 ÷ 30 ÷ 2`: são 320 pixels de altura na resolução lógica, 30 pixels por unidade de mundo, e o tamanho ortográfico corresponde à metade da altura visível.

## Pixel Perfect Camera

Componente que precisa ser adicionado na mão (`Add Component` → `Pixel Perfect Camera`).

- Assets Pixels Per Unit: `30`.
- Reference Resolution X: `180`.
- Reference Resolution Y: `320`.
- Upscale Render Texture: desmarcado.
- Pixel Snapping: marcado.
- Crop Frame: `None`.

Esses números não são arbitrários e não devem ser mudados isoladamente:

- `30` pixels por unidade combinado com `CellSize = 1` no `BattleGridConfig` faz uma casa medir exatamente 30x30 pixels.
- `180` de largura dividido por 30 dá 6 unidades, que é exatamente a largura do tabuleiro de 6 colunas. O tabuleiro preenche a largura inteira.
- `320` de altura dividido por 30 dá 10.67 unidades. O tabuleiro de 8 fileiras ocupa 8 dessas unidades, sobrando espaço acima e abaixo para a interface que ainda não existe.

Se um dia o tabuleiro mudar de tamanho, esses valores precisam ser recalculados junto.

### A resolução de referência é o tamanho da janela

Desde a 0.5.8.0, a janela do jogo é sempre `180×320` multiplicado por um número inteiro: 1×, 2× ou 4×. O `WindowScale` lê esses dois números **deste componente**, e não os repete em lugar nenhum, justamente porque eles já estão amarrados aos pixels por unidade e à largura do tabuleiro.

Isso significa que mudar a resolução de referência muda o tamanho de toda janela possível. Ela deixou de ser só uma configuração de câmera.

`Crop Frame` continua em `None` e está certo assim. Ele existiria para preencher com barras o que sobra quando a janela tem outra proporção, e com a escada de múltiplos inteiros isso nunca acontece.

**Não existe rung abaixo de 1×.** Para o jogo ficar menor seria preciso reduzir esta resolução de referência, o que significa mostrar menos tabuleiro, e aí os números acima todos mudam juntos.

## Global Light 2D

Já existe na `SampleScene`. Intensidade `1` e cor branca deixam as cores dos quadrados aparecerem exatamente como estão definidas.

### Target Sorting Layers precisa ser `All`

Esta é a configuração mais importante do objeto e a mais fácil de esquecer.

A URP 2D renderiza sprites com `Sprite-Lit-Default`, então **tudo que estiver em uma sorting layer fora do alcance da luz global aparece preto**. Não existe erro, aviso ou mensagem no Console: o objeto está lá, com a cor certa, e desenha preto.

A luz global nasce apontando apenas para as sorting layers que existiam quando ela foi criada. Como a `SampleScene` é anterior às nossas layers, ela vinha apontando só para `Default`, e por isso o tabuleiro e os personagens ficavam invisíveis.

Sempre que uma sorting layer nova for criada em `ProjectSettings/TagManager.asset`, confira se a luz global continua cobrindo todas elas.
