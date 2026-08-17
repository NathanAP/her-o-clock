# Save

## Objetivo

Especificar como o progresso de Her-o-clock é guardado e recuperado: onde os arquivos ficam, o que entra neles, como o jogo percebe que um arquivo foi mexido, e em que momentos ele grava.

O que o save guarda de progresso — experiência, nível, dinheiro, baldes de atividade — é descrito em `progress.md`. Esta página trata do arquivo.

## O que não se pode prometer

O jogo roda na máquina do jogador e o processo precisa ler os números para jogar. Qualquer chave que o binário use, o binário carrega junto. **Não existe forma de impedir que alguém determinado altere o próprio save**, e nenhuma decisão desta página tenta.

O único desenho que impediria de verdade é um servidor autoritativo, onde o save mora fora da máquina. Ele foi recusado: exige internet obrigatória e contradiz a proposta de um jogo leve aberto em um cantinho da tela, descrita em `gameplay.md` e `references.md`.

O que se pode escolher é **de quem cobrar quanto esforço**. Existem três perfis, e eles pedem respostas diferentes:

- Quem abre o arquivo em um editor de texto porque a linha diz `"money": 1500`. Custa dez segundos e nenhum conhecimento, e é a esmagadora maioria.
- Quem usa uma ferramenta genérica de edição de save.
- Quem abre o binário, acha a chave e recalcula a assinatura.

A assinatura descrita abaixo elimina o primeiro perfil e dá trabalho ao segundo. Contra o terceiro nada funciona, e isso é aceito de propósito.

### E não faz falta

Her-o-clock não tem propaganda, loja externa, pay-to-win, placar nem jogo entre jogadores. Quem altera o próprio save estraga o próprio brinquedo e não tira nada de ninguém.

O risco real de alguém perder progresso não é o trapaceiro, é o arquivo corrompido e a migração de versão. É para lá que o esforço desta página vai.

## Onde o save mora

- Na pasta de dados persistentes que o sistema operacional oferece ao jogo, e nunca dentro da pasta do jogo.
- **Progresso nunca vai para as preferências do jogador.** No Windows aquilo é registro, e é mais fácil de editar que um arquivo. As preferências guardam a escolha de tamanho da janela, como `window.md` descreve, e nada além de decisões sobre a tela.

### Um arquivo novo por save

- Cada save é um **arquivo novo**, nomeado pelo instante em que foi escrito: `save_20260817T143012487Z.json`.
- O instante é sempre **UTC**, com milissegundos:
    - UTC porque horário local reordenaria os arquivos ao mudar de fuso ou na virada do horário de verão, e a ordem dos arquivos é o que decide qual é o mais novo.
    - Milissegundos porque dois saves podem cair no mesmo segundo.
- O nome é a única fonte da ordem. Ele é comparado como texto, e o formato foi escolhido para que a ordem alfabética seja a ordem cronológica.
- Gravar um arquivo novo em vez de reescrever o mesmo é o que torna a escrita segura quase de graça: **nenhum arquivo bom é substituído**, então uma queda no meio da escrita não pode destruir nada que já existia.
- Ainda assim a escrita é feita em um arquivo temporário e renomeada por cima ao final, para que um arquivo incompleto nunca seja confundido com o save mais novo.

### Quantos arquivos ficam guardados

Os saves anteriores **são** o backup, então não existe um mecanismo separado para isso. O que existe é uma regra de retenção:

- Ficam guardados os **5 arquivos mais novos**, mais o **mais novo de cada um dos 3 dias distintos mais recentes** presentes na pasta. É a união dos dois conjuntos, então um arquivo que satisfaz as duas regras conta uma vez.
- Dia é dia em UTC, e "dias distintos" são os dias em que existe save, não os últimos três dias do calendário. Quem joga uma vez por mês continua tendo três sessões guardadas.
- Isso dá ao jogador "agora, hoje, ontem e o dia anterior em que jogou", em uns 8 arquivos de alguns kilobytes.

O motivo da segunda metade da regra é o único caso em que a redundância vale algo de verdade: **um bug do jogo gravar um save válido mas errado**. Um save cai a cada poucos segundos, então guardar só os mais novos cobriria menos de um minuto, e o jogador que percebesse o problema no dia seguinte já não teria para onde voltar.

Três regras protegem a poda:

- Ela só roda **depois** que o save novo está confirmado no disco.
- Ela **nunca apaga um arquivo cujo nome não entende**. O que ela não reconhece, ela ignora.
- Ela **nunca apaga um arquivo que falhou a assinatura**. Aquilo é evidência de problema, não lixo.

## O arquivo

O arquivo é JSON legível, com a assinatura na frente e o progresso dentro de `payload`:

