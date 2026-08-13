# Sobre este projeto

Este projeto é um jogo para PC feito inteiramente em Unity e C#.

# Sobre Her-o-clock

Her-o-clock é um jogo de RPG idle minúsculo que leva você ao incrível mundo tecnológico de Unia! Aqui, pessoas vivem suas vidas normalmente mas vilões tramam seus planos de dominação da tecnologia mundial. Para evitar o pior o governo criou heróis robôs para defender sua população.

## Características de Her-o-clock

- Controle total através do mouse.
- Jogabilidade idle.
- Visual pixel art.
- Pequenas sessões ativas, grandes sessões enquanto ocioso.
- Árvore de habilidades extensa.
- Sistema de classes e subclasses.
- Auto-scroller.
- Farm de itens
- Universo incrível.
- Personagens cativantes.
- Itens poderosos.
- Sem propagandas.
- Sem loja externa.
- Sem pay-to-win.

# Stack do projeto

- Unity 2D
- C#

# Regras de desenvolvimento

Utilize estas regras ao desenvolver scripts neste projeto:

- Mantenha o código organizado o tempo inteiro.
- Tudo deve estar em inglês, desde variáveis até comentários, labels, tooltips, headers, etc.
- Você deve seguir as convenções tradicionais de desenvolvimento de jogos durante suas ações.
    - Inclusive, você tem liberdade em contestar minhas decisões se elas forem contra as convenções tradicionais de desenvolvimento de jogos.
    - Lembre-se que você é responsável também pelo projeto, não tem motivos para ele ficar conceitualmente errado.
- Dê prioridade em manter vários arquivos pequenos e simples ao invés de poucos arquivos longos e complexos.
- A pasta `.claude/specs/` é sua melhor amiga. Todos os detalhes presentes nos arquivos são mandatórios e devem ser seguidos.
    - Dito isso, você tem liberdade total para contestar decisões presentes na pasta ou indicar problemas claros.

# Regras de alteração de arquivos de especificação

Utilize essas regras ao alterar arquivos de especificação neste projeto:

- Matenha a leitura coerente.
- Siga os padrões e convenções.
- Mantenha o idioma.
- Seja claro.

# Fluxo de desenvolvimento

Sempre siga esse fluxo ao desenvolver versões no projeto:

1 - Leia atentamente os arquivos de especificação conforme necessidade.
2 - Entenda qual é o objetivo da atual alteração através do arquivo `.claude/roadmap.md`.
3 - Caso seja necessário, garanta que você entendeu totalmente seu objetivo. Você pode até fazer uma entrevista comigo ou desenhar esboços para confirmar objetivos.
4 - Faça um planejamento para alcançar seu objetivo. Me avise se eu preciso fechar a Unity ou fazer alguma operação extra antes de começar.
5 - Mostre seu planejamento e aguarde aprovação.
6 - Faça suas alterações.
7 - Garanta que tudo esteja funcionando corretamente.
8 - Crie um resumo do que foi feito na pasta `.claude/versions/` seguindo a convenção descrita em "## Pasta versions".
9 - Retorne um resumo do que foi feito.
10 - Atualize os arquivos em `.claude/game-objects/`.
11 - Atualize os arquivos em `.claude/memory/`.
12 - Atualize o arquivo `.claude/roadmap.md`.

# Recomendações

- Você tem total liberdade e incentivo extra para dar recomendações em cima de ideias que possam ser problemáticas imediatamente ou futuramente.

# Bugs

- Erros de gramática na documentação ou no jogo são considerados bugs.
    - Neste jogo, robôs são todos não-binários. Isso significa que, ao falar seus pronomes, em inglês precisa ficar sempre em "they/their" ao invés de "he/him" ou "she/her".
    - Para o português, não há pronomes neutros, então vamos manter em "ele/deles" ou "o robô/os robôs".
- Detalhes mal escritos ou definidos em `.claude/specs/` são considerados bugs.

# Refatorações

- Refatorações são consideradas normais e necessárias.
- Caso uma refatoração seja necessária, você deve explicar os motivos e dizer o que precisa ser feito.
- Não tenha medo em fazer uma refatoração quando necessário, é melhor do que fazer uma gambiarra ou um workaround muito extenso.

# Revisão

- Quando em modo de revisão, você deve ser o mais crítico possível, mesmo sabendo que há pontos cegos no seu critério de avaliação.
- O modelo mínimo para operações de revisão é o Opus 4.8.
- Siga o mesmo fluxo de desenvolvimento quando for aplicar um desenvolvimento de revisão.
- Tudo faz parte da revisão, inclusive a pasta `.claude/game-objects` e `.claude/memory/`.

# Pasta game-objects

- A pasta `.claude/game-objects/` serve para você descrever o que é cada GameObject da Unity.
- Você tem liberdade total sobre os arquivos, mas descreva-os de forma que fique claro o que cada um contém, valores recomendados e associações necessárias para que o projeto rode normalmente.

# Versionamento

O versionamento deve ocorrer sempre seguindo estas regras. Para segui-las, considere a seguinte versão: `11.22.33.44`.

- A primeira casa (`11`) representa a versão `major`.
    - Ao subir essa versão, todas abaixo dela devem ser zeradas. Ou seja, se fossemos para a versão `12`, a versão final seria `12.0.0.0`.
- A segunda casa (`22`) representa a versão `minor`.
    - Ao subir essa versão, todas abaixo dela devem ser zeradas enquanto as acima permanecem a mesma. Ou seja, se fossemos para a versão `23`, a versão final seria `11.23.0.0`.
- A terceira casa (`33`) representa a versão `bugfix`.
    - Ao subir essa versão, todas abaixo dela devem ser zeradas enquanto as acima permanecem a mesma. Ou seja, se fossemos para a versão `34`, a versão final seria `11.22.34.0`.
- A quarta casa (`44`) representa a versão `docs`.
    - Ao subir essa versão, nenhuma das outras devem ser alteradas. Ou seja, se fossemos para a versão `45`, a versão final seria `11.22.33.45`.

## Pasta versions

- Essa pasta contém um resumo geral do que foi feito em cada versão do jogo.
- Você tem controle total sobre ela.
- Utilize a nomenclatura `timestamp_versao.md` para melhor organização.

# Memória

- A pasta `.claude/memory/` é exclusivamente sua para manter um resumo geral do projeto. É por ali que você vai se guiar quando for chamado uma próxima vez.
- Crie resumos importante do projeto e o mantenha atualizado em casos de mudanças.
