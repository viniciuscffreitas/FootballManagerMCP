# FM27: partida e tática para o copiloto

Pergunta (issue #12): o que o Claude precisa saber de partida e tática no FM 27 para ajudar na carreira do Santos?

Data da pesquisa: 16/11/2026. O FM27 saiu em 10/11/2026. Ainda há pouca coisa testada a fundo.

Como ler este arquivo:

- *FM27* marca o que é novo. *FM26* marca o que veio do FM26 e não mudou.
- *provável* marca o que não tem confirmação em fonte forte.
- Os números de teste vêm de ferramentas da comunidade que rodam o match engine real do FM27 fora do jogo. Elas não são oficiais. Um patch do match engine estava previsto para a semana de 16/11 e pode mudar os números ([FM-Arena, ETA dos testes](https://fm-arena.com/threads/eta-for-tactic-testing.81/)).
- O FM-Arena perdeu o banco de dados antigo e ainda não publicou a tabela oficial de testes do FM27 ([mesmo tópico](https://fm-arena.com/threads/eta-for-tactic-testing.81/)).

## 1. Resumo para o copiloto (10 regras)

1. *Forma física pesa mais que tática.* Ritmo de jogo em 75% custa 0,37 ponto por jogo. Condição em 90% custa 0,28. Condição em 80% custa 0,80. Nenhuma instrução de time chegou perto disso ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)). Rodar o elenco é a decisão de jogo mais valiosa.
2. *Comece de uma predefinição forte.* No teste de liga neutra, Gegenpress (1,39 ppj) e Contra-ataque Fluido (1,38) lideram as predefinições. Park the Bus (0,92) e Catenaccio (1,03) são as piores ([Study 7](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/07-tactic-league.md)).
3. *Fechar a casinha não salva time fraco.* Com o time mais fraco da Premier League, Park the Bus também foi a pior escolha ([Study 19](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/19-a-season-per-tactic.md)).
4. *Não há pedra-papel-tesoura.* A força geral da tática prevê quase todo confronto direto. Em 306 pares, só um fugiu disso ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)). Não troque a tática base só por causa do adversário.
5. *Instrução isolada quase não muda nada* numa tática que já funciona. Exceções medidas: uma mentalidade mais ofensiva custou 0,12 ppj; armadilha de pressão "mostrar para dentro" ou "para fora" custou 0,19 a 0,23 ppj ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
6. *Mantenha a identidade.* A Eficácia Tática do FM27 premia consistência, treino e jogadores na posição natural ([SI, Eficácia Tática](https://www.footballmanager.com/de/node/1149)).
7. *Tática simples primeiro.* Complexidade alta demora mais para ser aprendida. Suba a complexidade aos poucos ([SI, Treino e Eficácia Tática](https://www.footballmanager.com/node/1111)).
8. *Tweaks são para o fim do jogo.* Aplicado desde o apito inicial, "Lock It Down" custa cerca de 0,2 ponto por jogo ([Study 8](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/08-tactical-tweaks.md)). "Goleiro sobe nas bolas paradas" sempre aumentou os gols sofridos.
9. *Para contratar e escalar, físico manda.* Aceleração, Velocidade, Impulsão, Resistência e Antecipação lideram em todas as posições. No goleiro, Agilidade e Reflexos ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
10. *Deixe o assistente comandar a partida* se o Vini não for acompanhar. No teste, a IA de partida perdeu 0,56 ppj para o assistente. O próprio autor chama esse resultado de provisório ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).

## 2. O que mudou do FM26 para o FM27

| Tema | FM26 | FM27 |
|---|---|---|
| Formações | Duas: com bola (IP) e sem bola (OOP) | Igual ao FM26 |
| Funções | Funções separadas com e sem bola, sem "dever" | Igual, mais instruções de ataque à área |
| Familiaridade | Familiaridade Tática | Vira parte da *Eficácia Tática* (4 pilares) e ganha *Complexidade Tática* |
| Mudanças na partida | Pela tela de tática e pelos gritos | Novo: *Tactical Tweaks*, sem abrir a tela de tática |
| Gritos | Removidos no lançamento e devolvidos no Update 4 | Refeitos: agem no estado mental, com explicação e feedback |
| Treino | Unidades e treino de atributos | Unidades removidas. Treino focado no plano do próximo jogo |
| Motor | Novo risco de passe, dribles, corridas no meio-espaço | Velocidade e aceleração com dados Hawk-Eye, mais peso para Marcação e Antecipação, Resistência pesa mais |

Fontes: [SI, IP/OOP no FM26](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution), [SI, Tweaks e motor do FM27](https://www.footballmanager.com/node/1108), [SI, Treino e Eficácia Tática](https://www.footballmanager.com/node/1111), [Passion4FM, Update 4 do FM26](https://www.passion4fm.com/fm26-update-4-changelist-the-return-of-shouts/).

## 3. Como a tática é montada (FM26, igual no FM27)

### 3.1 Passo a passo

1. Escolha um estilo tático (predefinição).
2. Escolha a formação *com bola*. O jogo sugere três formações *sem bola* que combinam com ela. Exemplo: 4-3-3 com bola leva a 4-1-4-1 sem bola.
3. Escolha uma função com bola e uma sem bola para cada jogador.
4. Ajuste as instruções de time por fase e por terço do campo.
5. Confira o *Visualizador*. Ele divide o campo em nove quadrados e mostra o posicionamento em cada zona e fase.

Fonte: [SI, IP/OOP no FM26](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution).

A tela tem quatro visões: Com bola, Sem bola, Ambas (lado a lado) e Combinada (posição média). Troca de jogador feita na visão Combinada vale para as duas formações ([mesma fonte](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution)).

### 3.2 Regras para as duas formações

- *Quanto mais longe o jogador anda entre as duas formações, mais buracos aparecem.* O assistente mostra um aviso amarelo quando a distância é grande demais ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
- Mudança pequena basta. Exemplo: num 4-2-3-1, descer os pontas da linha de meias-ofensivos para a linha de meio-campo sem bola ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
- Pense na *defesa de resto*: quantos ficam atrás quando o time ataca ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
- Pense no *ataque de resto*: quantos ficam à frente para o contra-ataque quando o time defende ([mesma fonte](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
- Combine as funções. Jogador que avança com bola (ex.: armador) combina com função de pressão sem bola. Volante que segura posição combina com função de proteção ("screening") ([mesma fonte](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
- No teste de motor, jogar 3 ou 4 jogadores fora da posição natural *só na formação sem bola* não teve custo medido ([tópico do caiograco, study 17](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)). Mesmo assim, a SI recomenda posição natural para a Eficácia Tática ([SI](https://www.footballmanager.com/de/node/1149)).

### 3.3 Funções (FM26, provável que iguais no FM27)

Os "deveres" (Defender, Apoiar, Atacar) acabaram. Cada jogador tem uma função com bola e outra sem bola. Mezzala, Enganche e Trequartista saíram. A adequação usa familiaridade de posição e atributos-chave, com nota de 1 a 5 estrelas ([SI](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution)).

As funções sem bola caem em poucas famílias ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/), [sortitoutsi, lista de funções](https://sortitoutsi.net/content/74008/updated-fm26-player-roles-revealed)):

| Família | Comportamento | Exemplos |
|---|---|---|
| Pressão (Pressing) | Sai da zona para pressionar alto | Lateral, ala, volante e meia de pressão |
| Combate (Stopping) | Zagueiro sai para cortar o ataque na origem | Zagueiro de combate |
| Cobertura (Covering) | Segura a linha e reage | Zagueiro de cobertura |
| Proteção (Screening) | Fecha o espaço à frente da zaga e não pressiona | Volante e meia de proteção |
| Recomposição (Tracking) | Volta para ajudar a defesa | Ponta, meia aberto, meia-ofensivo e centroavante de recomposição |
| Saída (Outlet) | Não volta. Fica pronto para o contra-ataque | Ponta e centroavante de saída |
| Recuo (Dropping) | Volante entra na linha de zaga | Volante de recuo |
| Cobertura lateral (Wide Cover) | Volante ou meia cobre o lado | Volante e meia de cobertura lateral |

Heurística de equilíbrio: misture quem pressiona, quem protege e quem recompõe. Um lado recompõe e o outro fica de saída ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)). Funções de saída valem mais em estilos de contra-ataque ([mesma fonte](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).

Funções com bola novas no FM26: Goleiro Sem Firulas, Goleiro Construtor, Zagueiro que Avança pelo Lado, Ala Armador, Ala Avançado, Meia Armador, Meia Central Aberto, Atacante Aberto e Atacante de Canal ([SI](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution)).

*FM27:* novas instruções de jogador controlam o ataque à área. Dá para pedir chegada tardia ou ataque à linha defensiva. Dá para escolher o alvo: primeiro pau, segundo pau ou centro. Varia por posição ([SI](https://www.footballmanager.com/node/1108)).

### 3.4 Estilos (predefinições do FM27)

O FM27 traz 10 predefinições: Gegenpress, Control Possession, Fluid Counter-Attack, Wing Play, Vertical Tiki-Taka, Tiki-Taka, Route One, Direct Counter-Attack, Catenaccio e Park the Bus ([Study 7](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/07-tactic-league.md)).

Liga neutra, mesmo elenco para todos, 20 temporadas ([Study 7](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/07-tactic-league.md)):

| Predefinição | Pontos por jogo |
|---|---|
| Gegenpress | 1,39 |
| Fluid Counter-Attack | 1,38 |
| Wing Play | 1,32 |
| Control Possession | 1,28 |
| Route One | 1,21 |
| Direct Counter-Attack | 1,20 |
| Vertical Tiki-Taka | 1,19 |
| Tiki-Taka | 1,15 |
| Catenaccio | 1,03 |
| Park the Bus | 0,92 |

Diferença menor que 0,1 ppj entre linhas não é confiável. As 7 táticas da comunidade testadas fizeram de 1,44 a 1,78. Todas bateram todas as predefinições.

Temporada real com cada predefinição (Study 19): Gegenpress, Control Possession e Fluid Counter-Attack foram as três melhores para o favorito (Arsenal), para o lanterna (Hull City) e para um time de quinta divisão (Altrincham). A diferença entre a melhor e a pior foi de 26, 17 e 31 pontos por temporada ([Study 19](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/19-a-season-per-tactic.md)). Ou seja, a escolha de tática pesa mais em ligas mais fracas.

### 3.5 Mentalidade

A mentalidade continua existindo como ajuste em degraus. O Study 5 testa "um degrau a mais" e "um degrau a menos" ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)). Os nomes dos degraus (Muito Defensiva até Muito Ofensiva) são *provável* herança do FM24. Não achei fonte oficial do FM26/27 com a lista.

Resultado medido: um degrau mais ofensivo custou 0,12 ppj e 0,22 de saldo de xG. Um ou dois degraus mais defensivos não mudaram nada claro ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)). Heurística: não suba a mentalidade "para ganhar mais". Use para correr atrás do placar.

## 4. Instruções de time (FM26, igual no FM27)

As instruções aparecem por fase e por terço do campo. Com bola: Visão geral, Construção, Progressão e Último terço. Sem bola: Visão geral, Pressão alta, Bloco médio e Bloco baixo. A transição ofensiva fica em "com bola". A defensiva fica em "sem bola" ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)). O FM26 trouxe a instrução nova "Passe no pé" ([SI](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution)).

### 4.1 Lista principal

Com bola ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)):

- Objetividade do passe: muito mais curto até muito mais direto.
- Ritmo (tempo): mais baixo ou mais alto. Ritmo alto gasta mais energia.
- Transição ao recuperar: contra-ataque, padrão ou construção paciente.
- Largura: muito mais estreita até muito mais aberta.
- Liberdade criativa: mais disciplinado, equilibrado ou mais expressivo.
- Estratégia de construção: furar a pressão, misto ou direto.
- Tiros de meta: curto ou longo. Distribuição do goleiro: velocidade e alvo.
- Recepção do passe, drible, corridas de apoio, lado de progressão.
- Paciência no último terço, chutes de longe, tipo de cruzamento.
- Gastar tempo e jogar por bola parada.

Sem bola ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)):

- Linha de engajamento: pressão alta, bloco médio ou bloco baixo.
- Linha defensiva: mais baixa até muito mais alta.
- Comportamento da linha: equilibrado, linha de impedimento ou subir.
- Gatilho de pressão: menos frequente até mais frequente.
- Transição ao perder: contrapressão, padrão ou reagrupar.
- Desarme: aliviar, padrão ou agressivo.
- Armadilha de pressão e pressão no goleiro (impedir saída curta).
- Disputa de cruzamento: segurar posição, equilibrado ou disputar.

### 4.2 Combinações que se contradizem

Do guia do FMScout ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)). São heurísticas de guia, não teste:

