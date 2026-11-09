using UnityEngine;

namespace PoVMonsterLevels;

internal static class CreatureLevel
{
    private const string LevelKey = "PoVMonsterLevels_Level";

    internal static int Get(Character creature)
    {
        var zdo = creature.GetComponent<ZNetView>()?.GetZDO();
        return zdo == null ? 0 : zdo.GetInt(LevelKey, 0);
    }

    internal static void EnsureAssigned(Character creature)
    {
        var view = creature.GetComponent<ZNetView>();
        if (creature is Player || view == null || !view.IsValid() || !view.IsOwner()) return;
        var zdo = view.GetZDO();
        if (zdo == null || zdo.GetInt(LevelKey, 0) > 0) return;
        var definition = Plugin.Creatures.Find(creature);
        if (definition == null) return;
        var low = Mathf.Max(1, definition.MinLevel);
        var high = Mathf.Max(low, definition.MaxLevel);
        var level = Random.Range(low, high + 1);
        if (Plugin.StarsAddLevels.Value) level += creature.GetLevel() - 1;
        zdo.Set(LevelKey, level);
    }

    internal static Color ColorFor(int creatureLevel, int playerLevel)
    {
        var gap = Plugin.LevelGap.Value;
        if (creatureLevel > playerLevel + gap) return Color.red;
        if (creatureLevel < playerLevel - gap) return Color.cyan;
        return Color.white;
    }
}
