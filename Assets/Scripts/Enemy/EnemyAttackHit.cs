using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHit : MonoBehaviour
{
    private Character _charaState;

    private void Start()
    {
        _charaState = GetComponentInParent<Character>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 敵のCharacterコンポーネントを取得
        Character hitObject = other.GetComponent<Character>();

        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }
}
