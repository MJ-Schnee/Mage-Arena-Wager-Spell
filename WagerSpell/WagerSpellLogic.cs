using BlackMagicAPI.Modules.Spells;
using FishNet.Object;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellLogic : SpellLogic
{
    private static float SelfHitChance => WagerSpellConfig.ChanceConfig.Value;
    
    private static float Damage => WagerSpellConfig.DamageConfig.Value;

    private static float Range => WagerSpellConfig.RangeConfig.Value;

    private static readonly float maxAngle = 45f;

    public override void CastSpell(GameObject casterGO, Vector3 spawnPos, Vector3 viewDirectionVector, int castingLevel)
    { 
        float rand = Random.Range(0f, 1f);

        PlayerMovement casterMovementComp = casterGO.GetComponent<PlayerMovement>();
        if (casterMovementComp == null)
        {
            WagerSpell.Logger.LogError("Caster's PlayerMovement could not be found!");
        }

        PlayerMovement targetMovementComp = null;

        if (rand > SelfHitChance)
        {
            // Get unique network id of caster
            NetworkObject casterNetObj = casterMovementComp.GetComponent<NetworkObject>();
            if (casterNetObj == null)
                return;
            int casterId = casterNetObj.ObjectId;

            Vector3 casterPos = casterGO.transform.position;
            Vector3 forward = casterGO.transform.forward;

            GameObject bestTarget;
            PlayerMovement bestMovement;
            float bestScore = float.MaxValue;

            foreach (GameObject targetGO in GameObject.FindGameObjectsWithTag("Player"))
            {
                // Skip self
                NetworkObject targetNetObj = targetGO.GetComponent<NetworkObject>();
                if (targetNetObj == null || targetNetObj.ObjectId == casterId)
                    continue;

                targetMovementComp = targetGO.GetComponent<PlayerMovement>();
                if (targetMovementComp == null)
                    continue;

                Vector3 toTarget = targetGO.transform.position - casterPos;
                float dist = toTarget.magnitude;
                if (dist > Range)
                    continue;

                float angle = Vector3.Angle(forward, toTarget.normalized);
                if (angle > maxAngle)
                    continue;

                if (!Utils.HasLineOfSight(casterPos, targetGO.transform.position))
                    continue;

                float score = (angle * 2f) + dist;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = targetGO;
                    bestMovement = targetMovementComp;
                }
            }
        }
        else
        {
            targetMovementComp = casterMovementComp;
        }

        if (targetMovementComp != null)
        {
            WagerSpell.Logger.LogInfo($"{casterMovementComp.playername} wagered with score {rand}/{SelfHitChance} and is targeting {targetMovementComp.playername}!");

            Vector3 explosionPos;

            if (targetMovementComp == casterMovementComp)
            {
                explosionPos = casterGO.transform.position + (Vector3.up * 1.75f);
            }
            else
            {
                explosionPos = targetMovementComp.gameObject.transform.position + (Vector3.up * 1.75f);

                // Jackpot sound on enemy hit
                Vector3 jackpotPos = casterGO.transform.position + (Vector3.up * 1.75f);
                Utils.PlaySpatialSoundAtPosition(jackpotPos, WagerSpell.JackpotSound);
            }

            GameObject explosionGO = Instantiate(WagerSpell.ExplosionPrefab, explosionPos, Quaternion.identity);
            float effectDuration = WagerSpell.ExplosionPrefab.GetComponent<ParticleSystem>().main.duration + 0.25f;
            Destroy(explosionGO, effectDuration);
            
            Utils.PlaySpatialSoundAtPosition(explosionPos, WagerSpell.ExplodeSound);

            targetMovementComp.DamagePlayer(Damage, casterGO, "Wager");
        }
        else
        {
            WagerSpell.Logger.LogInfo($"{casterMovementComp.playername} wagered with score {rand}/{SelfHitChance} but found no valid targets!");
        }
    }
}
