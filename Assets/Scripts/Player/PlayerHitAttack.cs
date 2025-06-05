using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Echo�^�O�͖���
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
