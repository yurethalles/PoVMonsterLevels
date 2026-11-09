# Changelog

## 1.0.2

- Reformatted `Default.json` for readability without changing its 103 prefab-level entries.

## 1.0.1

- Added the `Fixed` low-damage mode for red creatures.
- Added the server-synchronized `Fixed red mob damage` setting. Its default value is `0.10`, so red creatures receive exactly 10% of player damage regardless of the level difference.
- Replaced all bundled creature tables with one `Default.json` containing the current configured Valheim creature prefabs and progression levels.

## 1.0.0

- First public release.
- Adds configurable JSON-based creature levels inspired by EpicMMOSystem.
- Uses Path of Valheiman main progression levels for damage-gap and red-creature checks.
- Adds vanilla-star level scaling, synchronized server configuration, and server-synchronized creature tables.
- Displays creature levels alongside the vanilla health bar.
- Includes configurable drop removal for red creatures, tamed-creature kills, creature kills, and deaths without a player attacker.
