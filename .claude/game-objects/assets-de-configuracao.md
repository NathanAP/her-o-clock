# Assets de configuração

## Objetivo

Descrever os ScriptableObjects que alimentam o `BattleBootstrap`. Eles não são GameObjects, mas sem eles a cena não monta nada.

Todos são criados pelo menu de contexto da janela Project: `Create` → `Her-o-clock` → nome do asset.

Local recomendado: `Assets/ScriptableObjects/`.

## BattleGridConfig

Dimensões do campo de batalha. Um único asset, chamado `BattleGridConfig`.

- Columns: `6`
- Rows: `8`
- Hero Rows: `4`
- Cell Size: `1`

Fileiras de 1 a 4 são a área dos heróis, de 5 a 8 a área dos lacaios e vilões, conforme `gameplay.md`.

Esses valores ficam em asset justamente para conseguirmos testar outros tamanhos sem mexer em código. Ao mudar as dimensões, os valores da `Main Camera` precisam ser recalculados junto.

## CharacterDefinition

Ficha de um personagem. É o molde imutável: história, design, cor e habilidades. O nível, os atributos e futuramente o equipamento pertencem à **instância** criada em campo, não à ficha.

O campo `Id` é obrigatório e é por ele que os arquivos de fase e, no futuro, os saves apontam para a ficha. **Nunca mude um `Id` depois que existir conteúdo referindo-se a ele.**

As cores seguem o roadmap: azul para heróis, rosa para lacaios, vermelho para vilões.

A ficha **não guarda nenhum texto que o jogador leia**. O nome mostrado vem do arquivo de strings, na chave `character.{id}.name`. Ver "## Arquivo de strings" no fim deste arquivo.

### As fichas do ato 1

Desde a 0.9.0.0 as fichas são geradas a partir dos documentos de design em
`.claude/specs/characters/`, campo a campo. **Não edite os números no Inspector**: mexa na ficha
de design e gere de novo, senão os dois lados passam a discordar em silêncio, que é o problema
que a ponte da 0.9.1.0 existe para pegar.

| Asset | Id | Tipo | Documento de design |
|---|---|---|---|
| Tempo | `tempo` | Hero | `heroes/tempo.json` |
| Gadrat | `gadrat` | Hero | `heroes/gadrat.json` |
| Gadrat NPC | `gadrat-npc` | Npc | `heroes/gadrat.json`, sem as habilidades |
| Discarded Prototype | `discarded-prototype` | Minion | `minions/discarded-prototype.json` |
| Exposed Prototype | `exposed-prototype` | Villain | `villains/exposed-prototype.json` |

O `Gadrat NPC` é o mesmo personagem espelhado: mesmos atributos, mesma defesa, **lista de
habilidades vazia**. Ele só dá ataque básico, e é assim que a fase 3 o usa.

As quatro fichas de teste (`hero-tank`, `hero-archer`, `minion-standard`, `villain-boss`) foram
apagadas nesta versão. Elas eram um andaime da 0.1.0.0 e nunca foram conteúdo.

### Crescimento de defesa por nível

Toda ficha declara, ao lado do valor inicial de armadura, de evasão e de cada resistência, **quanto aquele atributo ganha por nível**. Sem isso a defesa apodrece sozinha, porque a constante da curva cresce com o nível do atacante, e ela é a mesma para as três defesas.

**Ganho igual ao valor inicial mantém a defesa parada para sempre**, pois o valor e a constante passam a crescer juntos e se cancelam. Vale igual para armadura, evasão e resistência.

Todas as fichas do ato 1 usam essa forma. Os valores de cada uma vivem no documento de design
dela, e a mitigação que eles produzem em cada nível vive em `.claude/balance/snapshot.md`, que é
gerado por teste. **Nenhum dos dois é copiado para cá**, porque um número derivado dentro de um
`.md` envelhece em silêncio.

