# FM27: o que o Claude precisa saber de elenco e treino

Pesquisa do ticket #13 (mapa #1). Data: 16/11/2026. O FM27 saiu em 10/11/2026.
Objetivo: dar ao Claude regras práticas para ser copiloto da carreira do Vini no Santos.

## Como ler este arquivo

- *FM27*: confirmado em fonte oficial da Sports Interactive (SI) ou em cobertura do lançamento.
- *FM26, sem mudança*: regra do FM26. Nenhuma fonte do FM27 diz que mudou.
- *provável*: vem de teste da comunidade, de versão antiga ou de dedução. Use, mas confira no jogo.
- O FM-Arena migrou de site em 2026. As páginas antigas dão 404. Os números de lá vêm de trechos indexados em busca. Por isso estão todos como *provável*.

## Resumo para o copiloto (as 12 regras)

1. *Ritmo de jogo pesa mais que condição.* Um jogador com 90% de ritmo rende bem menos que com 100%. Cansaço se resolve com rodízio. Ritmo só se ganha jogando. (provável, FM-Arena)
2. *Rodízio em blocos, não em massa.* Troque 3 a 5 titulares por jogo. Trocar muitos de uma vez quebra o entrosamento. (FM26, sortitoutsi)
3. *Titular joga 2 de cada 3 jogos.* Uma partida a cada duas preserva condição. Duas a cada três preservam ritmo. (provável, FM-Arena)
4. *No FM27 o treino coletivo prepara o próximo jogo e ensina a tática.* Atributo se desenvolve no treino individual e no bloco "Footballing Development". (FM27, SI)
5. *Eficácia Tática tem quatro partes:* entendimento tático, capacidade dos jogadores, adesão (buy-in) e informação do próximo jogo. Melhore a mais fraca. (FM27, SI)
6. *Não troque de tática a cada derrota.* O entendimento cresce com o tempo. Treine no máximo uma ou duas táticas alternativas. (FM27, SI)
7. *Escale cada jogador em posição natural com e sem bola.* Papel fora da posição derruba a capacidade até ele se adaptar. (FM27, SI)
8. *Profissionalismo e determinação decidem quem chega ao PA.* PA é teto, não promessa. (FM26, sem mudança)
9. *Até 18 anos, treino. De 18 a 21, minutos.* Empréstimo bom tem titularidade, nível certo e cláusula de retorno. (FM26, SI e FM Scout)
10. *Líderes do vestiário vêm primeiro.* Insatisfação no topo da hierarquia contamina o grupo e agora derruba a adesão tática. (FM27, SI)
11. *Promessa quebrada custa caro.* Prometa pouco e cumpra. Diga cedo a quem não vai renovar. (FM26, SI e sortitoutsi)
12. *Base do Santos sai entre 22/09 e 20/10.* O chefe da base define o tipo e a personalidade dos garotos. (FM26, Passion4FM e SI; data no FM27 é provável)

## 1. Atributos visíveis

