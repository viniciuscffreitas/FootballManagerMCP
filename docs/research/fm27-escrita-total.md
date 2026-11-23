# Escrita total do FM 27: ação do jogo x tool do MCP

Ticket #64. Data: 17/11/2026. Save: `MCPFM investigation` (cópia de teste, Santos, abril de 2026).
Método: leitura do código (`src/Plugin`, clientes em `src/FootballManager/FootballManager.Infrastructure/Clients`), a memória do projeto e os dumps de enum já gravados no Windows (`C:\Code\mcpfm-out\events-enum.json` = `FM.GamePlugin.InteropEventIDs`, `actionid.json` = `FM.GamePlugin.ActionID`, `catalog-ref.json` = `/catalog?set=reference`). Nenhuma escrita no jogo.
A ponte ficou fora do ar durante a pesquisa (o FM estava fechado pelo outro agente: `/session` sem resposta às 21h30). Por isso nenhum caminho abaixo foi lido ao vivo nesta rodada. "Conferido" marca o que já rodou no jogo em tickets anteriores.

## Como ler a tabela

- "Evento" é um `InteropEventIDs` mandado por `/native?event=<nome>` (Target = a base do `/get`, Data no `field=Data:`). O número vem entre parênteses.
- "Ação" é um `FM.GamePlugin.ActionID` do menu de contexto. O caminho é sempre o mesmo:
  1. `/get?root=game&refList=Records:Human/<caminho do registro>&contexts=true&props=Actions` lista as ações que o menu oferece agora (script `C:\Code\myactions.py`).
  2. `/get?root=game&base=Action&ctx=ID:u:<id>&refList=Records:Human/<caminho>&contexts=true&props=Items` traz as entradas, cada uma com `Event` e `ActionData` (script `C:\Code\pmenu.py`).
  3. Entrada com `Event` 44: `/native` `EventFmxDoAction` (como `scout_player`). Entrada com `Event` 0 que abre card ou painel: `/contextaction` com `field=Data:@Items[0].ActionData` (como `talk_to_player`, `negotiate_contract`). Entrada com `IsOpenPanelAction`: `/newsaction` com o `NewsAction` da entrada (como `make_transfer_offer`).
- "Ref" é um tipo de `/catalog?set=reference`; `/schema?type=<ref>` mostra props e contextos dele.
- Botões sem evento próprio (diálogos Sim/Não, falas de reunião) vão por `/click` com o caminho do `/ui`.

## O que o MCP escreve hoje

`create_tactic`, `set_tactic` (estilo, mentalidade, instruções de time, formações, escalação, posições, funções, instruções de jogador, banco), `suggest_rotation` (só planeja; escreve por `set_tactic`), `set_training` (foco adicional, intensidade, descanso, sessões), `make_transfer_offer` (só valor), `negotiate_contract` (contrato de contratado ou renovação do próprio jogador), `talk_to_player` (6 conversas de elogio e crítica), `reply_to_message` (respostas simples do inbox), `scout_player`, `scout_team`, `shortlist_player`, `create_shortlist`, `create_recruitment_focus`, `remove_recruitment_focus`, `run_player_search`, `offer_trials`, `end_trials`, `restore_save`, `release_game_version`.

## Inventário

