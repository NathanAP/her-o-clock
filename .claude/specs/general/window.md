# Janela

## Objetivo

Especificar como a janela do jogo se comporta na área de trabalho do jogador.

Her-o-clock não é um jogo que se abre para jogar e se fecha ao terminar. Ele fica aberto o dia inteiro em um cantinho da tela, ao lado do que a pessoa está realmente fazendo, conforme descrito em `gameplay.md`. Isso faz da janela parte do design, e não uma configuração qualquer.

## A referência

O Task Bar Hero é a referência direta aqui, como em quase tudo. O que se quer dele:

- Fica acima de tudo, sem precisar ser procurado.
- Tem poucos tamanhos, escolhidos, e não um tamanho arrastável qualquer.
- Gruda nos cantos da tela.
- Não parece um programa, parece um enfeite da área de trabalho.

## Tamanho

- A janela **sempre** tem a resolução de referência multiplicada por um número inteiro. Os tamanhos permitidos são **1×, 2× e 4×**.
- A resolução de referência é a do `Pixel Perfect Camera`, hoje `180×320`. Ela é lida de lá, nunca repetida, pois esses números já estão amarrados aos pixels por unidade e à largura do tabuleiro.
- Múltiplos inteiros não são preciosismo. Em 2× cada pixel do jogo é exatamente quatro pixels da tela; em 2,3× alguns pixels sairiam maiores que outros e a arte cintilaria.
- Como a proporção nunca desvia, **nunca é preciso cortar a imagem nem preencher com barras**.

### Não existe tamanho abaixo de 1×

- Para ficar menor, a imagem precisaria ser **reduzida**, e reduzir pixel art quebra a grade.
- O único caminho para um jogo genuinamente menor é diminuir a resolução de referência, o que significa **mostrar menos tabuleiro**. Isso é decisão de enquadramento, e mexe junto nos pixels por unidade e no tamanho do tabuleiro.

### O tamanho inicial sai da tela do jogador

- 180×320 é uma janela pequena em um monitor 1080p e um selo postal em um 4K. Por isso o tamanho inicial é escolhido, e não fixado.
- Ele ocupa no máximo **40% da tela** ao abrir pela primeira vez. Deliberadamente pequeno: o jogo existe para ficar ao lado do que a pessoa está fazendo, e uma janela que abre ocupando metade da tela não é isso.
- O jogador pode chegar a no máximo **90% da tela**, para a janela nunca ficar maior que a área de trabalho.
- Na prática: **1× em 1080p e 1440p, 2× em 4K**.
- A escolha é guardada nas preferências do jogador, e não no save. Ela descreve a tela dele, não o progresso, então sobrevive a começar um jogo novo e não viaja para outra máquina.
- Ela é reconferida a cada abertura. Um tamanho lembrado pode não caber mais se o jogo foi levado para um notebook menor.

## Posição

- A janela **não tem moldura**: nada de borda, barra de título ou botões de minimizar, maximizar e fechar.
- Ela é movida **arrastando o próprio jogo**, clicando em qualquer lugar dele.
- Ela **fica acima de todas as outras janelas**, incluindo a barra de tarefas.
- Não existe tela cheia, e não é possível redimensionar arrastando as bordas.

### Grudar nos cantos

- Quando uma borda da janela chega perto de uma borda da tela, ela é puxada e encosta.
- Cada eixo decide sozinho, o que faz os cantos funcionarem sem serem um caso à parte: arrastar para o canto inferior direito dispara os dois puxões ao mesmo tempo.
- O limite usado é o **monitor inteiro**, e não a área de trabalho do Windows. Como o jogo fica acima da barra de tarefas, grudar na área de trabalho deixaria uma folga exatamente na borda onde a barra costuma estar, que é justamente onde mais se quer encostar a janela.
- O monitor é sempre o que a janela está ocupando, e nunca o principal. Segundo monitor é comum, e grudar na tela errada seria pior que não grudar.

## Consequências que precisam ser resolvidas

Tirar a moldura tem um preço que ainda está em aberto:

- **Não existe botão de fechar.** Fechar é Alt+F4 até existir um menu que ofereça isso.
- **Qualquer clique arrasta a janela.** Hoje é inofensivo, porque não existe nada clicável no jogo. Quando existir interface, o arrastar vai precisar distinguir um clique no fundo de um clique em um botão.
- **Não existe atalho para esconder a janela**, que é algo que um enfeite de área de trabalho deveria ter.

Os três pertencem à versão de menus.

## O tabuleiro não tem margem lateral

- O tabuleiro tem 6 colunas de 30 pixels, ou seja **exatamente** os 180 pixels da largura de referência.
- Isso é intencional: a largura inteira é do tabuleiro, e a interface futura vai ocupar o espaço que sobra acima e abaixo.
- O efeito colateral é que a largura da janela não tem nenhuma folga. Se ela ficar alguns pixels mais larga do que deveria, o chão simplesmente acaba e aparece o fundo da câmera.
- Por isso duas coisas existem juntas:
    - A janela **força o próprio tamanho** pelo sistema operacional, em vez de deixar isso para o motor. Pedir uma resolução ajusta a área útil e deixa o sistema calcular a janela em volta dela, contando com uma moldura que já não existe mais, e as duas contas discordam por alguns pixels.
    - O tabuleiro desenha **duas colunas decorativas de cada lado**, do mesmo jeito que já desenha fileiras decorativas acima e abaixo para a rolagem entre ondas. Ninguém anda nelas. Quando a janela está no tamanho certo elas ficam fora da tela e não custam nada; quando alguma coisa dá errado, a diferença aparece como um pouco mais de chão em vez de uma faixa preta.

## Sobre ficar acima de tudo

- Acima da barra de tarefas e de programas comuns, funciona.
- Acima de um jogo em **tela cheia exclusiva**, não é confiável. Esse modo entrega o controle direto do vídeo para a aplicação, e nada por cima é garantido. A maior parte dos jogos modernos usa janela sem borda em tela cheia, que é uma janela comum e se comporta.
- Ficar no topo não é permanente: outro programa indo para o topo, ou o sistema reorganizando a ordem das janelas, derruba a nossa sem avisar. Por isso ela reafirma a posição periodicamente.

## Plataforma

- Tudo nesta página, fora o tamanho, depende de conversar com o sistema operacional. Hoje isso existe **apenas para Windows**.
- Em qualquer outra plataforma, e dentro do editor, o comportamento é simplesmente desligado: a janela fica comum.
- Desligar dentro do editor não é conveniência, é proteção. A janela que essas chamadas encontrariam ali é a do próprio editor da Unity, e uma chamada perdida tiraria a barra de título dele ou o prenderia acima de todos os programas.
