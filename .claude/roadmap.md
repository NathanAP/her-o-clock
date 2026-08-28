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

**Essa regra vale apenas enquanto o personagem for um retângulo sem textura.** Assim que ele ganha desenho, a cor passa a ser dele e não do time, conforme a seção abaixo.

# Identidade cromática

- **Cada herói tem um tom exclusivo, e nenhum outro personagem pode usá-lo.**
- A referência é Megaman e Zero, ou Sonic e Shadow: azul é o Megaman porque vermelho é o Zero. A leitura por cor só funciona porque cada um tem exclusividade sobre a sua — não é o tom em si que torna alguém memorável, é o tom ser dele e de mais ninguém.
- A Tempo é o **anil**, `#149ECA`, desde a folha de referência em `.claude/specs/characters/heroes/tempo-reference.png`. Nenhum outro personagem usa esse tom, nem uma variação próxima dele.
- **O time não é comunicado por cor**, e não precisa ser: a metade do tabuleiro que o personagem ocupa já diz de que lado ele está, e posição é informação mais forte que tonalidade. Se os cinco outros heróis também fossem azuis, a Tempo deixaria de ser "a anil" e viraria "a azul mais clara", que é exatamente o problema que a escolha dela resolveu.
- As cores de apoio podem repetir entre personagens. O marfim, o grafite e o amarelo de sinalização são vocabulário comum da série, e é justamente isso que faz o tom exclusivo saltar.
- O violeta do Catarsis é **reservado** e não pertence a herói nenhum. Ele indica a origem, e quem o usar está dizendo algo sobre a própria história.
- Isso precisa ser decidido antes de o segundo personagem ganhar arte. Depois de três, virar essa regra é retrabalho.

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

## 0.6.0.0 (feita)

- Persistência. 419 verificações no EditMode, contra 313, e 2 no PlayMode.
- Cada save é um arquivo novo com o instante no nome, assinado por HMAC sobre o **texto**, que é o que faz um save antigo continuar válido quando o formato ganha um campo. Os saves anteriores são o backup, guardados por uma escada: os 5 mais novos e o mais novo de cada um dos 3 últimos dias com jogo.
- Um save adulterado carrega mesmo assim e fica marcado para sempre. Recusar puniria quem teve falha de disco, que é o caso que acontece de verdade. Não dá para impedir que alguém edite o próprio save, e `save.md` diz isso em voz alta em vez de fingir o contrário.
- **O save guarda em que fase o jogador está, e nunca em que ponto dela.** Carregar recomeça a fase do início com todo mundo inteiro, que é o mesmo que uma derrota já faz. Não existe estado de meio de fase para gravar, migrar ou errar.
- O jogo grava no fim de cada onda e no início de cada fase. Não existe save periódico: como nenhuma posição é gravada, um save no meio de uma onda não teria nada que o da fronteira já não tenha.
- A gravação da abertura é o que **consome a ausência**. Sem ela, fechar o jogo na hora errada pagaria a mesma ausência duas vezes.
- **O laço duplicado acabou.** Um campo `Destroy` no novo `StageContext` bastou para o `StageSimulation` dirigir o `StageRunner` de verdade. O snapshot não se moveu um dígito.
- Dois bugs achados pelos testes, nenhum no código novo: vida atual acima da máxima ao refazer a build, e dois heróis da mesma ficha compartilhando progresso ao carregar — a formação de hoje tem dois de cada.
- O resumo completo está em `.claude/versions/20260817_0.6.0.0.md`.

## 0.6.0.1 e 0.6.0.2 (feitas)

- A spec de habilidades, escrita por inteiro antes de existir código. Nasceram `.claude/specs/general/abilities.md` e `.claude/specs/general/buffs-and-debuffs.md`, e as habilidades saíram de `characters.md`.
- As quatro fases de uma habilidade — preparo, tempo de uso, recuo e recarga — com um personagem cobaia e contas concretas.
- A âncora `bestPlacement` foi removida em favor de área sempre centrada em quem usa, e o `behindLastTarget` virou `lastTargetAnySide`.
- A `tempo.json` virou a ficha de referência: três habilidades reais exercitando `self`, `area`, `chain`, os quatro tipos de efeito e a escala por `rank`.

## 0.6.0.3 (feita)

- Conferência da spec de habilidades contra o critério de "isto dá para testar?". Só documentação, nenhuma linha de código.
- Um erro de conta corrigido, uma contradição deixada pela remoção do `bestPlacement` resolvida, e os dois furos que faltavam fechados: empilhamento de buff e o `move_to` sem casa livre.
- `Intangível` e `Inalvejável` ganharam definição mecânica, e ficou registrado que prioridade de alvo alternativa **não existe** — em vez de ficar subentendido, virou coisa que o validador recusa.
- O plano de testes da 0.7.0.0 ficou escrito abaixo.
- O resumo completo está em `.claude/versions/20260817_0.6.0.3.md`.

## 0.6.0.4 (feita)

- Habilidade passou a morar em um lugar só. Tudo foi para `abilities.md`, e `gameplay.md` ficou com um resumo curto de onde as duas coisas se encostam.
- As "promessas" de `gameplay.md` não eram contradição, era texto escrito antes do `targeting` existir. O que faltava era um campo.
- **`priority` entrou no `targeting`.** Ele troca a primeira regra da cadeia de alvo, e o resto da cadeia padrão continua embaixo como desempate — o que responde "e se empatar" sem inventar um segundo sistema de desempate.
- `chain` e `line` ganharam o que faltava, incluindo a direção da linha, que nunca tinha sido escrita e sem a qual `line` não dava para implementar.
- A gramática de habilidades está fechada. A 0.7.0.0 não precisa de mais nenhuma decisão de spec.
- O resumo completo está em `.claude/versions/20260817_0.6.0.4.md`.

