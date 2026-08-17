# Objetivo

- Mapear o roadmap do projeto de forma que fique claro o que foi feito recentemente e o que vai ser feito no futuro. Este arquivo está diretamente ligado às pastas:
    - `.claude/memory/`, pois a memória desta pasta pode conter detalhes necessários nas versões futuras.
    - `.claude/specs/`, pois os principais detalhes do projeto estão descritos nesta pasta.
    - `.claude/versions/`, pois versões passadas estarão resumidas nesta pasta.
- Você tem liberdade em alterar este arquivo conforme achar necessário.

# Atual estado do projeto

Estamos em uma versão TOTALMENTE inicial. Muitos detalhes presentes na pasta `.claude/specs/` não estão 100% definidos, bem organizados e até mesmo bem descritos. A ideia atual é montar uma casca de código e organização no Unity para chegarmos aos poucos no que está descrito naquela pasta.

Até que toda a funcionalidade básica esteja pronta, vamos fazer o jogo funcionar em game objects lisos e sem nenhuma textura. Utilize cores para diferenciar personagens e cenário, não tem problema ficar algo feio agora. Vamos focar na jogabilidade e funcionalidade.

Utilize tons de azul para heróis; tons de vermelho para vilões; tons de rosa para lacaios; tons de verde para cenário.

# Versões

## 0.1.0.0 (feita)

- Base de código do projeto e a primeira coisa acontecendo na tela.
- Tabuleiro visível, personagens coloridos posicionados nele, e eles andando até o inimigo e parando no alcance.
- Sem combate: nada de dano, morte ou habilidade.
- O resumo completo está em `.claude/versions/20260811_0.1.0.0.md`.

## 0.2.0.0 (feita)

- Combate.
- Ataque básico, velocidade de ataque, mitigação, evasão e morte.
- Barra de vida, piscada ao levar dano e números de dano flutuantes.
- Enquanto não existem fases, a batalha reinicia sozinha para permitir observar o balanceamento.
- O resumo completo está em `.claude/versions/20260811_0.2.0.0.md`.

## 0.2.1.0 (feita)

- Padronização de idioma no código.
- Scripts, comentários, documentação em XML, textos do Inspector e mensagens de Console em inglês, junto com a pasta `.claude/memory/`.
- As specs, as versões e os game-objects continuam em português.
- O resumo completo está em `.claude/versions/20260811_0.2.1.0.md`.

## 0.3.0.0 (feita)

- Fase.
- Grupos de lacaios em sequência, vilão no fim, vitória e derrota, reinício automático.
- As fases são arquivos `.json`, escritos fora da Unity e validados antes de rodar. As fichas de personagem continuam ScriptableObject.
- Os heróis não recuperam vida entre ondas, o que cria a necessidade de farmar.
- O chão rola entre as ondas, representando o grupo avançando pela cidade.
- Ficha e instância passaram a ser coisas separadas, preparando a progressão.
- O resumo completo está em `.claude/versions/20260811_0.3.0.0.md`.

## 0.3.1.0 (feita)

- Correção do reposicionamento.

## 0.3.2.0 (feita)

- Controle de velocidade para desenvolvimento, de 0.25x a 8x.
- A taxa de quadros sobe junto com a velocidade, então acelerar não distorce a simulação.
- Não existe na build final.
- O resumo completo está em `.claude/versions/20260812_0.3.2.0.md`.

## 0.4.0.0 (feita)

- Progressão.
- Experiência, nível e distribuição de pontos de atributo.
- A vida atual não sobe ao subir de nível, apenas a máxima.
- O `enemyLevel` da fase passou a realmente deixar o inimigo mais forte, e ganhou um multiplicador de ajuste fino por inimigo.
- Todo cálculo derivado passa por `TotalOf`, onde itens, árvores e buffs vão entrar depois sem mexer em mais nada.
- O resumo completo está em `.claude/versions/20260812_0.4.0.0.md`.

## 0.5.0.0 (feita)

