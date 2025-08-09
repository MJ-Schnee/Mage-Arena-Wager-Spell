using BepInEx;
using BepInEx.Logging;
using BlackMagicAPI.Managers;
using FishNet;
using FishNet.Object;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

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

        LoadAssets();

        BlackMagicManager.RegisterSpell(this, typeof(WagerSpellData), typeof(WagerSpellLogic));

        BlackMagicManager.RegisterDeathIcon(
            this,
            "Wager",
            "Wager_Death"
        );

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        #if DEBUG
            if (Input.GetKeyDown(KeyCode.F4))
            {
                List<GameObject> players =
                    [..GameObject.FindGameObjectsWithTag("Player").Where(player => player.name.Contains("Player"))];
                if (players.Count == 0)
                {
                    Logger.LogError("No players found.");
                    return;
                }

                foreach (var player in players.Where(player =>
                             player.GetComponent<NetworkObject>()?.HasAuthority == true))
                {
                    var spawnPos = player.transform.position + player.transform.forward;
                    spawnPos.y += 1.5f;
                    BlackMagicManager.SpawnSpell<WagerSpellLogic>(spawnPos);

                    Logger.LogMessage($"[SERVER] Spawned page 'Wager' for player '{player.name}' at {spawnPos}");
                }
            }
        #endif
    }

    /// <summary>
    /// Loads in all assets for the spell
    /// </summary>
    private void LoadAssets()
    {
        Logger.LogInfo("Loading sounds");

        ExplodeSound = Utils.LoadSound("Explode.wav", AudioType.WAV);

        JackpotSound = Utils.LoadSound("Jackpot.wav", AudioType.WAV);

        Logger.LogInfo("Loading assets from bundle");

        string wagerAssetPath = Path.Combine(Utils.PluginDir, "AssetBundles", "explosion_prefab");
        AssetBundle wagerAssets = BlackMagicAPI.Helpers.Utils.LoadAssetBundleFromDisk(wagerAssetPath);

        string explosionPrefabAssetLocation = "assets/gabrielaguiarproductions/freequickeffectsvol1/prefabs/vfx_impact_01.prefab";
        ExplosionPrefab = wagerAssets.LoadAsset<GameObject>(explosionPrefabAssetLocation);
        DontDestroyOnLoad(ExplosionPrefab);

        wagerAssets.UnloadAsync(false);
    }
}
