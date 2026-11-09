using System;
using System.Reflection;

namespace PoVMonsterLevels;

// PoV exposes the primary character level through its public API. Keeping this reflection
// based avoids a binary dependency while deliberately selecting the main progression level,
// not the separate craft level.
internal static class PoVLevelAdapter
{
    private static MethodInfo? _peek;
    private static FieldInfo? _level;
    private static bool _searched;

    internal static int GetPlayerLevel(Player player)
    {
        FindGetter();
        if (_peek == null || _level == null) return 1;
        try
        {
            var progress = _peek.Invoke(null, new object[] { player });
            return progress == null ? 1 : Math.Max(1, Convert.ToInt32(_level.GetValue(progress)));
        }
        catch (Exception e)
        {
            Plugin.Log.LogWarning($"Could not read PoV character level: {e.Message}");
            return 1;
        }
    }

    private static void FindGetter()
    {
        if (_searched) return;
        _searched = true;
        Assembly? pov = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "PathOfValheiman")
            {
                pov = assembly;
                break;
            }
        }
        if (pov == null) { Plugin.Log.LogError("PathOfValheiman assembly was not found."); return; }
        try
        {
            var progression = pov.GetType("PassiveTree.Progress");
            _peek = progression?.GetMethod("Peek", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
            _level = progression?.GetField("Level", BindingFlags.Public | BindingFlags.Instance);
            if (_peek == null || _level == null)
                Plugin.Log.LogError("PoV per-player progression getter was not found; level gap is using fallback level 1.");
            else Plugin.Log.LogInfo("Using PoV per-player main progression level: PassiveTree.Progress.Peek(player).Level");
        }
        catch (ReflectionTypeLoadException e)
        {
            Plugin.Log.LogError($"Could not inspect PathOfValheiman: {e.Message}");
        }
    }
}
