using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NozzleController : MonoBehaviour
{
    [SerializeField] private WaterWallController waterWall; // Inspector�ŕR�t��

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �U���Z���T�[��Tag��Layer�Ŕ���
        if (other.CompareTag("AttackSensor"))
        {
            if (waterWall != null)
            {
                waterWall.DisableWall();
            }
            // �m�Y�����g�����������ꍇ�͉��L��L����
            // Destroy(gameObject);
        }
    }
}
