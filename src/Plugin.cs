using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace PoVMonsterLevels;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("treextr.pathofvalheiman", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string PluginGuid = "Dorn.PoVMonsterLevels";
    internal const string PluginName = "PoV Monster Levels";
    internal const string PluginVersion = "0.0.3";
    internal static ManualLogSource Log = null!;
    internal static Plugin Instance = null!;
    internal static CreatureDatabase Creatures = null!;
    internal static ConfigEntry<int> LevelGap = null!;
    internal static ConfigEntry<bool> EnabledCreatureLevel = null!;
    internal static ConfigEntry<DamageMode> DamageMode = null!;
    internal static ConfigEntry<float> MinimumDamage = null!;
    internal static ConfigEntry<int> DamageOffset = null!;
    internal static ConfigEntry<bool> StarsAddLevels = null!;
    internal static ConfigEntry<bool> ShowOverheadLevel = null!;
    internal static ConfigEntry<string> LevelText = null!;
    internal static readonly ConfigSync ConfigSync = new(PluginGuid)
    {
        DisplayName = PluginName,
        CurrentVersion = PluginVersion,
        MinimumRequiredVersion = PluginVersion
    };
    internal static CustomSyncedValue<string> CreatureTable = null!;

    private FileSystemWatcher? _watcher;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        var lockConfig = SyncConfig("General", "Force server config", true, "Impede clientes de alterar regras do servidor.");
        const string creatureControl = "2.Creature level control";
        EnabledCreatureLevel = SyncConfig(creatureControl, "Enabled creature level", true, "Ativa o controle de níveis de criaturas.");
        LevelGap = SyncConfig(creatureControl, "Level gap", 10, "O jogador causa dano total até esta diferença de nível.");
        DamageMode = SyncConfig(creatureControl, "Low damage mode", PoVMonsterLevels.DamageMode.Block, "Block: zera dano. Scale: reduz proporcionalmente como EpicMMO.");
        MinimumDamage = SyncConfig(creatureControl, "Minimum scaled damage", 0.10f, new ConfigDescription("Piso no modo Scale.", new AcceptableValueRange<float>(0f, 1f)));
        DamageOffset = SyncConfig(creatureControl, "Low damage config", 0, "Equivale ao Low_damage_config do EpicMMO.");
        StarsAddLevels = SyncConfig(creatureControl, "Mob level increase per star", true, "Cada estrela vanilla soma um nível.");
        ShowOverheadLevel = SyncConfig(creatureControl, "Show mob level", true, "Mostra o nível sobre a cabeça da criatura.");
        LevelText = SyncConfig(creatureControl, "Mob level UI string", "[Nv. @]", "@ será substituído pelo nível.");
        ConfigSync.AddLockingConfigEntry(lockConfig);

        var directory = System.IO.Path.Combine(Paths.ConfigPath, "PoVMonsterLevels");
        Directory.CreateDirectory(directory);
        CreateDefaultCreatureTables(directory);
        Creatures = new CreatureDatabase(directory);
        Creatures.Reload();
        CreatureTable = new CustomSyncedValue<string>(ConfigSync, "Creature table", Creatures.Serialize());
        CreatureTable.ValueChanged += () =>
        {
            var value = CreatureTable.Value;
            if (!string.IsNullOrWhiteSpace(value)) Creatures.LoadFromServer(value);
        };
        _watcher = new FileSystemWatcher(directory, "*.json") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
        _watcher.Changed += (_, _) => Invoke(nameof(ReloadDefinitions), 0.25f);
        _watcher.Created += (_, _) => Invoke(nameof(ReloadDefinitions), 0.25f);

        new Harmony(PluginGuid).PatchAll();
        Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void ReloadDefinitions()
    {
        if (ZNet.instance != null && !ZNet.instance.IsServer()) return;
        Creatures.Reload();
        CreatureTable.Value = Creatures.Serialize();
    }

    private ConfigEntry<T> SyncConfig<T>(string section, string key, T value, string description) =>
        SyncConfig(section, key, value, new ConfigDescription(description));

    private ConfigEntry<T> SyncConfig<T>(string section, string key, T value, ConfigDescription description)
    {
        var entry = Config.Bind(section, key, value, description);
        ConfigSync.AddConfigEntry(entry).SynchronizedConfig = true;
        return entry;
    }

    private static void CreateDefaultCreatureTables(string tableDirectory)
    {
        if (Directory.EnumerateFiles(tableDirectory, "*.json", SearchOption.TopDirectoryOnly).Any()) return;

        const string resourcePrefix = "PoVMonsterLevels.DefaultCreatureTables.";
        var copied = 0;
        foreach (var resourceName in typeof(Plugin).Assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase)) continue;
            var fileName = resourceName.Substring(resourcePrefix.Length);
            using var input = typeof(Plugin).Assembly.GetManifestResourceStream(resourceName);
            if (input == null) continue;
            using var output = File.Create(Path.Combine(tableDirectory, fileName));
            input.CopyTo(output);
            copied++;
        }

        Log.LogInfo($"Created {copied} default EpicMMO-compatible creature table(s) in {tableDirectory}.");
    }

    private void OnDestroy() => _watcher?.Dispose();
}

internal enum DamageMode { Block, Scale }
