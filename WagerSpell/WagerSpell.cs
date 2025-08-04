using BepInEx;
using BepInEx.Logging;
using BlackMagicAPI.Managers;

namespace WagerSpell;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("MageArena.exe")]
[BepInDependency("com.magearena.modsync", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.d1gq.black.magic.api", BepInDependency.DependencyFlags.HardDependency)]
public class WagerSpell : BaseUnityPlugin
{
    public static string modsync = "host";

    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo($"Initializing {PluginInfo.PLUGIN_GUID}...");

        WagerSpellConfig.LoadConfig(this);

        SpellManager.RegisterSpell(this, typeof(WagerSpellData), typeof(WagerSpellLogic));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}
