using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NozzleController : MonoBehaviour
{
    [SerializeField] private WaterWallController waterWall; // Inspectorで紐付け

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 攻撃センサーのTagやLayerで判定
        if (other.CompareTag("AttackSensor"))
        {
            if (waterWall != null)
            {
                waterWall.DisableWall();
            }
            // ノズル自身も消したい場合は下記を有効化
            // Destroy(gameObject);
        }
    }
}