- Revisão geral do projeto.
- Nenhuma correção: a versão lê tudo, registra os problemas e transforma cada um em uma versão de correção com ordem definida.
- Os três achados críticos foram a suíte de testes que nunca existiu, o combate que depende da taxa de quadros, e a armadura que não escala com nada.
- O resumo completo, com os 14 achados e a evidência de cada um, está em `.claude/versions/20260813_0.5.0.0.md`.

## 0.5.1.0 (feita)

- Passo fixo no combate. A simulação anda sempre 1/60 de segundo, nunca o delta do quadro.
- O `StageRunner` virou o único relógio do jogo, e o `BattleDirector` perdeu o `Update` em troca de um `Tick(float)` público. Uma fase inteira agora roda sem cena e sem renderizar, que é o que a 0.5.2.0 precisa.
- A sobra de tempo passou a ser carregada em vez de descartada, no ataque e no movimento. Era isso que fazia a velocidade real ficar até 6.25% abaixo da ficha, punindo justamente quem investe em AGI.
- A ordem de resolução alterna a cada passo, então os heróis pararam de vencer todo empate exato.
- O `DevSpeedControl` deixou de depender da taxa de quadros para estar correto.
- O resumo completo está em `.claude/versions/20260813_0.5.1.0.md`.

## 0.5.1.1 (feita)

- Documentação de testes fechada antes de escrever o primeiro teste, já que a 0.5.2.0 vai ser executada contra o que o `CLAUDE.md` disser.
- Entraram a regra de que o valor esperado sai da spec e não do código, e a decisão de onde cada número mora: entrada na spec e repetida no teste, saída só no snapshot e nunca em prosa.
- O resumo completo está em `.claude/versions/20260813_0.5.1.1.md`.

## 0.5.2.0 (feita)

- A suíte de testes. 209 verificações em EditMode e 2 em PlayMode, todas passando.
- O código do jogo ganhou `Assets/Scripts/HerOClock.asmdef`, sem o qual nenhum teste o enxergaria.
- Três bugs achados pelos próprios testes: o cálculo de dano dava resultados diferentes dentro e fora da Unity, porque estava em `float` e o C# deixa o runtime escolher a precisão dos intermediários; o gerador aleatório era quase linear nas sementes pequenas, então toda semente digitada à mão sorteava evasão perfeita no primeiro teste; e `attributes.md` guardava a travessia do tabuleiro 6x12 que foi recusado, dizendo 6 segundos onde são 3.5.
- O primeiro `.claude/balance/snapshot.md` foi gerado. Ele mediu pela primeira vez o que `fases.md` afirmava: a `act1-stage1` é vencível no nível 1 em 26,2 segundos, e a `act1-stage2` exige nível 20.
- O resumo completo está em `.claude/versions/20260813_0.5.2.0.md`.

## 0.5.3.0 (feita)

- Escala de defesa e regeneração de vida. 235 verificações passando, contra 209 antes.
- A ficha passou a declarar quanto cada atributo defensivo ganha por nível. Ganho igual à base mantém a mitigação parada para sempre, porque o nível cancela contra a constante da curva — foi assim que os valores de nível 1 de hoje ficaram preservados exatamente.
- A defesa passou a atravessar a costura, como os primários já faziam, e o multiplicador da fase passou a alcançá-la.
- Regeneração definida: base 0 para todo mundo e POW multiplicando o que existir, com acúmulo fracionário e correndo também entre as ondas.
- `attributes.md` ganhou de onde vem cada secundário: passivamente para heróis, escrito na ficha para lacaios e vilões, que não carregam equipamento.
- O snapshot mostrou o efeito: a fase 1 não se moveu nada, e a fase 2 caiu de nível 20 para 15 ficando mais longa, porque os heróis ganharam mais sobrevivência do que os inimigos ganharam dureza.
- O resumo completo está em `.claude/versions/20260814_0.5.3.0.md`.

## 0.5.4.0 (feita)

