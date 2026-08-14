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

| Ficha | Vida | Dano fis. | Atq/s | Casas/s | Evasão | Mit. vs nv1 | Mit. vs nv12 | Mit. vs nv50 |
|---|---|---|---|---|---|---|---|---|
| Tanque top | 65 | 5 | 1.00 | 2.02 | 0.4% | 50.0% | 10.7% | 2.9% |
| Arqueiro foda | 40 | 4 | 1.05 | 2.10 | 4.8% | 12.5% | 1.2% | 0.3% |
| Motorista | 40 | 2 | 1.01 | 2.02 | 1.0% | 0.0% | 0.0% | 0.0% |
| Carlos | 55 | 3 | 1.00 | 2.02 | 0.4% | 56.3% | 15.0% | 4.2% |

## Fases

A formação de heróis é a do asset. O tempo é só de combate, sem as transições.
"Nível mínimo" é o menor nível de herói testado que limpa a fase.

| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |
|---|---|---|---|---|---|
| Avenida das Turbinas | 1 | 1 | 26.2 s | 210 | 21 |
| Subestacao Norte | 12 | 20 | 46.6 s | 34560 | 24 |

