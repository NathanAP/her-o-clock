# Snapshot de balanceamento

Gerado por `BalanceTests.WriteTheBalanceSnapshot`. **Não edite à mão.**

Este arquivo é o retrato dos números de saída do jogo, conforme
"## Onde cada número mora" no `CLAUDE.md`. Ele entra no commit junto com a
alteração que o mudou, e é o diff dele que responde o que aquela alteração fez
com o resto do jogo.

## Ritmo de progressão

Farmando inimigos do próprio nível, a 11 inimigos por minuto de jogo aberto.

| Nível | Inimigos para o próximo | Horas acumuladas |
|---|---|---|
| 5 | 61 | 0.1 |
| 10 | 177 | 0.9 |
| 25 | 734 | 10.4 |
| 50 | 2150 | 62.2 |
| 75 | 4030 | 176.6 |
| 90 | 5346 | 281.9 |
| 100 | 0 | 369.3 |

## Fichas

Valores no nível inicial da ficha, sem itens.

| Ficha | Vida | Dano fis. | Atq/s | Casas/s | Evasão (pts) | Armadura | Regen/s |
|---|---|---|---|---|---|---|---|
| tempo | 70 | 5-7 | 1.09 | 2.18 | 34 | 12 | 0.00 |
| gadrat | 120 | 7-11 | 1.00 | 2.02 | 0 | 60 | 0.00 |
| gadrat-npc | 120 | 7-11 | 1.00 | 2.02 | 0 | 60 | 0.00 |
| discarded-prototype | 20 | 2-3 | 1.01 | 2.02 | 1 | 0 | 0.00 |
| exposed-prototype | 40 | 3-5 | 1.00 | 2.02 | 0 | 60 | 0.00 |

### Defesa contra um atacante do mesmo nível

Mitigação física e chance de evasão, que passam pela mesma curva contra a mesma
constante. Uma linha parada significa que aquela defesa acompanha a curva. Uma linha
que cai significa que a ficha perde aquela defesa conforme o jogo avança.

| Ficha | Defesa | Nível 1 | Nível 12 | Nível 30 | Nível 50 | Nível 100 |
|---|---|---|---|---|---|---|
| tempo | armadura | 14.5% | 14.5% | 14.5% | 14.5% | 14.5% |
| tempo | evasão | 40.5% | 35.3% | 35.1% | 35.0% | 34.9% |
| gadrat | armadura | 40.9% | 40.9% | 40.9% | 40.9% | 40.9% |
| gadrat | evasão | 0.0% | 0.2% | 0.1% | 0.1% | 0.1% |
| gadrat-npc | armadura | 40.9% | 40.9% | 40.9% | 40.9% | 40.9% |
| gadrat-npc | evasão | 0.0% | 0.2% | 0.1% | 0.1% | 0.1% |
| discarded-prototype | armadura | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| discarded-prototype | evasão | 2.0% | 2.0% | 2.0% | 2.0% | 2.0% |
| exposed-prototype | armadura | 40.9% | 40.9% | 40.9% | 40.9% | 40.9% |
| exposed-prototype | evasão | 0.0% | 0.2% | 0.2% | 0.2% | 0.2% |

## O kit de referência

A ficha da Tempo vestindo um kit sem modificador nenhum, do mesmo nível que ela:
os quatro cascos e a mão secundária, só com a defesa base da classe.

A coluna **Ativos** é a que interessa. Um kit do próprio nível exige 1.5 vezes esse
nível num atributo, e uma heroína que espalhou os pontos não atende isso em tudo.

| Nível | Kit | Ativos | Armadura | Mitigação | Evasão (pts) | Evasão | Resist. |
|---|---|---|---|---|---|---|---|
| 1 | sem kit | - | 12 | 14.5% | 34 | 40.5% | 0 |
| 1 | light | 5/5 | 12 | 14.5% | 96 | 65.8% | 0 |
| 1 | heavy | 5/5 | 52 | 38.2% | 27 | 35.1% | 0 |
| 1 | special | 5/5 | 12 | 14.5% | 30 | 37.5% | 17 |
| 12 | sem kit | - | 144 | 14.5% | 328 | 35.3% | 0 |
| 12 | light | 5/5 | 144 | 14.5% | 1063 | 63.9% | 0 |
| 12 | heavy | 5/5 | 633 | 38.5% | 306 | 33.8% | 0 |
| 12 | special | 5/5 | 144 | 14.5% | 314 | 34.4% | 204 |
| 30 | sem kit | - | 360 | 14.5% | 810 | 35.1% | 0 |
| 30 | light | 5/5 | 360 | 14.5% | 2646 | 63.8% | 0 |
| 30 | heavy | 0/5 | 360 | 14.5% | 810 | 35.1% | 0 |
| 30 | special | 0/5 | 360 | 14.5% | 810 | 35.1% | 0 |
| 50 | sem kit | - | 600 | 14.5% | 1345 | 35.0% | 0 |
| 50 | light | 5/5 | 600 | 14.5% | 4405 | 63.8% | 0 |
| 50 | heavy | 0/5 | 600 | 14.5% | 1345 | 35.0% | 0 |
| 50 | special | 0/5 | 600 | 14.5% | 1345 | 35.0% | 0 |
| 100 | sem kit | - | 1200 | 14.5% | 2682 | 34.9% | 0 |
| 100 | light | 5/5 | 1200 | 14.5% | 8802 | 63.8% | 0 |
| 100 | heavy | 0/5 | 1200 | 14.5% | 2682 | 34.9% | 0 |
| 100 | special | 0/5 | 1200 | 14.5% | 2682 | 34.9% | 0 |

