# Estrutura de arquivos

## Objetivo

Utilize este arquivo para visualizar e manter a estrutura geral do projeto organizada e coerente. Você tem total liberdade para alterar este arquivo conforme o projeto evolui. Você não precisa colocar aqui cada um dos arquivos ou subpastas presentes no projeto, apenas as principais.

## Estrutura de pastas e arquivos do projeto

- .claude (arquivos de especificação do Claude)
    - game-objects (descrição de cada GameObject da Unity)
    - memory (resumo geral do projeto que guia as próximas sessões)
    - specs (arquivos de specs do projeto)
        - characters (arquivos de informações sobre cada personagem)
        - general (arquivos com detalhes gerais do projeto)
        - items (números dos itens, separados por assunto: slots, subtipos, modificadores, camadas e um arquivo por item único)
        - stages (arquivos de informações sobre cada fase)
    - versions (resumo do que foi feito em cada versão)
- Assets (tudo que a Unity importa)
    - Art (sprites, animações e materiais)
    - Audio (músicas e efeitos sonoros)
    - Prefabs (prefabs de personagens, cenário e interface)
    - Scenes (cenas do jogo)
    - Stages (arquivos .json das fases)
    - Strings (arquivos .json com todo texto que o jogador lê, um por idioma)
    - ScriptableObjects (dados de personagens, itens, habilidades e fases)
    - Tests (testes automatizados, com um assembly para EditMode e outro para PlayMode)
    - Scripts (código C# do jogo)
    - Settings (configurações de render pipeline e input)
- Packages (dependências da Unity)
- ProjectSettings (configurações do projeto Unity)

## O que não fica na pasta do projeto

- O **save** do jogador mora na pasta de dados persistentes que o sistema operacional oferece ao jogo, e nunca aqui dentro. Uma pasta do projeto viaja no git e é apagada por uma reinstalação, então nenhuma das duas serve para guardar progresso. O formato e a organização dos arquivos estão em `save.md`.
- As **preferências** de tela ficam nas preferências do jogador oferecidas pelo motor, conforme `window.md`.
