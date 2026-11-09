using System;

namespace PoVMonsterLevels;

public sealed class CreatureDefinition
{
    public string Prefab { get; set; } = string.Empty;
    public string? Biome { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int MinXp { get; set; }
    public int MaxXp { get; set; }

    public bool Matches(string prefab, string biome) =>
        string.Equals(Prefab, prefab, StringComparison.OrdinalIgnoreCase) &&
        (string.IsNullOrWhiteSpace(Biome) || string.Equals(Biome, biome, StringComparison.OrdinalIgnoreCase));
}
