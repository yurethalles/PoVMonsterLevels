using HarmonyLib;

namespace PoVMonsterLevels;

[HarmonyPatch]
internal static class CreaturePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Character), "Awake")]
    private static void CharacterAwake(Character __instance)
    {
        if (__instance is Player) return;
        CreatureLevel.EnsureAssigned(__instance);
        if (__instance.GetComponent<LevelBillboard>() == null) __instance.gameObject.AddComponent<LevelBillboard>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), "RPC_Damage")]
    private static void LimitDamage(Character __instance, HitData hit)
    {
        if (__instance is Player || hit == null) return;
        var attacker = hit.GetAttacker() as Player;
        if (attacker == null) return;
        var creatureLevel = CreatureLevel.Get(__instance);
        if (creatureLevel <= 0) return;
        var playerLevel = PoVLevelAdapter.GetPlayerLevel(attacker);
        var difference = creatureLevel - playerLevel;
        if (difference <= Plugin.LevelGap.Value) return;

        if (Plugin.DamageMode.Value == DamageMode.Block)
        {
            hit.ApplyModifier(0f);
            return;
        }

        var multiplier = (playerLevel + Plugin.DamageOffset.Value) / (float)creatureLevel;
        hit.ApplyModifier(UnityEngine.Mathf.Clamp(multiplier, Plugin.MinimumDamage.Value, 1f));
    }
}
