# Decisões de design

## Objetivo

Guardar o porquê das decisões de sistema já fechadas. As regras em si estão em `.claude/specs/`, aqui fica o raciocínio, para ninguém "consertar" mais tarde algo que foi feito de propósito.

## Todo atributo limitado usa rendimento decrescente

`Teto × Pontos ÷ (Pontos + Constante)`.

O critério de quando usar a curva: atributo que representa chance ou porcentagem de mitigação é limitado por natureza e usa a curva. Atributo que representa dano, vida ou velocidade é ilimitado e continua linear, porque um multiplicador de taxa não satura.

Como o teto nunca é alcançado, **não existe trava manual em lugar nenhum**. Se aparecer código do tipo "se passar de X, considera X", é bug.

## Não existe atributo de aggro

A escolha de alvo é inteiramente posicional. Provocação é um efeito temporário vindo de habilidade, nunca um número que sobe e desce.

O motivo é o pitch do jogo: ele é minúsculo e fica no cantinho da tela, então o jogador precisa conseguir prever o combate de relance. Um número escondido variando durante a luta destrói essa leitura. Aggro numérico também seria muito mais difícil de reproduzir na simulação offline.

## A armadura escala com o nível de quem ataca

A constante da mitigação é `50 × nível do atacante`. Sem isso, uma armadura fixa daria a mesma porcentagem para sempre e o tank ficaria resolvido cedo demais em um jogo de farm infinito.

## Resistência elemental acima de 100% é objetivo de build

Fontes comuns nunca passam dos 75% da curva. Só fontes especiais somam depois da curva e podem levar acima disso. É o modelo do Divinity: absorver dano elemental é conquista, não acúmulo acidental.

## O tabuleiro é 6 colunas por 8 fileiras

Escolhido por três motivos concretos:

- Com 30 pixels por unidade, dá células quadradas de 30x30 e o tabuleiro ocupa 75% da altura da tela, sobrando espaço para a interface.
- Largura par permite centralizar um vilão que ocupa 2x2. Largura ímpar deixaria ele torto.
- Atravessar o tabuleiro leva 3.5 segundos na velocidade base, contra 5.5 do 6x12 que foi cogitado. Em um jogo cujo loop é engajar repetidamente, isso é tempo morto.

As duas áreas têm o mesmo tamanho de propósito. Se ficar claro que os heróis precisam de mais ou menos espaço, essa é a primeira coisa a ajustar.

## Movimentação livre, sem bloqueio de corpo

Personagens atravessam o tabuleiro inteiro atrás do alvo. As áreas definem apenas onde a batalha começa.

O tank continua funcionando porque a segunda regra da cadeia é "mais próximo", e quem está na frente é o mais próximo. O blink do assassino continua tendo identidade porque em tempo real o valor dele não é *conseguir chegar*, é *chegar agora*, economizando os segundos que o resto gasta andando.

Dar bloqueio de corpo ao corpo a corpo tiraria o valor da movimentação livre. São dois caminhos válidos e não dá para ter os dois.
