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
- Mostra `[Nv. X]` junto do nome na barra de vida vanilla da criatura, colorido conforme a diferença para o jogador local.
- Bloqueia ou escala dano de jogadores quando a criatura excede o gap configurado.
- Sincroniza regras e todas as tabelas de criaturas pelo servidor, usando ServerSync;
  clientes não precisam manter cópias do JSON.
- Não cria XP, atributos, orbes, poções, receitas nem pontos. O PoV continua sendo o
  responsável exclusivo pelo XP e pela árvore.

## Instalação

Baixe ou compile a DLL de lançamento e copie somente
`PoVMonsterLevels.dll` para `BepInEx/plugins/PoVMonsterLevels/` no servidor e em
todos os clientes. O ServerSync está incorporado à DLL.

Na primeira inicialização do servidor, o mod cria automaticamente os 26 arquivos
de tabela diretamente em `BepInEx/config/PoVMonsterLevels/`, usando os mesmos JSONs
incluídos com este projeto. Arquivos já existentes nunca são sobrescritos. O arquivo
principal de configuração é `BepInEx/config/Dorn.PoVMonsterLevels.cfg`.

Servidor e clientes precisam do PoV e desta DLL. Somente o servidor precisa dos JSONs.

## Compilação

Execute `dotnet build -c Release`. O processo une automaticamente o ServerSync ao
mod e grava o arquivo distribuível único em `dist/PoVMonsterLevels-0.0.4.dll`.
