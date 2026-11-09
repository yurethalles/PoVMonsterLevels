using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace PoVMonsterLevels;

internal sealed class CreatureDatabase
{
    private readonly List<CreatureDefinition> _definitions = new();
    internal string Path { get; }

    internal CreatureDatabase(string path) => Path = path;

    internal void Reload()
    {
        _definitions.Clear();
        if (!File.Exists(Path))
        {
            Plugin.Log.LogWarning($"Creature file not found: {Path}");
            return;
        }
        try
        {
            var definitions = JsonConvert.DeserializeObject<List<CreatureDefinition>>(File.ReadAllText(Path));
            if (definitions != null) _definitions.AddRange(definitions.Where(x => !string.IsNullOrWhiteSpace(x.Prefab)));
            Plugin.Log.LogInfo($"Loaded {_definitions.Count} creature definitions.");
        }
        catch (Exception e) { Plugin.Log.LogError($"Could not read creatures.json: {e}"); }
    }

    internal CreatureDefinition? Find(Character character)
    {
        var prefab = character.gameObject.name.Replace("(Clone)", string.Empty).Trim();
        var biome = Heightmap.FindBiome(character.transform.position).ToString();
        return _definitions.FirstOrDefault(x => x.Matches(prefab, biome) && !string.IsNullOrWhiteSpace(x.Biome))
            ?? _definitions.FirstOrDefault(x => x.Matches(prefab, biome));
    }
}
