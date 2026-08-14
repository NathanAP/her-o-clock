# Objetivo

- Mapear o roadmap do projeto de forma que fique claro o que foi feito recentemente e o que vai ser feito no futuro. Este arquivo está diretamente ligado às pastas:
    - `.claude/memory/`, pois a memória desta pasta pode conter detalhes necessários nas versões futuras.
    - `.claude/specs/`, pois os principais detalhes do projeto estão descritos nesta pasta.
    - `.claude/versions/`, pois versões passadas estarão resumidas nesta pasta.
- Você tem liberdade em alterar este arquivo conforme achar necessário.

# Atual estado do projeto

Estamos em uma versão TOTALMENTE inicial. Muitos detalhes presentes na pasta `.claude/specs/` não estão 100% definidos, bem organizados e até mesmo bem descritos. A ideia atual é montar uma casca de código e organização no Unity para chegarmos aos poucos no que está descrito naquela pasta.

Até que toda a funcionalidade básica esteja pronta, vamos fazer o jogo funcionar em game objects lisos e sem nenhuma textura. Utilize cores para diferenciar personagens e cenário, não tem problema ficar algo feio agora. Vamos focar na jogabilidade e funcionalidade.

Utilize tons de azul para heróis; tons de vermelho para vilões; tons de rosa para lacaios; tons de verde para cenário.

# Versões

## 0.1.0.0 (feita)

- Base de código do projeto e a primeira coisa acontecendo na tela.
- Tabuleiro visível, personagens coloridos posicionados nele, e eles andando até o inimigo e parando no alcance.
- Sem combate: nada de dano, morte ou habilidade.
- O resumo completo está em `.claude/versions/20260811_0.1.0.0.md`.

## 0.2.0.0 (feita)

- Combate.
- Ataque básico, velocidade de ataque, mitigação, evasão e morte.
- Barra de vida, piscada ao levar dano e números de dano flutuantes.
- Enquanto não existem fases, a batalha reinicia sozinha para permitir observar o balanceamento.
- O resumo completo está em `.claude/versions/20260811_0.2.0.0.md`.

## 0.2.1.0 (feita)

- Padronização de idioma no código.
- Scripts, comentários, documentação em XML, textos do Inspector e mensagens de Console em inglês, junto com a pasta `.claude/memory/`.
- As specs, as versões e os game-objects continuam em português.
- O resumo completo está em `.claude/versions/20260811_0.2.1.0.md`.

## 0.3.0.0 (feita)

- Fase.
- Grupos de lacaios em sequência, vilão no fim, vitória e derrota, reinício automático.
- As fases são arquivos `.json`, escritos fora da Unity e validados antes de rodar. As fichas de personagem continuam ScriptableObject.
- Os heróis não recuperam vida entre ondas, o que cria a necessidade de farmar.
- O chão rola entre as ondas, representando o grupo avançando pela cidade.
- Ficha e instância passaram a ser coisas separadas, preparando a progressão.
- O resumo completo está em `.claude/versions/20260811_0.3.0.0.md`.

## 0.3.1.0 (feita)

- Correção do reposicionamento.

## 0.3.2.0 (feita)

- Controle de velocidade para desenvolvimento, de 0.25x a 8x.
- A taxa de quadros sobe junto com a velocidade, então acelerar não distorce a simulação.
- Não existe na build final.
- O resumo completo está em `.claude/versions/20260812_0.3.2.0.md`.

## 0.4.0.0 (feita)

- Progressão.
- Experiência, nível e distribuição de pontos de atributo.
- A vida atual não sobe ao subir de nível, apenas a máxima.
- O `enemyLevel` da fase passou a realmente deixar o inimigo mais forte, e ganhou um multiplicador de ajuste fino por inimigo.
- Todo cálculo derivado passa por `TotalOf`, onde itens, árvores e buffs vão entrar depois sem mexer em mais nada.
- O resumo completo está em `.claude/versions/20260812_0.4.0.0.md`.

## 0.5.0.0 (feita)

- Revisão geral do projeto.
- Nenhuma correção: a versão lê tudo, registra os problemas e transforma cada um em uma versão de correção com ordem definida.
- Os três achados críticos foram a suíte de testes que nunca existiu, o combate que depende da taxa de quadros, e a armadura que não escala com nada.
- O resumo completo, com os 14 achados e a evidência de cada um, está em `.claude/versions/20260813_0.5.0.0.md`.

## 0.5.1.0 (feita)

- Passo fixo no combate. A simulação anda sempre 1/60 de segundo, nunca o delta do quadro.
- O `StageRunner` virou o único relógio do jogo, e o `BattleDirector` perdeu o `Update` em troca de um `Tick(float)` público. Uma fase inteira agora roda sem cena e sem renderizar, que é o que a 0.5.2.0 precisa.
- A sobra de tempo passou a ser carregada em vez de descartada, no ataque e no movimento. Era isso que fazia a velocidade real ficar até 6.25% abaixo da ficha, punindo justamente quem investe em AGI.
- A ordem de resolução alterna a cada passo, então os heróis pararam de vencer todo empate exato.
- O `DevSpeedControl` deixou de depender da taxa de quadros para estar correto.
- O resumo completo está em `.claude/versions/20260813_0.5.1.0.md`.

