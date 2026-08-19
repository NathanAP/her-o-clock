# Fases

## Objetivo

Detalhar o que cada fase contém para ser transformado em `json` na pasta `.claude/specs/stages/`.

## Como ler uma fase

Cada fase é descrita em linhas curtas, e duas delas já foram lidas errado. O que cada uma significa:

- **Característica do background** — o cenário de fundo daquela fase. É arte, não é o nome dela.
- **Limite de heróis** — quantos heróis o jogador leva para esta fase. É uma regra da fase e vale sempre, inclusive ao repetí-la em uma passada posterior com a equipe cheia. A ordem da equipe que decide quem vai para a fase.
- **Ondas** — quantas ondas a fase tem no total, **contando a onda do vilão**, e quantos lacaios cada uma traz.
- **Nível recomendado** — o nível recomendado para que os heróis passem daquela fase.
    - Note que aqui pode haver uma grande variação de dificuldade (combos, itens, habilidades, RNG, etc) - inclusive um jogador acima do nível recomendado pode ter dificuldades numa fase por conta de uma build de menos sinergia.
    - A ideia desse trecho é entender mais ou menos onde estão os pontos de pico de dificuldade e momentos que um grind precisa ocorrer ou onde o jogador vai travar. Entenda que fases de nível mais alto que a anterior servem exatamente para indicar uma parede ou um trecho de nivelamento.
    - Não tem a ver com a frase "preciso obrigatoriamente estar no nível X" para passar, mas sim com a frase "preciso me fortalecer mais pra passar disso".
    - Serve muito mais para balancear a fase do que para obrigar o jogador a atingir um nível específico.
    - Dito isso: não é um dado de entrada. Quem escolhe o nível dos lacaios e do vilão de cada fase é quem está balanceando ela.
- **Onde mora a dificuldade** — o foco da dificuldade enfrentada em uma fase.
    - Sem essa linha toda fase acaba calibrada pelo mesmo lugar, que na prática é sempre a onda final: ela é de longe o que mais move o nível exigido.
    - Pico no vilão:
        - Dificuldade graduada.
        - Mais fácil de calibrar pois o vilão é uma única entidade e só possui uma ficha.
        - Também torna possível mexer em quem o acompanha na última onda.
    - Pico no desgaste:
        - Dificuldade complexa.
        - Bem mais difícil de calibrar pois o dano acaba atravessando a fase inteira e não há cura entre ondas.
        - Provavelmente é necessário ter mais de uma instância do mesmo inimigo diversas vezes para ter uma calibragem mais ampla.
        - Mexer em ondas é mais complicado porque envolve cada vez mais aleatoriedades.
    - Equilibrado:
        - Dificuldade constante.
        - Significa que a fase como um todo traz desafio ao jogador.
        - Não quer dizer que a fase é muito fácil ou difícil, quer dizer que o desafio trazido é equilibrado.
- **O que é liberado ao completar a fase pela primeira vez** — novo conteúdo que aquela fase libera. Só acontece na primeira vez; repetir a fase não libera nada de novo.
    - Um slot liberado e um herói liberado são coisas **diferentes**. O slot é quantas posições a equipe tem; o herói é quem existe para ser escalado nelas.

## Ato 1

- Este ato contém 10 fases.
- O ato todo se passa dentro da cidade de Halo. Os cenários variam entre ruas, praças e parques da cidade.

### Fase 1

- Característica do background: nascer do sol.
- Limite de heróis: 1.
- Nível recomendado: 1.
- Onde mora a dificuldade: equilibrado.
- 5 ondas totais contendo entre 1 e 2 `Discarded Prototype`.
- A última onda contém um único vilão `Exposed Prototype`.
- Novo registro de diário é resgatado ao completar a fase pela primeira vez.

### Fase 2

- Característica do background: manhã ensolarada.
- Limite de heróis: 1.
- Nível recomendado: 2 a 3.
- Onde mora a dificuldade: equilibrado.
- 6 ondas totais contendo entre 1 e 2 `Discarded Prototype`.
- A última onda contém um único vilão `Exposed Prototype` junto com 2 `Discarded Prototype`.

