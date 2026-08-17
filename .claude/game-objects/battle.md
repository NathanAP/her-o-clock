# Battle

## Objetivo

Único GameObject que precisa existir na cena além da câmera e da luz. Ele monta o campo de batalha inteiro em tempo de execução.

A cena guarda apenas este objeto de propósito. Todo o resto (tabuleiro, personagens, diretor do combate) é criado por código, o que mantém o arquivo `.unity` pequeno e praticamente livre de conflito no git.

## Onde fica

Na raiz da hierarquia da cena. Precisa ser criado na mão (`GameObject` → `Create Empty`) e renomeado para `Battle`.

## Transform

Todos os valores padrão. Position `(0, 0, 0)`, sem rotação, escala `1`.

Os objetos criados em tempo de execução entram como filhos dele, então mover o `Battle` move o campo de batalha inteiro.

## BattleBootstrap

Componente que precisa ser adicionado na mão (`Add Component` → `Battle Bootstrap`).

### Associações obrigatórias

- Grid Config: o asset `BattleGridConfig`.
- Character Database: o asset `CharacterDatabase`.
- Stage Database: o asset `StageDatabase`.
- Strings File: o arquivo `Assets/Strings/en.json`.
- Hero Formation: o asset `BattleFormation` dos heróis.

O campo `Stage Index` escolhe qual fase do banco será jogada, contando de 0, **quando não existe save**. Havendo save, quem manda é a fase gravada nele. Seleção de fase pelo jogador ainda não existe.

Sem essas referências nada aparece. Os assets estão descritos em `assets-de-configuracao.md`.

### Mensagens de validação

O componente confere a configuração ao iniciar e escreve no Console quando encontra problema:

- Falta alguma das referências obrigatórias.
- A formação aponta para uma casa fora do tabuleiro, ou para uma casa já ocupada.
- Algum herói nasce com vida máxima 0, ou seja, nunca age.
- O arquivo de fase tem qualquer problema que o `StageValidator` pegue.
- O arquivo de strings tem chave faltando, ou texto que ninguém pede.

Quando está tudo certo, ele escreve no Console quantas fichas e quantas strings carregou, e qual semente sorteou.

Essas validações existem porque quase nenhum desses casos gera erro da Unity. O jogo simplesmente fica parado, ou mostra a coisa errada, com o Console limpo.

### Valores recomendados

**Aparência provisória**, dentro de `View Settings`. Todas as medidas são frações de uma casa, então `1` equivale à casa inteira. Podem ser ajustadas com o jogo rodando.

- Body Size: `0.5` por `0.7`, e Body Offset Y `-0.1`. Retângulo em pé, porque o sprite de um robô dificilmente será quadrado.
- Health Bar Size: `0.8` por `0.1`, e Health Bar Offset Y `0.4`. Fica no topo da casa, acima do corpo.
- Flash Duration: `0.12`. Quanto tempo o corpo fica claro depois de levar um golpe.

**Combate**

- Random Seed: `0` sorteia uma semente nova a cada Play. Qualquer outro valor reproduz sempre a mesma batalha, o que serve para investigar algo que aconteceu. A semente usada é escrita no Console ao iniciar.

**Desempenho**

- Target Frame Rate: `30`. O jogo fica aberto o dia inteiro em um cantinho da tela, então não faz sentido gastar GPU à toa.

**Desenvolvimento**, presente apenas no editor e em builds de desenvolvimento.

- Ignore Save: desligado. Ligando, o jogo não lê o save existente e a fase volta a sair do `Stage Index`. Serve para quem está montando uma fase, pois assim que existe um save aquele campo deixa de fazer efeito. O save existente **não é apagado**, e a partida ainda grava por cima da pilha normalmente.

## BattleDirector e StageRunner

Ambos são adicionados automaticamente pelo `BattleBootstrap` em tempo de execução. **Nenhum dos dois deve ser adicionado na mão.**

O `BattleDirector` é o único laço de atualização do combate. Os personagens não possuem `Update` próprio de propósito: com um laço só, em ordem fixa e em passo fixo, o combate roda sempre igual para o mesmo estado inicial e a mesma semente. Ele roda **um** combate e para.

Desde a 0.5.1.0 o `BattleDirector` **não possui `Update`**. Ele expõe um `Tick(float)` que só é chamado de fora, e a simulação anda sempre `BattleDirector.FixedStep` de cada vez, que é 1/60 de segundo.

