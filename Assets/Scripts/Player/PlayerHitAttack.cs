using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Echoƒ^ƒO‚Í–³Ž‹
        if (other.CompareTag("Echo"))
        {
            return;
        }

        var player = GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            player.OwnAttackHit(other);
        }
    }
}