## 0.7.0.0 (feita)

- Habilidades. 510 verificações no EditMode, contra 419, e 2 no PlayMode. O snapshot não se moveu.
- A gramática de `abilities.md` virou dado: as quatro fases, os cinco formatos, a prioridade, os quatro tipos de efeito. Uma habilidade agora é ficha, e não código.
- **A costura do `TotalOf` se estendeu aos derivados.** O conteúdo real buffa velocidade de ataque e redução de recarga, que não são atributos principais, então os modificadores precisaram alcançar as propriedades calculadas também.
- O `AbilityCaster` roda antes do mover e do attacker, e é essa ordem que faz a prioridade do ataque básico existir sem nenhuma regra explícita.
- O validador de autoria recusa em vez de ignorar, e já roda contra todas as fichas do projeto. Hoje passa por vacuidade; ele existe para a 0.9.0.0.
- Dois achados: o smoke test do PlayMode dependia do relógio e quebrou quando o save deixou a party mais forte, e a provocação fora de alcance nunca tinha sido escrita.
- **Nenhuma ficha de teste ganhou habilidade**, de propósito. O sistema está pronto e só aparece no jogo rodando quando a 0.9.0.0 trouxer personagens de verdade.
- O resumo completo está em `.claude/versions/20260818_0.7.0.0.md`.

## 0.7.1.0 (feita)

- Queda de dano por distância e autodano que não mata. 529 verificações no EditMode, contra 510, e 2 no PlayMode. O snapshot não se moveu.
- A gramática de habilidades tinha sido declarada fechada na 0.7.0.0, e **a primeira ficha escrita à mão de verdade abriu ela de novo**. Era o previsto: o validador só começa a pagar quando encontra ficha escrita por uma pessoa.
- **`falloff` entrou no `deal_damage`.** Multiplicativo, com expoente `distância − 1`, então o alvo colado leva o número cheio da ficha e o dano nunca chega a zero. Recusado em `self` e `single`.
- **Dano da habilidade em quem a usou trava em 1 de vida.** Proibir o uso teria dois furos: o preparo desatualiza a conta, e o personagem ficaria desarmado justamente com a vida baixa.
- A trava vale só para o dano da própria habilidade. Espinhos, área de aliado e ataque básico continuam matando, senão o custo viraria defesa.
- O resumo completo está em `.claude/versions/20260818_0.7.1.0.md`.

## 0.7.1.1 (feita)

- A ficha do Gadrat e as specs que ela revelou. 534 verificações no EditMode, contra 529. Só documentação e um teste novo.
- A ficha tinha onze problemas, cinco deles impedindo ela de ser lida por qualquer coisa: não era JSON, falava em "cone", falava em "dano por segundo", tinha 10 segundos de `casting` e nenhuma habilidade tinha `effects`.
- **`characters.md` ganhou o `abilityTrees`**, que estava divergente desde a ficha da Tempo, e o escopo do `initialLevel` (só herói; lacaio e vilão tiram o nível da fase).
- **`progress.md` ganhou a regra de que o `attributeGrowth` soma 100.** Ela precisava existir porque o cálculo não depende dela: uma ficha somando 125 continua funcionando, então não havia nada para o erro violar.
- **`DesignSheetTests` passou a ler as fichas de `.claude/specs/characters/`**, que nenhum teste lia. É o detector que faltava, e foi conferido reintroduzindo os dois erros originais do Gadrat.
- O resumo completo está em `.claude/versions/20260818_0.7.1.1.md`.

## 0.8.0.0 (feita)

- Projéteis, puramente visuais. 537 verificações no EditMode, contra 534, e 2 no PlayMode. O snapshot não se moveu.
- **O `autoAttacks.type` entrou no asset.** Ele estava nas fichas de design desde sempre e não tinha campo equivalente no `CharacterDefinition`, então nada sabia dizer quem atira. É por natureza tarefa da 0.9.0.0, adiantada porque é um campo só.
- **O achado da versão foi o evento.** O `Attacked` é disparado por três motivos — o golpe, os espinhos voltando, e cada golpe de habilidade — então ele não consegue responder "isto foi um ataque básico?".
    - Pendurar o projétil nele faria flecha sair de quem levou o golpe, por causa dos espinhos, e faria o Gadrat atirar flechas ao usar o Dragon Breath na 0.9.2.0.
    - Nasceu o `BasicAttackLanded`, disparado uma vez por ataque básico e **antes** do dano ser resolvido, para que quem desenha veja o alvo ainda de pé.
- O projétil voa para uma posição capturada no disparo e nunca segue o alvo, que pode morrer no meio do caminho e ser desativado.
- Ele anda com `Time.deltaTime`, que o controle de velocidade já escala, então fica em passo com a batalha de 0.25x a 8x sem saber que essas velocidades existem.
- Testes: nenhum para o desenho, conforme planejado. O `BasicAttackLanded` ganhou os seus, porque ele é contrato de combate e não desenho.
- **Os valores de aparência nunca foram vistos rodando** e estão expostos no Inspector para serem corrigidos a olho.
- O resumo completo está em `.claude/versions/20260818_0.8.0.0.md`.

## 0.9.0.0 (feita)

