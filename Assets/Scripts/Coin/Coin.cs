using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoin(1); // 1–‡’Ç‰Á
            Destroy(gameObject);             // ƒRƒCƒ“‚ğÁ‚·
        }
    }
}