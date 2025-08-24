using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace WagerSpell;

internal static class WagerSpellConfig
{
    internal static ConfigEntry<float> CooldownConfig { get; private set; }

    internal static ConfigEntry<float> RangeConfig { get; private set; }

    internal static ConfigEntry<float> DamageConfig { get; private set; }

    internal static ConfigEntry<float> ChanceConfig { get; private set; }

    internal static ConfigEntry<bool> TeamChestConfig { get; private set; }

    private static bool _mageConfigApiExists;

    private static BaseUnityPlugin _plugin;

    /// <summary>
    /// Loads all configuration options
    /// </summary>
    /// <param name="plugin">Plugin to attach</param>
    public static void LoadConfig(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
        _mageConfigApiExists = Chainloader.PluginInfos.ContainsKey("com.d1gq.mage.configuration.api");
        
        if (!_mageConfigApiExists)
            WagerSpell.Logger.LogInfo(
                "MageConfigurationAPI not present. Continuing to bind from config, but will not add to in-game menu.");

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

        TeamChestConfig = BindConfig(
            "Team Chest",
            true,
            "Whether the page can spawn in the team chest"
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
    private static ConfigEntry<T> BindConfig<T>(string key,
        T defaultValue,
        string description,
        AcceptableValueBase acceptableValues = null)
    {
        var configEntry = _plugin.Config.Bind(
            PluginInfo.PLUGIN_NAME,
            key,
            defaultValue,
            new ConfigDescription(description, acceptableValues)
        );

        if (_mageConfigApiExists)
            TryRegisterWithMageConfig(configEntry);

        return configEntry;
    }
    
    /// <summary>
    /// Uses reflection to call: MageConfigurationAPI.Data.ModConfig(plugin, configEntry, SettingsFlag.ShowInLobbyMenu)
    /// without a hard compile-time dependency.
    /// </summary>
    private static void TryRegisterWithMageConfig(object configEntry)
    {
        try
        {
            // find the MageConfigurationAPI assembly that the other plugin loaded
            var apiAsm = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a =>
                    a.GetName().Name.Equals("MageConfigurationAPI", StringComparison.OrdinalIgnoreCase));

            if (apiAsm == null)
            {
                // Fallback: try to grab it via the plugin instance’s assembly (if exposed)
                if (Chainloader.PluginInfos.TryGetValue("com.d1gq.mage.configuration.api", out var pi))
                    apiAsm = pi.Instance?.GetType().Assembly;
            }

            if (apiAsm == null)
            {
                WagerSpell.Logger.LogInfo(
                    "MageConfigurationAPI present in PluginInfos, but assembly not found. Skipping menu registration.");
                return;
            }

            var settingsFlagType = apiAsm.GetType("MageConfigurationAPI.Enums.SettingsFlag", throwOnError: false);
            var modConfigType    = apiAsm.GetType("MageConfigurationAPI.Data.ModConfig",     throwOnError: false);
            if (settingsFlagType == null || modConfigType == null)
            {
                WagerSpell.Logger.LogWarning("MageConfigurationAPI types not found. Skipping menu registration.");
                return;
            }

            // Get enum value: SettingsFlag.ShowInLobbyMenu
            var showInLobbyValue = Enum.Parse(settingsFlagType, "ShowInLobbyMenu");

            // Find a matching ctor: (BaseUnityPlugin, object(ConfigEntry<T>), SettingsFlag)
            var ctor = modConfigType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(ci =>
                {
                    var ps = ci.GetParameters();
                    return ps.Length == 3
                        && typeof(BaseUnityPlugin).IsAssignableFrom(ps[0].ParameterType)
                        && ps[2].ParameterType == settingsFlagType;
                });

            if (ctor == null)
            {
                WagerSpell.Logger.LogWarning(
                    "MageConfigurationAPI ModConfig constructor signature changed. Skipping menu registration.");
                return;
            }

            ctor.Invoke([_plugin, configEntry, showInLobbyValue]);
        }
        catch (Exception e)
        {
            WagerSpell.Logger.LogWarning(
                $"MageConfigurationAPI present but reflection failed: {e.GetType().Name}: {e.Message}");
        }
    }
}
