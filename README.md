# PoV Monster Levels

Plugin BepInEx para usar o nível de personagem do Path of Valheiman como a única
progressão do jogador, com níveis de criaturas e gap no estilo do EpicMMO.

## O que ele faz

- Lê `config/creatures.json` e atribui um nível persistente (ZDO) ao monstro.
- Suporta entrada geral por prefab e substituição por bioma.
- Soma as estrelas vanilla ao nível, se configurado.
- Mostra `[Nv. X]` acima da criatura, colorido conforme a diferença para o jogador local.
- Bloqueia ou escala dano de jogadores quando a criatura excede o gap configurado.
- Não cria XP, atributos, orbes, poções, receitas nem pontos. O PoV continua sendo o
  responsável exclusivo pelo XP e pela árvore.

## Instalação de desenvolvimento

Compile com `dotnet build`. Copie `bin/Debug/net48/PoVMonsterLevels.dll`
para `BepInEx/plugins/PoVMonsterLevels/`. Copie `config/creatures.json` para
`BepInEx/config/PoVMonsterLevels/creatures.json` antes da primeira execução.

Servidor e clientes precisam ter exatamente a mesma DLL e o mesmo JSON.
