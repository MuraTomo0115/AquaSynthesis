using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHitAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Player"ƒ^ƒO‚ª•t‚¢‚Ä‚¢‚éê‡‚Í‰½‚à‚µ‚È‚¢
        if (other.CompareTag("Player"))
        {
            return;
        }

        var ghost = GetComponentInParent<GhostMovement>();
        if (ghost != null)
        {
            ghost.OwnAttackHit(other);
        }
    }
}