- Os personagens de verdade do ato 1. 537 verificações no EditMode e 2 no PlayMode, todas passando.
- **O ato 1 usa duas fichas de inimigo.** As fases 1 a 4 só pedem `Discarded Prototype` e `Exposed Prototype`; o que muda entre elas é quantidade, posição e nível.
- **NPC virou o quarto tipo.** Ele não ganha experiência e não conta para a derrota — sem essa segunda regra, uma fase perdida continuaria rodando até o NPC morrer.
- **Experiência saiu de quem nunca a teve.** Todo lacaio e vilão nascia com uma barra de experiência que nada tocava. Só herói tem `Progress` agora.
- A fase ganhou `allies`, e `level` e `startingHealthPercent` por posicionamento. Os dois últimos existem para o vilão ferido da fase 3.
- **As fichas foram geradas a partir dos documentos de design**, campo a campo, para a ponte da 0.9.1.0 verificar algo que nasceu correto.
- Os quatro mocks, o `Form Enemies` e as chaves de texto deles foram apagados. Cinco arquivos de teste que citavam os ids na mão passaram a usar literais inventados.
- **O ato não está balanceado**, e o snapshot mostra onde: a fase 2 e a 3 são limpas um nível abaixo do declarado, e a fase 4 pede muito acima. Mexer no `enemyLevel` piorou — a dificuldade da 4 vem do volume de ondas, não do nível.
- O resumo completo está em `.claude/versions/20260818_0.9.0.0.md`.

## 0.9.1.0 (feita)

- Equipe, banco e o balanceamento do ato 1. 548 verificações no EditMode, contra 537, e 2 no PlayMode.
- **A 0.9.0.0 mediu o ato com um time que não existe**: ela punha Tempo e Gadrat juntos desde a fase 1, quando a Tempo começa sozinha e o Gadrat só entra depois da fase 3.
- **`Roster`** separa três coisas que se parecem: slots (quantas posições), possuídos (quem existe) e equipe (quem entra, em ordem). O `heroLimit` é o quarto número e mora na fase.
- A fase ganhou `heroLimit` e `firstClear` (`unlocksCharacter`, `grantsTeamSlots`). A formação virou leiaute, e não mais o time.
- **O `StageSimulation` passou a respeitar o limite de heróis** — sem isso o snapshot continuaria medindo dois heróis em toda fase, que era a origem do erro anterior.
- O ato flui sem parede, conforme a intenção de não pedir farm até a fase 6.
- **A descoberta: a dificuldade inicial é um penhasco, não uma ladeira.** Com um herói e sem cura entre ondas, o nível exigido salta entre 1 e muitos com mudanças mínimas. Alargar esse meio é decisão de design (regeneração entre ondas, menos ondas, cura ao limpar), não número.
- O resumo completo está em `.claude/versions/20260818_0.9.1.0.md`.

## 0.9.2.0 (feita)

- Avançar de fase e o degrau de rank. 551 verificações no EditMode, contra 548, e 2 no PlayMode.
- **`rankAvailability`**: uma entrada por rank com o nível necessário, **por habilidade e nunca por árvore**. O caster escolhe o rank pelo nível, então já vale hoje.
    - É ele que torna possível escrever um rank 1 fraco. O Dragon Breath saiu de 63 para 18 de dano.
- **Vencer avança sozinho, perder reinicia, a última fase se repete.** O runner pergunta para onde ir em vez de decidir, o que deixa o menu da 0.11.0.0 acrescentar "farmar aqui" sem ele aprender nada.
- **O grupo é montado no começo de cada fase**, lendo limite, equipe, ordem e formação de uma vez. Mexer em item, equipe ou formação só vale na próxima fase.
    - Os heróis não são recriados: uma instância por herói atravessa a sessão, então nível e experiência não se perdem na troca.
- **O ato 1 fechou nas faixas** de `stages.md`, com duas paredinhas de 2,4 e 3,3 min.
- Três aprendizados de balanceamento: o pico é quase sempre a onda do vilão; mais instâncias dão calibragem mais fina; e o penhasco era em boa parte a habilidade apagando ondas.
- O resumo completo está em `.claude/versions/20260818_0.9.2.0.md`.

## 0.9.3.0 (feita)

- A ponte e a varredura de balanceamento. 563 verificações no EditMode e 1 ignorada, contra 551, e 2 no PlayMode.
- **A ponte**: cada asset é conferido campo a campo contra o documento de design que ele espelha. Conferida quebrando dois valores de propósito, e ela pegou os dois.
- **`StageSweep`** substituiu o "nível mínimo" por uma distribuição: várias equipes, várias builds e várias sementes por fase, com a amostra crescendo com o espaço.
    - O número antigo era o **melhor caso vestido de requisito**: ele dizia que a fase 4 pedia nível 5, e a varredura mostrou que no nível 5 menos de um terço das builds passa.
    - Um erro meu no caminho: dimensionar a amostra por linha em vez de por fase fazia a fase 3 parecer piorar com o nível, porque cada linha tinha denominador diferente.
- **As assertivas mudaram de natureza**: a build da ficha passa no nível recomendado, mais de uma build passa em cada fase, e abaixo da faixa nem todas passam. Porcentagem virou informação no snapshot, não afirmação.
- **Dois achados que só a varredura mostraria**, ambos deixados de propósito para depois dos itens:
    - Nenhuma build de AGI ou SPE passa da fase 3 em diante, em nível nenhum.
    - Na fase 4, `só POW` limpa tudo e a distribuição declarada nas fichas limpa metade.
- O resumo completo está em `.claude/versions/20260819_0.9.3.0.md`.

## 0.9.4.0 (feita)

