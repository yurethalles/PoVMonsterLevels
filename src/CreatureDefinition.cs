using System;
using Newtonsoft.Json;

namespace PoVMonsterLevels;

public sealed class CreatureDefinition
{
    [JsonProperty("name")]
    public string Prefab { get; set; } = string.Empty;
    [JsonProperty("level")]
    public int Level { get; set; }
    public bool Matches(string prefab) =>
        string.Equals(Prefab, prefab, StringComparison.OrdinalIgnoreCase);
}