- Pontos de atributo dos heróis. 257 verificações passando, contra 235 antes.
- A contagem dupla acabou: `UnspentAttributePoints` saiu do `LevelProgress` e a nova `AttributeAllocation` passou a ser dona dos dois lados.
- O modelo: pontos à mão, um interruptor de automático por herói e um botão de resetar. Automático é o estado inicial, ligar o automático não desfaz o que foi colocado à mão, e resetar com ele ligado volta para a build da ficha.
- A parte automática guarda a **contagem** de pontos e é redistribuída inteira, nunca somada de cinco em cinco. É o que mantém a regra de um lacaio criado direto no nível 40 ser idêntico a um que subiu até lá.
- `attributes.md` e `progress.md` discordavam sobre o modelo e agora descrevem o mesmo.
- Entrou a medição de paredes no snapshot: com que nível o time chega em cada fase, o que ela exige, e quantas repetições da anterior fecham o buraco. As duas fases de hoje são fixtures, então a parede gigante entre elas é o resultado esperado; o instrumento é que passa a valer quando o ato 1 for escrito.
- O snapshot não se moveu com a mudança de atributos, o que é a prova de que o refactor preservou o comportamento.
- O resumo completo está em `.claude/versions/20260814_0.5.4.0.md`.

## 0.5.5.0 (feita)

- Conteúdo em inglês e arquivo de strings. 273 verificações no EditMode, contra 257, e 2 no PlayMode.
- Todo texto que o jogador lê saiu dos assets e das fases e foi para `Assets/Strings/en.json`. `CharacterDefinition` perdeu o `DisplayName` e `StageData` perdeu `name` e `lore`.
- As chaves são derivadas do `Id` que o conteúdo já carrega, então não existe nada para manter em sincronia: renomear um id é o mesmo ato que renomear o texto dele.
- Chave faltando volta como `#chave#` e não como vazio, e o validador reclama tanto de chave faltando quanto de texto sobrando de conteúdo apagado.
- O snapshot passou a rotular as linhas por id, então ele deixou de conter texto traduzível e uma tradução não suja mais o diff dos números.
- O resumo completo está em `.claude/versions/20260814_0.5.5.0.md`.

## 0.5.6.0 (feita)

- Fechamento do bloco de revisão. **Os 14 achados da 0.5.0.0 estão resolvidos.** 278 verificações no EditMode e 2 no PlayMode.
- Espinhos passou a conceder roubo de vida a quem devolveu, o que torna possível o tanque que se cura apanhando.
- O validador confere a área dos inimigos, o bootstrap parou de ler a ficha compartilhada, e os números de dano são reaproveitados em vez de recriados a cada golpe.
- `game-objects/` conferido linha a linha contra os assets reais, e a ficha de fase da spec passou para o formato que o jogo lê.
- `characters.md` deixou registrado que as fichas de `specs/` são documento de design, e não dado que o jogo lê.
- O resumo completo está em `.claude/versions/20260814_0.5.6.0.md`.

## 0.5.7.0 (feita)

- Modelo de atributos de lacaios e vilões. **O modelo não mudou:** o problema era de ergonomia, não de conceito.
- Inimigos mantêm os atributos principais, porque as habilidades escalam por atributo e os `modify_stat` de buff e debuff precisam de algo em que morder. Tirá-los exigiria um segundo caminho só para eles nos dois sistemas.
- A dificuldade de autoria foi resolvida no Inspector: a ficha passou a mostrar ao vivo o que produz, em qualquer nível e com qualquer multiplicador de fase, lendo as mesmas propriedades que o jogo lê.
- `characters.md` fechou a estrutura da ficha, que a 0.5.6.0 tinha adiado para cá, e registrou o porquê de inimigos terem primários.
- O resumo completo está em `.claude/versions/20260814_0.5.7.0.md`.

## 0.5.8.0 (feita)

