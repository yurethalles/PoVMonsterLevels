using System;
using System.Linq;
using System.Reflection;

namespace PoVMonsterLevels;

// The PoV DLL does not publish a stable public API. This adapter deliberately locates its
// player-level getter at runtime, avoiding a compile-time dependency on private PoV types.
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
            var arguments = _getter.GetParameters().Length == 0 ? Array.Empty<object>() : new object[] { player };
            return Math.Max(1, Convert.ToInt32(_getter.Invoke(null, arguments)));
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
        var pov = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "PathOfValheiman");
        if (pov == null) { Plugin.Log.LogError("PathOfValheiman assembly was not found."); return; }
        try
        {
            _getter = pov.GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(method => method.ReturnType == typeof(int) && method.Name.IndexOf("level", StringComparison.OrdinalIgnoreCase) >= 0)
                .FirstOrDefault(method =>
                {
                    var p = method.GetParameters();
                    return p.Length == 1 && p[0].ParameterType == typeof(Player);
                });
            if (_getter == null) Plugin.Log.LogError("PoV level getter not found; gap protection is disabled until this adapter is updated.");
            else Plugin.Log.LogInfo($"Using PoV level getter: {_getter.DeclaringType?.FullName}.{_getter.Name}");
        }
        catch (ReflectionTypeLoadException e)
        {
            Plugin.Log.LogError($"Could not inspect PathOfValheiman: {e.Message}");
        }
    }
}
