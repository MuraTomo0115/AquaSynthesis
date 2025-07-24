using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBattle : MonoBehaviour
{
    [Header("ボスのGameObject（非アクティブで配置）")]
    public GameObject bossObject;

    [Header("壁のGameObject（非アクティブで配置）")]
    public GameObject wallObject;

    private bool bossActivated = false;

    private void Start()
    {
        // シーン開始時に一瞬だけボスをアクティブ化して初期化
        if (bossObject != null && !bossObject.activeSelf)
        {
            bossObject.SetActive(true);
            StartCoroutine(DeactivateBossNextFrame());
        }
    }

    private IEnumerator DeactivateBossNextFrame()
    {
        yield return null; // 1フレーム待つ
        if (bossObject != null)
            bossObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bossActivated) return;
        if (other.CompareTag("Player"))
        {
            if (bossObject != null)
                bossObject.SetActive(true);

            if (bossObject != null)
            {
                bossObject.SetActive(true);
                var manager = FindObjectOfType<CharacterManager>();
                if (manager != null)
                    manager.LoadBossStatus(bossObject); // ←ボスのみ初期化
            }
            wallObject.SetActive(true);

            bossActivated = true;
        }
    }
}