| Combinação ruim | Problema | Correção |
|---|---|---|
| Passe direto + trabalhar a bola na área | Uma manda acelerar, a outra manda esperar | Direto com paciência baixa, ou curto com trabalhar a bola |
| Pressão alta + linha defensiva baixa | Buraco enorme entre meio e defesa | Pressão alta com linha alta. Bloco baixo com linha baixa |
| Contrapressão + bloco baixo + reagrupar | Ordens opostas | Contrapressão com pressão alta |
| Furar a pressão + tiro de meta longo | A estratégia some a cada tiro de meta | Furar a pressão com tiro de meta curto |
| Mais expressivo + jogadores fracos tecnicamente | Perda de bola constante | Use equilibrado |

### 4.3 O que os testes dizem das instruções

- Num teste com 125 mudanças isoladas numa tática que já funciona, quase nenhuma instrução mudou o resultado de forma confirmada ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
- *Armadilha de pressão:* deixe no padrão. "Mostrar para fora" custou 0,23 ppj. "Mostrar para dentro" custou 0,19 ppj ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
- *Mentalidade:* um degrau a mais custou 0,12 ppj (ver 3.5).
- Testes de estilos inteiros, mudando várias instruções juntas, ainda não foram feitos ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Como o motor usa as instruções: largura, linha defensiva e mentalidade mudam a forma no apito inicial. Objetividade do passe, drible, chute de longe, desarme e liberdade criativa viram "interruptores" nas decisões. Armadilha, contrapressão, cruzamento e saída da pressão são checados na hora da jogada ([tópico do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/)).

