using BepInEx;
using BepInEx.Configuration;
using MageConfigurationAPI.Data;

namespace WagerSpell;

internal static class WagerSpellConfig
{
    internal static ConfigEntry<float> CooldownConfig { get; private set; }

    internal static ConfigEntry<float> RangeConfig { get; private set; }

    internal static ConfigEntry<float> DamageConfig { get; private set; }

    internal static ConfigEntry<float> ChanceConfig { get; private set; }

    public static void LoadConfig(BaseUnityPlugin plugin)
    {
        CooldownConfig = plugin.Config.Bind(
            "WagerSpell",
            "Cooldown",
            60f,
            new ConfigDescription(
                "Time until spell can be used again (> 0)",
                new AcceptableValueRange<float>(0f, float.MaxValue)
            )
        );
        new ModConfig(plugin, CooldownConfig);

        RangeConfig = plugin.Config.Bind(
            "WagerSpell",
            "Range",
            20f,
            new ConfigDescription(
                "Maximum distance a target can be from the caster (> 0)",
                new AcceptableValueRange<float>(0f, float.MaxValue)
            )
        );
        new ModConfig(plugin, RangeConfig);

        DamageConfig = plugin.Config.Bind(
            "WagerSpell",
            "Damage",
            1000f,
            new ConfigDescription(
                "Damage that will be dealt to target",
                new AcceptableValueRange<float>(float.MinValue, float.MaxValue)
            )
        );
        new ModConfig(plugin, DamageConfig);

        ChanceConfig = plugin.Config.Bind(
            "WagerSpell",
            "Chance",
            0.5f,
            new ConfigDescription(
                "Chance that caster will be the target of the spell (0 - 1)",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        new ModConfig(plugin, ChanceConfig);
    }
}