- Comportamento da janela. 300 verificações, contra 278.
- A janela deixou de ser redimensionável e não vai mais para tela cheia. `gameplay.md` sempre descreveu a janelinha no cantinho, mas nada forçava isso, e alargar um pixel já revelava a borda do tabuleiro.
- A janela agora só assume tamanhos da escada 1×, 2× e 4×, sempre múltiplos inteiros da resolução de referência. Com isso a proporção nunca desvia e não é preciso cortar nem preencher com barras.
- O tamanho inicial sai do tamanho da tela do jogador: 1× em 1080p, 2× em 4K. Atalhos `=` e `-` mudam, e a escolha fica em `PlayerPrefs`.
- Não existe 0.5×: abaixo de 1× o Pixel Perfect precisaria reduzir, e reduzir pixel art quebra a grade.
- O resumo completo está em `.claude/versions/20260817_0.5.8.0.md`.

## 0.5.9.0 (feita)

- A janela virou um widget de desktop. 313 verificações, contra 300.
- Sem moldura, acima de qualquer programa e da barra de tarefas, arrastada clicando no próprio jogo, e magnética às bordas do monitor.
- As três vieram juntas porque tirar a barra de título obriga o resto: sem ela o Windows não oferece jeito de mover a janela, e arrastar sem magnetismo torna o canto um trabalho de precisão.
- Não é confiável acima de um jogo em tela cheia **exclusiva**, que assume o controle do vídeo. Acima de tela cheia sem borda, que é o comum hoje, funciona.
- Custou o botão de fechar: sem barra de título, fechar é Alt+F4 até a tela de opções existir.
- O resumo completo está em `.claude/versions/20260817_0.5.9.0.md`.

## 0.5.9.1 (feita)

- As faixas pretas nas laterais. Ao tirar a moldura, a área útil da janela cresceu alguns pixels, e o tabuleiro tem exatamente 180 de largura, sem folga nenhuma.
- Corrigido nas duas pontas: a janela força o próprio tamanho pelo sistema operacional a cada quadro, e o tabuleiro ganhou duas colunas decorativas de cada lado, como já tinha fileiras.
- A moldura e o tamanho saíram do `WindowDrag` e foram para o `WindowScale`, que é quem sabe de que tamanho a janela deve ser.
- Nasceu `.claude/specs/general/window.md`, reunindo o comportamento da janela que estava espalhado entre resumos de versão.
- O resumo completo está em `.claude/versions/20260817_0.5.9.1.md`.

## 0.6.0.0

- Persistência.
- Save e load. É pré-requisito da progressão offline e entra antes de existir muito dado para migrar depois.
- **Leva junto a duplicação do laço de fase.** O `StageSimulation` dos testes repete o laço do `StageRunner`, porque o runner destrói os inimigos entre as ondas e o `Destroy` adiado não roda fora do Play Mode. Se o laço mudar e a cópia não, os testes de balanceamento passam a medir o laço antigo em silêncio. Esta versão mexe no `StageRunner` de qualquer forma, então é o momento de dar a ele um destruidor injetável e deixar a simulação dirigir o runner de verdade.
- Testes: ida e volta (salvar, carregar, estado idêntico), save de versão antiga carregando na versão nova, e os baldes de 10 minutos de `progress.md` com os três tetos da progressão offline. A ida e volta é uma categoria que só aparece nesta versão e é a que impede corromper o progresso de quem já joga.

## 0.7.0.0

- Habilidades.
- Recarga, área, provocação e reposicionamento.
- Testes: é a maior superfície de regra do jogo inteiro, e quase tudo já está escrito como exemplo em `gameplay.md` e `characters.md`. A pontuação de posicionamento em área (o exemplo de 3 aliados e 2 inimigos pontuando 1), os formatos `chain`, `line` e `area`, os efeitos resolvidos na ordem em que aparecem, a habilidade pronta que segura a carga em vez de ser usada no vazio, e a provocação sobrescrevendo a cadeia de alvo.
    - Uma dúvida: a gente consegue fazer uma bateria de teste para cada habilidade? Acho que seria a melhor forma de garantir que cada habilidade funciona, principalmente em combinações né?
- Junto entra um validador de ficha, pois `characters.md` já avisa que um `array` menor que o `ranks` faria o último nível ler um valor inexistente **sem dar erro nenhum**. Uma spec que nomeia o próprio problema silencioso está pedindo um teste.

## 0.8.0.0

