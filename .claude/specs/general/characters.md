# Personagens

## Objetivo

Especificar todos os detalhes gerais sobre os personagens presentes em Her-o-clock.

## Fichas

- As fichas de cada personagem do jogo estão presentes na pasta `.claude/specs/characters`.
- O desenvolvimento do jogo deve espelhar as fichas dos personagens.

### Estas fichas são documento de design, não dado do jogo

- O jogo não lê estes arquivos. Ele lê os `CharacterDefinition` em `Assets/ScriptableObjects/`, e os arquivos daqui são a **fonte da intenção** que aqueles assets espelham.
- É por isso que eles guardam coisas que um asset não guarda: história, características físicas, o bloco `hidden` e as habilidades, que ainda nem existem no código.
- O texto que o jogador lê **não fica na ficha nem no asset**. Ele mora em `Assets/Strings/`, na chave `character.{id}.name`, conforme descrito em `attributes.md`.
- As quatro fichas que existem hoje em `Assets/ScriptableObjects/` são de teste e não correspondem a nenhuma ficha desta pasta. Os personagens de verdade do ato 1 chegam na 0.9.0.0.

### Lacaios e vilões também têm atributos principais

- Pode parecer estranho, já que eles não carregam equipamento e ninguém distribui pontos por eles. Foi discutido e a decisão é mantê-los, por dois motivos que só aparecem mais adiante:
    - **As habilidades escalam por atributo.** O bloco `scaling` de um `deal_damage` fala em frações de POW ou de SPE, e vilões vão ter habilidades.
    - **Buffs e debuffs precisam de um atributo em que morder.** Um `modify_stat` que reduz POW não teria efeito nenhum sobre um inimigo que não tem POW.
- Tirar os primários dos inimigos exigiria construir um segundo caminho, só para eles, nos dois sistemas.
- A dificuldade real que isso cria é de autoria: escrever "POW 24, CON 25" para chegar em "370 de vida, 24 de dano" é indireto. Isso é resolvido no Inspector, que mostra ao vivo o que a ficha produz em qualquer nível, e não no modelo.
- As fichas também indicam valores de dano, redução de recarga, escala para buffs e debuffs, entre outros.

### Estrutura de uma ficha

- `id` — identificador estável em texto. É por ele que fases, saves, o arquivo de strings e outras fichas apontam para este personagem, então ele nunca muda depois que existe conteúdo o referenciando.
    - O nome mostrado ao jogador **não fica na ficha**. Ele mora em `Assets/Strings/`, na chave `character.{id}.name`.
    - Escrito em minúsculas, com hífen separando as palavras.
    - **O id diz quem o personagem é, e nunca quantos são.** Nada de `discarded-prototype-1`, `-2`, `-3`.
        - Vários inimigos iguais na mesma onda são o **mesmo id em posições diferentes**, que é exatamente o que o formato de fase já faz. A instância é a posição somada ao nível da fase, e nada disso mora na ficha.
        - Numerar os ids criaria uma ficha por cópia, com o mesmo bloco de números repetido, que é o problema descrito em "### A ficha diz quem o personagem é, a fase diz quão forte ele está" em `progress.md`. Criaria também uma chave de texto por cópia, todas escrevendo o mesmo nome.
        - E o número nunca significaria nada: se um dia a onda precisar de sete inimigos e só existirem seis ids, alguém teria que inventar um sétimo personagem para posicionar o mesmo inimigo mais uma vez.
    - Quando dois personagens são **de fato diferentes**, eles são personagens diferentes e cada um ganha o seu id descritivo, como `discarded-prototype-armored`. Aí o sufixo diz alguma coisa sobre quem ele é.
    - Isso vale igualmente para heróis. Uma eventual cópia de um herói é um personagem com identidade própria, e não `gadrat-2`.
- `kind` — o tipo: `hero`, `minion`, `villain` ou `npc`.
- `initialLevel` e `maxLevel` — a faixa de níveis que aquele personagem alcança.
    - `initialLevel` existe **apenas na ficha de herói**, e vale `1`. É o nível com que ele entra no time.
    - Lacaios, vilões e NPCs não declaram esse campo, pois o nível deles vem da fase em que aparecem, e não da ficha, conforme "### A ficha diz quem o personagem é, a fase diz quão forte ele está" em `progress.md`. Um nível inicial escrito na ficha deles seria um número que nada lê.
    - `maxLevel` limita a curva de crescimento de qualquer personagem.