O `StageRunner` manda no ciclo da fase: cria a onda, espera o combate acabar, faz a transição, cria a próxima onda, e trata vitória e derrota. Os heróis vivem a fase inteira e carregam o dano de uma onda para a outra; os inimigos são criados por onda e descartados.

Ele é também **o único lugar do jogo que lê o relógio**. O tempo real entra pelo `Update`, vai para um acumulador, e sai em passos fixos que vão ou para a batalha ou para a transição. Ter um acumulador só, em vez de um para cada, é o que impede sobra de tempo se perder a cada troca de fase.

O método `Advance(float)` é público justamente para que um teste consiga rodar uma fase inteira sem cena e sem renderizar, chamando ele num laço. Desde a 0.6.0.0 é isso que os testes de balanceamento fazem: eles dirigem o `StageRunner` de verdade, e não uma cópia do laço dele.

Tudo que ele precisa chega em um `StageContext`, e não em uma lista de nove parâmetros posicionais. Dois campos de lá existem só para a fase poder rodar sem cena:

- `Spawn` — cria um inimigo.
- `Destroy` — descarta um inimigo quando a onda acaba. O jogo deixa em branco e ganha o `Destroy` da Unity; um teste passa `DestroyImmediate`, pois fora do Play Mode o `Destroy` adiado nunca roda e cada inimigo de cada onda ficaria no tabuleiro segurando a casa.

Ele avisa o resto do jogo pelos eventos `Stepped`, `WaveCleared`, `StageStarted`, `StageEnded` e `EnemyDefeated`. Os dois do meio são os momentos em que o save acontece.

`StartStage` é a **única** entrada de uma fase, e é usada igualmente pela primeira tentativa, pelo reinício após a derrota e pelo jogo sendo reaberto. Sempre da primeira onda e com todo mundo inteiro. É isso que faz "em que ponto da fase eu estava" ser uma pergunta que o save nunca precisa responder.

Os tempos de espera ficam expostos nele:

- `Advance Duration` — quantos segundos o chão rola depois que o grupo se reagrupou. Padrão `3`.
- `Max Regroup Duration` — limite de segurança para a caminhada de volta. Quem não chegou até lá é reposicionado. Padrão `6`.
- `Celebration Duration` — a comemoração ao vencer a fase. Padrão `3`.
- `Defeat Duration` — a pausa antes de recomeçar após a derrota. Padrão `2.5`.

A transição entre ondas tem duas partes. Primeiro os heróis vivos **caminham** de volta às casas iniciais, o que leva o tempo que levar conforme a distância e a velocidade de movimento de cada um. Só quando todos chegam é que o chão começa a rolar, por `Advance Duration` segundos.

O deslizamento do tabuleiro é de **6 fileiras**, definido em `BoardScroller.RowsPerTransition`. Precisa ser um número **par**, pois o xadrez se repete a cada duas fileiras e é isso que torna o salto de volta invisível. O `BoardRenderer` desenha fileiras decorativas suficientes para cobrir esse deslizamento, e as duas constantes estão amarradas no código justamente para não saírem de sincronia.

## O que é criado em tempo de execução

Ao entrar em Play, a hierarquia abaixo do `Battle` fica assim:

- `Board` — um filho por casa, cada um com um `SpriteRenderer` verde na sorting layer `Background`. É este objeto que desliza para baixo entre as ondas, representando o grupo avançando pela cidade. Ele desenha nove fileiras além da área jogável em cada ponta (`BoardScroller.RowsPerTransition + 3`), para que a rolagem nunca revele um vazio.
- `AreaDivider` — a linha fina que marca onde termina a área dos heróis. É irmã do tabuleiro, e não filha, para ficar parada enquanto o chão desliza.
- Um filho por personagem, nomeado com o `Id` da ficha, com os componentes `Character` e `CharacterView`. Cada personagem tem quatro filhos próprios: `Body`, `HealthBarBackground`, `HealthBarFill` e `LevelLabel`.
- `DamageNumber` — os números que sobem e somem. São criados sob demanda e **reaproveitados**, não destruídos: num jogo que fica aberto o dia inteiro, criar um TextMeshPro por golpe seria alocação contínua.

## SaveService

Adicionado automaticamente pelo `BattleBootstrap`, depois que a fase já está de pé. **Não deve ser adicionado na mão.**