Ganhos menores que a base fazem a defesa perder força devagar, e maiores fazem ganhar. As duas coisas são escolhas válidas.

A armadura e a evasão nas fichas de **herói** são um substituto de equipamento, já que itens ainda não existem. Quando existirem, as duas devem ir a zero e o equipamento assumir.

A **evasão** tem uma segunda fonte além da ficha: o AGI, convertido a 1, 0.5 ou 0.2 ponto por ponto conforme a classe. Por isso um personagem com evasão zero na ficha ainda desvia um pouco, e é o que acontece com todo lacaio e vilão hoje.

### Crescimento por nível

Toda ficha precisa declarar como o personagem gasta os **5 pontos que recebe a cada nível**, em porcentagens que somam 100. O campo fica logo abaixo do nível.

Para heróis é a distribuição padrão, que o jogador vai poder substituir quando existir interface. Para lacaios e vilões é a **única** forma que eles têm de ficar mais fortes, já que ninguém distribui pontos por eles.

Os quatro números de cada ficha vêm do documento de design dela.

O campo `Max Level` fica em `100` para heróis. Para lacaios e vilões ele pode ser menor, se você quiser que aquele inimigo não exista acima de certo nível.

O Console avisa se as porcentagens não somarem 100. Elas continuam funcionando (são normalizadas), mas quase sempre é erro de digitação.

## `ItemDatabase`

O catálogo dos arquivos de item, em `Assets/ScriptableObjects/ItemDatabase.asset`. Seis referências de `TextAsset`, uma por arquivo de `Assets/Items/`.

São referências de asset e não caminhos em texto, pelo mesmo motivo do `StageDatabase`: renomear ou mover um arquivo não quebra nada.

**Não guarde nada derivado nele.** O projeto roda com Domain Reload desligado, então um campo de ScriptableObject mantém o valor entre sessões de Play para sempre, e um cache montado numa sessão ruim ficaria ruim para sempre. Foi assim que o `CharacterDatabase` quebrou todas as fases uma vez.

Um teste confere que as seis referências resolvem. Sem ele, uma entrada vazia apareceria como conteúdo que simplesmente não existe.

### O dano do ataque básico é uma faixa

A ficha declara um **mínimo e um máximo**, cada um com o próprio ganho por nível, e cada golpe sorteia entre os dois. Os campos são `Base Damage Min`, `Base Damage Min Per Level`, `Base Damage Max` e `Base Damage Max Per Level`.

Deixar os dois iguais faz o personagem bater sempre o mesmo, o que é válido.

**A abertura entre eles é escolha de ficha**, e é o que separa um personagem que bate igual de um que bate em picos. Escreva-a em proporção à média e não em pontos fixos, senão ela vira irrelevante conforme o nível sobe.

Os quatro números são `float`, e não é firula: um lacaio entre 1.5 e 2.5 perderia a faixa inteira se as pontas fossem inteiras.

### Campos de defesa e ofensiva

Além dos acima, toda ficha possui campos que ainda não têm fonte no jogo, pois dependem de itens e habilidades. Podem ficar em zero, ou ser preenchidos na mão para experimentar:

- Fire Resistance, Water Resistance, Electric Resistance: pontos de resistência a cada elemento. Passam pela mesma curva da armadura física.
- Thorns Percent: dano físico devolvido a quem ataca, de 0 a 100.
- Life Steal Percent: vida recuperada ao causar dano físico, de 0 a 100.

Como o ataque básico ainda é sempre físico, as três resistências elementais não têm efeito nenhum por enquanto. Espinhos e roubo de vida funcionam desde já.

### Tipo de ataque básico

O campo `Auto Attack` diz se o ataque básico desenha um projétil (`Ranged`) ou não (`Melee`). Ele é **puramente visual** e não muda regra nenhuma: quem decide alcance são o `Min Range` e o `Max Range`.

