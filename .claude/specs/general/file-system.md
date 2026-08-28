# Estrutura de arquivos

## Objetivo

Utilize este arquivo para visualizar e manter a estrutura geral do projeto organizada e coerente. Você tem total liberdade para alterar este arquivo conforme o projeto evolui. Você não precisa colocar aqui cada um dos arquivos ou subpastas presentes no projeto, apenas as principais.

## Estrutura de pastas e arquivos do projeto

- .claude (arquivos de especificação do Claude)
    - game-objects (descrição de cada GameObject da Unity)
    - memory (resumo geral do projeto que guia as próximas sessões)
    - specs (arquivos de specs do projeto)
        - characters (fichas de design de cada personagem e a arte de referência delas)
        - general (arquivos com detalhes gerais do projeto)
        - items (só o `items.md`, com as regras que os arquivos de item obedecem)
    - versions (resumo do que foi feito em cada versão)
- Assets (tudo que a Unity importa)
    - Art (sprites, animações e materiais)
    - Audio (músicas e efeitos sonoros)
    - Prefabs (prefabs de personagens, cenário e interface)
    - Scenes (cenas do jogo)
    - Items (arquivos .json dos itens: slots, subtipos, modificadores, camadas, regras de único, strings, e a pasta Uniques com um arquivo por item único)
    - Stages (arquivos .json das fases)
    - Strings (arquivos .json com todo texto que o jogador lê, um por idioma)
    - ScriptableObjects (fichas de personagem e os catálogos de fase e de item)
    - Tests (testes automatizados, com um assembly para EditMode e outro para PlayMode)
    - Scripts (código C# do jogo)
    - Settings (configurações de render pipeline e input)
- Packages (dependências da Unity)
- ProjectSettings (configurações do projeto Unity)

## Onde cada número mora

**Um número mora em exatamente um arquivo, dentro de `Assets/`.** A pasta `.claude/specs/` guarda as regras que esses arquivos obedecem — prosa, fórmulas e exemplos numéricos — e nunca uma segunda cópia dos números.

A regra e o motivo estão em "A number lives in exactly one file", em `.claude/memory/architecture.md`. A exceção que ainda existe é a ficha de personagem, que hoje vive em dois lugares e é vigiada pelo `DesignBridgeTests` enquanto isso não é resolvido.

## O que não fica na pasta do projeto

- O **save** do jogador mora na pasta de dados persistentes que o sistema operacional oferece ao jogo, e nunca aqui dentro. Uma pasta do projeto viaja no git e é apagada por uma reinstalação, então nenhuma das duas serve para guardar progresso. O formato e a organização dos arquivos estão em `save.md`.
- As **preferências** de tela ficam nas preferências do jogador oferecidas pelo motor, conforme `window.md`.
