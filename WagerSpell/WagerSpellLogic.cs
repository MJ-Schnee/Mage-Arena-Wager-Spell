using BlackMagicAPI.Modules.Spells;
using BlackMagicAPI.Network;
using FishNet.Object;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellLogic : SpellLogic
{
    private static float SelfHitChance => WagerSpellConfig.ChanceConfig.Value;

    private static float Damage => WagerSpellConfig.DamageConfig.Value;

    private static float Range => WagerSpellConfig.RangeConfig.Value;

    private const float MaxAngle = 45f;

    private int _casterNetworkId;

    private int _targetNetworkId;

    /// <summary>
    /// Client generates random number for cast and stores caster/target network ids
    /// </summary>
    public override void WriteData(DataWriter dataWriter,
        PageController page,
        GameObject caster,
        Vector3 spawnPos,
        Vector3 viewDirectionVector,
        int spellLevel)
    {
        var casterNetObj = caster.GetComponent<NetworkObject>();
        if (casterNetObj is null)
        {
            WagerSpell.Logger.LogError("Spell network object couldn't be found!");
            return;
        }
        
        var casterNetId = casterNetObj.ObjectId;
        var targetNetId = -1;
        var rand = Random.Range(0f, 1f);
        
        var casterMovementComp = caster.GetComponent<PlayerMovement>();
        if (casterMovementComp is null)
        {
            WagerSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
            return;
        }

        if (rand > SelfHitChance)
        {
            var casterPos = caster.transform.position;
            var forward = caster.transform.forward;

            var bestScore = float.MaxValue;

            foreach (var target in GameObject.FindGameObjectsWithTag("Player"))
            {
                // Skip self
                var targetNetObj = target.GetComponent<NetworkObject>();
                if (targetNetObj is null || targetNetObj.ObjectId == casterNetId)
                    continue;

                var tempTargetMovement = target.GetComponent<PlayerMovement>();
                if (tempTargetMovement is null)
                    continue;

                var toTarget = target.transform.position - casterPos;
                var dist = toTarget.magnitude;
                if (dist > Range)
                    continue;

                var angle = Vector3.Angle(forward, toTarget.normalized);
                if (angle > MaxAngle)
                    continue;

                if (!Utils.HasLineOfSight(casterPos, target.transform.position))
                    continue;

                var score = angle * 2f + dist;
                if (!(score < bestScore))
                    continue;
                
                bestScore = score;
                targetNetId = targetNetObj.ObjectId;
            }
        }
        else
        {
            targetNetId = casterNetObj.ObjectId;
        }

        dataWriter.Write(casterNetId);
        dataWriter.Write(targetNetId);
    }
    
    /// <summary>
    /// Stores the caster and target's network ids for spell casting
    /// </summary>
    /// <param name="values">Should be int[]: [casterNetworkId, targetNetworkId]</param>
    public override void SyncData(object[] values)
    {
        if (values.Length != 2 || (values[0].GetType() != typeof(int) && values[1].GetType() != typeof(int)))
        {
            WagerSpell.Logger.LogError("SyncData values does not contain 2 int entries!");
            return;
        }
        
        _casterNetworkId = (int)values[0];
        _targetNetworkId = (int)values[1];
    }

    public override void CastSpell(GameObject caster,
        PageController page,
        Vector3 spawnPos,
        Vector3 viewDirection,
        int castingLevel)
    {
        PlayerMovement casterMovementComp = null;
        PlayerMovement targetMovementComp = null;
        
        // Search for PlayerMovement components
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var playerNetObj = player.GetComponent<NetworkObject>();
            if (playerNetObj is null)
                continue;

            if (playerNetObj.ObjectId == _casterNetworkId)
            {
                var playerMovementComp = player.GetComponent<PlayerMovement>();
                if (playerMovementComp is null)
                {
                    WagerSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
                    return;   
                }
                
                casterMovementComp = playerMovementComp;
            }

            if (playerNetObj.ObjectId == _targetNetworkId)
            {
                var playerMovementComp = player.GetComponent<PlayerMovement>();
                if (playerMovementComp is null)
                {
                    WagerSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
                    return;   
                }
                
                targetMovementComp = playerMovementComp;
            }
        }

        if (casterMovementComp is null)
        {
            WagerSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
            return;
        }

        if (_targetNetworkId != 0)
        {
            if (targetMovementComp is null)
            {
                WagerSpell.Logger.LogError("Targets's PlayerMovement could not be found!");
                return;
            }
            
            WagerSpell.Logger.LogInfo(
                $"{casterMovementComp.playername} wagered and is targeting {targetMovementComp.playername}!");

            Vector3 explosionPos;

            if (targetMovementComp == casterMovementComp)
            {
                explosionPos = casterMovementComp.gameObject.transform.position + (Vector3.up * 1.75f);
            }
            else
            {
                explosionPos = targetMovementComp.gameObject.transform.position + (Vector3.up * 1.75f);

                // Jackpot sound on enemy hit
                var jackpotPos = casterMovementComp.gameObject.transform.position + (Vector3.up * 1.75f);
                Utils.PlaySpatialSoundAtPosition(jackpotPos, WagerSpell.JackpotSound);
            }

            var explosion = Instantiate(WagerSpell.ExplosionPrefab, explosionPos, Quaternion.identity);
            var effectDuration = WagerSpell.ExplosionPrefab.GetComponent<ParticleSystem>().main.duration;
            Destroy(explosion, effectDuration);
            
            Utils.PlaySpatialSoundAtPosition(explosionPos, WagerSpell.ExplodeSound);

            targetMovementComp.DamagePlayer(Damage, casterMovementComp.gameObject, "Wager");
        }
        else
        {
            WagerSpell.Logger.LogInfo($"{casterMovementComp.playername} wagered but found no valid targets!");
        }
    }
}
