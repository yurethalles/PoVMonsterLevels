using System;
using Newtonsoft.Json;

namespace PoVMonsterLevels;

public sealed class CreatureDefinition
{
    [JsonProperty("name")]
    public string Prefab { get; set; } = string.Empty;
    [JsonProperty("biome")]
    public string? Biome { get; set; }
    [JsonProperty("level")]
    public int Level { get; set; }
    // Retained to use EpicMMO tables unchanged. PoV awards the combat XP itself.
    [JsonProperty("minExp")]
    public int MinXp { get; set; }
    [JsonProperty("maxExp")]
    public int MaxXp { get; set; }

    public bool Matches(string prefab, string biome) =>
        string.Equals(Prefab, prefab, StringComparison.OrdinalIgnoreCase) &&
        (string.IsNullOrWhiteSpace(Biome) || string.Equals(Biome, biome, StringComparison.OrdinalIgnoreCase));
}