- O reposicionamento conferido e uma promessa retirada. 566 verificações no EditMode e 1 ignorada, contra 563.
- **A suspeita da Tempo reaparecendo errado foi investigada e o código está certo.** Dois testes novos, um sintético e um contra o ato real, afirmam que todo herói começa cada onda na casa da formação. Os dois passam.
    - O que está errado é a casa: na 0.9.2.0 a Tempo virou "herói 1" e herdou a coluna 4, fileira 1 — o fundo da área. Sendo corpo a corpo e jogando sozinha por três fases, ela atravessa o tabuleiro toda onda. É decisão de conteúdo e ficou em aberto.
- **A promessa de "1 ponto de dano elemental por SPE" saiu de `attributes.md`.** O jogo nunca a cumpriu: o valor era calculado, aparecia no Inspector e nenhum cálculo de combate o lia. Retirada em vez de implementada, porque a 0.10.0.0 muda o modelo inteiro.
- O resumo completo está em `.claude/versions/20260819_0.9.4.0.md`.

## 0.10.0.0 (feita)

- O atributo multiplica e nunca soma. 562 verificações no EditMode e 1 ignorada, e 2 no PlayMode.
- **O conteúdo fornece a base e o atributo multiplica ela**, a 10 pontos = 1%. A base do ataque básico é o soco do personagem até uma arma substituí-la; a da habilidade é o rank.
- **O POW não dá mais vida**, e o multiplicador de regeneração foi para CON. Ele era o único atributo que pagava dos dois lados da luta.
- **O dano base cresce por nível** como a armadura, porque com o atributo multiplicando o nível deixaria de aumentar dano — e inimigo não ganha item.
- O domínio inverteu: `só POW` saiu de 12/12 para 0/12 e `só CON` de 6/12 para 12/12. É o custo aceito do modelo: antes dos itens, ofensiva quase não paga.
- Dezessete testes quebraram e **nenhum foi afrouxado** — cada valor esperado foi reescrito a partir da spec nova.
- O resumo completo está em `.claude/versions/20260819_0.10.0.0.md`.

## 0.10.1.0 (feita)

- A Tempo deixa de ser um retângulo. Oito verificações novas no EditMode.
- **Entrou na frente dos itens** porque a folha de sprites apareceu pronta e havia uma pergunta que só se responde na tela: se a arte cabe na casa de 30 pixels. Descobrir erro de enquadramento depois de os itens dependerem dos mesmos números seria pior.
- **A folha gerada não é uma animação.** Quadros vizinhos diferem de 34% a 50% dos pixels; um ciclo de verdade muda de 10% a 15%. Entraram quatro poses paradas, e animação espera uma folha feita para isso.
- **A piscada de dano não funcionava em sprite** e ninguém teria percebido: a cor do `SpriteRenderer` multiplica, então o branco que lavava o retângulo é exatamente a cor que não muda nada.
- O resumo completo está em `.claude/versions/20260824_0.10.1.0.md`.

## 0.10.2.0 (feita)

- A Tempo golpeia, e para de lutar de lado. Onze verificações no arquivo de direção, três novas.
- **O ataque é um estado e nunca uma sequência.** Cada golpe reinicia a pose e nunca enfileira: com 8x de velocidade sobrando em cima da velocidade de ataque, os golpes chegam em menos de quatro quadros e uma fila ficaria correndo atrás da luta.
- **Duas coisas da 0.10.1.0 estavam erradas sem reclamar.** A direção não voltava ao normal depois de um passo lateral, então a heroína lutava de perfil contra quem estava acima dela; e a piscada de dano usava tempo escalado, ou seja nunca foi vista em velocidade alta.
- O resumo completo está em `.claude/versions/20260824_0.10.2.0.md`.

## 0.10.2.2 (feita)

- A cor é do personagem e não do time. Nenhuma linha de lógica, nenhum teste tocado.
- **A regra de cor do projeto estava trabalhando contra a identidade da Tempo.** "Tons de azul para heróis" foi escrito para retângulos sem textura; com ela ganhando o anil, cinco outros heróis azuis a transformariam em "a azul mais clara". Virou identidade cromática por herói, e o time passa a ser lido pela metade do tabuleiro.
- **Um campo que parecia morto não estava.** `Definition.Color` deixou de pintar o corpo na 0.10.1.0 mas continua colorindo o projétil do ataque à distância. Ficou cobalto enquanto a Tempo virou ciano, e só apareceria no dia em que ela equipasse uma arma de alcance — muitas versões adiante e longe da causa.
- O resumo completo está em `.claude/versions/20260824_0.10.2.2.md`.

## 0.10.2.3 (feita)

- A Tempo corre. Ciclo de oito quadros dirigido por distância percorrida, três verificações novas.
- **Desfaz uma conclusão errada da 0.10.1.0.** Eu tinha afirmado que a folha gerada não servia para animação; a linha 1 é uma corrida perfeitamente legível e o Nathan pegou olhando. Foram duas medições ruins: o filtro procurava tronco parado com pernas alternando, que é critério de caminhada e não de corrida, e o parâmetro de 10% a 15% de diferença entre quadros vale para sprite grande, não para 24×32.
- **Churn de pixel não mede se uma folha anima.** O que mede é âncora dos pés, altura constante e paleta unificada — e as três já estavam medidas desde a 0.10.1.0.
- **O 8x é ferramenta de desenvolvimento e não requisito de produto.** Eu vinha usando ele para justificar decisão de projeto em três versões seguidas, e o `battle.md` já dizia o contrário desde sempre.
- **O chão deslizando entre ondas passou a contar como caminhada.** O comentário do `BoardScroller` já dizia que a ficção é que os heróis estão andando, e a arte mostrava todo mundo parado enquanto o mundo passava por baixo. Durante a rolagem todos ficam de perfil, que é a leitura de auto-scroller.
- **O golpe virou sequência de três desenhos** em vez de um interruptor ligado e desligado. E o desenho único que eu tinha escolhido antes era a preparação do golpe, não o golpe: o quadro com o arco da lâmina estava na folha o tempo todo.
- **A base de animação foi aceita como provisória.** Nenhuma das três veio de arte feita para animar, e falta caminhada de costas para a transição entre ondas. O caminho combinado é gerar as animações a partir de um sprite único, a testar primeiro com o Gadrat.
- O resumo completo está em `.claude/versions/20260824_0.10.2.3.md`.

