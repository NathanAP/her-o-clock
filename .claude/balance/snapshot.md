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

| Ficha | Vida | Dano fis. | Atq/s | Casas/s | Evasão | Armadura | Regen/s |
|---|---|---|---|---|---|---|---|
| hero-tank | 65 | 5 | 1.00 | 2.02 | 0.4% | 100 | 0.00 |
| hero-archer | 40 | 4 | 1.05 | 2.10 | 4.8% | 10 | 0.00 |
| minion-standard | 40 | 2 | 1.01 | 2.02 | 1.0% | 0 | 0.00 |
| villain-boss | 55 | 3 | 1.00 | 2.02 | 0.4% | 150 | 0.00 |

### Mitigação física contra um atacante do mesmo nível

Uma linha parada significa que a armadura acompanha a curva. Uma linha que cai
significa que aquela ficha perde defesa conforme o jogo avança.

| Ficha | Nível 1 | Nível 12 | Nível 30 | Nível 50 | Nível 100 |
|---|---|---|---|---|---|
| hero-tank | 50.0% | 50.0% | 50.0% | 50.0% | 50.0% |
| hero-archer | 12.5% | 12.5% | 12.5% | 12.5% | 12.5% |
| minion-standard | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| villain-boss | 56.3% | 56.3% | 56.3% | 56.3% | 56.3% |

## Fases

A formação de heróis é a do asset. O tempo é só de combate, sem as transições.
"Nível mínimo" é o menor nível de herói que limpa a fase.

| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |
|---|---|---|---|---|---|
| act1-stage1 | 1 | 1 | 26.2 s | 210 | 21 |
| act1-stage2 | 12 | 15 | 64.5 s | 34560 | 24 |

### Paredes

Jogando as fases em ordem e limpando cada uma uma vez. "Chega com" é o nível
que o time tem ao encostar na fase; "exige" é o nível que ela pede. Quando o
exigido passa o de chegada, o jogador é obrigado a parar e farmar.

| Fase | Chega com | Exige | Parede | Custo |
|---|---|---|---|---|
| act1-stage1 | 1 | 1 | não | - |
| act1-stage2 | 2 | 15 | sim | 10037x a fase anterior, 3986.9 min de combate |

O custo conta só o tempo de combate, e despreza a experiência parcial que sobra
de um nível para o outro. As transições entre ondas somam vários segundos por
repetição, então o tempo real de relógio é maior que o mostrado.

**As fases que existem hoje são casos de teste, não conteúdo.** A segunda tem
inimigos de nível 12 de propósito, para exercitar herói caindo em combate e avanço
bloqueado. A parede gigante entre as duas é o resultado esperado desse par, e não
um problema de balanceamento. Ver `game-objects/fases.md`.

