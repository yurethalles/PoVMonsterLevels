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
        if (!Directory.Exists(DirectoryPath))
        {
            Plugin.Log.LogWarning($"Creature directory not found: {DirectoryPath}");
            return;
        }
        var loaded = new List<CreatureDefinition>();
        var knownPrefabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var valid = true;
        foreach (var file in Directory.GetFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var definitions = JsonConvert.DeserializeObject<List<CreatureDefinition>>(File.ReadAllText(file));
                if (definitions == null) throw new JsonSerializationException("A raiz deve ser uma lista de criaturas.");
                foreach (var definition in definitions.Where(x => !string.IsNullOrWhiteSpace(x.Prefab)))
                {
                    if (!knownPrefabs.Add(definition.Prefab))
                    {
                        Plugin.Log.LogWarning($"Duplicate creature prefab '{definition.Prefab}' in {Path.GetFileName(file)}; keeping the first entry.");
                        continue;
                    }
                    loaded.Add(definition);
                }
            }
            catch (Exception e)
            {
                valid = false;
                Plugin.Log.LogError($"Could not read creature table '{Path.GetFileName(file)}': {e.Message}");
            }
        }
        if (!valid)
        {
            Plugin.Log.LogWarning("Creature tables were not replaced because one or more JSON files are invalid.");
            return;
        }
        _definitions.Clear();
        _definitions.AddRange(loaded);
        Plugin.Log.LogInfo($"Loaded {_definitions.Count} creature definitions.");
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
        return _definitions.FirstOrDefault(x => x.Matches(prefab));
    }
}