## 0.5.1.1 (feita)

- Documentação de testes fechada antes de escrever o primeiro teste, já que a 0.5.2.0 vai ser executada contra o que o `CLAUDE.md` disser.
- Entraram a regra de que o valor esperado sai da spec e não do código, e a decisão de onde cada número mora: entrada na spec e repetida no teste, saída só no snapshot e nunca em prosa.
- O resumo completo está em `.claude/versions/20260813_0.5.1.1.md`.

## 0.5.2.0 (próxima)

- Suíte de testes. É o pagamento da dívida deixada até a 0.4.0.6, e vem antes de qualquer mudança de regra para que as mudanças seguintes tenham como ser verificadas.
- **Pré-requisito:** o código do jogo precisa ganhar um `Assets/Scripts/HerOClock.asmdef`. Hoje ele mora em `Assembly-CSharp`, e um assembly criado por `.asmdef` não consegue referenciar `Assembly-CSharp` — a dependência só existe no sentido contrário. Como o Test Framework exige `.asmdef` em todo assembly de teste, sem esse passo nenhum teste enxerga o jogo.
- Estrutura: `Assets/Tests/EditMode` com asmdef próprio referenciando o do jogo.
- As categorias, em ordem de valor:
    - **A spec como teste executável.** Os exemplos numéricos literais de `attributes.md` (evasão por classe de equipamento, mitigação por nível do atacante, as reduções multiplicando) e a tabela de horas de `progress.md` viram assertivas. Este é o grupo mais importante: ele não confere que o código faz o que o código faz, confere que o código faz o que a spec prometeu.
    - **Fórmulas puras.** `DamageCalculator`, `ExperienceTable`, `LevelProgress`, `AttributeGrowth`, `GridPosition`, `BattleRandom` e `CharacterStats`.
    - **Conteúdo.** Todo `.json` de `Assets/Stages/` passado pelo `StageValidator`, e todo `CharacterDefinition` do banco conferido (id único e não vazio, crescimento somando 100, vida máxima acima de zero).
    - **Simulação.** Uma batalha inteira e depois uma fase inteira, em memória, com semente fixa e sem renderizar. É o equivalente a um teste ponta a ponta neste jogo, já que não existe input do jogador.
    - **Determinismo.** A mesma batalha rodada duas vezes com a mesma semente dá resultado idêntico, e continua idêntica quando alimentada com `deltaTime` irregular. É o teste que prova a 0.5.1.0 e impede o problema de voltar.
    - **Caracterização de balanceamento.** A `act1-stage1` é vencível no nível 1, a `act1-stage2` não é, e uma sessão rende inimigos por minuto dentro de uma faixa. Não afirmam certo e errado, afirmam que o balanceamento não mudou sem querer.
- O primeiro `.claude/balance/snapshot.md`, gerado por teste, conforme "## Onde cada número mora" no `CLAUDE.md`. O conteúdo inicial: horas até os níveis 10, 25, 50, 75 e 100; por ficha, vida, dano, ataques por segundo, evasão e mitigação contra atacantes de nível 1, 12 e 50; por fase, em que nível ela vira vencível, quanto tempo leva e quanto paga; e inimigos por minuto.
- A tabela de cadência medida à mão na 0.5.1.0 vira assertiva aqui, junto com o teste de determinismo. É a dívida que aquela versão deixou registrada.
- Um único smoke test em PlayMode: entra em Play, o bootstrap monta a cena, Console limpo. Cobre referência faltando e sorting layer renomeada, que são os erros que a Unity não reporta.

## 0.5.3.0

- Escala de defesa e regeneração de vida.
- Armadura e resistências são valores fixos da ficha: não passam por `TotalOf`, não crescem com o nível e não recebem o multiplicador da fase, enquanto a constante da curva cresce com o nível do atacante.
- Precisa decidir de onde vem a defesa de um inimigo que só existe como ficha mais nível, antes de implementar.
- A regeneração de vida entra junto porque é o mesmo buraco: `attributes.md` a define, o jogo não a tem, e `gameplay.md` usa a existência dela para justificar o desgaste entre ondas.

## 0.5.4.0

- Pontos de atributo dos heróis.
- Hoje o gasto automático pela `Growth` da ficha e o acúmulo de `UnspentAttributePoints` coexistem, então os pontos são contados duas vezes.
- Decidir se a `Growth` é só o padrão dos inimigos ou também a distribuição inicial dos heróis, e o que acontece quando o jogador redistribuir.

## 0.5.5.0

- Conteúdo em inglês e arquivo de strings.
- Os `Display Name` dos assets e os textos das fases estão em português; o `CLAUDE.md` pede inglês.
- Os textos visíveis saem dos assets e vão para um arquivo de strings, preparando localização.
- Vem depois do código estabilizar, pois mexe em asset e em fase.