Um personagem de alcance longo com ataque `Melee` bate de longe sem nada voando, e isso é uma combinação válida de se querer. **Nenhuma ficha do ato 1 é `Ranged`**, então o projétil da 0.8.0.0 não aparece em jogo até existir um personagem à distância.

### Lista de habilidades

A Tempo e o Gadrat têm habilidade; o `Gadrat NPC`, os lacaios e os vilões têm a lista vazia. Elas são geradas junto com o resto da ficha, a partir do documento de design.

Ao preencher uma habilidade à mão no Inspector, dois campos carregam regra e passam despercebidos:

- **Falloff**, dentro de um efeito de dano: o quanto o dano perde a cada casa de distância de quem usou. Fica em `0` quando a habilidade não quer isso, e aí o dano é o mesmo em qualquer distância. Ele é recusado nas formas `Self` e `Single`, onde todo alvo está sempre à mesma distância.
- **Target igual a Self em um efeito de dano**: é assim que se escreve o custo de vida de uma habilidade. Esse dano nunca mata quem a usou, ele para em 1 de vida.

Todo campo numérico é um `RankedValue`, ou seja, uma lista. **Uma entrada** significa constante em todos os ranks, e **uma por rank** significa que o valor escala. Qualquer outro tamanho é recusado pelo validador, e é o erro mais fácil de cometer no Inspector, porque a lista cresce com um clique e ninguém confere o tamanho depois.

## CharacterDatabase

Um único asset chamado `CharacterDatabase`, com a lista de todas as fichas.

Existe porque os arquivos de fase são JSON e não conseguem guardar referência de asset. Eles nomeiam o personagem por `Id` e este banco resolve. Os saves vão precisar exatamente da mesma coisa.

Arraste todas as fichas para a lista `Characters`. O teste `ContentTests` reprova se alguma ficha do projeto ficar de fora. O banco reclama no Console se alguma ficha estiver sem `Id` ou se dois `Id` forem iguais.

## StageDatabase

Um único asset chamado `StageDatabase`, com a lista dos arquivos `.json` das fases, na ordem em que devem ser jogadas.

Arraste as quatro fases de `Assets/Stages/` para a lista `Stages`, na ordem em que devem ser jogadas.

Os arquivos são referenciados como `TextAsset`, e não carregados por nome de uma pasta `Resources`, para que renomear ou mover uma fase nunca quebre a referência.

## BattleFormation

O time do jogador e onde ele começa no tabuleiro. Agora existe **apenas a formação dos heróis** — os inimigos vêm das fases.

- Team: `Heroes`
- Placements:
    - `Gadrat`, Column `3`, Row `2`
    - `Tempo`, Column `4`, Row `1`
A fileira 1 é a mais recuada, então o Gadrat na fileira 2 fica **à frente** da Tempo. É o que a ficha dele pede: ele é o tanque pesado, ela é a rápida e frágil.

A mesma ficha pode aparecer várias vezes. Cada linha cria um personagem separado, com atributos próprios.

O `Form Enemies` foi apagado na 0.9.0.0. Ele era um resto de antes das fases existirem, não era usado por script nenhum, e apontava para fichas que não existem mais.

## Arquivo de strings

`Assets/Strings/en.json`, apontado pelo campo `Strings File` do `BattleBootstrap`.

Ele guarda **todo** texto que o jogador lê, e as chaves são montadas a partir do `Id` que o conteúdo já carrega:

- `character.{id}.name` — o nome mostrado de uma ficha.
- `stage.{id}.name` e `stage.{id}.lore` — o nome e a pequena história de uma fase.

Não existe nada para manter em sincronia: renomear um `Id` é o mesmo ato que renomear o texto dele.

O `BattleBootstrap` confere o arquivo ao iniciar e reclama no Console de chave faltando **e** de texto sobrando, que é o de conteúdo apagado. Uma chave que não existe aparece na tela como `#chave#`, e não como vazio, porque um rótulo que some sem deixar rastro é muito mais difícil de notar.
