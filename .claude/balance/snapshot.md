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
| gadratNpc | 120 | 7-11 | 1.00 | 2.02 | 0 | 60 | 0.00 |
| discardedPrototype | 20 | 2-3 | 1.01 | 2.02 | 1 | 0 | 0.00 |
| exposedPrototype | 40 | 3-5 | 1.00 | 2.02 | 0 | 60 | 0.00 |

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
| gadratNpc | armadura | 40.9% | 40.9% | 40.9% | 40.9% | 40.9% |
| gadratNpc | evasão | 0.0% | 0.2% | 0.1% | 0.1% | 0.1% |
| discardedPrototype | armadura | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| discardedPrototype | evasão | 2.0% | 2.0% | 2.0% | 2.0% | 2.0% |
| exposedPrototype | armadura | 40.9% | 40.9% | 40.9% | 40.9% | 40.9% |
| exposedPrototype | evasão | 0.0% | 0.2% | 0.2% | 0.2% | 0.2% |

## As armas

O que cada subtipo rende sozinho, antes de POW e de AGI. O dano por segundo é o
dano médio vezes a velocidade, que é o orçamento que a família paga.

| Subtipo | Mãos | Alcance | Atq/s | Dano nv. 1 | DPS nv. 1 | Dano nv. 100 | DPS nv. 100 |
|---|---|---|---|---|---|---|---|
| blade | 1 | 1-1 | 1.30 | 6-8 | 9.1 | 65-87 | 99.2 |
| claw | 1 | 1-1 | 1.70 | 5-6 | 9.4 | 55-65 | 101.9 |
| ram | 1 | 1-1 | 0.80 | 7-15 | 8.8 | 76-164 | 95.9 |
| prod | 1 | 1-1 | 1.00 | 7-11 | 9.0 | 76-120 | 98.1 |
| catalyst | 1 | 1-2 | 1.00 | 5-7 | 6.0 | 55-76 | 65.4 |
| emitter | 1 | 1-4 | 1.20 | 5-7 | 7.2 | 55-76 | 78.5 |
| greatblade | 2 | 1-1 | 0.90 | 12-19 | 13.9 | 131-207 | 152.1 |
| piledriver | 2 | 1-1 | 0.50 | 16-40 | 14.0 | 174-436 | 152.6 |
| lance | 2 | 1-2 | 0.70 | 14-22 | 12.6 | 153-240 | 137.3 |
| rifle | 2 | 2-6 | 1.00 | 9-13 | 11.0 | 98-142 | 119.9 |
| cannon | 2 | 3-8 | 0.45 | 14-35 | 11.0 | 153-382 | 120.2 |
| reactor | 2 | 1-3 | 0.70 | 10-16 | 9.1 | 109-174 | 99.2 |

## As habilidades

O que cada habilidade rende, lida no nível em que aquele rank abre e sem item nenhum.
A recarga efetiva já conta a redução de recarga da ficha e o tempo parado da
habilidade. O DPS é **por alvo**: multiplique pelos alvos para o total.

| Ficha | Habilidade | Nível | Rank | Dano/alvo | Alvos | Custo | Recarga ef. | DPS/alvo |
|---|---|---|---|---|---|---|---|---|
| tempo | fastAndFurious | 1 | 1 | 12.1 | 1 | - | 19.6s | 0.6 |
| tempo | fastAndFurious | 10 | 2 | 40.7 | 1 | - | 16.4s | 2.5 |
| tempo | fastAndFurious | 20 | 3 | 72.1 | 1 | - | 15.2s | 4.7 |
| tempo | fastAndFurious | 32 | 4 | 99.3 | 1 | - | 14.3s | 7.0 |
| tempo | fastAndFurious | 45 | 5 | 127.3 | 1 | - | 12.1s | 10.5 |
| gadrat | dragonBreath | 1 | 1 | 15.1 | 4 | - | 37.1s | 0.4 |
| gadrat | dragonBreath | 10 | 2 | 50.8 | 4 | - | 31.5s | 1.6 |
| gadrat | dragonBreath | 20 | 3 | 92.7 | 4 | - | 26.1s | 3.5 |
| gadrat | dragonBreath | 35 | 4 | 125.8 | 4 | - | 20.8s | 6.0 |
| gadrat | dragonBreath | 50 | 5 | 160.1 | 4 | - | 15.9s | 10.1 |
| gadrat | warmUp | 1 | 1 | 10.1 | 9 | 8.0 | 41.5s | 0.2 |
| gadrat | warmUp | 8 | 2 | 35.5 | 9 | 12.0 | 40.9s | 0.9 |
| gadrat | warmUp | 18 | 3 | 61.6 | 9 | 16.0 | 35.3s | 1.7 |
| gadrat | warmUp | 30 | 4 | 88.6 | 9 | 20.0 | 34.4s | 2.6 |
| gadrat | warmUp | 45 | 5 | 116.7 | 9 | 24.0 | 29.0s | 4.0 |

## O kit de referência

A ficha da Tempo vestindo um kit sem modificador nenhum, do mesmo nível que ela:
os quatro cascos e a mão secundária, só com a defesa base da classe.

A coluna **Ativos** é a que interessa. Um kit do próprio nível exige 1.5 vezes esse
nível num atributo, e uma heroína que espalhou os pontos não atende isso em tudo.