## 0.10.2.4 (feita)

- **O herói não sabia o próprio nível.** O nível morava em dois lugares: `Progress.Level`, que o save carrega, e o campo do `Character`, que todo cálculo usa. A restauração mexia só no primeiro.
- Não era cosmético: armadura e dano base crescem por nível, então um herói de nível 9 lutava com defesa de nível 1 enquanto carregava os pontos de atributo do nível 9. A ficha do teste entregava só a base, sem nenhum dos oito crescimentos que o nível devia ter pago.
- **A suíte não pegou porque afirmava `Progress.Level` e nunca `Character.Level`** — olhava só o lado bom da fresta onde o bug morava.
- Achado no caminho: o formato do save não carrega vida atual, então um grupo carregado passa a começar inteiro. Guardar a vida no arquivo fica como escolha em aberto.
- O resumo completo está em `.claude/versions/20260824_0.10.2.4.md`.

## 0.10.2.5 (feita)

- Uma janela no editor com todas as fichas lado a lado no mesmo nível, em `Her-o-clock` → `Character sheets`. Nenhum teste tocado.
- **Faltava comparar duas fichas no nível em que elas se encontram.** O Inspector responde por uma ficha e o `snapshot.md` responde no nível inicial de cada uma; nenhum dos dois responde isso.
- **Cada habilidade mostra o alcance por extenso, com `CATCHES ALLIES` em toda área.** Era a informação que não existia em lugar nenhum do editor e que fez o fogo amigo do Gadrat parecer bug.
- O resumo completo está em `.claude/versions/20260824_0.10.2.5.md`.

## 0.10.2.6 (feita)

- **A suíte da 0.10.2.5 tinha sido entregue vermelha.** A assertiva do golpe pousava numa fronteira que o float não tem: `3 × 0.1f` é menor que `0.3f`, então um terço do caminho lê como um tiquinho a menos e o quadro sai um cedo demais.
- A regra estava certa e o jogo também — em jogo o cronômetro é soma de deltas e nunca cai em cima de uma fronteira. Quem escolheu mal foi o teste.
- A duração da assertiva virou três oitavos de segundo, cujos terços são potências de dois.
- O resumo completo está em `.claude/versions/20260827_0.10.2.6.md`.

## 0.10.2.7 (feita)

- **A caminhada entre ondas podia simplesmente não acontecer, e sem avisar.** O guarda de meia casa, medido para personagem, foi reaproveitado para o tabuleiro, que anda oito vezes mais rápido em 8x. Numa máquina que não segure a taxa de quadros, a rolagem inteira era descartada como salto.
- O tabuleiro passou a ser julgado pelo **sinal** e não por limiar: ele só desce durante a transição e só sobe no retorno à origem, então a direção separa os dois sem depender de quantos quadros foram desenhados.
- O resumo completo está em `.claude/versions/20260827_0.10.2.7.md`.

## 0.10.2.8 (feita)

- A janela `Character sheets` buscava as fichas dentro do `OnGUI`, ou seja, varria o projeto a cada repintura. Virou campo, relido ao abrir, ao focar e quando o editor avisa que um asset mudou.
- O resumo completo está em `.claude/versions/20260827_0.10.2.8.md`.

## 0.10.2.9 (feita)

- **Uma frase envelheceu no mesmo commit que a criou.** A promessa de que o ataque em velocidade alta degrada bem valia para o desenho único; com sequência, o que trava na tela é a preparação — o mesmo erro que a 0.10.2.3 corrigiu na escolha do desenho, voltando pelo ritmo.
- **Cinco números de saída tinham virado prosa**, contra a regra de "Onde cada número mora". Todos corretos no dia em que foram escritos, que é exatamente o risco. Reescritos pela regra, com o valor de hoje mandado para o `snapshot.md`.
- O resumo completo está em `.claude/versions/20260827_0.10.2.9.md`.

## 0.10.2.10 (feita)

- **Um ponto colocado só passa a valer na próxima fase**, escrito em `attributes.md` antes de existir interface de atributos — senão ela nasceria errada.
- **O automático não é exceção.** Ele decide onde o ponto vai sem decidir quando ele conta.
- Sem a regra existe um exploit inteiro: redefinir pontos é livre e sem custo, então dava para trocar a build no meio da luta e enfrentar cada inimigo com a build feita para ele.
- **Resolveu um problema de graça:** como a vida máxima só vem de CON, ela deixa de poder mudar no meio de uma fase. A correção que estava planejada para a vida virou código que não precisa existir.
- O nível em si continua imediato: armadura, resistências e dano base por nível existem para o personagem não apodrecer, e segurá-los iria contra isso.
- O resumo completo está em `.claude/versions/20260827_0.10.2.10.md`.

## 0.10.3.0 (feita)

