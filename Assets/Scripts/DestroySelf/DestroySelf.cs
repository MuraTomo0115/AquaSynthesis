using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public void DestroySelfGameObject()
    {
        // 自身のGameObjectを削除
        Destroy(gameObject);
    }
}
