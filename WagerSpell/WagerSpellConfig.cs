using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using MageConfigurationAPI.Data;

namespace WagerSpell;

internal static class WagerSpellConfig
{
    internal static ConfigEntry<float> CooldownConfig { get; private set; }

    internal static ConfigEntry<float> RangeConfig { get; private set; }

    internal static ConfigEntry<float> DamageConfig { get; private set; }

    internal static ConfigEntry<float> ChanceConfig { get; private set; }

    private static bool mageConfigApiExists;

    private static BaseUnityPlugin plugin;

    /// <summary>
    /// Loads all configuration options
    /// </summary>
    /// <param name="plugin">Plugin to attach</param>
    public static void LoadConfig(BaseUnityPlugin plugin)
    {
        WagerSpellConfig.plugin = plugin;
        mageConfigApiExists = Chainloader.PluginInfos.ContainsKey("com.d1gq.mage.configuration.api");

        CooldownConfig = BindConfig(
            "Cooldown",
            60f,
            "Time until spell can be used again (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        RangeConfig = BindConfig(
            "Range",
            20f,
            "Maximum distance a target can be from the caster (> 0)",
            new AcceptableValueRange<float>(0f, float.MaxValue)
        );

        DamageConfig = BindConfig(
            "Damage",
            1000f,
            "Damage that will be dealt to target",
            new AcceptableValueRange<float>(float.MinValue, float.MaxValue)
        );

        ChanceConfig = BindConfig(
            "Chance",
            0.5f,
            "Chance that caster will be the target of the spell (0 - 1)",
            new AcceptableValueRange<float>(0f, 1f)
        );
    }

    /// <summary>
    /// Binds the config option to a variable and adds it to the in-game config menu if available
    /// </summary>
    /// <typeparam name="T">Type of config entry</typeparam>
    /// <param name="key">Config name</param>
    /// <param name="defaultValue">Config default value</param>
    /// <param name="description">Config description</param>
    /// <param name="acceptableValues">Optional config value range/list</param>
    /// <returns></returns>
    private static ConfigEntry<T> BindConfig<T>(string key, T defaultValue, string description, AcceptableValueBase acceptableValues = null)
    {
        ConfigEntry<T> configEntry = plugin.Config.Bind(
            PluginInfo.PLUGIN_NAME,
            key,
            defaultValue,
            new ConfigDescription(description, acceptableValues)
        );

        if (mageConfigApiExists)
        {
            new ModConfig(plugin, configEntry, MageConfigurationAPI.Enums.SettingsFlag.ShowInLobbyMenu);
        }

        return configEntry;
    }
}
