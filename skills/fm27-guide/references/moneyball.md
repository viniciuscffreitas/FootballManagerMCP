# Moneyball (método do Allan FCL)

Fonte: 16 vídeos do canal youtube.com/@AllanFCL (transcrições lidas em 17/11/2026) e a planilha
"Allan FCL - Moneyball - FM2026.rar" (Moneyball FM26.xlsm + views .fmf). Tudo aqui é *probable*: é o método
dele, não lido no save. IDs entre parênteses são vídeos do YouTube.

## Ideia
- Contratar pelo desempenho da temporada inteira, não por atributos ou estrelas. A IA escala pelo CA, então quem
  joga muito e rende numa liga tem nível para ela (Cgyrf9t7brc, 57det5fh3fY).
- Atributos só confirmam perfil físico e encaixe na função: velocidade, aceleração, impulsão, altura (AkdNA-VV0eE).
- Moneyball = o melhor jogador que o clube pode pagar. Não é garimpar newgen nem revender promessa (AkdNA-VV0eE, 35qxExedLz0).

## Fluxo
1. Diagnosticar pela tabela e pelo xG do próprio time qual setor falha (Co4we5CXd2s, Cgyrf9t7brc).
2. Busca na base: posição natural, ligas (Brasil A/B, Argentina, Uruguai, Colômbia, Equador, Chile; top 5 da Europa
   se couber), teto de valor, mínimo de 1000 minutos (2000 para goleiro, zagueiro e atacante), só quem tem interesse
   em vir (ofCHI-DA9GQ, Co4we5CXd2s).
3. Exportar e eliminar coluna por coluna pelos cortes abaixo, até sobrar 3 a 5 nomes.
4. Truque: marcar o próprio titular na lista e excluir todo mundo pior que ele (Cgyrf9t7brc).
5. Confirmar perfil físico e função nos atributos; contrato curto ou empréstimo para testar quando der (ofCHI-DA9GQ).

## Cortes
- Geral: nota média 6,9 (6,8 aceitável, 6,69 já não é consistente); 11 a 12 jogos completos ou mais.
- Atacante: gols/90 ≥ 0,33 (alvo 0,5); gols acima do xG; conversão ≥ 15%; ≥ 40-50% dos chutes no alvo;
  ≤ 7 finalizações por gol; descontar pênaltis. Atacante de pressão: ≥ 8 pressões/90 e > 30% concluídas.
- Ponta e lateral ofensivo: xG+xA sem pênalti ≥ 0,2/90, passes decisivos, oportunidades claras criadas, 12 km/90.
- Armador: xA/90, passes decisivos/90; bola parada: ≥ 38 tentativas e > 15% de acerto.
- Volante/pivô: > 40 passes/90 com > 88% de acerto, desarme > 75%.
- Box-to-box: > 12 km/90 e pressões concluídas/90.
- Zagueiro: ≥ 1,85 m, cabeceios ganhos ≥ 70%, ≥ 40 passes/90 com ≥ 85-88%, ≤ 6 posses perdidas/90, zero erros.
- Goleiro: ≥ 2000 min, ≥ 70% de defesas, xG evitado positivo, passe 65-70%.
- Índices da planilha FM26 (aba Avançados): IPO = ((gols sem pên×10 + chutes no gol×5 + dist/90×6 + xG sem pên×8 +
  xA×6) / jogos completos) × √(min/900); IVO pesa volume de chutes e oportunidades; média = 0,4·IPO + 0,6·IVO.

## Armadilhas
- Liga fraca ou não carregada infla números ("posse perdida zero" = liga inativa); MLS e México inflados.
- Amostra pequena e reserva de luxo (média alta com 24-32 min por jogo).
- Estadual infla os números no começo do ano.
- Comparar sempre por 90, não totais.
- Atacante que não converte pode ser problema de tática/parceiro, não do jogador (-7E7utk7rfI).

## Custo, venda e elenco
- Dividir orçamento e folga salarial pelo número de reforços; guardar mais para goleiro, zagueiro e atacante.
- Prioridade: fim de contrato, livres, empréstimo. Teto salarial próprio. Propostas com parcelas e bônus por jogos.
- Vender velho com salário alto ("olhe pelo salário, não pelo valor"), pedindo 50% de futura venda; veterano de
  35+ por zero para liberar folha; baixar o preço em passos até sair (miDLvzZjJYo, Iw65P7pb-fY).
- Elenco de 3 por posição (33-34 jogadores). Sub-23 com futuro: empréstimo; sem futuro: venda.
- Renovar por ordem de importância, cortando bônus e subindo a multa (jQqDgcYAbIk).
- Não delegar transferências à diretoria: ela compra caro (jQqDgcYAbIk).

## Com as tools
1. `run_player_search` (dry run, depois `apply`): posição natural, idade, interesse em vir (`interestType`/`interestLevel`).
   A busca do jogo não tem regra de liga, minutos nem valor.
2. `get_scouted_players source=search` com `leagues` (nome ou parte, sem acento), `minMinutes` e `minAverageRating`:
   lê os primeiros `scan` resultados (300 por padrão, até 1000) com os números da temporada e corta na API;
   `scanned` diz quantos leu. `includeStats` sozinho traz os números sem cortar (vale para shortlist e focos).
3. Aplicar os cortes por posição acima nos campos de `performance` (totais e por 90; goleiro: `savePercentage`,
   `expectedGoalsPrevented`).
4. Limites: os números somam todas as competições do clube na temporada (estadual e copa entram); valor e salário
   vêm só como texto, então o teto de preço é conferido à mão.
