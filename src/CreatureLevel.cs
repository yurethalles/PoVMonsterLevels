using UnityEngine;

namespace PoVMonsterLevels;

internal static class CreatureLevel
{
    private const string LastPlayerLevelKey = "PoVMonsterLevels_LastPlayerLevel";

    internal static int Get(Character creature)
    {
        if (!Plugin.EnabledCreatureLevel.Value || creature == null || creature is Player) return 0;
        var definition = Plugin.Creatures.Find(creature);
        if (definition == null) return 0;

        // Exactly like EpicMMO: the JSON level is the base and the current vanilla
        // star level is evaluated when needed. It must not be cached during Awake,
        // because the networked star level can be assigned after that callback.
        var level = Mathf.Max(0, definition.Level);
        if (level > 0 && Plugin.StarsAddLevels.Value) level += Mathf.Max(0, creature.GetLevel() - 1);
        return level;
    }

    internal static Color ColorFor(int creatureLevel, int playerLevel)
    {
        var gap = Plugin.LevelGap.Value;
        if (creatureLevel > playerLevel + gap) return Color.red;
        if (creatureLevel < playerLevel - gap) return Color.cyan;
        return Color.white;
    }

    internal static void RecordAttackerLevel(Character creature, int playerLevel)
    {
        var view = creature.GetComponent<ZNetView>();
        if (view == null || !view.IsValid() || !view.IsOwner()) return;
        view.GetZDO()?.Set(LastPlayerLevelKey, playerLevel);
    }

    internal static int GetRecordedAttackerLevel(Character creature)
    {
        var zdo = creature.GetComponent<ZNetView>()?.GetZDO();
        return zdo?.GetInt(LastPlayerLevelKey, 0) ?? 0;
    }
}
