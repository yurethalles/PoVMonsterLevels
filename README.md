# PoV Monster Levels

Plugin BepInEx para usar o nível de personagem do Path of Valheiman como a única
progressão do jogador, com níveis de criaturas e gap no estilo do EpicMMO.

O mod é deliberadamente apenas o módulo de controle de criaturas do EpicMMO:
mesma tabela JSON e mesma lógica de nível base + estrelas, com o nível do jogador
vindo do PoV. XP, atributos, HUD de XP, orbes, poções, receitas e pontos do EpicMMO
não fazem parte dele.

## O que ele faz

- Lê todos os JSONs em `config/creatures/`, no mesmo formato e com os mesmos prefabs
  das tabelas do EpicMMO, e atribui um nível persistente (ZDO) ao monstro.
- O campo `level` do JSON é fixo; somente as estrelas vanilla são somadas ao nível,
  se configurado.
- Mostra `[Nv. X]` acima da criatura, colorido conforme a diferença para o jogador local.
- Bloqueia ou escala dano de jogadores quando a criatura excede o gap configurado.
- Sincroniza regras e todas as tabelas de criaturas pelo servidor, usando ServerSync;
  clientes não precisam manter cópias do JSON.
- Não cria XP, atributos, orbes, poções, receitas nem pontos. O PoV continua sendo o
  responsável exclusivo pelo XP e pela árvore.

## Instalação de desenvolvimento

Compile com `dotnet build`. Copie `bin/Release/net48/PoVMonsterLevels.dll` e
`bin/Release/net48/ServerSync.dll` para `BepInEx/plugins/PoVMonsterLevels/`.
No servidor, copie a pasta `config/creatures/` para
`BepInEx/config/PoVMonsterLevels/creatures/` antes da primeira execução.

Servidor e clientes precisam da DLL e do ServerSync. Somente o servidor precisa dos JSONs.