Ele não tem campo nenhum para configurar. O que faz é decidir quando gravar, e são só três momentos: quando uma onda é limpa, quando uma fase começa, e ao fechar a janela. Não existe save periódico, e o motivo está em `specs/general/save.md`.

Os arquivos ficam na pasta de dados persistentes do sistema operacional, **fora do projeto**, em `Saves/`. Um detalhe que atrapalha o desenvolvimento: rodar a aba PlayMode joga o jogo de verdade, então o smoke test grava um save real e a corrida seguinte retoma dele. Se algum teste futuro precisar começar do zero, ele vai precisar apontar o `SaveStore` para outra pasta.

## DevSpeedControl

Adicionado automaticamente pelo `BattleBootstrap`, **apenas no editor e em builds de desenvolvimento**. Não existe na build final.

- `Speed` — de `0.25x` a `8x`, arrastável com o jogo rodando.
- Atalhos: `0` para 0.5x, `1` para 1x, `2` para 2x, `3` para 4x, `4` para 8x.

O combate acelerado é idêntico ao normal por construção, pois a simulação anda em passo fixo e o `Time.timeScale` apenas faz o acumulador pedir mais passos por quadro. A 8x em 30 quadros por segundo são 16 passos por quadro.

A taxa de quadros continua subindo junto com a velocidade, mas **apenas para a coisa ficar assistível**. Antes da 0.5.1.0 ela era o motivo de o combate acelerado estar correto, o que era frágil: `Application.targetFrameRate` é um teto e não uma garantia. O limite de 8x hoje existe só porque acima disso nada na tela é legível.

## TextMeshPro

Os números de dano usam TextMeshPro, que vem junto com o pacote `com.unity.ugui`.

Na primeira vez a Unity abre um diálogo pedindo para importar o **TMP Essential Resources**. É preciso aceitar, senão os números não aparecem. É uma vez só por projeto.

## Testes

O código do jogo fica no assembly `HerOClock`, definido por `Assets/Scripts/HerOClock.asmdef`. Os testes ficam em `Assets/Tests/EditMode` e `Assets/Tests/PlayMode`, cada um com o seu.

O assembly do jogo existe por uma regra da Unity fácil de esbarrar: um assembly criado por `.asmdef` **não consegue** referenciar o `Assembly-CSharp`, só o contrário. Como todo assembly de teste precisa ser um `.asmdef`, sem esse arquivo nenhum teste enxergaria o jogo.

Para rodar: `Window > General > Test Runner`, e as abas `EditMode` e `PlayMode`.

Quase tudo está em EditMode, que não precisa de cena e roda a suíte inteira em poucos segundos. O PlayMode tem apenas o smoke test, que é a única coisa que realmente precisa do motor rodando: ele carrega a `SampleScene`, confere que os heróis e a primeira onda existem e que o tabuleiro foi desenhado nas sorting layers certas.

O teste `BalanceTests.WriteTheBalanceSnapshot` não confere nada: ele **gera** o arquivo `.claude/balance/snapshot.md`, que entra no commit junto com a alteração que o mudou.

### Rodar o PlayMode esvazia a Hierarchy, e está tudo bem

Ao terminar a aba PlayMode você provavelmente vai cair em uma cena vazia sem nome. **Nada foi perdido.** Basta abrir `Assets/Scenes/SampleScene.unity` de novo.

O que acontece é o Test Framework guardando a cena aberta em `Temp/__Backupscenes/`, criando uma cena temporária para hospedar os testes, e restaurando no fim. O smoke test ainda carrega a `SampleScene` por cima disso em modo `Single`, que descarrega tudo antes — é o único jeito de conferir que a cena real se monta sozinha. No fim, o que a Unity restaura é a cena temporária que estava aberta quando a corrida começou, e não a sua.

O arquivo da cena nunca corre risco: em Play Mode a Unity não grava alteração de cena no disco. Se a dúvida bater de novo, `git status Assets/Scenes/` responde na hora.

## Sorting layers necessárias

Já configuradas em `ProjectSettings/TagManager.asset`: `Default`, `Background`, `Ground`, `Characters`, `Projectiles`, `VFX`, `UI`.

O código usa quatro delas pelo nome: `Background` para as casas, `Ground` para a linha divisória, `Characters` para os personagens e `VFX` para os números de dano. Renomear qualquer uma quebra a renderização em silêncio.
