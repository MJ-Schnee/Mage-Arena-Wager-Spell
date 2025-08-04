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
public class WagerSpell : BaseUnityPlugin
{
    public static string modsync = "all";

    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo($"Initializing {PluginInfo.PLUGIN_GUID}...");

        WagerSpellConfig.LoadConfig(this);

        SpellManager.RegisterSpell(this, typeof(WagerSpellData), typeof(WagerSpellLogic));

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
    public void SpawnPage()
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
}
