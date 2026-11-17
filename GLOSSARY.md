# MCPFM (fork do Vini)

Como o Claude joga uma carreira do Football Manager 27 junto com o Vini, pelas tools do MCPFM.

## Language

**Copiloto**:
O modo padrão de jogo: o Vini guia a carreira, e o Claude analisa, sugere e executa o que ele pede, com uma frase de motivo para cada decisão.
_Avoid_: modo guiado, assistente

**Modo piloto**:
O modo em que o Claude avança o calendário e decide tudo sozinho até um ponto de parada que o Vini definiu.
_Avoid_: autopilot, piloto automático, modo autônomo

**Ordem de piloto**:
O pedido do Vini que liga o Modo piloto: diz até onde avançar e, se ele quiser, em quais decisões o Claude deve chamá-lo.
_Avoid_: comando, missão

**Ponto de decisão**:
Um momento em que o avanço do calendário para no Modo piloto e o Claude decide: o dia de jogo, um estado do jogo que o avanço não conhece e a conversa semanal com jogadores.
_Avoid_: checkpoint, parada

**Save de teste**:
A cópia da carreira (`MCPFM investigation`) onde toda descoberta e todo teste acontecem; o save real do Vini nunca recebe escrita de teste.
_Avoid_: save de investigação, sandbox
