# Versions

## Objetivo

Esta pasta contém um resumo geral do que foi feito em cada versão do jogo.

## Convenção

- Um arquivo por versão publicada.
- Utilize a nomenclatura `timestamp_versao.md`.
- Cada arquivo deve deixar claro:
    - Qual era o objetivo daquela versão.
    - O que foi alterado, criado ou removido.
    - O que ficou pendente para versões futuras.

## Esta pasta não acompanha os commits, e não deve tentar

O número de um commit e o número de um documento daqui **não se correspondem**, de propósito. São duas granularidades diferentes:

- O **commit** é granularidade de trabalho. Ele marca um ponto em que a árvore estava íntegra e os testes passavam.
- O **documento** é granularidade de decisão. Ele existe quando houve alguma coisa a explicar: uma regra nova, um motivo, um caminho recusado.

Por isso os dois divergem nos dois sentidos:

- Um commit pode conter mais de um documento, quando o trabalho foi feito de uma vez só mas envolveu decisões separáveis.
- Um commit pode não ter documento nenhum, como um ajuste de uma linha no roadmap. Forçar um arquivo para ele geraria um documento sem conteúdo, que é pior do que não ter.

**Não tente casar os dois depois.** Se a dúvida for "o que mudou naquele dia", quem responde é o `git log`. Se for "por que isso é assim", quem responde é esta pasta.

O que precisa continuar verdadeiro é só isto: **toda decisão tem documento**. Um commit sem documento é normal; uma decisão sem documento é o que não pode acontecer.