```json
{
    "signature": "3f8a...",
    "payload": {
        "version": 1,
        "savedAtUtc": "2026-08-17T14:30:12.4870000Z",
        "integrity": "ok"
    }
}
```

- Ser legível é uma escolha, e o preço dela é que fica óbvio o que editar. O que se compra em troca é poder abrir o save que um jogador anexou a um relato de bug, e poder investigar uma migração que deu errado.
- Cifrar o conteúdo é uma camada que pode ser colocada por cima depois sem mudar nada do resto desta página. Ela não entrou agora porque custa exatamente a capacidade do parágrafo anterior.

### A assinatura

- É um HMAC-SHA256 do **texto** do `payload`, exatamente como ele está escrito no arquivo.
- A chave está embutida no jogo. Ela **não é um segredo** e não é tratada como um: quem abrir o binário acha. Ela serve para que alterar o arquivo exija mais do que um editor de texto.
- Assinar o texto, e não o resultado de reserializar o objeto, é o que mantém a compatibilidade com versões futuras. Um save antigo pode conter campos que a versão de hoje não conhece; reserializar perderia esses campos e a assinatura nunca mais bateria.

### Um save que falha a assinatura carrega de qualquer forma

- Ele é carregado normalmente, e o save passa a carregar a marca `"integrity": "broken"` **para sempre**, em todo save escrito dali em diante.
- Recusar carregar puniria o jogador cujo disco falhou, que é o caso que realmente acontece. Apagar e começar de novo seria pior ainda, porque é irreversível.
- A marca serve para duas coisas: aparecer em um relato de bug, e servir de porta para o que um dia aponte para fora do jogo, como conquista ou placar. Ela **não** limita nada dentro do jogo.
- O arquivo que falhou não é apagado nem sobrescrito.

## Carregar

Ao abrir, o jogo:

1. Lista os arquivos de save que reconhece pelo nome e os ordena do mais novo para o mais velho.
2. Toma o mais novo que consegue ler. Um arquivo que não é JSON válido é registrado e descartado, e o jogo tenta o seguinte.
3. Se não houver nenhum arquivo legível, começa um jogo novo. Uma pasta vazia é o primeiro jogo de alguém, e não um erro.
4. Confere a assinatura, aplicando a regra da seção anterior.
5. Calcula a progressão offline, descrita em `progress.md`.
6. **Grava um save imediatamente**, antes de a primeira onda começar.
7. Poda os arquivos antigos.

A ordem dos passos 5, 6 e 7 não é livre:

- **A gravação é o que consome a ausência.** O tempo ausente é medido a partir do `savedAtUtc` do arquivo carregado. Se o jogo creditasse a progressão offline e caísse antes de gravar, a abertura seguinte leria o mesmo instante e creditaria a mesma ausência outra vez. Não é uma trapaça que alguém precise procurar: basta fechar o jogo na hora errada.
- Esse primeiro save é também o arquivo de abertura da sessão, e é ele que a regra dos 3 dias preserva.

### Versão e migração

- O `payload` declara a `version` do formato. A versão atual é **1**.
- Um save de versão **anterior** é convertido, passo a passo, até a versão atual. Um campo que só existe na versão nova assume o valor de um jogo que nunca o teve, nunca um valor inventado.
- Um save de versão **posterior** à do jogo é recusado, e o jogo tenta o arquivo seguinte. Ler um formato do futuro adivinhando o significado dos campos é a forma mais fácil de corromper o progresso de quem trocou de versão.

## O que entra no save

- **Por herói:** o `id` da ficha, o nível, a experiência acumulada rumo ao próximo nível, os pontos de habilidade, e onde os pontos de atributo foram colocados — o que foi colocado à mão, quantos foram gastos pela distribuição automática, quantos estão sem gastar, e se o automático está ligado.
    - Os pontos automáticos são guardados como **contagem**, e nunca como resultado distribuído. É a mesma razão descrita em `progress.md`: guardar o resultado faria um personagem carregado deixar de ser idêntico a um que subiu de nível jogando, e a reprodutibilidade das batalhas depende disso.
- **Do jogador:** o dinheiro.
- **Da fase:** o `id` da fase atual, e nada mais fino que isso.
- **Da atividade:** os baldes de 10 minutos descritos em `progress.md`.

### O que não entra

- **Onde o jogador estava dentro da fase.** Nem a onda, nem a vida de ninguém. **Carregar recomeça a fase gravada do início, com todo mundo inteiro.**
    - É exatamente o que uma derrota já faz. Fechar o jogo não abre nada que o jogador não conseguisse perdendo, e perder é de graça.
    - O que se compra é não existir estado de meio de fase para gravar, para migrar, ou para errar sutilmente.
    - O desgaste de `gameplay.md` continua intacto, porque ele sempre foi uma regra sobre o **interior** de uma fase: o dano passa de onda para onda, e uma fase recomeçada começa inteira.
