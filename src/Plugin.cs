using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace PoVMonsterLevels;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("treextr.pathofvalheiman", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string PluginGuid = "br.povmonsterlevels";
    internal const string PluginName = "PoV Monster Levels";
    internal const string PluginVersion = "0.1.0";
    internal static ManualLogSource Log = null!;
    internal static Plugin Instance = null!;
    internal static CreatureDatabase Creatures = null!;
    internal static ConfigEntry<int> LevelGap = null!;
    internal static ConfigEntry<DamageMode> DamageMode = null!;
    internal static ConfigEntry<float> MinimumDamage = null!;
    internal static ConfigEntry<int> DamageOffset = null!;
    internal static ConfigEntry<bool> StarsAddLevels = null!;
    internal static ConfigEntry<bool> ShowOverheadLevel = null!;
    internal static ConfigEntry<string> LevelText = null!;

    private FileSystemWatcher? _watcher;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        LevelGap = Config.Bind("Gap", "Level gap", 10, "O jogador causa dano total até esta diferença de nível.");
        DamageMode = Config.Bind("Gap", "Damage mode", PoVMonsterLevels.DamageMode.Block, "Block: zera dano. Scale: reduz proporcionalmente.");
        MinimumDamage = Config.Bind("Gap", "Minimum scaled damage", 0.10f, new ConfigDescription("Piso no modo Scale.", new AcceptableValueRange<float>(0f, 1f)));
        DamageOffset = Config.Bind("Gap", "Damage offset", 0, "Equivale ao Low_damage_config do EpicMMO.");
        StarsAddLevels = Config.Bind("Creatures", "Stars add levels", true, "Cada estrela vanilla soma um nível.");
        ShowOverheadLevel = Config.Bind("HUD", "Show level above creature", true, "Mostra o nível sobre a cabeça da criatura.");
        LevelText = Config.Bind("HUD", "Level text", "[Nv. @]", "@ será substituído pelo nível.");

        var directory = System.IO.Path.Combine(Paths.ConfigPath, "PoVMonsterLevels");
        Directory.CreateDirectory(directory);
        var file = System.IO.Path.Combine(directory, "creatures.json");
        Creatures = new CreatureDatabase(file);
        Creatures.Reload();
        _watcher = new FileSystemWatcher(directory, "creatures.json") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
        _watcher.Changed += (_, _) => Invoke(nameof(ReloadDefinitions), 0.25f);
        _watcher.Created += (_, _) => Invoke(nameof(ReloadDefinitions), 0.25f);

        new Harmony(PluginGuid).PatchAll();
        Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void ReloadDefinitions() => Creatures.Reload();
    private void OnDestroy() => _watcher?.Dispose();
}

internal enum DamageMode { Block, Scale }