- `equipment` — a classe de equipamento (`light`, `magic` ou `heavy`), que decide como os atributos principais viram secundários, conforme `items.md`.
    - Lacaios, vilões e NPCs não carregam equipamento de verdade, mas declaram uma classe assim mesmo, pois é ela que define as constantes de evasão, velocidade e recarga deles.
- `minRange` e `maxRange` — a faixa de alcance do ataque básico, em casas. Corpo a corpo é `1` e `1`. Um alcance mínimo acima de 1 faz o personagem recuar quando um inimigo encosta.
- `autoAttacks` — como o ataque básico se apresenta. Hoje só carrega `type`, que é `melee` ou `ranged`, e serve ao visual: um ataque `ranged` dispara um projétil. Ele não muda regra nenhuma de combate, pois quem decide alcance são os dois campos acima.
- `defence` — os atributos defensivos, cada um com o valor inicial e **quanto ganha por nível**, conforme "Atributos limitados crescem com o nível" em `attributes.md`.
    - `physicalArmor`, `fireResistance`, `waterResistance` e `electricResistance`.
    - Ganho igual ao valor inicial mantém a mitigação parada durante o jogo inteiro, e é o caso mais comum.
- `thornsPercent`, `lifeStealPercent` e `healthRegen` — secundários que são porcentagem ou taxa, e por isso não precisam de ganho por nível.
- `info` — tudo que o jogador pode ver: descrição, história, características físicas e visual.
- `hidden` — a verdade por trás do personagem, que o jogador só descobre jogando.
    - Este bloco existe **apenas como documento de design**. Ele nunca deve ir para o dado que o jogo lê, senão qualquer pessoa abre o arquivo do jogo e encontra a revelação antes de merecê-la.
- `baseAttributes` — os atributos principais no nível inicial, antes de qualquer outra fonte.
- `attributeGrowth` — como o personagem distribui os 5 pontos que recebe a cada nível, em porcentagens. Descrito em `progress.md`.
- `abilityTrees` ou `abilities` — as habilidades do personagem, descritas por inteiro em `abilities.md`. Qual dos dois campos a ficha usa depende do tipo, e está detalhado logo abaixo.

### Herói guarda habilidade em árvore, lacaio e vilão guardam em lista

- **Heróis usam `abilityTrees`**, uma lista de árvores. Cada árvore declara:
    - `id` — identificador estável, no mesmo padrão do `id` do personagem.
    - `name` — o nome da árvore.
    - `abilities` — as habilidades que moram nela.
- **Lacaios, vilões e NPCs usam `abilities`**, uma lista lisa, sem árvore nenhuma em volta.
- A diferença não é inconsistência, é a diferença real entre os dois lados: **árvore é progressão, e ninguém progride um lacaio.** O jogador investe pontos, escolhe caminho e destrava rank em um herói. Um lacaio nasce com o que a ficha dele diz e morre com isso.
    - Envolver as habilidades de um lacaio em uma árvore criaria um nó que nunca é comprado por ninguém, e alguém acabaria tentando dar sentido a ele mais tarde.
- Um herói pode ter **mais de uma árvore**, e é isso que sustenta o sistema de classes e subclasses. Uma árvore é a unidade que o jogador escolhe seguir ou não.
- Enquanto a árvore de habilidades não existir, toda habilidade é usada no rank 1, conforme `abilities.md`. A árvore já é escrita agora porque é ela que diz a qual caminho cada habilidade pertence, e isso é decisão de design, não de implementação.

## Heróis

- Os heróis são os personagens usados pelo jogador.
- A principal característica de design é que todos eles são robôs, mesmo que possuam traços humanos (movimentação, expressões).
- Os heróis podem ser equipados por itens conforme o jogador escolher.

### Grupo de heróis

- Um grupo de heróis é composto de até 4 personagens e pode ser escolhido pelo jogador a qualquer momento.
    - Perceba que se um jogador quiser usar apenas um único personagem na fase, também é possível.
        - Ele pode querer fazer isso para maximizar o ganho de experiência daquele personagem por exemplo.
- Um grupo de heróis pode ser montado do jeito que o jogador preferir.
- Um grupo de heróis só é alterado verdadeiramente no início da fase.
    - Ou seja, alterar o grupo de heróis no meio da fase não tem efeito imediato, o jogador precisa aguardar a atual fase terminar para que o grupo seja atualizado.