## Fases

A formação de heróis é a do asset. O tempo é só de combate, sem as transições.
"Nível mínimo" é o menor nível de herói que limpa a fase.

| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |
|---|---|---|---|---|---|
| act1-stage1 | 1 | 1 | 23.5 s | 160 | 16 |
| act1-stage2 | 2 | 3 | 38.7 s | 800 | 20 |
| act1-stage3 | 4 | 5 | 42.5 s | 3280 | 28 |
| act1-stage4 | 3 | 4 | 43.8 s | 2250 | 25 |

### Paredes

Jogando as fases em ordem e limpando cada uma uma vez. "Chega com" é o nível
que o time tem ao encostar na fase; "exige" é o nível que ela pede. Quando o
exigido passa o de chegada, o jogador é obrigado a parar e farmar.

| Fase | Chega com | Exige | Parede | Custo |
|---|---|---|---|---|
| act1-stage1 | 1 | 1 | não | - |
| act1-stage2 | 2 | 3 | sim | 4x a fase anterior, 1.5 min de combate |
| act1-stage3 | 3 | 5 | sim | 12x a fase anterior, 7.7 min de combate |
| act1-stage4 | 5 | 4 | não | - |

O custo conta só o tempo de combate, e despreza a experiência parcial que sobra
de um nível para o outro. As transições entre ondas somam vários segundos por
repetição, então o tempo real de relógio é maior que o mostrado.

**As fases que existem hoje são casos de teste, não conteúdo.** A segunda tem
inimigos de nível 12 de propósito, para exercitar herói caindo em combate e avanço
bloqueado. A parede gigante entre as duas é o resultado esperado desse par, e não
um problema de balanceamento. Ver `game-objects/fases.md`.

## Quantas equipes limpam cada fase

Cada linha roda várias equipes, várias builds e várias sementes na mesma
fase e no mesmo nível, e conta quantas limparam.

**Isto é uma amostra, e nunca uma prova.** O espaço de equipes, builds,
ordens e, mais adiante, itens e árvores é grande demais para ser coberto.
O que a tabela mostra é a forma: no nível recomendado a maioria passa, e
alguns níveis abaixo a maioria não passa.

A amostra cresce junto com o espaço. No começo não há o que variar: um herói
no nível 1 não tem ponto nenhum para distribuir.

### act1-stage1 (recomendado 1 a 1)

| Nível | Limparam | Testadas | % |
|---|---|---|---|
| 1 | 6 | 6 | 100% |
| 2 | 6 | 6 | 100% |
| 3 | 6 | 6 | 100% |

Por build, no nível 1:

| Build | Limpou | Tentativas |
|---|---|---|
| a da ficha | 6 | 6 |

### act1-stage2 (recomendado 2 a 3)

| Nível | Limparam | Testadas | % |
|---|---|---|---|
| 1 | 0 | 24 | 0% |
| 2 | 2 | 24 | 8% |
| 3 | 8 | 24 | 33% |
| 4 | 11 | 24 | 46% |
| 5 | 15 | 24 | 63% |

Por build, no nível 3:

| Build | Limpou | Tentativas |
|---|---|---|
| a da ficha | 5 | 6 |
| só POW | 1 | 6 |
| só AGI | 1 | 6 |
| só SPE | 1 | 6 |

### act1-stage3 (recomendado 4 a 5)

| Nível | Limparam | Testadas | % |
|---|---|---|---|
| 2 | 6 | 36 | 17% |
| 3 | 15 | 36 | 42% |
| 4 | 15 | 36 | 42% |
| 5 | 20 | 36 | 56% |
| 6 | 21 | 36 | 58% |
| 7 | 22 | 36 | 61% |

Por build, no nível 5:

| Build | Limpou | Tentativas |
|---|---|---|
| a da ficha | 6 | 6 |
| só POW | 0 | 6 |
| só AGI | 2 | 6 |
| só SPE | 0 | 6 |
| só CON | 6 | 6 |
| POW/CON | 6 | 6 |

### act1-stage4 (recomendado 5 a 6)

| Nível | Limparam | Testadas | % |
|---|---|---|---|
| 3 | 10 | 84 | 12% |
| 4 | 18 | 84 | 21% |
| 5 | 24 | 84 | 29% |
| 6 | 33 | 84 | 39% |
| 7 | 42 | 84 | 50% |
| 8 | 32 | 84 | 38% |

Por build, no nível 6:

| Build | Limpou | Tentativas |
|---|---|---|
| a da ficha | 8 | 12 |
| só POW | 1 | 12 |
| só AGI | 1 | 12 |
| só SPE | 1 | 12 |
| só CON | 12 | 12 |
| POW/CON | 10 | 12 |
| AGI/SPE | 0 | 12 |