- Escala de 1 a 20. Grupos: técnicos (bola parada agora separada), mentais e físicos. (FM26, sem mudança) [sortitoutsi FM26](https://sortitoutsi.net/content/74854/fm26-hidden-attributes-explained)
- *Velocidade (Pace) e Aceleração* são os atributos que mais pesam no motor de jogo. No FM26 o domínio é menor que no FM24, mas continuam no topo. Depois vêm Impulsão (Jumping Reach) e Drible. (provável, FM-Arena) [comentário FM-Arena](https://fm-arena.com/find-comment/44412/), [comentário FM-Arena](https://fm-arena.com/find-comment/53145/)
- Um teste com 60 atributos aleatórios em cerca de 16 mil jogos achou Pace e Acceleration valendo cerca de 3 vezes os outros. (provável) [comentário FM-Arena](https://fm-arena.com/find-comment/53230/)
- Regra de uso: em empate de duas opções, escolha o mais rápido. Exceção: goleiro e centroavante de área dependem menos disso. (provável)
- Determinação e Liderança são visíveis e entram no cálculo da personalidade. [sortitoutsi personalidade FM26](https://sortitoutsi.net/content/75646/fm26-player-personality-guide)

## 2. Atributos ocultos

Não aparecem na tela. Relatórios de olheiro e de treinador dão pistas, como "gosta de jogos grandes". Valor exato só no editor. (FM26, sem mudança) [sortitoutsi FM26](https://sortitoutsi.net/content/74854/fm26-hidden-attributes-explained)

| Oculto | O que faz | Uso pelo copiloto |
|---|---|---|
| Profissionalismo | Ética de treino e disciplina | O mais importante para desenvolvimento |
| Ambição | Vontade de crescer | Acelera evolução, mas pede transferência maior |
| Pressão | Rende sob tensão | Pesa na escolha de batedor de pênalti e capitão |
| Jogos importantes | Rende em final e clássico | Clássico com Corinthians, Palmeiras, São Paulo |
| Consistência | Joga perto do nível real com frequência | Titular confiável |
| Propensão a lesão | Chance de se machucar | Reduza carga de quem tem alta |
| Versatilidade | Velocidade para aprender posição nova | Defina quem vale retreinar |
| Adaptabilidade | Se adapta a país novo | Estrangeiro contratado |
| Lealdade, Temperamento, Esportividade, Controvérsia | Comportamento e vestiário | Evite controversos em grupo frágil |

- Entre Consistência, Jogos importantes e Pressão, Pressão é a que mais pesa no rendimento. (provável, FM-Arena) [tópico FM-Arena](https://fm-arena.com/thread/18466-fm24-consistency-important-matches-pressure-fm24-fm26-how-to-develop-positional-proficiency)
- O MCPFM esconde ocultos por padrão. Só leia quando o Vini pedir. (README do repo)

### Personalidade

A personalidade sai de Determinação, Liderança e seis ocultos. Faixas: Ruim 7 a 9, Médio 10 a 14, Bom 15 a 17, Excelente 18 a 20. (FM26) [sortitoutsi personalidade FM26](https://sortitoutsi.net/content/75646/fm26-player-personality-guide)

- *Melhores para evoluir:* Model Citizen, Model Professional, Perfectionist, Professional, Resolute. [SI, base FM26](https://www.footballmanager.com/the-dugout/developing-and-maximising-your-youth-intakes-fm26)
- *Evitar em jovem caro:* Slack, Casual, Temperamental, Low Self-Belief, Easily Discouraged. Todas têm profissionalismo ou determinação muito baixos.
- *Cuidado com:* Mercenary, Fickle e Very Ambitious. Evoluem, mas forçam saída.

## 3. CA e PA

- CA é a habilidade atual. PA é o teto. Os dois vão de 0 a 200. (FM26, sem mudança) [FM Scout](https://www.fmscout.com/a-football-manager-current-and-potential-ability-guide.html)
- PA mostra quanto o jogador *pode* chegar, não quanto vai chegar. Personalidade, minutos, treino e lesões decidem. [FM Scout](https://www.fmscout.com/a-football-manager-current-and-potential-ability-guide.html)
- As estrelas são relativas ao seu elenco e à liga. Um jogador de 4 estrelas num time médio pode ter 2 num time grande. O julgamento depende da qualidade do staff que avalia. [FM Scout](https://www.fmscout.com/a-football-manager-current-and-potential-ability-guide.html)
- Faixa de evolução por idade: 16 a 19 cresce muito, 20 a 23 cresce bem, 24 a 26 cresce pouco, 27 em diante quase nada. Físicos começam a cair perto dos 30. (provável, FM-Arena e FM Scout) [comentário FM-Arena](https://fm-arena.com/find-comment/46546/)
- Atributos físicos sobem primeiro e mentais por último. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- Regra do copiloto: nunca diga "vai virar craque". Diga "tem teto alto segundo o staff X" e cite a personalidade.

## 4. Condição física, cansaço e ritmo de jogo

- *Condição* é a energia de hoje. *Cansaço (fatigue)* acumula: o jogador pode estar com coração verde e ainda cansado. *Ritmo de jogo (match sharpness)* é o costume de jogar. (FM26) [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
- O Centro Médico mostra risco total, carga de jogos nos últimos 14 dias, carga de treino, suscetibilidade a lesão e ritmo. Consulte antes de escalar. (FM26) [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
- *FM27:* as telas de treino mostram a carga física da semana atual e das próximas. [SI, treino FM27](https://www.footballmanager.com/node/1111)
- Ritmo de 100% para 90% derrubou a taxa de vitória em cerca de 33% num teste. Também subiu lesões em cerca de 25%. Nem o cansaço máximo causou queda tão grande. (provável, FM-Arena) [tópico FM-Arena](https://fm-arena.com/thread/16275-part-1-conditions-management-one-of-the-core-fm26-mechanics)
- Amistoso dá ritmo a cerca de 55% da taxa de um jogo oficial. Ganho de ritmo desacelera perto de 100%. (provável, FM-Arena) [tópico FM-Arena, condicionamento](https://fm-arena.com/thread/15972-how-to-optimize-player-fitness-and-why-it-s-crucial)
- Natural Fitness acelera a recuperação entre jogos. Stamina segura a queda durante o jogo. (provável)
- Oficial: aplique sessões de Descanso entre jogos em semana com 3 partidas. Reduza carga de quem tem risco alto. [SI, reta final FM26](https://www.footballmanager.com/node/1067)

## 5. Rodízio sem perder rendimento

O calendário brasileiro tem Paulistão, Série A, Copa do Brasil e talvez torneio continental. Semana com dois jogos será comum. (provável)

Regras:

1. *Elenco com 2 jogadores por posição e cerca de 25 nomes.* Inclua 3 veteranos mentores: um por setor. [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
2. *Titular: 2 de cada 3 jogos.* Reserva de confiança: o jogo restante, mais 20 a 30 minutos nos outros. Assim os dois mantêm ritmo. (provável, FM-Arena) [tópico FM-Arena, resumo FM24 e FM26](https://fm-arena.com/thread/15934-summary-of-recent-findings-for-optimal-play-in-fm24-amp-fm26)
3. *Troque 3 a 5 por jogo.* Mais que isso quebra entrosamento. [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
4. *Reserva joga no papel que já conhece.* Volante sem desarme não vira "ball-winner" no fim do jogo. [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
5. *Priorize os jogos-chave.* Clássicos e confrontos diretos levam os titulares descansados. Copa contra time menor leva reservas e jovens. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
6. *Não volte lesionado cedo.* Recaída custa mais que um jogo de banco. [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
7. *Estrela no banco demais fica infeliz.* Cumpra a expectativa de minutos combinada. [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
8. *FM27:* treine o time reserva na mesma tática. A adesão e o entendimento são por jogador. Reserva que não treinou a tática reduz a Eficácia Tática. [SI, Eficácia Tática](https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature)

## 6. Treino coletivo (FM27)

Mudanças confirmadas pela SI: [SI, treino FM27](https://www.footballmanager.com/node/1111)

- Treino virou área do menu principal.
- Voltou copiar e colar plano semanal.
- O sistema antigo de "Units" saiu. O treino agora mira executar a tática e o próximo adversário. Exemplos: pressionar alto, defender em bloco baixo.
- Elenco principal foca em preparação, coesão e entendimento tático. Base foca em desenvolvimento individual.
- Dá para mudar o plano da semana e o tipo de preparação direto da mensagem pré-jogo. [SI, deep dive 3](https://www.footballmanager.com/node/1120)
- Voltou a revisão periódica de treino, com elogio ou crítica rápida. [SI, deep dive 3](https://www.footballmanager.com/node/1120)

Blocos de sessão no FM27, segundo a Passion4FM: [Passion4FM, novidades FM27](https://www.passion4fm.com/football-manager-27-features/)

| Bloco | Sessões |
|---|---|
| Tactical Understanding | Passing Style, Build Up, Chance Creation, Pressing Intensity, Pressing Trap, Line of Engagement, In Possession Shape, Out of Possession Shape |
| Match Preparation | Defensive Block, Defensive Positioning, Defensive Engagement, Corners, Free-Kicks, Penalties |
| Footballing Development | Individual Development, Attacking Ball Work, Defensive Ball Work, Possession Ball Work |
| Physical & Recovery | Rest, Recovery, Light Conditioning, Conditioning, Heavy Conditioning |
| Extra-Curricular | Team Bonding, Community Outreach |

Heurísticas para montar a semana:

- *Dia seguinte ao jogo:* Recovery ou Rest. Pico de carga dois dias depois do jogo, depois reduz até o próximo. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- *Véspera:* Match Preparation contra o ponto forte do adversário. Bola parada se ele é forte nisso.
- *Semana livre:* sessões de Tactical Understanding da sua tática e Individual Development.
- *Semana com 3 jogos:* quase só Recovery, Rest e uma preparação leve.
- *Moral em queda:* Team Bonding ou Community Outreach. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- *Carga do staff:* mantenha cada categoria em Light ou Average. Carga pesada derruba a qualidade do treino. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- *Copiar plano:* revise quando o calendário muda. Não copie semana de um jogo para semana de três.

### Eficácia e Complexidade Tática (FM27)

Fonte principal: [SI, Eficácia Tática](https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature)

| Parte | O que mede | Como subir |
|---|---|---|
| Entendimento tático | Se o jogador entende forma, linha, pressão e paciência | Sessões da tática, paciência, bons treinadores. Jogador que já trabalhou com o técnico aprende mais rápido |
| Capacidade | Atributos e posição para cumprir o papel com e sem bola | Posição natural nas duas fases, poucas instruções no começo |
| Adesão (buy-in) | Se o grupo acredita no técnico | Reputação, hierarquia feliz, grupos sociais coesos, elogio, contratar pela personalidade |
| Info do próximo jogo | Quanto você sabe do adversário | Analistas e relatórios de adversário. Contra time desconhecido, tática simples |

- Treine até 3 táticas. As alternativas ficam menos familiares que a principal.
- Complexidade conta posições e volume de instruções. Tática complexa demora mais e tem teto maior. Tática simples dá resultado rápido, mas o adversário descobre antes. [SI, treino FM27](https://www.footballmanager.com/node/1111)
- Regra do copiloto: no início da carreira, tática simples. Suba a complexidade quando o entendimento estiver alto.
- O MCPFM lê esses números em `get_tactics`.

## 7. Treino individual

- Cada jogador treina posição e papel *com bola* e *sem bola*, mais um foco adicional. (FM26) [SI, base FM26](https://www.footballmanager.com/node/1037)
- *FM27:* novos focos individuais: Chance Conversion, Chance Creation, Ball Retention, Ball Progression, Pressing, Containment. Os do FM26 continuam. [Passion4FM, novidades FM27](https://www.passion4fm.com/football-manager-27-features/)
- Instruções individuais ensinam traits e posições novas. (FM27) [SI, treino FM27](https://www.footballmanager.com/node/1111)
- *Intensidade:* metade, normal, dupla ou automática. Deixe automática para quase todos. Use dupla só em jovem saudável com Natural Fitness alto. Use metade ou descanso em quem tem risco alto. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- Elogie treino com nota 7,5 ou mais. Critique 6,5 ou menos. Uma vez por mês. (FM26, provável como regra fixa) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- O MCPFM lê isso em `get_training` e muda em `set_training` (foco e intensidade). Sempre peça o "sim" do Vini antes de aplicar.

### Retreinar posição

- Escala de posição: Natural 18 a 20, Accomplished 15 a 17, Competent 12 a 14, Unconvincing 9 a 11. Mire em 12 ou mais. (FM24, provável no FM27) [sortitoutsi retreino](https://sortitoutsi.net/content/67473/fm24-guide-how-to-retrain-players-position)
- Posição nova do zero: 6 a 9 meses para ficar útil, cerca de 12 para Natural. Jogar na posição acelera. Versatilidade alta acelera e segura o aprendizado. (provável) [sortitoutsi retreino](https://sortitoutsi.net/content/67473/fm24-guide-how-to-retrain-players-position)
- Jovem aprende mais rápido. Use a pré-temporada para testar.

## 8. Moral, hierarquia e dinâmica

- Moral alimenta dinâmica, coesão, treino, forma e preparação. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- A hierarquia tem camadas: Líderes, Muito Influentes, Influentes e Outros. Mantenha o topo feliz primeiro. (FM23 a FM26, sem mudança conhecida) [manual SI FM23](https://community.sports-interactive.com/sigames-manual/football-manager-2023/your-squad-team-report-and-dynamics-r4727)
- Contrariar um membro do grupo social central faz o grupo inteiro tomar o lado dele. (provável, FM23)
- *FM27:* hierarquia e grupos sociais agora pesam na adesão tática. Vestiário rachado piora o rendimento da tática. [SI, Eficácia Tática](https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature)
- *FM27:* o jogo explica por que um líder não aceita conversar com um colega insatisfeito por você. [SI, deep dive 3](https://www.footballmanager.com/node/1120)
- Para subir moral na reta final: capitão e líderes felizes, resolver insatisfeitos, reunião de time sobre metas, elogio na imprensa, Team Bonding, treinadores com Motivação alta. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- Contratação: personalidade e encaixe no grupo central valem tanto quanto atributo. [SI, Eficácia Tática](https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature)

## 9. Promessas e conversas

- Tempo de jogo está no centro da felicidade. Cumpra o que prometeu. Promessa quebrada gera problema. (FM20 a FM26) [manual SI FM21](https://community.sports-interactive.com/manual/football-manager-2021/your-squad-team-report-and-dynamics-r766)
- Promessa feita em negociação de contrato também conta. Quebrar destrói relação e harmonia. [sortitoutsi rodízio FM26](https://sortitoutsi.net/content/74657/fm26-guide-mastering-squad-rotation)
- Diga cedo a quem não vai renovar. Jogador sem futuro fica inquieto e afeta o grupo. Renove os melhores com antecedência. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- Pré-contrato com nome de peso a 6 meses do fim do contrato dele sobe a moral. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- Conversa individual: uns pedem apoio, outros elogio público, outros crítica. A personalidade define a reação. [SI, reta final FM26](https://www.footballmanager.com/node/1067)
- Reunião de time com moderação. Muitas irritam o grupo. (provável) [FMInside](https://fminside.net/guides/tactical-guides/41-tips-to-end-a-losing-streak)
- Não critique duro na imprensa. (provável) [FMInside](https://fminside.net/guides/tactical-guides/41-tips-to-end-a-losing-streak)
- No FM26 houve bug de "promessa não cumprida" com lista vazia. Se aparecer no FM27, avise o Vini que pode ser bug. (provável)

Regras do copiloto:

1. Antes de prometer minutos, conte quantos titulares a posição já tem.
2. Nunca prometa a dois jogadores a mesma vaga.
3. Registre cada promessa e o prazo. Lembre o Vini antes de vencer.

## 10. Desenvolvimento de jovens

- *15 a 17 anos:* foco físico e de setor (defesa, meio, ataque), não papel exato. [SI, base FM26](https://www.footballmanager.com/node/1037)
- *18 a 21:* corrija falhas técnicas, defina papel, comece empréstimos. [SI, base FM26](https://www.footballmanager.com/node/1037)
- *21 em diante:* briga por vaga ou é vendido. Aos 21 o jovem costuma valer mais que a habilidade. Vender e reinvestir é válido. [SI, base FM26](https://www.footballmanager.com/node/1037)
- Até 18 anos evolui mais pelo treino. Depois, por jogos e nota. 15 minutos já dão nota de jogo. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- Meta para jovem promovido: 900 minutos ou 10 a 20 jogos por temporada no time principal. Entre no fim de jogo ganho. (provável) [Passion4FM, empréstimos](https://www.passion4fm.com/using-loans-to-improve-youth-development-on-football-manager/)
- Só promova ao principal quem vai jogar. Jovem que treina no principal e joga na base perde até um mês de treino na temporada. (FM26) [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- Revise o foco individual a cada poucos meses. Físicos crescem de forma desigual. [SI, base FM26](https://www.footballmanager.com/node/1037)
- *Mentoria:* muda personalidade, não atributo. Junte o jovem a veteranos de personalidade boa. Grupo mínimo de 3. (FM24 a FM26, provável no FM27) [footballgpt, guia não oficial](https://www.footballgpt.co/fm/fm-training)
- *FM27:* base passa mais tempo em Individual Development. Relatórios mensais de jovens emprestados ficaram mais completos. [Passion4FM](https://www.passion4fm.com/football-manager-27-features/), [SI, deep dive 3](https://www.footballmanager.com/node/1120)

## 11. Empréstimos

- Empreste quem tem 18 a 24 anos e não vai jogar no principal. Acima de 24, prefira vender. [sortitoutsi empréstimo FM26](https://sortitoutsi.net/content/76032/fm26-loan-vs-keep-mastering-player-development)
- Clube de destino: titularidade, bons centros de treino, liga de nível adequado, posição natural. [sortitoutsi empréstimo FM26](https://sortitoutsi.net/content/76032/fm26-loan-vs-keep-mastering-player-development)
- Sempre peça cláusula de retorno e exija tempo de jogo no acordo. [FM Scout, JonasMorais](https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html)
- Nível certo: igual ou pouco acima do "nível de atuação" do relatório do treinador. Liga fraca demais não ensina. Forte demais derruba moral. [Passion4FM, empréstimos](https://www.passion4fm.com/using-loans-to-improve-youth-development-on-football-manager/)
- *FM27:* a IA do clube dono chama de volta com menos facilidade. O Diretor de Futebol aconselha taxas de empréstimo. [SI, deep dive 3](https://www.footballmanager.com/node/1120)
- Para o Santos: Série B e clubes da Série A de meio de tabela costumam dar minutos. Europa só para quem já tem nível. (provável)

## 12. Base (youth intake)

- Brasil: intake entre 22/09 e 20/10. A Passion4FM lista o Santos com base "Exceptional". (FM26; FM27 provável igual) [Passion4FM, datas](https://www.passion4fm.com/football-manager-youth-intake-dates/)
- A prévia chega cerca de 3 meses antes. Nem sempre bate com o resultado. (provável)
- Rating de base do país e importância do futebol são fixos. O Brasil tem piso alto. [SI, base FM26](https://www.footballmanager.com/the-dugout/developing-and-maximising-your-youth-intakes-fm26)
- O que dá para mexer: Youth Facilities, Junior Coaching e Youth Recruitment. Só sobem pedindo à diretoria. [SI, base FM26](https://www.footballmanager.com/the-dugout/developing-and-maximising-your-youth-intakes-fm26)
- O *chefe da base (HoYD)* define o tipo de jogador pela formação preferida dele. Ele também passa a personalidade aos garotos. Contrate um HoYD com sua formação e personalidade Model Citizen, Perfectionist, Model Professional ou Professional. [SI, base FM26](https://www.footballmanager.com/the-dugout/developing-and-maximising-your-youth-intakes-fm26)
- Regra do copiloto: em agosto, confira HoYD, formação dele e pedido de melhoria da base. Antes de setembro, abra vaga no elenco sub-20.

## 13. Escolher funções pelos atributos

- No FM26 e FM27 cada jogador tem papel *com bola* e papel *sem bola*. O papel sem bola precisa dos atributos dele também. [SI, Eficácia Tática](https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature)
- Exemplo oficial: meia central serve como Channel Midfielder com bola. Mas pode falhar como Screening Defensive Midfielder sem bola, se faltam atributos ou costume.
- Passo a passo:
  1. Rode `get_role_fit` na posição. O jogo dá a nota de cada jogador em cada papel.
  2. Confira os atributos-chave do papel na busca do jogo ("Highlight Key Attributes for Role"). Mire em 12 ou mais na liga do Santos. [sortitoutsi retreino](https://sortitoutsi.net/content/67473/fm24-guide-how-to-retrain-players-position)
  3. Desempate por Pace e Acceleration em papéis de lado, lateral e atacante. (provável)
  4. Prefira posição natural nas duas fases a um papel "melhor" fora da posição.
  5. Use traits como reforço: um trait que combina com o papel ajuda. Um que contradiz atrapalha. (provável)
- Desde o FM27, a busca de jogadores mostra papéis sem bola nos atributos-chave. Os relatórios de olheiro mostram encaixe na Eficácia Tática. [SI, deep dive 3](https://www.footballmanager.com/node/1120), [SI, treino FM27](https://www.footballmanager.com/node/1111)

## 14. Checklist semanal do copiloto

1. `get_squad`: condição, moral e quem joga pouco.
2. Centro Médico, se o Vini abrir: quem tem risco alto.
3. Calendário da semana: 1, 2 ou 3 jogos? Ajuste o plano.
4. `get_training`: carga, nota de treino e intensidade individual.
5. `get_tactics`: as quatro partes da Eficácia Tática. Ataque a mais fraca.
6. `get_team` do adversário: escolha a sessão de Match Preparation.
7. Proponha a escalação com rodízio de 3 a 5. Mostre quem precisa de ritmo.
8. Lembre promessas e contratos que vencem.

## Lacunas

- A SI não publicou números do novo treino do FM27: peso de cada sessão, carga exata, ganho por sessão.
- Não achei fonte oficial do FM27 sobre mentoria, promessas ou conversas. As regras aqui vêm do FM26 ou antes.
- Os testes de motor do FM-Arena para o FM27 ainda não saíram. Os números de atributo e ritmo são do FM26.
- A data do intake brasileiro no FM27 não tem fonte. Confirme na primeira temporada.

## Fontes principais

- SI, treino e Eficácia Tática FM27: https://www.footballmanager.com/node/1111
- SI, guia Eficácia Tática FM27: https://www.footballmanager.com/the-dugout/tactical-effectiveness-understanding-fm27s-key-new-feature
- SI, deep dive 3 FM27: https://www.footballmanager.com/node/1120
- SI, desenvolvimento de jovens FM26: https://www.footballmanager.com/node/1037
- SI, intake FM26: https://www.footballmanager.com/the-dugout/developing-and-maximising-your-youth-intakes-fm26
- SI, reta final FM26: https://www.footballmanager.com/node/1067
- Passion4FM, novidades FM27: https://www.passion4fm.com/football-manager-27-features/
- FM Scout, treino FM26 (JonasMorais): https://www.fmscout.com/a-fm26-training-guide-and-schedules-by-jonasmorais.html
- sortitoutsi, ocultos, personalidade, rodízio e empréstimo FM26: links nas seções.
