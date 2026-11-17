# Leitura total do FM 27: tela do jogo x tool do MCP

Ticket #45. Data: 17/11/2026. Save: `MCPFM investigation` (cópia de teste, Santos, março de 2026).
Método: `/catalog`, `/schema?type=` e `/get` leves na ponte (:5121). Só GET, um por segundo, sem `/ui`. Nenhuma escrita.

## Como ler a tabela

- Todo caminho usa `root=Human` e `contexts=true`, salvo nota.
- "Valor conferido" é um valor real lido no save de teste.
- "pending" é a resposta da ponte quando a propriedade não responde nessa base ou falta contexto.
- Os inteiros de dinheiro estão na moeda base do jogo, não em R$. Exemplo: `FullContract.Wage.SortValue` 11588 = "R$80 mil p/s" (fator perto de 6,95). Os campos `DisplayValue` já vêm em R$.

## O que o MCP lê hoje

37 tools (README, `src/McpServer/Tools`). Leitura: `get_squad`, `get_availability`, `get_tactics`, `get_role_fit`, `get_tactic_options`, `get_matches`, `get_match`, `get_team`, `get_scouting`, `get_scouted_players`, `run_player_search`, `get_competitions`, `get_competition_stats`, `get_player_conversations`, `get_training`, `search_database`, `get_inbox`, `get_message_options`, `list_save_backups`.

## Inventário

| Área | O que o jogo mostra | Tool que lê hoje | Caminho da ponte achado | Valor conferido | Ticket |
|---|---|---|---|---|---|
| Partida ao vivo | Placar, eventos, números ao vivo | nada | Fora deste inventário | — | #44 |
| Classificação | Tabela da liga: posição, jogos, V/E/D, gols, pontos | nada (`get_matches` só dá a posição do adversário) | base `Club.MainTeam.Division.CurrentStage`, prop `LeagueTable`, `ctx=LeagueTableType:0`. Base `Club.MainTeam`: `LeaguePosition`, `MediaExpectedPosition` | Santos 2º, 3 pts, 3:0. Imprensa espera 10º | #46 |
| Calendário | Todos os próximos jogos, todas as competições | `get_matches` (só os 3 próximos) | base `Club.MainTeam`, `FixtureList` com `FutureAmount:20` (e `PreviousAmount:0`, `FixturePadding:0`, `YearIndex:0`). Liga: list `Club.MainTeam.RemainingLeagueFixtures` (`Team.Name`, `Fixture.FixtureDate`) | 20 jogos futuros. Próximo da liga: Atlético Mineiro, 2026-04-05 | #47 |
| Finanças | Saldo, verbas, folha, receitas e despesas, projeção | nada (`get_scouting` só a verba de observação) | base `Club`: `OverallBalance`, `TransferBudget`, `WageBudget`, `RemainingWageBudget`, `WagesData`, `IncomeBreakdownThisMonth`, `ExpenditureBreakdownThisMonth`, `NotableIncomeBreakdown`, `FinanceBalanceHistory`, `ProjectionBalance` | Folha R$3.396.473 p/s. Verba salarial R$3.486.460 p/s. TV no mês R$8.01 MI. Saldo bruto -7889929 | #48 |
| Diretoria e torcida | Confiança, elogios, críticas, visão do clube, promessas | nada | base `Club.MainTeam.TeamConfidence`: `BoardConfidenceOverallGrade`, `Board*String`, `BoardFeedbackHighlights`, `BoardFeedbackCriticism`, `Supporter*`. Base `Club` com `ctx=VisionIsUsingPerformanceData:false`: `VisionObjectiveThisYearObjective.*`, `VisionObjectiveFinalYearObjective.*`, `BoardRequestsCount`. Root `Human`: `HumanBoardPromises` | Diretoria nota 6, "contente com a sua gestão". Torcida nota 3. Objetivo do ano: "Bater o recorde e terminar na metade de cima da tabela Brasileirão" | #49 |
| Staff | Quem é, função, atributos, estrelas, responsabilidades | nada | list `Club.MainTeam.AllStaffFilteredByTeam`: `Name`, `JobString`, `CoachingStyleString`, `AttributesCoaching`, `AttributesMedical` (`ctx=JobType:0`), `FullContract.*`. Base `Club.MainTeam`: `StaffAllCount`, `StaffCoachingCount`, `StaffMedicalCount` | 17 no time principal. Eudes Pedro, Treinador, Tático 13 | #50 |
| Contratos do elenco | Salário, vencimento, cláusulas, tempo de jogo, renovação | nada para o próprio elenco (`get_competition_stats` lê contrato de jogadores de outras ligas) | list `Club.MainTeam.Players`: `FullContract.Wage.DisplayValue`, `FullContract.EndDate`, `FullContract.AgreedPlayingTime.DisplayValue`, `FullContract.MinimumFeeReleaseClauseValue.DisplayValue`, `FullContract.ContractClausesCount`, `FullContract.HasRenewalOffer`, `ContractRenewalDecisionStaffAdviceString`. Base `Club`: `PlayerExpiringContractsCount` | Gabriel Brazão R$80 mil p/s até 2028-12-31, "Goleiro titular". Staff: "Não oferecer contrato". 19 contratos vencendo | #51 |
| Vestiário | Hierarquia, felicidade, clima, promessas a jogadores | `get_squad` (só moral) | base `Club.MainTeam`: `HierarchyTeamLeaders`, `HierarchyHighlyInfluential`, `HierarchyInfluential`, `HierarchyOther`, `HappinessIssuesList`, `ClubAtmosphereFeedback`. Por jogador: `HierarchyInfluenceCategoryString`, `HappinessPositives`, `HappinessNegatives`. Root `Human`: `HumanPlayerPromises` | Líderes: Neymar, Gabriel Barbosa, Lucas Veríssimo. Clima "Muito bom" 80/100. 1 problema: tempo de jogo | #52 |
| Lesões e médico | Histórico, risco, resumo da temporada | `get_availability` (só a lesão atual) | por jogador: `InjuriesHistory`, `InjuriesThisSeason`, `InjuryRiskLevel`, `InjuryRiskOverview`. Base `Club.MainTeam`: `SeasonInjurySummaryData`, `HighRiskInjuryListCount`, `ExhaustedAndRecurringInjuriesString` | Brazão: 3 lesões, "A torn ACL" em 2021-07-01, 547 dias | #53 |
| Relatório do adversário | Tática esperada, ameaça, pontos da análise | `get_team` (relatório montado das estatísticas) | base do adversário (ex. `Club.MainTeam.RemainingLeagueFixtures[0].Team`) com `ref=TeamRequestingReport:Club.MainTeam`, `ctx=InPossession:true`: `TeamReportExpectedTactic`, `OppositionReportVisuals`. Base `Club.MainTeam`: `NextOppositionThreatLevelAssessment` | Atlético Mineiro: "4-2-3-1 VOL MEA Com Pontas". "Deve ser um jogo equilibrado..." | #54 |
| Relatórios do auxiliar | Reuniões e conselhos do staff | nada | não achado (`StaffMeeting`, `TacticalAdviceMeeting` pedem `Date` e não foram conferidos) | — | — |
| Base e empréstimos | Sub-20, captação, emprestados | nada (`Club.IntakeDate` conhecido) | list `Club.PlayersOutOnLoan`: `LoanClub.Name`, `LoanContract.EndDate`, `LoanAppearances`, `LoanAverageRating`, `LoanHappiness`, `LoanReportComments`. Base `Club`: `HighestLevelYouthTeam`, `DaysUntilYouthIntake`, `YouthFacilityRating`, `YouthCoachingLevel`. List `Club.HighestLevelYouthTeam.Players` | 12 emprestados. Luisão no Goiás, 15 jogos, nota 7,29. Sub-20 com 42. Captação em 305 dias | #55 |
| Notícias | Portal de notícias do time e da liga | `get_inbox` (só a caixa de entrada) | list `HumanTeamOtherNews` (`ref=NewsContextTeam:Club.MainTeam`, `ctx=Number:3`) e `HumanCompetitionOtherNews` (`ref=Competition:Club.MainTeam.Division`). Props: `NewsTitleString`, `HeadlineSubTitle`, `Date`, `NewsMediaSource.Name` | "Oliva estreia pelo Santos Futebol Clube" (Planet Football) | #56 |
| Transferências e propostas | Negociações em andamento, propostas recebidas | `get_inbox` (as mensagens) | base `Club.TransferActivity`: `TransferOffersPlayerCountAll/In/Out`, `TransferOffersRequireResponseCount*`, `TransferActivityPlayerListAll/In/Out` | Tudo 0, lista vazia. Formato do item não visto | #57 |