| Área | Ação no jogo | Tool que faz hoje | Caminho da ponte achado | Conferido | Ticket |
|---|---|---|---|---|---|
| Partida ao vivo | Mentalidade, gritos, substituição, função em campo, preleção do intervalo e do fim | nada (`set_tactic` muda mentalidade em 1,8 s) | Fora deste inventário | sim (#63) | #63 |
| Escalação | Titulares, posições, funções | `set_tactic` | `/pick`, `/move`, `EventFmxTeamSelectionPickPlayer` | sim | — |
| Escalação | Banco de reservas | `set_tactic` (`bench`) | `EventFmxQuickPick` (43) zera o aviso; `/pick` nos índices 11+ | sim | — |
| Escalação | Escolha rápida (melhor XI, rodízio leve/forte, jovens) | nada (o cliente usa a 43 por dentro) | `EventFmxQuickPickBestXI`, `QuickPickRotatedSlight/More/Full`, `QuickPickYoungsters` | não | — (`suggest_rotation` cobre o uso) |
| Escalação | Seleções salvas (guardar, escolher, apagar) | nada | `EventFmxSaveTeamSelection`, `SelectTeamSelection`, `DeleteTeamSelection` | não | não priorizado |
| Capitães | Capitão, vice e ordem de substitutos da braçadeira | nada | Ação `TeamSelectCaptain` (1112887921), `TeamSelectViceCaptain` (859991371) no time; `PlayerMakeCaptain` (1349150752), `PlayerMakeViceCaptain` (944393847) no jogador. Eventos `EventFmxCaptainQuickPick`, `SetPieceCaptainsCandidatesMove`, `SetPieceCaptainCandidatesRemove`, `AddCandidatesToCaptainStandin`, `SetCaptaincyStandinIndex`. Ref `captainstandincandidate` | não | #66 |
| Bolas paradas | Cobradores de pênalti, falta e escanteio (lados, pé) | nada | Sessão `EventFmxSetPieceEditSessionBegin` / `End` / `Cancel`. `EventFmxPenaltyTakerQuickPick`, `SetPiecePenaltyCandidatesAdd/Move/Remove`, `SetPieceTakerAddToList/Move/RemoveFromList`, `SetPieceTakerBothSides`, `AddSetPieceLeftFooter/RightFooter/BothFeet`, `SetPieceAskToAssignTakers`. Refs `setpieceresponsibilities`, `setpiecepenaltytakercandidate`, `setpieceroutinetaker` | não | #66 |
| Bolas paradas | Rotinas (criador, instruções, frequência) | nada | `EventFmxSetPieceStartCreator`, `SetPieceCreatorNextStep/BackStep`, `SetPieceStrategyConfirmed`, `SetPieceSelectMenuRoutine`, `SetPieceInstrChanged`, `SetPieceRoutineDeliverySelect`, `SetPieceSetRoutineFrequency`, `SetPieceAskToAssignCategory`. Refs `setpiececreator`, `setpieceroutine`, `setpieceroutinecategory` | não | #66 |
| Tática | Funções e instruções de jogador | `set_tactic` | `EventFmxSelectRole`, `/set` nas instruções | sim | — |
| Tática | Instruções de time e mentalidade | `set_tactic` | `/set` nos dropdowns da tática | sim | — |
| Tática | Função personalizada da posição | nada | `EventFmxEnablePositionPersonalisedRole` (58), `DisablePositionPersonalisedRole` (59) | não | não priorizado |
| Tática | Tática só para o próximo jogo | nada | `EventFmxSetTacticNextMatchOnly` | não | #67 |
| Adversário | Instruções contra o adversário (por jogador e por posição) | nada | `EventFmxOppositionInstructionsClear`, `OppositionInstructionsApplyAdvice`, `OppositionInstructionsNext/Previous`. Refs `tacticsoppositionspecificinstructions`, `tacticsoppositionplayerinstructions`, `OppositionInstructionSuggestion` | não | #67 |
| Adversário | Aplicar conselhos do staff (todos, só próximo jogo, uma categoria) | nada | `EventFmxApplyAllAdvice` (70699), `ApplyAllAdviceAllMatches` (70696), `ApplyChangeForNextMatch` (70697), `ApplyChangeForAllMatches` (70698), `ApplyAllAdviceSingleCategory` (70701), `AcknowledgeAdvice` (70702). Ação `ClubViewAdvice` (1667395916) | não | #67 |
| Venda | Pôr na lista de transferência ou de empréstimo, tirar dela | nada | Ação `PersonSetPlayersTransferStatus` (1766943801), `PersonRemovePlayersFromTransferList` (860959573), `PersonRemovePlayersFromLoanList` (862601504). Refs `SetTransferStatus`, `playertransferstatus` | não | #68 |
| Venda | Oferecer a clubes, anúncio na Sala de Transferências | nada | Ação `PersonOfferToClubs` (1164992875), `AskPlayerAgentAboutGenerateMarketInterest` (1449750636). `EventFmxTransferRoomAddAdvertConfirm` (35), `TransferRoomDeleteAdvert` (36), `TransferRoomOfferToClubsCancel` (81). Ref `teamsettingseditsessionoffertoclubs`, `TransferRoomAdvert` | não | #68 |
| Venda | Proposta recebida: aceitar, recusar, negociar, segurar | nada (`get_transfer_activity` lê; `reply_to_message` só se a mensagem tiver botão simples) | `EventFmxTransferOfferResponse` (32), `TransferCentreResponseAccept` (1832014419), `TransferCentreResponseStall` (810905175), `TransferCentreResponseCancelReject` (929649002), `TransferOfferSetNegotiable` (40), `TransferClauseResponse/Confirm`. Lista `Club.TransferActivity.TransferActivityPlayerListOut` | não (nenhuma proposta chegou em 14 dias no save) | #69 |
| Venda | Venda ou empréstimo imediato | nada | Ação `PersonImmediateTransfer`, evento `EventFmxRequestImmediateTransfer` (1668240212). Ref `BuyOrSellImmediately` | não | #68 |
| Empréstimo de saída | Chamar de volta, estender, emprestar por um jogo | nada | Ação `PersonRecallPlayerFromLoan` (1917875764), `PersonOfferExtendLoan` (1768114263), `PersonLoanForOneMatch` (1953320022). Lista `Club.PlayersOutOnLoan` | não | #68 |
| Empréstimo de entrada | Proposta de empréstimo (taxa, salário pago, opção de compra) | nada (`make_transfer_offer` só manda valor de compra) | Painel da proposta (`/newsaction` 6): `EventFmxTransferOfferSelectOfferType` (33), `TransferOfferRemoveAllLoanToBuyFees` (144) | não | #70 |
| Contratação | Cláusulas da proposta (parcelas, bônus, revenda) e troca de jogadores | nada | `EventFmxTransferOfferAddClause` (122), `RemoveClause` (123), `ChangeClausePrimaryValue` (124) a `ChangeClauseEndDateValue` (129), `TransferOfferAddExchangePlayer` (130), `RemoveExchangePlayer` (132). Ref `TransferOfferEditSession`, `TransferOfferClause` | não | #70 |
| Contratação | Alvo do DoF (marcar, tirar) | nada | Ação `PersonAddPlayersAsTransferTargets` (1230532153, aparece no menu: `pactions.json`), `PersonRemovePlayersAsTransferTargets`. `EventFmxRemoveDofTransferTarget`. Ref `DofTarget`, `DofTransferTargets` | menu lido em 16/11 | #70 |
| Contratação | Retirar ou mudar proposta feita | nada (`end_trials` retira só teste) | Ação `PersonWithdrawPlayerTransferOffer` (1951493231), `PersonChangePlayerTransferOffer`. `EventFmxTransferOfferWithdraw` | não | #70 |
| Contratos | Renovar, contrato de contratado | `negotiate_contract` | Ação `PersonOfferContract` (2000111204) por `/contextaction`; `/newsaction` 7 na mensagem | sim | — |
| Contratos | Bônus e cláusulas no contrato do jogador | nada (`negotiate_contract` aceita o que o agente pede) | `EventFmxAddContractBonus` (1668685894), `AddContractClause` (1936536162), `AddPreContractPromise` | não | #71 |
| Contratos | Liberar (rescisão paga), rescisão amigável, marcar para sair | nada | Ação `PersonReleaseClubFreeTransfer` (1214855239), `PersonMutualTerminationContract` (1632134254), `PersonTerminateContract` (1701671793), `SetForReleasePlayer` (1299390547). Evento `EventFmxSetForRelease` | não | #71 |
| Contratos | Entregar a renovação ao DoF | nada | Ação `SetForDofContractRenewalPlayer` (1819821377), `PersonAssignPersonContractRenewalHandling` (1328903712). Evento `EventFmxSetDofToNegotiateContract` | não | #71 |
| Contratos | Status no elenco (tempo de jogo prometido) | nada | Ação `PersonSetPlayersPlayingTime` (1714897475) | não | #71 |
| Base | Promover ou mover entre elencos (principal, sub-20, reservas) | nada | Ação `PersonChangePlayersClubSquad` (1516726887), `PersonMoveToMyTeam` (1132098422), `PersonMoveToOtherTeam` (1934316104), `PersonMakePlayersAvailableForReserves` (1245135978), `PersonMovePlayersToAffiliates` (1348090160) | não | #72 |
| Base | Captação: escolher quem fica, contrato de jovem | nada (`negotiate_contract` pode servir ao contrato) | `EventFmxAddRemoveYouthPlayer` (1246585426). Refs `YouthCandidatesPreview`, `RankedYouthCandidates`, `YouthPromoted`. Captação em 14/01 (`Club.IntakeDate`) | não (captação a 305 dias no save) | #72 |
| Inscrição | Inscrever jogadores nas competições | nada | `/newsaction` 10 abre o painel (conferido 16/11). Refs `squadregistrationEditSession`, `squadregistrationPlayers`. Regras em `Club.MainTeam.SquadRegistrationComps[i]` | painel abre | #73 |
| Inscrição | Números de camisa | nada | `EventFmxNumberAllPlayers` (57) = "Pedir ao auxiliar para atribuir"; Confirmar por `/click`. Ref `SquadNumbersEditSession`, `squadnumber` | sim, por script (`autopilot.py`) | #73 |
| Staff | Anunciar vaga, entrevistar, contratar | nada | Ação `PlaceJobAdvert` (880036918), `PlaceCoachingJobAdvert` (1951295084), `PlaceScoutingJobAdvert` (1852196152), `PlaceMedicalJobAdvert` (1816937269). `EventFmxOfferJobInterview` (893546033), `RemoveJobAdvert`. Contrato do staff: `PersonOfferContract` no membro do staff. Refs `jobAdvert`, `jobvacancyinfo` | não | #74 |
| Staff | Demitir | nada | Ação `PersonTerminateContract` (1701671793), `PersonMutualTerminationContract` (1632134254) no membro do staff | não | #74 |
| Staff | Responsabilidades (assumir ou delegar cada item) | nada (`get_staff` não lê; `ResponsibilityList` pending no #45) | Ação `TeamContainerEditResponsibilities` (1749697863). `EventFmxResponsibilityTakeOrDelegateControl` (141), `ResponsibilityGroupTakeControl` (142), `ResponsibilityGroupDelegateRecommended` (143), `ResponsibilitySettingsChangeConfirm` (1733778041). Refs `teamsettingseditsessionresponsibilitygroup`, `teamsettingseditsessionindividualresponsibility`, `teamsettingsstaffmemberresponsibilities` | não | #74 |
| Staff | Cursos de treinador e liderança | nada | Ação `PersonCoachingCourse` (1900504944), `PersonLeadershipCourse` (1297173097) | não | #74 |
| Diretoria | Pedidos (verba, estrutura, base, staff, afiliado) | nada (`get_board` lê os pedidos feitos) | Ação `MakeBoardRequest` (1297567845), `ClubRequestIncreaseBudgets` (1497974126), `ClubRequestAffiliateLink` (1229607543). Refs `boardRequest`, `BoardRequestsByYear` | não | #75 |
| Diretoria | Ajuste de verba (transferência x salário) | nada | Ação `ClubBudgetAdjustmentTransfersAndWages` (1885623898), `ClubBudgetAdjustmentTransfers`, `ClubBudgetAdjustmentWages`, `ClubBudgetAdjustmentScouting` | não | #75 |
| Diretoria | Pedir contrato novo para o técnico | nada | Ação `ClubRequestManagerNewContract` (896087905) | não | #75 |
| Diretoria | Reunião de visão e expectativas da temporada | nada (`autopilot.py` responde por script) | Falas `EventFmxConversationReplySelected` (34) ou `/click` em `#SpeakToMeetingAnswerActionButton` (`C:\Code\meeting.py`). Refs `clubvisionmeetingeditsession`, `adjustseasonexpectations`. `EventFmxContinueToSeasonExpectations` | sim, por script | #75 |
| Imprensa | Coletiva: responder, sem comentário, sair, mandar o auxiliar | nada (só `reply_to_message` com "Pedir a ...") | `EventFmxStartPressConference` (863384693), `PressConferenceAnswerQuestion` (943147586), `PressConferenceNoComment`, `PressConferenceEarlyExit`, `PressConferenceStormOut`, `PressConferenceSendAssistant` (1330852694). Ref `PressConferenceEditSession` | não | #76 |
| Dia de jogo | Confirmar escalação, preleção antes do jogo, entrevista de túnel | nada (`tomatch.py`, `pre.py` por script) | `EventFmxConfirmTeamSelection` (diálogo "Sim"), `EventFmxTeamTalkSuggestedComment` (135), `TeamTalkProcessTalk` (155), `TeamTalkHandOver` (154). Túnel = falas como reunião (`EventFmxConversationReplySelected`). Refs `teamtalk`, `teamtalkplayerreaction` | sim, por script | #76 |
| Vestiário | Reunião com o elenco, resolver insatisfação do grupo | nada | Ação `TeamHoldTeamMeeting` (1950955128), `TeamResolvePlayerConcerns` (1781610574), `PlayerUnhappinessRelatedTeamMeetings` (1162950521), `TeamResolveGroupUnhappiness` | não | #77 |
| Vestiário | Conversas que faltam: descanso, lesão, conselho, empréstimo, preocupação, colega insatisfeito, tirar a braçadeira, multa | nada (`talk_to_player` faz 6 de elogio e crítica) | Ação `RestPlayerConversation` (1329025354), `TalkToPlayerAboutInjury` (1968202070), `PlayerAdvice` (1398364259), `PraisePlayerLoanSpell`, `WarnPlayerLoanSpell`, `PlayerConcerned`, `TalkToUnhappyTeammate` (1799508555), `WarnPlayerRemoveCaptaincy` (1313297476), `PersonDisciplinePlayer` (2048932173). Mesmo `/contextaction` do `talk_to_player` | não | #77 |
| Treino | Foco adicional, intensidade, descanso | `set_training` | `EventFmxPlayerTrainingFocusChanged`, `PlayerTrainingIntensityLevelChanged` | sim | — |
| Treino | Sessões e agenda da semana | `set_training` (`sessions`) | `EventFmxBeginTrainingWeekEditFromCard` … `CommitTrainingWeekFromCard` | sim | — |
| Treino | Posição e função de treino | nada (`get_training` lê) | `EventFmxPlayerTrainingPositionChanged` (2052018511), `PlayerOOPTrainingRoleChanged` (1176516205), `MultiplePlayerTrainingPositionChanged`. Ação `PersonTrainingNewPosition` (1799516016), `PersonTrainingOOPRole` (1380077924) | não | #78 |
| Treino | Traços (preferred moves), pé fraco | nada | Ação `PersonTrainingNewPreferredMove` (911493462), `PlayerAskStaffToSuggestPlayerTraitTraining`, `PlayerFootednessTraitTraining` | não | #78 |
| Treino | Grupos de mentoria | nada | `EventFmxAddMentoringGroup` (63), `EditMentoringGroup` (64), `DeleteMentoringGroup` (62), `AutoSelectMentoringGroupPlayers`. Ação `TeamSetRecommendedMentoringGroups` (1882280514) | não | #78 |
| Treino | Tática treinada principal | nada | `EventFmxSetPrimaryTrainingTacticSlot` (88). Ação `TeamChangePrimaryTrainedTactic` (543781442) | não | #78 |
| Treino | Curso intensivo de idioma | nada | Ação `PersonSendOnIntensiveLanguageCourse` (844776261) | não | #78 |
| Calendário | Marcar e cancelar amistosos, pré-temporada | nada | Ação `TeamProposeFriendly` (1363750991), `TeamContainerProposeFriendly` (1865959495). `EventFmxConfirmArrangeFriendly` (2032162915), `CancelArrangeFriendly`, `CancelFriendly`, `CancelFriendlyConfirmed`. Ref `arrangefriendly`, `ArrangePreTournamentFriendlyNews` | não | #79 |
| Calendário | Continuar até a próxima decisão | nada (`autopilot.py` fora do MCP) | `/continue` (plugin) e `EventFmxContinueGame` (13). Trava em `ManagerResponseRequiredCount` > 0 e em mensagens não lidas (`#MarkAllReadButton`) | sim, por script | #79 |
| Calendário | Salvar o jogo | nada | `EventFmxSaveGame` (14), `EventConfirmSaveGame`, fim em `EventFmxSaveGameCompleted` (15) | não | #79 |
| Técnico | Férias, pedir demissão, candidatar a vaga | nada | `EventFmxGoOnHoliday` (22), ação `HumanClubResign`, `TeamApplyForJob` | não | fora do escopo do Copiloto |

## Tickets abertos

#66 bolas paradas, #67 instruções contra o adversário e conselhos, #68 venda e empréstimo de saída, #69 propostas recebidas, #70 proposta completa, #71 contratos, #72 base, #73 inscrição e números, #74 staff, #75 diretoria, #76 imprensa e pré-jogo, #77 vestiário, #78 treino, #79 calendário. A partida ao vivo fica no #63.

## Lacunas sem ticket

- Seleções salvas, função personalizada da posição e escolha rápida isolada: o Copiloto já faz o mesmo efeito com `set_tactic` e `suggest_rotation`.
- Férias, demissão do técnico e candidatura a vagas: decisão de carreira, fora do Copiloto.
- Afiliados (`ClubRequestAffiliateLink`, `EventFmxProposeAffiliateLink`): entram no ticket da diretoria só como pedido; acordo de afiliado não foi mapeado.
- Trocas de jogadores no estilo MLS (`TradeOffer*`): regra da MLS, não vale para o Santos.

## Notas para quem implementar

- Toda ação de menu deve ser conferida antes no save de teste com os passos 1 e 2 de "Como ler a tabela". O menu muda com o estado: a ação some depois de usada (elogio) ou troca por outra (proposta vira "retirar").
- O tipo da entrada decide a rota: `Event` 44 vai por `/native EventFmxDoAction`; `Event` 0 vai por `/contextaction`; entrada de painel vai por `/newsaction`. `EventFmxDoAction` não abre painel nem card.
- Diálogos de confirmação (Sim/Não) aparecem em liberar, rescindir, cancelar amistoso e cancelar negociação. Eles vão por `/click` no botão do diálogo (`C:\Code\dialogs.py`).
- Sessões de edição (`*EditSession`, `EventFmxSetPieceEditSessionBegin/End`) precisam abrir e fechar como a tela faz; o `/native` sozinho num campo sem a sessão aberta pode não gravar.
- Regra do save: dry run por padrão, `apply: true` para escrever, `ISaveGuard.PrepareWriteAsync()` antes de todo apply. Smoke só no save de teste e fora de partida ao vivo.