- A regra acima em vigor. `AttributeAllocation` passou a ter dois `AttributeSplit`: o que o jogador edita e o que os atributos leem. `Commit` é a única passagem entre os dois, e acontece ao nascer e ao começar uma fase.
- Saiu o `Heal` que a 0.10.2.4 tinha colocado no `SaveMapper`: era código morto, e o comentário que o justificava afirmava algo falso sobre o sistema.
- **587 testes, 0 falhas.** Quatro novos sobre a regra, um guardando a fronteira da armadura, e quatro que mudaram de significado de propósito.
- **Impacto no balanceamento:** uma linha do snapshot, `act1-stage1` de 23.3 s para 23.4 s. Nenhum nível mínimo mudou.
- O resumo completo está em `.claude/versions/20260827_0.10.3.0.md`.

## 0.10.4.0 (feita)

- **O herói e o que luta viraram duas coisas.** `HeroRecord` guarda nível, experiência, pontos e atributos; `Character` é construído dele quando a fase começa e destruído no fim. Ideia do Nathan.
- **Quatro problemas eram o mesmo problema.** O nível em dois lugares (0.10.2.4), a redistribuição no meio da luta (0.10.3.0), a vida máxima se mexendo debaixo de um ferido, e a lista à mão do `ResetForBattle` — todos deixaram de existir em vez de serem tratados.
- **A lista já estava incompleta:** o `regenerationCarry` atravessava fases sem ninguém notar.
- Lacaios, vilões e NPCs sempre foram recriados por fase. O herói era a exceção, e a exceção era o bug.
- **Apagados:** o `AttributeSplit` da 0.10.3.0, o `ResetForBattle`, o `SpawnHeroes` (código morto), e o rótulo de nível embaixo do personagem.
- Achado no caminho: o `SaveService` recebia a lista de heróis uma vez, então um herói desbloqueado no meio da sessão nunca era salvo.
- **583 testes, 0 falhas.** Impacto no balanceamento: `act1-stage1` de 23.4 s para 24.3 s, sem mexer em nenhum nível mínimo nem parede.
- **Fica em aberto:** não existe mais lugar para ver o nível de um herói durante o desenvolvimento.
- O resumo completo está em `.claude/versions/20260827_0.10.4.0.md`.

## 0.10.4.2 (feita)

- **As specs pararam de contar história.** Saíram três citações de versão e cinco seções de justificativa, todas transportadas para `.claude/memory/design-decisions.md`.
- O critério: sai seção dedicada a justificar, comparação com alternativa não adotada, história de calibração e número derivado. Fica a cláusula curta que impede a regra de ser mal entendida.
- As duas seções do `save.md` tinham regra junto do argumento, e a regra ficou.
- O resumo completo está em `.claude/versions/20260827_0.10.4.2.md`.

## 0.10.4.3 (feita)

- Aba **Heroes in play** na janela `Character sheets`, com nível, experiência, pontos de habilidade e atributos dos heróis da sessão rodando. Devolve a quem desenvolve o que o rótulo de nível mostrava, sem devolver nada à tela do jogo.
- **Não é o menu e não vira o menu.** O perfil do herói é conteúdo de versão futura e serve ao jogador; esta aba serve a quem desenvolve, e continua útil depois que o menu existir.
- Ela lê registros e nunca combatentes, então torna visível a regra da 0.10.4.0: durante uma fase os dois podem discordar de nível, e isso é regra e não defeito.
- O resumo completo está em `.claude/versions/20260827_0.10.4.3.md`.

## 0.10.4.4 (feita)

- Quatro bugs achados ao levantar o que os itens pedem do motor, corrigidos antes de qualquer código de item.
- `EquipmentClass.Magic` virou `Special`, que é a palavra que as specs e o arquivo de strings sempre usaram. O enum guarda número e não nome, então nenhum asset mudou.
- `modifiers.json` e `slots.json` falavam vocabulários diferentes de slot. Os dois papéis da mão secundária agora são declarados no slot, e o `slotGroups` aponta para lá.
- **O desconto de alcance não era aplicado a quem devia.** A Lance pagava por alcançar duas casas sendo corpo a corpo; Catalyst e Reactor alcançavam duas e três de graça. A regra passou a valer para os três, e as velocidades de Catalyst e Reactor caíram para fechar o orçamento sem quebrar os rótulos da tabela de `items.md`.
- `+X de armadura neste equipamento` ganhou a nota de que a soma é direta e de que não existe multiplicador local em lugar nenhum. Nenhum número mudou: a frase é que prometia um comportamento que o jogo não tem.
- O resumo completo está em `.claude/versions/20260827_0.10.4.4.md`.

## 0.10.4.5 (feita)

- AGI e SPE deixaram de ser descritos como a fonte **principal** de evasão e de redução de recarga. Com o equipamento chegando, essa palavra passaria a ser falsa.

## 0.10.5.0 (feita)

- **A classe do personagem passou a ser a mistura das classes do que ele veste.** Cada equipamento ativo entrega uma fatia, um híbrido entrega meia para cada lado, e as quatro fórmulas por classe leem a média ponderada.
- Era o buraco que travava a versão de itens: `attributes.md` escrevia número para três classes e `items.md` dava seis, uma por item, em oito slots, sem nada dizendo como oito viram um.
- **As seis classes de item cabem nas três da ficha sem constante nova**, porque cada híbrido é literalmente metade de duas puras.
- **É a constante que mistura, nunca o resultado**, senão a evasão deixaria de ser uma curva sobre um total de pontos e os pontos de evasão do equipamento ficariam sem onde entrar.
- Sem nada vestido vale a classe da ficha, que é todo lacaio, vilão e NPC, e todo herói de hoje.
- **599 testes, 0 falhas. O snapshot não mudou um dígito**, que é a afirmação central da versão.
- O resumo completo está em `.claude/versions/20260827_0.10.5.0.md`.

## 0.10.6.0 (feita)

