using BlackMagicAPI.Enums;
using BlackMagicAPI.Modules.Spells;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellData : SpellData
{
    public override SpellType SpellType => SpellType.Page;

    public override string Name => "Wager";

    public override float Cooldown => WagerSpellConfig.CooldownConfig.Value;

    public override Color GlowColor => new(255f, 226f, 109f); // Gold
}