- **O quadro da batalha.** Nada de recarga de ataque, sobra de movimento, posição no tabuleiro ou estado do gerador aleatório.
- **A semente da batalha.** Uma batalha não é retomada, então repetir a semente não significaria nada.
- **A formação e as fichas.** Onde cada herói começa vem do asset de formação, e os atributos base vêm da ficha. O save guarda a instância, nunca o molde, conforme "Ficha e instância são coisas separadas" em `memory/architecture.md`.
- **A escolha de tamanho da janela**, que é preferência e não progresso.

### Quando a ficha e o save discordam

- Cada entrada do save é usada **uma única vez**: o segundo herói de uma ficha recebe a segunda entrada com aquele `id`.
    - Um grupo pode conter dois heróis da mesma ficha, e a formação de teste de hoje contém dois de cada. Casar pelo primeiro `id` encontrado daria o mesmo progresso aos dois e perderia um deles a cada carregamento, sem erro nenhum.
- Um herói no save cujo `id` não existe mais é ignorado, com registro no Console.
- Um herói da formação que não está no save entra como novo, no nível que a ficha declara.
- Uma fase no save que não existe mais faz o jogo voltar para a primeira fase, mantendo tudo o mais.

Nenhum desses casos apaga progresso do jogador, e todos são consequência de o conteúdo ser editado enquanto o jogo é desenvolvido.

## Quando o save acontece

- **No fim de cada onda**, no instante em que ela é limpa e antes de a próxima começar. Não pelo lugar em que o grupo está, e sim pelo que a onda pagou: experiência, dinheiro e os baldes da última hora.
- **No início de cada fase**, o que cobre o fim da fase anterior. Vitória e derrota reiniciam a fase, e é o estado depois desse reinício que o jogador leva adiante.
- **Na abertura**, conforme o passo 6 de "Carregar".
- **Ao fechar o jogo**, como cortesia e nunca como garantia. Fechar pela janela avisa o jogo, matar o processo não.

### Por que não existe save periódico

- O save não guarda posição nenhuma dentro de uma fase, então um save no meio de uma onda não gravaria nada que o save da fronteira de onda já não tenha.
- A fronteira de onda é frequente. As transições entre ondas duram alguns segundos, então o save nunca está longe.
- Uma morte súbita do processo custa, no máximo, o que a onda em andamento ia pagar.

### Por que não existe save ao mexer nos atributos

- O jogo nunca para. Enquanto o jogador distribui pontos, o grupo continua enfrentando ondas, e a próxima fronteira de onda chega em segundos.
- O preço é uma janela pequena: quem redistribuir pontos e tiver o processo **morto** nos segundos seguintes perde a redistribuição.

### O jogador nunca vê o jogo parar para salvar

O fim de uma onda é seguido pelo grupo caminhando de volta à formação e pelo chão rolando. É nesse intervalo que o save acontece, então ele cai em um momento que já é de transição.

## O relógio

A progressão offline paga o jogador pelo tempo ausente, e o relógio do sistema é do jogador. Puxá-lo para frente é mais fácil que editar o save e não exige ferramenta nenhuma.

- Se o instante atual for **anterior** ao `savedAtUtc` do save carregado, o tempo ausente é **zero**. Relógio andando para trás não é um caso a modelar, é um sinal de que o horário não serve.
- Contra o relógio puxado para frente não existe defesa local, e os tetos de `progress.md` já limitam o estrago: um mês de ausência rende o mesmo que 12 horas.

## Exemplos

Os exemplos existem para serem copiados para dentro dos testes. Os da progressão offline estão em `progress.md`.

### Retenção

Dada a pasta abaixo, e considerando `save_20260817T120000000Z.json` o save que acabou de ser escrito:

```
save_20260817T120000000Z.json
save_20260817T115950000Z.json
save_20260817T115940000Z.json
save_20260817T115930000Z.json
save_20260817T115920000Z.json
save_20260817T115910000Z.json
save_20260816T220000000Z.json
save_20260816T100000000Z.json
save_20260814T090000000Z.json
save_20260811T080000000Z.json
```

Ficam guardados sete arquivos:

- Os 5 mais novos: os cinco primeiros da lista.
- O mais novo de cada um dos 3 dias distintos mais recentes: dia 17 (já entre os cinco), dia 16 (`220000000`) e dia 14 (`090000000`).

São apagados `save_20260817T115910000Z.json`, `save_20260816T100000000Z.json` e `save_20260811T080000000Z.json`.

Note que o dia 11 é descartado mesmo sendo o único save daquele dia: os três dias distintos mais recentes são 17, 16 e 14.