### Fase 3

- Característica do background: tarde quente.
- Limite de heróis: 1.
- Nível recomendado: 4 a 5.
- Onde mora a dificuldade: pico no desgaste.
- 8 ondas totais contendo entre 2 e 3 `Discarded Prototype`.
- A última onda contém um único vilão `Exposed Prototype` entrando com vida reduzida (50%) e nível abaixo do normal (2).
- NPC `Gadrat` auxilia nessa batalha.
- Novo registro de diário é resgatado ao completar a fase pela primeira vez.
- Ao final dessa fase pela primeira vez, Gadrat entra para o grupo de heróis.
- Ao final dessa fase pela primeira vez, o segundo slot no grupo de heróis é liberado.

### Fase 4

- Característica do background: fim de tarde.
- Limite de heróis: 2.
- Nível recomendado: 5 a 6.
- Onde mora a dificuldade: pico no vilão.
- 6 ondas totais contendo entre 2 e 3 `Discarded Prototype`.
- A última onda contém um vilão `Exposed Prototype` junto com 2 `Discarded Prototype`.

### Fase 5

- Característica do background: noite com nuvens densas.
- Limite de heróis: 2.
- Nível recomendado: 5 a 6.
- Onde mora a dificuldade: pico no desgaste.
- 7 ondas totais contendo entre 2 e 3 `Pickpocket Bandit`.
- A última onda contém um único vilão `Robber Leader`.

### Fase 6

- Característica do background: madrugada com relâmpagos.
- Limite de heróis: 2.
- Nível recomendado: 6 a 7.
- Onde mora a dificuldade: equilibrado.
- 8 ondas totais contendo entre 3 e 4 `Pickpocket Bandit`.
- A última onda contém um único vilão `Robber Leader`.

### Fase 7

- Característica do background: madrugada com chuva.
- Limite de heróis: 2.
- Nível recomendado: 9 a 10.
- Onde mora a dificuldade: pico no vilão.
- 10 ondas totais cotendo entre 3 e 4 `Pickpocket Bandit`.
- A última onda contém três vilões `Robber Leader`.
- NPC `Gadrat's Father` auxilia nessa batalha entrando com a vida atual e máxima em 10. Ele sofre 100 de dano após 2 segundos e morre 100% das vezes.
- Novo registro de diário é resgatado ao completar a fase pela primeira vez.

### Fase 8

- Característica do background: manhã escura.
- Limite de heróis: 2.
- Nível recomendado: 10 a 11.
- Onde mora a dificuldade: pico no vilão.
- 8 ondas totais contendo entre 2 e 3 `Pickpocket Bandit` e 1 `Armed Bandit`.
- A última onda contém dois vilões `Robber Leader` junto com 1 `Pickpocked Bandit` e 1 `Armed Bandit`.

### Fase 9

- Característica do background: manhã chuvosa.
- Limite de heróis: 2.
- Nível recomendado: 12 a 14.
- Onde mora a dificuldade: pico no vilão.
- 8 ondas totais contendo 2 e 3 `Pickpocket Bandit` e 1 e 2 `Armed Bandit`.
- A última onda contém dois vilões `Robber Leader` junto com 2 `Pickpocked Bandit` e 1 `Armed Bandit`.

### Fase 10

- Característica do background: temporal.
- Limite de heróis: 2.
- Nível recomendado: 15 a 16.
- Onde mora a dificuldade: pico no desgaste.
- 12 ondas totais contendo 3 e 4 `Pickpocket Bandit` e 2 e 3 `Armed Bandit`.
- A última onda contém o vilão `Hader The Danger` junto com 2 `Pickpocked Bandit` e 2 `Armed Bandit`.
- Novo registro de diário é resgatado ao completar a fase pela primeira vez.
- Ato 2 é liberado ao completar a fase pela primeira vez.
