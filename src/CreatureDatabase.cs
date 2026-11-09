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
    internal string DirectoryPath { get; }

    internal CreatureDatabase(string directoryPath) => DirectoryPath = directoryPath;

    internal void Reload()
    {
        _definitions.Clear();
        if (!Directory.Exists(DirectoryPath))
        {
            Plugin.Log.LogWarning($"Creature directory not found: {DirectoryPath}");
            return;
        }
        try
        {
            foreach (var file in Directory.GetFiles(DirectoryPath, "*.json", SearchOption.AllDirectories))
            {
                var definitions = JsonConvert.DeserializeObject<List<CreatureDefinition>>(File.ReadAllText(file));
                if (definitions != null) _definitions.AddRange(definitions.Where(x => !string.IsNullOrWhiteSpace(x.Prefab)));
            }
            Plugin.Log.LogInfo($"Loaded {_definitions.Count} creature definitions.");
        }
        catch (Exception e) { Plugin.Log.LogError($"Could not read creatures.json: {e}"); }
    }

    internal string Serialize() => JsonConvert.SerializeObject(_definitions, Formatting.None);

    internal void LoadFromServer(string json)
    {
        try
        {
            var definitions = JsonConvert.DeserializeObject<List<CreatureDefinition>>(json) ?? new List<CreatureDefinition>();
            _definitions.Clear();
            _definitions.AddRange(definitions.Where(x => !string.IsNullOrWhiteSpace(x.Prefab)));
            Plugin.Log.LogInfo($"Received {_definitions.Count} creature definitions from server.");
        }
        catch (Exception e) { Plugin.Log.LogError($"Could not apply creature table from server: {e}"); }
    }

    internal CreatureDefinition? Find(Character character)
    {
        var prefab = character.gameObject.name.Replace("(Clone)", string.Empty).Trim();
        var biome = Heightmap.FindBiome(character.transform.position).ToString();
        return _definitions.FirstOrDefault(x => x.Matches(prefab, biome) && !string.IsNullOrWhiteSpace(x.Biome))
            ?? _definitions.FirstOrDefault(x => x.Matches(prefab, biome));
    }
}
