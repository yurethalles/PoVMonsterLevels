using System;
using System.Reflection;

namespace PoVMonsterLevels;

// PoV exposes the primary character level through its public API. Keeping this reflection
// based avoids a binary dependency while deliberately selecting the main progression level,
// not the separate craft level.
internal static class PoVLevelAdapter
{
    private static MethodInfo? _getter;
    private static bool _searched;

    internal static int GetPlayerLevel(Player player)
    {
        FindGetter();
        if (_getter == null) return 1;
        try
        {
            return Math.Max(1, Convert.ToInt32(_getter.Invoke(null, Array.Empty<object>())));
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
            var progression = pov.GetType("Valheiman.Api.Progression");
            _getter = progression?.GetProperty("Level", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
            if (_getter == null) Plugin.Log.LogError("PoV main progression level getter was not found; level gap is using fallback level 1.");
            else Plugin.Log.LogInfo($"Using PoV level getter: {_getter.DeclaringType?.FullName}.{_getter.Name}");
        }
        catch (ReflectionTypeLoadException e)
        {
            Plugin.Log.LogError($"Could not inspect PathOfValheiman: {e.Message}");
        }
    }
}
