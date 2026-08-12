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

## 0.4.0.0 (próxima)

- Progressão.
- Experiência, nível e distribuição de pontos de atributo.

## 0.5.0.0

- Persistência.
- Save e load. É pré-requisito da progressão offline e entra antes de existir muito dado para migrar depois.

## 0.6.0.0

- Habilidades.
- Recarga, área, provocação e reposicionamento.

## 0.7.0.0

- Items.
- Ataques básicos agora variam de acordo com o item.
- Uma mesma seed ainda define como a batalha vai ocorrer.

# Ordem escolhida

- Cada versão depende apenas das anteriores.
- As habilidades ficam por último de propósito, pois são o sistema que mais mexe em todos os outros. Fazer habilidade antes do combate estar estável significa refazer habilidade.
- Enquanto toda a funcionalidade básica não estiver pronta, o jogo continua em game objects lisos e coloridos.
