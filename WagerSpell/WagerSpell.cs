using BepInEx;
using BepInEx.Logging;
using BlackMagicAPI.Managers;
using FishNet;
using FishNet.Object;
using System.Linq;
using UnityEngine;

namespace WagerSpell;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("MageArena.exe")]
[BepInDependency("com.magearena.modsync", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.black.magic.api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.mage.configuration.api", BepInDependency.DependencyFlags.SoftDependency)]
public class WagerSpell : BaseUnityPlugin
{
    public static WagerSpell Instance { get; private set; }

    public static readonly string modsync = "all";

    internal static AudioClip ExplodeSound { get; private set; }

    internal static AudioClip JackpotSound { get; private set; }

    internal static GameObject ExplosionPrefab { get; private set; }

    internal static new ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        Instance = this;

        Logger = base.Logger;

        Logger.LogInfo($"Initializing {PluginInfo.PLUGIN_GUID}...");

        WagerSpellConfig.LoadConfig(this);

        LoadSounds();

        LoadPrefabs();

        BlackMagicManager.RegisterSpell(this, typeof(WagerSpellData), typeof(WagerSpellLogic));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    void Update()
    {
        #if DEBUG
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SpawnPage();
            }
        #endif
    }

    /// <summary>
    /// Spawns an instance of this spell's page
    /// </summary>
    private void SpawnPage()
    {
        if (!InstanceFinder.IsServerStarted)
        {
            Logger.LogError("This must be run on the server.");
            return;
        }

        PageLootTable lootTable = FindFirstObjectByType<PageLootTable>();
        if (lootTable == null || lootTable.Pages == null || lootTable.Pages.Length == 0)
        {
            Logger.LogError("PageLootTable is missing or empty.");
            return;
        }

        GameObject prefab = lootTable.Pages
                            .Where(page => page.name.Contains("PageWager"))
                            .FirstOrDefault();
        if (prefab == null)
        {
            Logger.LogError("Page could not be found.");
            return;
        }

        if (prefab.GetComponent<NetworkObject>() == null)
        {
            Logger.LogError($"Prefab '{prefab.name}' is missing a NetworkObject component.");
            return;
        }

        var players = GameObject.FindGameObjectsWithTag("Player")
                        .Where(player => player.name.Contains("Player"))
                        .ToList();
        if (players.Count == 0)
        {
            Logger.LogError("No players found.");
            return;
        }

        foreach (GameObject player in players)
        {
            Vector3 spawnPos = player.transform.position + Vector3.forward;
            spawnPos.y += 1.5f;
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(instance);

            Logger.LogMessage($"[SERVER] Spawned page '{prefab.name}' for player '{player.name}' at {spawnPos}");
        }
    }

    /// <summary>
    /// Loads in all sounds necessary for the spell
    /// </summary>
    private void LoadSounds()
    {
        ExplodeSound = Utils.LoadSound("Explode.wav", AudioType.WAV);

        JackpotSound = Utils.LoadSound("Jackpot.wav", AudioType.WAV);
    }

    /// <summary>
    /// Loads in all prefabs necessary for the spell
    /// </summary>
    private void LoadPrefabs()
    {
        string explosionPrefabAssetBundlePath = Path.Combine(Utils.PluginDir, "AssetBundles", "explosion_prefab");
        AssetBundle explosionPrefabAssetBundle = BlackMagicAPI.Helpers.Utils.LoadAssetBundleFromDisk(explosionPrefabAssetBundlePath);
        string explosionPrefabAssetLocation = "assets/gabrielaguiarproductions/freequickeffectsvol1/prefabs/vfx_impact_01.prefab";
        ExplosionPrefab = explosionPrefabAssetBundle.LoadAsset<GameObject>(explosionPrefabAssetLocation);
        DontDestroyOnLoad(ExplosionPrefab);
        explosionPrefabAssetBundle.UnloadAsync(false);
    }
}