- **A evasão virou a mesma fórmula das outras duas defesas.** Ela era a única cuja constante não crescia com o nível do atacante, então era a única que não apodrecia — e isso a tornava estritamente a melhor defesa no fim do jogo sem ninguém ter decidido.
- Agora é um total de pontos contra `50 × nível do atacante`, igual à armadura e à resistência. Os pontos vêm da ficha, que declara valor inicial e ganho por nível, e do AGI, convertido a 1, 0.5 ou 0.2 por ponto conforme a classe.
- **A chance de evasão deixou de ser um número do defensor sozinho.** `EvasionChance` virou `EvasionPoints`, e quem transforma pontos em chance é o `DamageCalculator`, que já tinha o nível do atacante para a mitigação.
- **A calibração de 10/5/2 planejada aqui estava errada** e foi descartada: ela vale assintoticamente, mas no nível 1 a constante está no mínimo enquanto a ficha já carrega o AGI base inteiro, e a Tempo saltaria para 64.3% de evasão na fase 1. A evasão na ficha resolve o pico pela causa.
- A Tempo trocou armadura por evasão a pedido: 20/20 de armadura viraram 12/12, e ela ganhou 25/25 de evasão. A redução média total dela ficou onde estava; o que mudou é qual das duas carrega a maior parte.
- Fechou a pergunta em aberto de `items.md`, e os números de evasão de `slots.json` e `modifiers.json` foram reescritos para a mesma ordem que os de armadura.
- **602 testes, 0 falhas.** O snapshot se moveu bastante, e **a parede da act1-stage4 sumiu** — nenhuma das três calibrações testadas preserva as três paredes do ato.
- O resumo completo está em `.claude/versions/20260828_0.10.6.0.md`.

## 0.10.7.0 (próxima)

- **O golpe passa a variar.** O dano base vira mínimo e máximo em todas as fichas, e o sorteio sai do `BattleRandom`.
- Vale para lacaio e vilão também. Hoje um Discarded Prototype bate exatamente 2, sempre.
- **Habilidade continua fixa.** A habilidade é o dano com que se pode contar e o ataque básico é o que balança, e essa diferença é textura de graça.
- É aqui que as seeds antigas param de reproduzir as batalhas que reproduziam, sozinho, sem nada de item no diff.

## 0.11.0.0

- **A base do item, só dados.** Os cinco JSON de `.claude/specs/items/` carregados, modelo de dados, validador no molde do `StageValidator`, catálogo e strings. Nada é vestível ainda.
- Paga a maior dívida de teste do projeto: hoje nenhuma linha daquela pasta é conferida por nada.
- Testes: as invariantes de `tiers.json`, a derivação do orçamento de dano de `subtypes.json`, as tabelas de nomenclatura, e todo id tendo texto no arquivo de strings.

## 0.11.1.0

- **Vestir.** Equipamento no `HeroRecord`, a fotografia no `Character`, o item como fonte nova dentro do `TotalOf` e das defesas, e a composição de classe da 0.10.5.0 recebendo enfim entrada de verdade.
- **O requerimento é conferido contra o total, com tudo que está vestido emprestando atributo.** Dois itens que se sustentam mutuamente ficam os dois ativos, e tirar um derruba o outro para a borda vermelha. Recombinar depois é problema do jogador, como no Path of Exile.
- Entram as entradas novas de `ModifiableStat` para dano de habilidade por elemento e para resistência ignorada, que hoje não têm onde pousar.
- O save ganha o primeiro campo que não é escalar: itens já sorteados, que precisam sobreviver intactos.
- Ferramenta de editor que entrega item a um herói, já que drop é 0.12.0.0 e inventário é 0.14.0.0.

## 0.11.2.0

- **A arma.** Substitui dano base, velocidade de ataque, alcance e corpo a corpo/à distância.
- Tira `MinRange`, `MaxRange` e `AutoAttack` de dentro do `CharacterDefinition` compartilhado, que é onde eles moram hoje.
- **Armas duplas alternam as mãos**: golpe 1 na primária, golpe 2 na secundária, e cada golpe usa os modificadores da arma que golpeou.
- **Item nenhum aumenta o rank de uma habilidade acima do 5.** É um problema conhecido do Path of Exile e a decisão é não repeti-lo: o rank é o degrau que a ficha controla, e um item que o ultrapassa devolve ao jogo o pico que o degrau existe para evitar.

## 0.11.3.0

- **O gerador.** Tecnologia, quantidade, divisão hardware/software, pesos, camadas, valores e a montagem do nome.
- C# puro, sem Unity, com teste de distribuição.

## 0.11.4.0

- **Únicos.** A pasta `Assets/Items/Uniques/`, o `AssetPostprocessor` de autodescoberta e o teste que sustenta a promessa de que um arquivo novo entra sozinho.
- Os itens são tão importantes quanto a árvore de passivas, e a troca entre os dois é o que torna o respec estratégico.

## 0.12.0.0

- **Drop de itens.** Versão própria e não parte do menu: taxa, raridade e sorteio têm superfície de balanceamento própria, e o `CLAUDE.md` já lista drop rate como coisa a testar.

## 0.13.0.0

- Menus.
- **Leva junto o que a janela sem borda deixou pendente**, já que ela chegou na 0.5.9.0:
    - Um jeito de fechar que não seja Alt+F4, porque não existe mais botão de fechar.
    - O atalho para esconder ou minimizar a janela.
    - O arrastar distinguindo fundo de interface. Hoje qualquer clique arrasta, o que é inofensivo só porque não há nada clicável.
    - O controle de zoom saindo do atalho e virando um item da tela de opções.
