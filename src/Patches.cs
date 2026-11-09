using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PoVMonsterLevels;

[HarmonyPatch]
internal static class CreaturePatches
{
    private static readonly FieldInfo? LastHitField = AccessTools.Field(typeof(Character), "m_lastHit");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
    private static void EnemyHudShowHud(EnemyHud __instance, Character c)
    {
        var huds = AccessTools.Field(typeof(EnemyHud), "m_huds")?.GetValue(__instance) as IDictionary;
        if (huds == null || !huds.Contains(c)) return;
        var hudData = huds[c];
        var gui = hudData == null ? null : AccessTools.Field(hudData.GetType(), "m_gui")?.GetValue(hudData) as GameObject;
        UpdateLevelHud(c, gui);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
    private static void EnemyHudUpdateHuds(EnemyHud __instance)
    {
        var huds = AccessTools.Field(typeof(EnemyHud), "m_huds")?.GetValue(__instance) as IDictionary;
        if (huds == null) return;
        foreach (DictionaryEntry entry in huds)
        {
            if (entry.Key is not Character character || entry.Value == null) continue;
            var gui = AccessTools.Field(entry.Value.GetType(), "m_gui")?.GetValue(entry.Value) as GameObject;
            UpdateLevelHud(character, gui);
        }
    }

    private static void UpdateLevelHud(Character c, GameObject? gui)
    {
        if (!Plugin.ShowOverheadLevel.Value || c == null || c is Player || c.IsTamed() || gui == null) return;
        var name = gui.transform.Find("Name");
        if (name == null) return;

        var configured = CreatureLevel.IsConfigured(c);
        var level = CreatureLevel.Get(c);
        var levelTransform = name.Find("PoVMonsterLevel");
        if (!configured)
        {
            if (levelTransform != null) levelTransform.gameObject.SetActive(false);
            var normalNameText = name.GetComponent<TextMeshProUGUI>();
            if (normalNameText != null) normalNameText.color = Color.white;
            return;
        }

        if (levelTransform == null)
        {
            var clone = Object.Instantiate(name.gameObject, name);
            clone.name = "PoVMonsterLevel";
            levelTransform = clone.transform;
        }

        levelTransform.gameObject.SetActive(true);
        levelTransform.GetComponent<RectTransform>().anchoredPosition = c.IsBoss()
            ? Plugin.BossLevelPosition.Value
            : Plugin.MobLevelPosition.Value;
        var nameText = name.GetComponent<TextMeshProUGUI>();
        var levelText = levelTransform.GetComponent<TextMeshProUGUI>();
        if (nameText == null || levelText == null) return;

        var playerLevel = Player.m_localPlayer == null ? 0 : PoVLevelAdapter.GetPlayerLevel(Player.m_localPlayer);
        var color = level <= 0 ? Color.yellow : CreatureLevel.ColorFor(level, playerLevel);
        levelText.text = Plugin.LevelText.Value.Replace("@", level <= 0 ? "???" : level.ToString());
        levelText.color = color;
        nameText.color = color;
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

        if (Plugin.DamageMode.Value == DamageMode.Fixed)
        {
            hit.ApplyModifier(UnityEngine.Mathf.Clamp01(Plugin.FixedDamage.Value));
            return;
        }

        var multiplier = (playerLevel + Plugin.DamageOffset.Value) / (float)creatureLevel;
        hit.ApplyModifier(UnityEngine.Mathf.Clamp(multiplier, Plugin.MinimumDamage.Value, 1f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    private static void RemoveRedMobDrops(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
    {
        if (__result == null) return;
        var creature = __instance.GetComponent<Character>();
        if (creature == null) return;
        var lastHit = LastHitField?.GetValue(creature) as HitData;
        var killer = lastHit?.GetAttacker();

        if (killer is Player player)
        {
            if (!Plugin.RemoveDropsFromRedMobs.Value) return;
            var creatureLevel = CreatureLevel.Get(creature);
            var playerLevel = PoVLevelAdapter.GetPlayerLevel(player);
            if (creatureLevel > playerLevel + Plugin.LevelGap.Value) __result.Clear();
            return;
        }

        if (killer is Character killerCreature)
        {
            if (killerCreature.IsTamed() && Plugin.RemoveDropsFromTamedKills.Value) __result.Clear();
            else if (!killerCreature.IsTamed() && Plugin.RemoveDropsFromCreatureKills.Value) __result.Clear();
            return;
        }

        if (Plugin.RemoveDropsWithoutKiller.Value) __result.Clear();
    }
}