## Não achado

- Lista completa de objetivos da visão (`VisionObjectiveList`): pending. Pede `VisionObjectiveSetter`, `VisionObjectiveType`, `VisionObjectiveYear`, `ViewID`. Só os objetivos do ano e final responderam.
- `FinancialStatus` e `FinanceTotals*` na base `Club`: pending.
- Estrelas do staff e do potencial (`CoachCurrentAbilityStarRange` null, `CoachPotentialAbilityStarRange.MaxValue` -1).
- Responsabilidades do staff (`ResponsibilityList` pending).
- Pontos fortes e fracos do relatório de observação (`ScoutReportStrengths`, `ScoutReportWeaknesses` vazios) e `LastMatchAnalysis` (pending).
- Reuniões do staff e conselhos do auxiliar (`StaffMeeting`, `TacticalAdviceMeeting`): não conferido.
- Time reserva e segundo time da base (`ReservesTeam`, `SecondHighestLevelYouthTeam`): pending. O Santos pode não ter.
- Item de `TransferActivityPlayerListAll`: lista vazia no save. O caminho responde; o formato do item falta.
- Tabelas de outras competições (copas, Sul-Americana): só a liga foi conferida. O caminho é o mesmo `CurrentStage` na competição.

## Notas para quem implementar

- `/schema?type=<tipo>` mostra cada propriedade, o tipo e os contextos que ela pede. Tipos úteis: `club`, `team`, `person`, `contract`, `teamconfidence`, `CompetitionStage`, `TransferActivity`.
- A lista do sub-20 inclui jogadores emprestados.
- `ClubAtmosphereFeedback` fala de "jogadoras" no texto do jogo. É texto do jogo, não erro da ponte.