- A ordem do grupo de heróis é importante pois em fases que demandam menos que o total de heróis liberados (slots), a escolha é sempre na ordem do slot. Por exemplo:
    - Se a fase exige apenas um personagem e meu grupo é composto pelos heróis A, B, C e D (nesta ordem), sempre o A será escolhido para aquela fase. Se eu alterar o grupo para B, A, D, C, sempre o B será escolhido para aquela fase. O mesmo vale para fases que exigem dois heróis (A e B seriam escolhidos no primeiro exemplo e B e A no segundo).
- Durante a escolha do grupo, o jogador pode também alterar a formação inicial em campo de batalha, dispondo das 24 casas da área dos heróis (6 colunas por 4 fileiras).
    - Como o grupo possui no máximo 4 heróis, a maior parte da área fica vazia. Isso é intencional e é o que permite ao jogador escolher entre concentrar o grupo ou espalhá-lo contra ataques em área.
    - A formação define apenas onde a batalha começa. A partir dali os personagens se movimentam sozinhos, conforme descrito em `gameplay.md`.

### Banco de heróis

- Além do grupo de heróis, o jogador conta com um banco de heróis (ou só "banco").
- O banco é usado para guardar os heróis que não estão sendo usados no grupo de heróis.
- O banco pode ser consultado e seus heróis podem ser manipulados a qualquer momento (troca de itens, alterar habilidades e atributos, etc).
- Heróis que estão no grupo podem ser substituídos pelos que estão no banco a qualquer momento.
    - Lembrando que o efeito da troca não é imediata.
- Alguns itens podem afetar o banco (divisão de experiência).
- Heróis do banco não ganham experiência, conforme aponta o `progress.md`.

### Slots

- Os slots definem quantas posições a equipe tem, começando por 1 e com o limite de 4.
- Os slots são liberados conforme o jogador completa fases específicas, conforme aponta o `stages.md`.
- Liberação de slot e liberação de heróis são eventos separados. Em um certo ponto o jogador terá 3 slots disponíveis mas um quarto ou quinto herói fica disponível. Cabe a ele decidir quem vai ficar no grupo e quem vai ir para o banco de heróis.

## Lacaios

- Os lacaios são personagens que aparecem nas fases antes e/ou junto com os vilões.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades criadas pelos vilões.
- Os lacaios aparecem repetidamente em grupos pré definidos nas fases.

## Vilões

- Os vilões são personagens que aparecem ao final de cada fase sozinhos ou junto de seus lacaios.
- Vilões podem ser também chamados e considerados os "bosses" ou "chefões" do jogo.
- A principal característica de design é que eles podem ser humanos, robôs ou monstruosidades.
- Os vilões podem aplicar buffs aos lacaios ou debuffs aos jogadores conforme suas características.

## NPCs

- Os NPCs são personagens que aparecem em certas fases de forma fixa.
- Eles lutam junto com os heróis e avançam as ondas junto deles.
- NPCs podem ser personagens já existentes dentre os atuais heróis ou vilões e isso é totalmente normal.
- NPCs tem o mesmo comportamento de um personagem como qualquer outro, então eles podem atacar, andar pelo tabuleiro, tomar dano, se curar ou até mesmo morrer.
    - A morte de NPCs não implicam em falha da fase.
- **NPCs não ganham experiência nenhuma**, conforme `progress.md`. Eles não pertencem ao grupo do jogador.
- **NPCs não contam para a derrota.** Uma fase é perdida quando o grupo do jogador cai, mesmo que o NPC continue de pé. Sem essa regra, um NPC venceria sozinho uma fase que o jogador perdeu.
- Alguns NPCs podem precisar passar por algum tipo de script automático (uma morte automática, um posicionamento automático ou lançar uma habilidade específica). A forma desse acontecimento automático estará descrito na fase.

## Nível de personagem

- Personagens possuem níveis que vão de 1 até 100.
- Quanto mais alto o nível de um personagem:
    - Mais atributos ele terá.
    - Mais habilidades ele irá possuir.
- Para subir o nível de seus heróis, o jogador deve enfrentar lacaios e vilões.
    - Muitas vezes o jogador precisará repetir a mesma fase inúmeras vezes para conseguir ficar mais forte e enfrentar a próxima fase.
- Lacaios, vilões e NPCs possuem níveis de acordo com a fase em que estão presentes, e não de acordo com a ficha deles.
- Tudo que diz respeito a ganhar experiência, subir de nível e distribuir os pontos recebidos está descrito em `progress.md`.