## 0.5.6.0

- Sincronizar a documentação e limpar o resto.
- `.claude/specs/`, `.claude/game-objects/` e `.claude/memory/` conferidos linha a linha contra o projeto de verdade.
- A ficha de fase e as três fichas de personagem da spec precisam virar o formato que o jogo realmente lê.
- Leva junto os itens menores: área dos inimigos no validador, leitura direta da ficha no bootstrap, roubo de vida descartado nos espinhos, e um pool para os números de dano.
- Vem por último porque as cinco versões anteriores mudam o que precisa ser documentado.

## 0.6.0.0

- Persistência.
- Save e load. É pré-requisito da progressão offline e entra antes de existir muito dado para migrar depois.
- Testes: ida e volta (salvar, carregar, estado idêntico), save de versão antiga carregando na versão nova, e os baldes de 10 minutos de `progress.md` com os três tetos da progressão offline. A ida e volta é uma categoria que só aparece nesta versão e é a que impede corromper o progresso de quem já joga.

## 0.7.0.0

- Habilidades.
- Recarga, área, provocação e reposicionamento.
- Testes: é a maior superfície de regra do jogo inteiro, e quase tudo já está escrito como exemplo em `gameplay.md` e `characters.md`. A pontuação de posicionamento em área (o exemplo de 3 aliados e 2 inimigos pontuando 1), os formatos `chain`, `line` e `area`, os efeitos resolvidos na ordem em que aparecem, a habilidade pronta que segura a carga em vez de ser usada no vazio, e a provocação sobrescrevendo a cadeia de alvo.
- Junto entra um validador de ficha, pois `characters.md` já avisa que um `array` menor que o `ranks` faria o último nível ler um valor inexistente **sem dar erro nenhum**. Uma spec que nomeia o próprio problema silencioso está pedindo um teste.

## 0.8.0.0

- Projéteis.
- Podemos fazer primeiro uns laserzinhos simples e coloridos, apenas para ver a coisa acontecer.
- Testes: só se o projétil tiver tempo de voo capaz de mudar quando o dano é aplicado. Se for puramente visual, não precisa de nenhum.

## 0.9.0.0

- Lore do ato 1.
- Equipe de 2 heróis.
    - A ideia é terminar o ato 1 com 3 heróis.
- 5 fases.
- 5 vilões.
- 12 lacaios.
- Instâncias de todo mundo organizada na Unity.
- Testes: nenhuma regra nova, mas muito conteúdo novo. Os testes de conteúdo da 0.5.2.0 passam a valer para as 5 fases e os 17 personagens, e cada fase ganha um teste de caracterização dizendo em que nível ela deveria ser vencível.

## 0.10.0.0

- Items.
- Ataques básicos agora variam de acordo com o item.
- Uma mesma seed ainda define como a batalha vai ocorrer.
- Testes: o item entrando como nova fonte dentro do `TotalOf` sem que nada fora dele mude, e o teste de determinismo rodado de novo, já que o item passa a alterar o ataque básico. A própria linha "uma mesma seed ainda define como a batalha vai ocorrer" é uma assertiva.

## 0.11.0.0

- Menus.
- Testes: praticamente nenhum. Interface é a única parte do jogo em que o custo de testar não se paga.

## 0.12.0.0

- Inventário.
- Baús.
- Testes: as regras de espaço e de empilhamento, que são aritmética e não interface.

## 0.13.0.0

- Mapa dos atos.

## 0.14.0.0

- Árvore de progressão.
- Dinheiro.
- Testes: a curva de custo dos nós, que por `progress.md` depende de quantos nós já foram comprados e não de qual nó é. E a projeção de espera de cada trecho da árvore, pelo mesmo motivo da tabela de horas: é um número publicado na spec.

## 0.15.0.0

- Classes.
- Testes: a classe entrando como mais uma fonte no `TotalOf`.

## 0.16.0.0

- Árvore de habilidades
- Testes: superfície de regra grande de novo, e ela multiplica com as habilidades da 0.7.0.0. É a versão em que a suíte existente mais paga o próprio custo.

# Ordem escolhida

- Cada versão depende apenas das anteriores.
- As habilidades ficam por último de propósito, pois são o sistema que mais mexe em todos os outros. Fazer habilidade antes do combate estar estável significa refazer habilidade.
- Enquanto toda a funcionalidade básica não estiver pronta, o jogo continua em game objects lisos e coloridos.
- O bloco 0.5.x é a revisão sendo aplicada, e a ordem dele é por dependência e não por gravidade: primeiro o passo fixo, que torna o combate verificável; depois os testes, que tornam as mudanças seguintes verificáveis; só então as mudanças de regra.
- A 0.5.2.0 é a única versão que existe só para testar, e é a última vez que isso acontece. Ela paga a dívida acumulada até a 0.4.0.6. Dali em diante o teste faz parte da versão que muda o comportamento, conforme "# Testes automatizados" no `CLAUDE.md`.
