using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace PoVMonsterLevels;

[HarmonyPatch]
internal static class CreaturePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnemyHud), "ShowHud")]
    private static void EnemyHudShowHud(EnemyHud __instance, Character c)
    {
        if (!Plugin.ShowOverheadLevel.Value || c == null || c is Player || c.IsTamed()) return;
        var huds = AccessTools.Field(typeof(EnemyHud), "m_huds")?.GetValue(__instance) as IDictionary;
        if (huds == null || !huds.Contains(c)) return;
        var hudData = huds[c];
        var gui = hudData == null ? null : AccessTools.Field(hudData.GetType(), "m_gui")?.GetValue(hudData) as GameObject;
        if (gui == null) return;

        var name = gui.transform.Find("Name");
        if (name == null) return;

        var level = CreatureLevel.Get(c);
        var levelTransform = name.Find("PoVMonsterLevel");
        if (level <= 0)
        {
            if (levelTransform != null) levelTransform.gameObject.SetActive(false);
            return;
        }

        if (levelTransform == null)
        {
            var clone = Object.Instantiate(name.gameObject, name);
            clone.name = "PoVMonsterLevel";
            levelTransform = clone.transform;
            levelTransform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -22f);
        }

        levelTransform.gameObject.SetActive(true);
        var nameText = name.GetComponent<TextMeshProUGUI>();
        var levelText = levelTransform.GetComponent<TextMeshProUGUI>();
        if (nameText == null || levelText == null) return;

        var playerLevel = Player.m_localPlayer == null ? 0 : PoVLevelAdapter.GetPlayerLevel(Player.m_localPlayer);
        var color = CreatureLevel.ColorFor(level, playerLevel);
        levelText.text = Plugin.LevelText.Value.Replace("@", level.ToString());
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

        var multiplier = (playerLevel + Plugin.DamageOffset.Value) / (float)creatureLevel;
        hit.ApplyModifier(UnityEngine.Mathf.Clamp(multiplier, Plugin.MinimumDamage.Value, 1f));
    }
}