- **Controle de para onde ir ao terminar uma fase**, que hoje não existe:
    - Voltar à fase anterior ao falhar, para tentar avançar de novo depois de fortalecer.
    - Não avançar à próxima ao vencer, que é o que torna o farm possível numa fase escolhida.
- **A abertura da fase mostrando o número e o nome**, algo como "1 - 1" grande com o nome embaixo. Os dois textos já existem no arquivo de strings desde a 0.9.0.0 e hoje só aparecem no Console.
- **Leva junto a tela de volta ao jogo**, que a 0.6.0.0 deixou pendente. Os números da ausência já são calculados e escritos no Console; falta mostrá-los. Eles saem inteiros do `OfflineCredit`, então é só apresentação.
- Testes: praticamente nenhum. Interface é a única parte do jogo em que o custo de testar não se paga.

## 0.14.0.0

- Inventário.
- Baús.
- Testes: as regras de espaço e de empilhamento, que são aritmética e não interface.

## 0.15.0.0

- Mapa dos atos.

## 0.16.0.0

- Árvore de progressão.
- Dinheiro.
- Testes: a curva de custo dos nós, que por `progress.md` depende de quantos nós já foram comprados e não de qual nó é. E a projeção de espera de cada trecho da árvore, pelo mesmo motivo da tabela de horas: é um número publicado na spec.

## 0.17.0.0

- Classes.
- Testes: a classe entrando como mais uma fonte no `TotalOf`.

## 0.18.0.0

- Árvore de habilidades
- Testes: superfície de regra grande de novo, e ela multiplica com as habilidades da 0.7.0.0. É a versão em que a suíte existente mais paga o próprio custo.

## 0.19.0.0

- Criar uma timeline da fase para mostrar o atual progresso.
- Adicionar um timer ao começar a fase e finalizar ela.
    - Assim o jogador poderia ver quanto tempo demora pra completar uma fase ou ter uma noção de qual build é mais rápida.
- Quem sabe da pra colocar uma espécie de "ranking" próprio pra saber qual foi a melho run da pessoa em cada fase.

## 0.20.0.0

- Adicionar diário

# No radar

Coisas decididas conscientemente como "não agora". Elas não têm versão marcada, e estão aqui para não serem redescobertas do zero mais tarde.

## Efeito periódico

- Um `deal_damage` que acontece várias vezes ao longo de uma duração, em vez de uma só. Precisa da `duration`, que já existe, e de um intervalo entre os tiques, que não existe.
- Destrava aura, queimadura, veneno e zona de cura de uma vez, pois todos eles são a mesma peça com números diferentes.
- Saiu da 0.7.1.0: o `warm-up` do Gadrat foi escrito assim originalmente e teve que virar um pulso único.
- **A condição para ele deixar de esperar** é alguma ficha depender dele de verdade. Se uma ficha nova pedir dano ao longo do tempo, ele vem antes daquela ficha, e nunca depois — senão a ficha é escrita torta e reescrita em seguida.
- A regra que ficou clara ao discutir isso: **`casting` é congelamento, `duration` é consequência.** Um `casting` alto quase sempre é um número escrito no campo errado.

## Projétil com tempo de voo

- Um projétil que demora para chegar e **atrasa o dano**, em vez de só desenhar a viagem depois do fato.
- Foi avaliado na 0.8.0.0 e recusado por enquanto, porque não existe nenhuma mecânica que dependa dele. Ele só se paga quando o voo virar jogo: projétil que dá para desviar, interceptar ou errar.
- O custo não é o projétil, é o dano deixar de ser instantâneo, coisa que o motor inteiro assume hoje. O que abre junto:
    - **Determinismo.** O voo tem que andar dentro do passo fixo, nunca no `Update`.
    - **Quando a evasão é sorteada.** Sortear na chegada faz a ordem dos sorteios depender dos tempos de voo. Sortear na saída é a resposta segura.
    - O alvo morrendo no meio do voo, o alvo saindo do lugar, espinhos e roubo de vida tendo que viajar junto, a onda acabando com projétil no ar, e o log de atividade registrando o dano num instante diferente do que ele aconteceu.
- Cada um desses é regra nova na spec com teste próprio.

# Ordem escolhida

- Cada versão depende apenas das anteriores.
- As habilidades ficam por último de propósito, pois são o sistema que mais mexe em todos os outros. Fazer habilidade antes do combate estar estável significa refazer habilidade.
- Enquanto toda a funcionalidade básica não estiver pronta, o jogo continua em game objects lisos e coloridos.
- O bloco 0.5.x é a revisão sendo aplicada, e a ordem dele é por dependência e não por gravidade: primeiro o passo fixo, que torna o combate verificável; depois os testes, que tornam as mudanças seguintes verificáveis; só então as mudanças de regra.
- **O bloco 0.10.5.0 a 0.10.7.0 é preparação para os itens, e nenhuma das três precisa de uma linha de código de item.** A classe efetiva, a escala da evasão e o dano com faixa são mudanças de regra que valem para todo personagem, inclusive os que nunca vão vestir nada. Separá-las é o que faz cada uma mover o snapshot sozinha, com diff legível, e o que faz o bloco de itens encontrar todas as costuras já abertas e verificadas.
- **O bloco 0.11.x vai da menor superfície de risco para a maior**: dados primeiro, depois vestir, depois a arma, depois o gerador, e os únicos por último. Só a terceira mexe em combate, e ela chega com as três de preparação já pagas.
- A 0.5.2.0 é a única versão que existe só para testar, e é a última vez que isso acontece. Ela paga a dívida acumulada até a 0.4.0.6. Dali em diante o teste faz parte da versão que muda o comportamento, conforme "# Testes automatizados" no `CLAUDE.md`.