- Projéteis.
- Podemos fazer primeiro uns laserzinhos simples e coloridos, apenas para ver a coisa acontecer.
- Testes: só se o projétil tiver tempo de voo capaz de mudar quando o dano é aplicado. Se for puramente visual, não precisa de nenhum.

## 0.9.0.0

- Lore do ato 1.
- Equipe de 2 heróis.
    - A ideia é terminar o ato 1 com 3 heróis.
- 5 fases.
    - Precisam constar o nível mínimo esperado para avançar, assim fica mais fácil balancear o ato.
- 5 vilões.
- 12 lacaios.
- Instâncias de todo mundo organizada na Unity.
- Testes: nenhuma regra nova, mas muito conteúdo novo. Os testes de conteúdo da 0.5.2.0 passam a valer para as 5 fases e os 17 personagens, e cada fase ganha um teste de caracterização dizendo em que nível ela deveria ser vencível.

## 0.10.0.0

- Items.
- Ataques básicos agora variam de acordo com o item.
- Uma mesma seed ainda define como a batalha vai ocorrer.
- Testes: o item entrando como nova fonte dentro do `TotalOf` sem que nada fora dele mude, e o teste de determinismo rodado de novo, já que o item passa a alterar o ataque básico. A própria linha "uma mesma seed ainda define como a batalha vai ocorrer" é uma assertiva.

## 0.11.0.0

- Menus.
- **Leva junto o que a janela sem borda deixou pendente**, já que ela chegou na 0.5.9.0:
    - Um jeito de fechar que não seja Alt+F4, porque não existe mais botão de fechar.
    - O atalho para esconder ou minimizar a janela.
    - O arrastar distinguindo fundo de interface. Hoje qualquer clique arrasta, o que é inofensivo só porque não há nada clicável.
    - O controle de zoom saindo do atalho e virando um item da tela de opções.
- Testes: praticamente nenhum. Interface é a única parte do jogo em que o custo de testar não se paga.

## 0.12.0.0

- Inventário.
- Baús.
- Testes: as regras de espaço e de empilhamento, que são aritmética e não interface.

## 0.13.0.0

- Mapa dos atos.

## 0.14.0.0

- Árvore de progressão.
- Dinheiro.
- Testes: a curva de custo dos nós, que por `progress.md` depende de quantos nós já foram comprados e não de qual nó é. E a projeção de espera de cada trecho da árvore, pelo mesmo motivo da tabela de horas: é um número publicado na spec.

## 0.15.0.0

- Classes.
- Testes: a classe entrando como mais uma fonte no `TotalOf`.

## 0.16.0.0

- Árvore de habilidades
- Testes: superfície de regra grande de novo, e ela multiplica com as habilidades da 0.7.0.0. É a versão em que a suíte existente mais paga o próprio custo.

## 0.17.0.0

- Criar uma timeline da fase.
- Adicionar um timer ao começar a fase e finalizar ela.
    - Assim o jogador poderia ver quanto tempo demora pra completar uma fase ou ter uma noção de qual build é mais rápida.
- Quem sabe da pra colocar uma espécie de "ranking" próprio pra saber qual foi a melho run da pessoa em cada fase.

# Ordem escolhida

- Cada versão depende apenas das anteriores.
- As habilidades ficam por último de propósito, pois são o sistema que mais mexe em todos os outros. Fazer habilidade antes do combate estar estável significa refazer habilidade.
- Enquanto toda a funcionalidade básica não estiver pronta, o jogo continua em game objects lisos e coloridos.
- O bloco 0.5.x é a revisão sendo aplicada, e a ordem dele é por dependência e não por gravidade: primeiro o passo fixo, que torna o combate verificável; depois os testes, que tornam as mudanças seguintes verificáveis; só então as mudanças de regra.
- A 0.5.2.0 é a única versão que existe só para testar, e é a última vez que isso acontece. Ela paga a dívida acumulada até a 0.4.0.6. Dali em diante o teste faz parte da versão que muda o comportamento, conforme "# Testes automatizados" no `CLAUDE.md`.
