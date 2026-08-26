# Ferramentas do editor

## Objetivo

Descrever o que existe no menu `Her-o-clock` da Unity, para que serve cada coisa e quando usar. Nada aqui roda no jogo: são todas ferramentas de quem desenvolve, e vivem em `Assets/Scripts/Editor/`.

## `Her-o-clock` → `Character sheets`

Uma janela com **todas as fichas do projeto lado a lado, no mesmo nível**.

### Por que ela existe

Já havia dois lugares para olhar número de ficha, e nenhum respondia a esta pergunta:

- O **Inspector de cada ficha** responde "o que esta ficha produz", uma de cada vez.
- O **`.claude/balance/snapshot.md`** responde "o que esta alteração fez com o resto do jogo", mas só no nível inicial de cada ficha e só depois de commitado.

Falta comparar duas fichas **no nível em que elas se encontram de verdade**. Um lacaio é escrito no nível que a fase entrega, e um herói chega no nível que o jogador trouxe — pôr os dois no mesmo nível é uma escolha de quem está olhando, e isso é um controle deslizante, não um arquivo.

### O que ela mostra

- Um controle de **nível** e um de **multiplicador de fase**, iguais aos do Inspector de ficha.
- Uma tabela por tipo de personagem — heróis, lacaios, vilões e NPCs — com os quatro atributos, vida, dano, ataques por segundo, dano por segundo, casas por segundo, evasão, armadura e mitigação.
- A mitigação é lida **contra um atacante do mesmo nível**, que é o número que o crescimento da defesa existe para manter parado. Contra um atacante fixo ela pareceria despencar sem estar despencando.
- Clicar no nome seleciona a ficha no Project, então a janela também é um caminho para dentro delas.

### As habilidades, e por que elas estão aqui

Cada ficha lista as habilidades disponíveis naquele nível, com o rank, o dano e — o principal — **o alcance escrito por extenso**.

Uma área é centrada em quem usa e pega todo mundo dentro dela, aliado incluído, exatamente como o `abilities.md` manda. Essa regra é fácil de concordar no abstrato e fácil de levar susto no tabuleiro, e **não existia lugar nenhum no editor que a mostrasse**. Por isso a janela escreve `CATCHES ALLIES` em toda área, e avisa que linha pega quem estiver no caminho.

Foi assim que se descobriu que o Gadrat tirava um terço da vida da Tempo por caste sem ninguém ter errado nada.

### O que ela não faz

- **Não escreve nada.** Tudo é lido pelas mesmas propriedades que o jogo lê, então não pode divergir do que acontece de verdade.
- **Não considera itens**, que ainda não existem.
- **Não substitui o snapshot.** O snapshot é retrato commitado e conferível por diff; a janela é exploração ao vivo. Os dois respondem perguntas diferentes.

## `Her-o-clock` → `Link character sprites`

Liga os desenhos de cada personagem à ficha dele, por nome de arquivo, e conserta importação quebrada no caminho. Descrita em `arte-de-personagem.md`.

## Inspector de `CharacterDefinition`

Não fica no menu: aparece sozinho ao selecionar uma ficha. Mostra o que aquela ficha produz num nível escolhido, e é o irmão de uma ficha só da janela acima.