Heurística para o copiloto: ajuste instrução para resolver um problema que aparece no jogo. Mude uma ou duas por vez. Não troque tudo depois de uma derrota ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)).

## 5. Eficácia Tática e Complexidade (FM27)

A Eficácia Tática substitui a Familiaridade Tática. Ela tem quatro pilares ([SI](https://www.footballmanager.com/node/1111), [SI, artigo dedicado](https://www.footballmanager.com/de/node/1149)):

| Pilar | O que mede | Como subir |
|---|---|---|
| Entendimento Tático | Se os jogadores entendem o sistema | Tempo de treino nos pontos-chave, bons auxiliares no treino, identidade estável |
| Capacidade dos Jogadores | Atributos e posição para executar | Jogadores na posição natural, poucas instruções no começo |
| Adesão (buy-in) | Vontade de seguir o técnico | Elogiar quem joga bem, ambiente positivo, contratar quem combina com o grupo |
| Informação do Próximo Jogo | Conhecimento do adversário | Bons analistas de desempenho, relatórios do adversário |

A SI diz para manter a estratégia simples contra adversários pouco conhecidos ([SI](https://www.footballmanager.com/de/node/1149)).

*Complexidade Tática* mede o volume de instruções de time, de jogador e contra o adversário. Plano simples funciona rápido, mas tem teto baixo. Plano complexo demora, mas tem teto alto. Futebol fluido com contrapressão intensa pode ser demais para time de divisão baixa ([SI](https://www.footballmanager.com/node/1111)).

O que os testes mostram:

- Entendimento tático de baixo (20) para alto (80) vale +0,52 ppj. Isso equivale a +7 de Velocidade no elenco todo. A partir de cerca de 50, quase todo o ganho já existe. Nível 90 e 100 jogam igual ([tópico do caiograco, study 3](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- São oito áreas: forma com bola, estilo de passe, construção, criação de chances, forma sem bola, linha de engajamento, intensidade de pressão e armadilha. Subir só uma quase não muda nada. A tática precisa ser aprendida como um todo ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- No motor, treino pesa o dobro de jogo para o entendimento (0,6 contra 0,3) ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Num teste dentro do jogo, Eficácia de 30% para 70% subiu as vitórias de 50% para 63% em cinco temporadas ([harvestgreen22, FM-Arena](https://fm-arena.com/threads/for-the-last-tactical-effectiveness-test-i-was-wrong.144/)).
- O mesmo autor relata que o Entendimento Tático cai rápido toda semana na versão de lançamento. Ele espera correção em patch. *Provável* que mude ([mesma fonte](https://fm-arena.com/threads/for-the-last-tactical-effectiveness-test-i-was-wrong.144/)).

Heurística: escolha uma tática principal e no máximo uma variação com a mesma forma. Treine o entendimento até a faixa verde. Não precisa chegar a 100% ([Destructoid](https://www.destructoid.com/how-to-get-full-tactical-effectiveness-in-football-manager-27/), guia de terceiros).

## 6. Durante a partida

### 6.1 Tactical Tweaks (FM27)

Tweaks mudam a tática sem abrir a tela de tática. Valem para o time ou para um jogador. Exemplo: trocar os pontas de lado com um botão. Ficam ativos até você desligar. Não há limite de quantidade. A comissão avisa no banco quando o técnico rival muda a tática e sugere quando ligar e desligar um tweak ([SI](https://www.footballmanager.com/node/1108)). Tweaks convivem com os gritos ([FourFourTwo](https://www.fourfourtwo.com/products-kit/football-manager-27-serves-up-a-new-tactical-tweaks-feature-to-revolutionise-in-game-instructions)).

Cada tweak é um pacote de mudanças: instruções movidas um degrau, instruções de jogador e às vezes mentalidade. Nomes lidos dos dados do jogo: Lock It Down, Throw The Kitchen Sink, Play With More Patience, Play With More Urgency, Make Something Happen, Go Narrower, Exploit The Width, Go Long, Play Out The Back, Disrupt, Press Aggressively, Stick To The Plan, Swap Wings, Goalkeeper Up For Set Pieces, entre outros ([Study 8](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/08-tactical-tweaks.md)). Uma prévia citou "Shut Up Shop". *Provável* que tenha virado "Lock It Down".

Resultado com o tweak ligado o jogo inteiro ([Study 8](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/08-tactical-tweaks.md)):

- *Lock It Down:* cerca de -0,2 ppj, menos gols nos dois lados. Use só para segurar vantagem no fim.
- *Goalkeeper Up For Set Pieces:* +0,22 a +0,28 gol sofrido por jogo. Use só nos últimos lances, perdendo.
- *Throw The Kitchen Sink, Go Long, Play With More Urgency:* neutros em táticas medianas. Custam pontos em tática que já está no extremo (pressão e ritmo no máximo). O tweak empurra um degrau. Se não há degrau, ele só atrapalha.
- Nenhum tweak ajudou na média. Nenhum se mostrou bom contra um estilo específico.
- Limite: o teste não aplicou tweak no meio do jogo, depois de um gol. Esse é o uso real. O efeito no fim do jogo é desconhecido.

### 6.2 Gritos

- FM26: saíram no lançamento e voltaram no Update 4 ([Passion4FM](https://www.passion4fm.com/fm26-update-4-changelist-the-return-of-shouts/)).
- FM27: refeitos para agir no estado mental, não em pequenos bônus de atributo. Têm explicação de contexto. A comissão resume o efeito no banco. A reação depende da personalidade e do momento do jogo ([SI](https://www.footballmanager.com/node/1108)).
- Não achei lista oficial dos gritos do FM27 nem teste de efeito.
- Prática da comunidade, de versões antigas e sem teste: Encorajar com empate ou perdendo por um. Acalmar depois do primeiro gol. Elogiar com vantagem de dois. Exigir mais quando o time não reage ([FM-Arena, comentário antigo](https://fm-arena.com/find-comment/1667/)). Trate como *provável*.

### 6.3 Substituições e fôlego

- FM27: Resistência pesa mais na queda da condição durante o jogo. A SI diz que os reservas passam a influir mais no resultado ([SI](https://www.footballmanager.com/node/1108)).
- No motor, cansaço reduz Velocidade, Aceleração, Decisões e Posicionamento. Com 70% de condição, o jogador perde quase 3 pontos nesses atributos ([tópico do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/)).
- Heurística: troque cedo quem pressiona muito. Os de pressão e de ritmo alto cansam mais. Ritmo alto e pressão frequente gastam mais energia ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)).

### 6.4 Feedback da comissão

- FM26: os conselhos do banco usam xG e xA. Exemplo: sugerir baixar o ritmo quando o jogo está ganho no fim ([SI, dia de jogo FM26](https://www.footballmanager.com/es-419/fm26/funciones/donde-la-narrativa-evoluciona-la-experiencia-del-dia-de-partido-de-fm26)).
- FM27: alertas sobre mudanças táticas do rival e sugestão de tweaks ([SI](https://www.footballmanager.com/node/1108)).

## 7. Preleção (team talk)

- Não achei guia oficial do FM26 ou FM27 sobre preleção. A mecânica básica é *provável* que siga a das versões anteriores.
- No motor do FM27, a moral no início do jogo quase não muda o resultado. Moral 5, 10 ou 20 ficaram dentro de ±0,04 ppj ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)). A moral move os atributos em menos de um ponto, quase só Talento (Flair) ([tópico do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/)).
- A moral ainda pode pesar na temporada por treino, forma e felicidade. O teste não mede isso ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- A Adesão da Eficácia Tática sobe quando você elogia bom desempenho ([SI](https://www.footballmanager.com/de/node/1149)).
- Heurísticas de guias antigos: preleção agressiva quando o time ganha gera acomodação. Preleção passiva quando perde passa ideia de desistência. Não repita a mesma mensagem toda semana ([FootballGPT, moral](https://footballgpt.co/fm/fm-morale), [FM Blog](https://www.footballmanagerblog.org/2023/05/mastering-team-talks-in-football-manager.html)).
- Conclusão para o copiloto: a preleção é seguro contra estrago, não fonte de pontos. Não gaste muito tempo nela.

## 8. Instruções contra o adversário (OI)

- Use contra 1 a 3 jogadores perigosos. Não marque tudo ([sortitoutsi](https://sortitoutsi.net/content/76647/fm26-guide-opposition-instructions)).
- Ponta que corta para o pé bom: mostrar para o pé fraco e pressionar ([sortitoutsi](https://sortitoutsi.net/content/76647/fm26-guide-opposition-instructions)).
- Jogador só de um pé: mostrar para o pé fraco. Antes, veja se ele não está treinando o pé fraco ([Passion4FM, cola de OI](https://www.passion4fm.com/football-manager-opposition-instructions-cheat-sheet/)).
- Marcação individual apertada: boa contra criador lento. Ruim contra atacante muito mais rápido que o marcador ([Passion4FM](https://www.passion4fm.com/football-manager-opposition-instructions-cheat-sheet/)).
- Pressionar todos o tempo todo abre espaço ([sortitoutsi](https://sortitoutsi.net/content/76647/fm26-guide-opposition-instructions)).
- OI entram na Complexidade Tática do FM27 ([SI](https://www.footballmanager.com/node/1111)). Muitas OI deixam a tática mais difícil de aprender.
- Teste: desligar as OI não mudou nada medido. O autor não sabe se o assistente cria OI próprias. Efeito *incerto* ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).

## 9. Bola parada

- Nenhuma fonte forte sobre mudança no criador de bolas paradas do FM27.
- Impulsão (Jumping Reach) é o atributo que decide o jogo aéreo. Cada ponto dá cerca de 3,3 cm de salto. Bola fora do alcance não é disputada ([tópico do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/)).
- Impulsão vale muito mais para zagueiro e centroavante que para volante. No zagueiro, ela ajuda nas duas áreas ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/), [Purity, FM-Arena](https://fm-arena.com/threads/attribute-importance-by-position-in-fm27-a-test-on-my-own-tactic.125/)).
- Três jogadores com Impulsão alta já trazem a maior parte do ganho aéreo ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- No FM26, testes da comunidade relatam 10 a 15 gols de escanteio por temporada e 5 a 6 de falta indireta. O resultado varia muito de uma temporada para outra ([FM-Arena, comentário](https://fm-arena.com/find-comment/45072/), [FMScout](https://www.fmscout.com/c-fm26-set-pieces.html)).
- Cobrador: Escanteios, Cruzamento e Técnica altos. Ajuste a lista de cobradores e a lista de ameaças aéreas ([FMScout](https://www.fmscout.com/c-fm26-set-pieces.html)).
- Batedor de falta direta usa Cobrança de Falta (de longe mistura com Chute de Longe). Pênalti usa Cobrança de Pênalti ([tópico do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/)).
- Não suba o goleiro para bola parada fora do desespero (ver 6.1).

## 10. Atributos que o motor do FM27 valoriza

Útil para escalar e contratar. Valores por ponto de atributo, elenco todo, em pontos esperados por jogo ([tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)):

| Atributo | Por ponto | Observação |
|---|---|---|
| Aceleração | +0,068 | Vale mais abaixo de 11 |
| Velocidade | +0,062 | Vale mais abaixo de 13 |
| Impulsão | +0,035 | Vale mais acima de 9 |
| Resistência | +0,034 | Muito ruim abaixo de 4 |
| Antecipação | +0,024 | Linear |
| Empenho (Work Rate) | +0,022 | Só pesa abaixo de 9 |
| Concentração | +0,021 | Linear |
| Passe, Decisões, Técnica, Desarme, Marcação, Posicionamento | perto de zero | |

- Finalização e Força não aparecem em xG, mas viram gols. Finalização vale cerca de 0,3 gol por jogo de 1 a 20 ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Goleiro: Agilidade e Reflexos dominam. Cada nível salva 0,8 a 0,9 gol por temporada ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Habilidade Atual (CA) não conta dentro do jogo. O motor usa os atributos ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- O ranking vale de Premier League à quinta divisão inglesa, para favorito e azarão ([mesma fonte](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Lacuna: o teste usou ligas inglesas sintéticas. Brasil não foi testado. *Provável* que valha igual, porque o motor é o mesmo.

## 11. Contexto fora da tela de tática

Medidas do motor do FM27 ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md), [tópico do caiograco](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)):

| Situação | Efeito por jogo |
|---|---|
| Ritmo de jogo 50% | -1,03 ponto |
| Ritmo de jogo 75% | -0,37 ponto |
| Condição 80% | -0,80 ponto |
| Condição 90% | -0,28 ponto |
| Coesão do time, mínima para máxima | +0,2 a +0,3 ponto |
| Goleiro com nota 10 na posição | -0,53 ponto |
| Jogar em casa | +0,37 ponto, +0,22 de xG |
| Moral no início do jogo | sem efeito medido |

- Mando de campo: estádios de 40 mil lugares ou mais dão mais que o dobro de vantagem (+0,39 xG). Estádios menores dão +0,15 a +0,18 xG, sem tendência entre 4 mil e 40 mil ([tópico do caiograco, study 15b](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).
- Um testador diz que a torcida presente conta, e que torcedor visitante vale um quarto do mandante. Dado do FM24, *provável* que siga no FM27 ([mesmo tópico](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/)).

## 12. Aplicação ao Santos

Isto é inferência a partir das seções acima. Não há teste específico do Brasil.

- *Calendário apertado.* Brasileirão, Copa do Brasil, estadual e possível torneio continental somam muitos jogos. Como ritmo e condição pesam mais que tática, o copiloto deve priorizar rodízio e checar a condição antes de cada escalação.
- *Tática base.* Comece de Gegenpress ou Fluid Counter-Attack. Ajuste a pressão à Resistência do elenco. Elenco com pouco fôlego: Fluid Counter-Attack ou bloco médio. *Provável*, porque os testes usam elencos ingleses sintéticos.
- *Nada de retranca contra time grande.* Os testes mostram que Park the Bus e Catenaccio perdem pontos até para o azarão.
- *Mando.* A Vila Belmiro tem menos de 40 mil lugares. Pelos dados, o efeito é parecido com qualquer estádio entre 4 mil e 40 mil. Jogos grandes num estádio de 40 mil ou mais podem render vantagem extra. *Provável*, porque a ferramenta não separa estádio, torcida e clube.
- *Contratação.* Priorize Aceleração e Velocidade em todo setor. Impulsão em zagueiros e centroavante. Agilidade e Reflexos no goleiro.
- *Eficácia Tática.* Contratação nova entra com pouco entendimento. Prefira quem combina com o grupo e com a forma da tática.
- *Adversário pouco conhecido* (ex.: estrangeiro na Libertadores): tática simples e poucas OI, como a SI recomenda.

## 13. Erros comuns

1. Trocar a tática inteira depois de uma derrota. Mude uma ou duas coisas e observe ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html), [SI](https://www.footballmanager.com/de/node/1149)).
2. Escalar jogador sem ritmo ou cansado. É o maior custo medido ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
3. Formações com e sem bola muito diferentes. Jogador que cruza o campo abre buraco ([Passion4FM](https://www.passion4fm.com/explaining-in-out-of-possession-formations/)).
4. Pressão alta com linha baixa, ou com elenco sem fôlego ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)).
5. Mudar a armadilha de pressão para dentro ou para fora ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
6. Subir a mentalidade por padrão ([Study 5](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/05-everything-else.md)).
7. Ligar "Lock It Down" cedo demais ou subir o goleiro sem necessidade ([Study 8](https://github.com/caiograco289/Fm-Match-Lab/blob/main/docs/studies/08-tactical-tweaks.md)).
8. Encher de OI e instruções de jogador. Sobe a complexidade e atrasa o aprendizado ([SI](https://www.footballmanager.com/node/1111)).
9. Usar "mais expressivo" com jogadores sem técnica ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)).
10. Copiar tática da internet sem checar se o elenco tem os atributos dela ([FMScout](https://www.fmscout.com/a-fm26-team-instructions-guide.html)).

## 14. Lacunas e próximos passos

- Lista oficial de gritos do FM27 e efeito medido de cada um.
- Efeito dos tweaks aplicados no fim do jogo, depois de um gol.
- Tabela de testes do FM-Arena para o FM27. Deve sair depois do patch do motor previsto para a semana de 16/11.
- Mudanças no criador de bolas paradas do FM27.
- Nomes oficiais dos degraus de mentalidade no FM26/27.
- Revisar os números deste arquivo depois do patch do motor.

## Fontes principais

- Sports Interactive: [IP/OOP no FM26](https://www.footballmanager.com/fm26/features/possession-out-possession-fm26s-new-tactical-evolution), [Tactical Tweaks e motor do FM27](https://www.footballmanager.com/node/1108), [Treino e Eficácia Tática no FM27](https://www.footballmanager.com/node/1111), [Eficácia Tática, artigo dedicado](https://www.footballmanager.com/de/node/1149), [Dia de jogo no FM26](https://www.footballmanager.com/es-419/fm26/funciones/donde-la-narrativa-evoluciona-la-experiencia-del-dia-de-partido-de-fm26).
- Testes do motor do FM27: [Fm-Match-Lab (caiograco289)](https://github.com/caiograco289/Fm-Match-Lab/tree/main/docs/studies), [FM-Arena, o que sabemos do motor do FM27](https://fm-arena.com/threads/fm27-match-engine-what-we-know-so-far.122/), [FM-Arena, por dentro do motor](https://fm-arena.com/threads/inside-the-fm27-match-engine-how-it-actually-works.140/), [FM-Arena, atributos por posição (Purity)](https://fm-arena.com/threads/attribute-importance-by-position-in-fm27-a-test-on-my-own-tactic.125/), [FM-Arena, teste de Eficácia Tática](https://fm-arena.com/threads/for-the-last-tactical-effectiveness-test-i-was-wrong.144/).
- Guias: [Passion4FM, formações com e sem bola](https://www.passion4fm.com/explaining-in-out-of-possession-formations/), [FMScout, instruções de time do FM26](https://www.fmscout.com/a-fm26-team-instructions-guide.html), [sortitoutsi, funções do FM26](https://sortitoutsi.net/content/74008/updated-fm26-player-roles-revealed), [sortitoutsi, OI no FM26](https://sortitoutsi.net/content/76647/fm26-guide-opposition-instructions), [Passion4FM, Update 4 do FM26](https://www.passion4fm.com/fm26-update-4-changelist-the-return-of-shouts/).