| Nível | Kit | Ativos | Armadura | Mitigação | Evasão (pts) | Evasão | Resist. |
|---|---|---|---|---|---|---|---|
| 1 | sem kit | - | 12 | 14.5% | 34 | 40.5% | 0 |
| 1 | light | 6/6 | 12 | 14.5% | 96 | 65.8% | 0 |
| 1 | heavy | 6/6 | 52 | 38.2% | 27 | 35.1% | 0 |
| 1 | special | 6/6 | 12 | 14.5% | 30 | 37.5% | 17 |
| 12 | sem kit | - | 144 | 14.5% | 328 | 35.3% | 0 |
| 12 | light | 6/6 | 144 | 14.5% | 1063 | 63.9% | 0 |
| 12 | heavy | 6/6 | 633 | 38.5% | 306 | 33.8% | 0 |
| 12 | special | 6/6 | 144 | 14.5% | 314 | 34.4% | 204 |
| 30 | sem kit | - | 360 | 14.5% | 810 | 35.1% | 0 |
| 30 | light | 6/6 | 360 | 14.5% | 2646 | 63.8% | 0 |
| 30 | heavy | 0/6 | 360 | 14.5% | 810 | 35.1% | 0 |
| 30 | special | 0/6 | 360 | 14.5% | 810 | 35.1% | 0 |
| 50 | sem kit | - | 600 | 14.5% | 1345 | 35.0% | 0 |
| 50 | light | 6/6 | 600 | 14.5% | 4405 | 63.8% | 0 |
| 50 | heavy | 0/6 | 600 | 14.5% | 1345 | 35.0% | 0 |
| 50 | special | 0/6 | 600 | 14.5% | 1345 | 35.0% | 0 |
| 100 | sem kit | - | 1200 | 14.5% | 2682 | 34.9% | 0 |
| 100 | light | 6/6 | 1200 | 14.5% | 8802 | 63.8% | 0 |
| 100 | heavy | 0/6 | 1200 | 14.5% | 2682 | 34.9% | 0 |
| 100 | special | 0/6 | 1200 | 14.5% | 2682 | 34.9% | 0 |

A mesma Tempo, do lado ofensivo. Uma arma inativa devolve o soco da ficha, então
uma linha igual à do kit ausente quer dizer que a arma não passou no requerimento.

| Nível | Kit | Mãos | Dano | Atq/s | DPS |
|---|---|---|---|---|---|
| 1 | sem kit | 1 | 5.0-7.0 | 1.09 | 6.6 |
| 1 | light | 1 | 6.0-8.0 | 1.42 | 10.0 |
| 1 | heavy | 1 | 7.0-15.1 | 0.81 | 9.0 |
| 1 | special | 1 | 5.0-7.0 | 1.04 | 6.3 |
| 12 | sem kit | 1 | 5.1-7.1 | 1.28 | 7.8 |
| 12 | light | 1 | 12.9-17.1 | 1.66 | 25.0 |
| 12 | heavy | 1 | 15.0-32.1 | 0.84 | 19.9 |
| 12 | special | 1 | 10.7-15.0 | 1.14 | 14.7 |
| 30 | sem kit | 1 | 5.2-7.3 | 1.60 | 10.0 |
| 30 | light | 1 | 24.4-32.5 | 2.08 | 59.2 |
| 30 | heavy | 1 | 5.2-7.3 | 1.60 | 10.0 |
| 30 | special | 1 | 5.2-7.3 | 1.60 | 10.0 |
| 50 | sem kit | 1 | 5.3-7.5 | 1.95 | 12.5 |
| 50 | light | 1 | 37.8-50.4 | 2.53 | 111.7 |
| 50 | heavy | 1 | 5.3-7.5 | 1.95 | 12.5 |
| 50 | special | 1 | 5.3-7.5 | 1.95 | 12.5 |
| 100 | sem kit | 1 | 5.7-7.9 | 2.82 | 19.1 |
| 100 | light | 1 | 73.9-98.5 | 3.67 | 316.1 |
| 100 | heavy | 1 | 5.7-7.9 | 2.82 | 19.1 |
| 100 | special | 1 | 5.7-7.9 | 2.82 | 19.1 |

## Fases

A formação de heróis é a do asset. O tempo é só de combate, sem as transições.
"Nível mínimo" é o menor nível de herói que limpa a fase.

| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |
|---|---|---|---|---|---|
| act1Stage1 | 1 | 1 | 23.5 s | 160 | 16 |
| act1Stage2 | 2 | 3 | 38.7 s | 800 | 20 |
| act1Stage3 | 4 | 5 | 42.5 s | 3280 | 28 |
| act1Stage4 | 3 | 4 | 43.8 s | 2250 | 25 |

### Paredes

Jogando as fases em ordem e limpando cada uma uma vez. "Chega com" é o nível
que o time tem ao encostar na fase; "exige" é o nível que ela pede. Quando o
exigido passa o de chegada, o jogador é obrigado a parar e farmar.

| Fase | Chega com | Exige | Parede | Custo |
|---|---|---|---|---|
| act1Stage1 | 1 | 1 | não | - |
| act1Stage2 | 2 | 3 | sim | 4x a fase anterior, 1.5 min de combate |
| act1Stage3 | 3 | 5 | sim | 12x a fase anterior, 7.7 min de combate |
| act1Stage4 | 5 | 4 | não | - |

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

### act1Stage1 (recomendado 1 a 1)

| Nível | Limparam | Testadas | % |
|---|---|---|---|
| 1 | 6 | 6 | 100% |
| 2 | 6 | 6 | 100% |
| 3 | 6 | 6 | 100% |

Por build, no nível 1:

| Build | Limpou | Tentativas |
|---|---|---|
| a da ficha | 6 | 6 |

### act1Stage2 (recomendado 2 a 3)

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

### act1Stage3 (recomendado 4 a 5)

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

### act1Stage4 (recomendado 5 a 6)

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

