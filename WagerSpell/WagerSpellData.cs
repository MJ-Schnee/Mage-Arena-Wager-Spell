using BlackMagicAPI.Enums;
using BlackMagicAPI.Modules.Spells;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellData : SpellData
{
    public override SpellType SpellType => SpellType.Page;

    public override string Name => "Wager";

    public override float Cooldown => WagerSpellConfig.CooldownConfig.Value;

    public override Color GlowColor => new(2f, 1.886f, 1.427f); // Gold
}
