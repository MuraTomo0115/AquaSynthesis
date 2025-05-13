using UnityEngine;

public class Coin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoin(1); // 1���ǉ�
            Destroy(gameObject);             // �R�C��������
        }
    }
}
