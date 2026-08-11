# Arquitetura

## Objetivo

Guardar as decisões de estrutura de código que não dá para deduzir lendo os arquivos, e que devem ser respeitadas nas próximas versões.

## A cena guarda um único GameObject

A `SampleScene` tem apenas `Main Camera`, `Global Light 2D` e `Battle`. Tabuleiro, personagens e diretor do combate são criados em tempo de execução pelo `BattleBootstrap`.

O motivo é git: arquivo `.unity` é dos piores de resolver conflito, e cena enxuta praticamente elimina o problema. Ao adicionar coisas novas, prefira criar por código a arrastar na hierarquia.

## Existe um laço único de atualização

Nenhum personagem tem `Update`. Quem atualiza todo mundo é o `BattleDirector`, sempre na mesma ordem.

Isso não é preciosismo: a progressão offline, que é o coração do gênero segundo `references.md`, precisa simular um combate inteiro sem renderizar nada. Com laço único e ordem fixa isso é só chamar o mesmo código com outro delta de tempo. Com `Update` espalhado, seria reescrever tudo.

Pelo mesmo motivo, a cadeia de escolha de alvo em `TargetSelector` compara apenas inteiros, sem float no meio.

## A cadeia de alvo serve para duas coisas

`TargetSelector.Select` recebe um parâmetro `respectRange`:

- `true` decide quem atacar, considerando apenas quem está dentro do alcance.
- `false` decide em direção a quem andar, ignorando o alcance.

É a mesma cadeia nos dois casos, e é isso que faz uma provocação continuar valendo mesmo quando o provocador está longe demais para ser atacado.

## Números que estão amarrados entre si

A resolução da câmera e o tamanho do tabuleiro precisam mudar juntos:

- `30` pixels por unidade na `Pixel Perfect Camera`, com `CellSize = 1`, faz a casa medir 30x30 pixels.
- Resolução de referência `180x320`: a largura dividida por 30 dá exatamente as 6 colunas do tabuleiro.
- Tamanho ortográfico `5.33` vem de `320 ÷ 30 ÷ 2`.

Mexer em um sem recalcular os outros desalinha o pixel art em silêncio.

## Sorting layers usadas pelo código

`BoardRenderer` usa `Background` e `BattleBootstrap` usa `Characters`, ambas pelo nome. Renomear qualquer uma quebra a renderização sem erro no Console.

## A armadilha das sorting layers com luz 2D

Já custou uma sessão de depuração na 0.1.0.0, então vale registrar.

A URP 2D renderiza sprites com `Sprite-Lit-Default`. Uma sorting layer que não esteja em `Target Sorting Layers` da `Global Light 2D` renderiza **preto**, sem erro, sem aviso, sem nada no Console. O objeto existe na Hierarchy, tem a cor certa no Inspector, e some na tela.

A luz global nasce apontando apenas para as sorting layers que existiam quando ela foi criada. Como criamos as layers depois da cena, ela ficou apontando só para `Default` e o tabuleiro inteiro sumiu.

Ao criar qualquer sorting layer nova, confira a luz global. Se algo desenhar preto e o Console estiver limpo, suspeite disso primeiro.
