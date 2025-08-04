using BlackMagicAPI.Modules.Spells;
using FishNet.Object;
using UnityEngine;

namespace WagerSpell;

internal class WagerSpellLogic : SpellLogic
{
    private static float SelfHitChance => WagerSpellConfig.ChanceConfig.Value;
    
    private static float Damage => WagerSpellConfig.DamageConfig.Value;

    private static float Range => WagerSpellConfig.RangeConfig.Value;

    private static readonly float MaxAngle = 45f;

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
                if (angle > MaxAngle)
                    continue;

                if (!HasLineOfSight(casterPos, targetGO.transform.position))
                    continue;

                float score = angle * 2f + dist;
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

            if (targetMovementComp == casterMovementComp)
            {
                // TODO: Explosion effect
            }
            else
            {
                // TODO: Laser beam effect
            }

                targetMovementComp.DamagePlayer(Damage, casterGO, "Wager");
        }
        else
        {
            WagerSpell.Logger.LogInfo($"{casterMovementComp.playername} wagered with score {rand}/{SelfHitChance} but found no valid targets!");
        }
    }

    /// <summary>
    /// Determines if there is a clear line of sight between two positions using multiple raycasts
    /// with slight horizontal offsets to simulate a wider "vision cone".
    /// </summary>
    /// <param name="origin">The world position of viewer.</param>
    /// <param name="target">The world position of target.</param>
    /// <returns>True if at least one of the raycasts from origin to target is unobstructed; otherwise, false.</returns>
    private bool HasLineOfSight(Vector3 origin, Vector3 target)
    {
        Vector3 eyeOrigin = origin + Vector3.up * 1.5f;
        Vector3 eyeTarget = target + Vector3.up * 1.5f;
        Vector3 dir = (eyeTarget - eyeOrigin).normalized;
        float distance = Vector3.Distance(eyeOrigin, eyeTarget);
        
        // Ignore player layer
        int mask = ~(1 << LayerMask.NameToLayer("Player"));

        float[] angleOffsets = [-5f, 0f, 5f];
        foreach (float offset in angleOffsets)
        {
            Vector3 offsetDir = Quaternion.Euler(0f, offset, 0f) * dir;
            if (!Physics.Raycast(eyeOrigin, offsetDir, distance, mask))
            {
                // At least one clear ray
                return true;
            }
        }

        return false;
    }
}
