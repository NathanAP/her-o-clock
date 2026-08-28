# Ferramentas do editor

## Objetivo

Descrever o que existe no menu `Her-o-clock` da Unity, para que serve cada coisa e quando usar. Nada aqui roda no jogo: são todas ferramentas de quem desenvolve, e vivem em `Assets/Scripts/Editor/`.

## `Her-o-clock` → `Character sheets`

Uma janela com **todas as fichas do projeto lado a lado, no mesmo nível**.

### Por que ela existe

Já havia dois lugares para olhar número de ficha, e nenhum respondia a esta pergunta:

- O **Inspector de cada ficha** responde "o que esta ficha produz", uma de cada vez.
- O **`.claude/balance/snapshot.md`** responde "o que esta alteração fez com o resto do jogo", mas só no nível inicial de cada ficha e só depois de commitado.

Falta comparar duas fichas **no nível em que elas se encontram de verdade**. Um lacaio é escrito no nível que a fase entrega, e um herói chega no nível que o jogador trouxe — pôr os dois no mesmo nível é uma escolha de quem está olhando, e isso é um controle deslizante, não um arquivo.

### O que ela mostra

- Um controle de **nível** e um de **multiplicador de fase**, iguais aos do Inspector de ficha.
- Uma tabela por tipo de personagem — heróis, lacaios, vilões e NPCs — com os quatro atributos, vida, dano, ataques por segundo, dano por segundo, casas por segundo, evasão, armadura e mitigação.
- A mitigação e a evasão são lidas **contra um atacante do mesmo nível**, que é o número que o crescimento da defesa existe para manter parado. Contra um atacante fixo elas pareceriam despencar sem estar despencando.
- A evasão precisa de um atacante para existir: a constante da curva dela cresce com o nível de quem ataca, então um personagem não tem chance de evasão própria. A ficha do personagem mostra os pontos ao lado da chance por esse motivo.
- Clicar no nome seleciona a ficha no Project, então a janela também é um caminho para dentro delas.

### As habilidades, e por que elas estão aqui

Cada ficha lista as habilidades disponíveis naquele nível, com o rank, o dano e — o principal — **o alcance escrito por extenso**.

Uma área é centrada em quem usa e pega todo mundo dentro dela, aliado incluído, exatamente como o `abilities.md` manda. Essa regra é fácil de concordar no abstrato e fácil de levar susto no tabuleiro, e **não existia lugar nenhum no editor que a mostrasse**. Por isso a janela escreve `CATCHES ALLIES` em toda área, e avisa que linha pega quem estiver no caminho.

Foi assim que se descobriu que o Gadrat tirava um terço da vida da Tempo por caste sem ninguém ter errado nada.

### Aba `Heroes in play`

A segunda aba mostra os **heróis da sessão que está rodando**: nível, experiência para o próximo, pontos de habilidade, atributos e pontos não gastos. Fora do Play ela avisa que não há nada para mostrar.

- Ela lê **registros** e nunca combatentes. Uma ficha é conteúdo e um registro é save, então esta é uma informação que a aba de fichas não pode dar.
- Durante uma fase, o nível aqui pode ser maior que o do personagem no tabuleiro. Isso é a regra descrita em `gameplay.md`, e a aba diz isso em texto para que a diferença não seja lida como defeito.
- Ela existe porque o rótulo de nível saiu de cima do personagem, e ele era o único lugar onde esse número aparecia. **Não é o perfil do herói**, que é conteúdo de versão futura e serve ao jogador.
- `BattleBootstrap.LiveRecords` devolve o que existe e nunca cria registro, senão abrir a aba mudaria o jogo. A aba se repinta sozinha, só durante o Play e só quando está selecionada.

### Quando ela relê o projeto

A lista de fichas é lida ao abrir a janela, ao voltar o foco para ela e quando o editor avisa que um asset mudou — nunca dentro do `OnGUI`. `OnGUI` roda a cada repintura, inclusive com o mouse só passando por cima, então buscar as fichas de lá custaria uma varredura do projeto inteiro por quadro para montar uma lista que quase nunca muda. Uma ficha destruída debaixo da janela é pulada em vez de estourar.

### O que ela não faz

- **Não escreve nada.** Tudo é lido pelas mesmas propriedades que o jogo lê, então não pode divergir do que acontece de verdade.
- **Não considera itens**, que ainda não existem.
- **Não substitui o snapshot.** O snapshot é retrato commitado e conferível por diff; a janela é exploração ao vivo. Os dois respondem perguntas diferentes.

## `Her-o-clock` → `Delete saved games`

Apaga os saves, para recomeçar de um ponto limpo.

### Para que serve

Para quem mexe nos números. Um rebalanceamento é muito mais fácil de ler do nível 1 do que com um time carregando progresso feito sob os números antigos.

**Não é o "recomeçar" do jogador.** Esse é opção de menu do jogo, com texto próprio e consequências próprias, e chega quando o menu existir.

### O que o diálogo diz antes de apagar

Quantos arquivos, qual a pasta e qual é o mais recente. Isso não é firula: todo save é um arquivo novo e os antigos **são o backup**, que é o que permite ao jogador voltar para ontem conforme `save.md`. Então resetar joga fora um histórico inteiro e não um arquivo, e uma confirmação que não diz o tamanho do estrago é um clique errado esperando acontecer.

### O que ele se recusa a tocar

Apenas arquivos cujo nome o jogo entende. Não é cautela gratuita, é a regra de `save.md`: um arquivo com nome ilegível fica onde está, porque não há como saber o que ele é.

### Por que fica cinza durante o Play

Apagar os arquivos no meio de uma sessão não reseta nada — o elenco vive na memória e o fim da próxima onda simplesmente escreve um arquivo novo. Desabilitar é mais honesto do que deixar parecer que não funcionou.

## `Her-o-clock` → `Grant a reference kit`

Veste todos os heróis da sessão com um conjunto simples de equipamento, no nível de cada um e na classe da ficha deles.

### Para que serve

Nada dropa ainda e não existe inventário, então sem isso **não há como assistir uma luta com equipamento**. A única evidência de que a coisa funciona seriam os testes.

Ele entrega o mesmo kit sem graça que o snapshot mede — só a defesa base dos quatro cascos e da mão secundária, sem modificador nenhum. Assim o que aparece na tela e o que está escrito no snapshot são a mesma coisa.

### Por que a classe é a da ficha

Um kit que o herói não consegue sustentar seria entregue e desconsiderado na mesma hora, o que parece exatamente com a ferramenta quebrada. Escolher a classe da ficha é a aposta que tem mais chance de realmente vestir.

### Por que só vale na próxima fase

Ele dá o item ao **registro**, e quem luta é uma fotografia tirada quando a fase começou. Não é limitação: é a regra de `gameplay.md` de que trocar equipamento no meio de uma fase não alcança o tabuleiro, e a ferramenta obedecendo ela é o ponto.

### Por que fica cinza fora do Play

Registro só existe durante uma sessão. Fora dela não há herói para vestir.

## `Her-o-clock` → `Link character sprites`

Liga os desenhos de cada personagem à ficha dele, por nome de arquivo, e conserta importação quebrada no caminho. Descrita em `arte-de-personagem.md`.

## Inspector de `CharacterDefinition`

Não fica no menu: aparece sozinho ao selecionar uma ficha. Mostra o que aquela ficha produz num nível escolhido, e é o irmão de uma ficha só da janela acima.
