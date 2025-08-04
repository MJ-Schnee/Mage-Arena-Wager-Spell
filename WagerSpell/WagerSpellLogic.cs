using BlackMagicAPI.Modules.Spells;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellLogic : SpellLogic
{
    public override void CastSpell(GameObject playerObj, Vector3 spawnPos, Vector3 viewDirectionVector, int castingLevel)
    {
        WagerSpell.Logger.LogInfo($"WagerSpell has been cast to deal {WagerSpellConfig.DamageConfig.Value} damage");
    }
}
