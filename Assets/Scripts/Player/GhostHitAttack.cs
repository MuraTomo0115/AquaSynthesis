using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHitAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var ghost = GetComponentInParent<GhostMovement>();
        if (ghost != null)
        {
            ghost.OwnAttackHit(other);
            Debug.Log("GhostHitAttack: " + other.name + "‚Éƒqƒbƒg");
        }
    }
}