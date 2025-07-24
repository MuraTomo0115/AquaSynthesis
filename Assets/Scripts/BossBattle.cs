using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBattle : MonoBehaviour
{
    [Header("�{�X��GameObject�i��A�N�e�B�u�Ŕz�u�j")]
    public GameObject bossObject;

    [Header("�ǂ�GameObject�i��A�N�e�B�u�Ŕz�u�j")]
    public GameObject wallObject;

    private bool bossActivated = false;

    private void Start()
    {
        // �V�[���J�n���Ɉ�u�����{�X���A�N�e�B�u�����ď�����
        if (bossObject != null && !bossObject.activeSelf)
        {
            bossObject.SetActive(true);
            StartCoroutine(DeactivateBossNextFrame());
        }
    }

    private IEnumerator DeactivateBossNextFrame()
    {
        yield return null; // 1�t���[���҂�
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
                    manager.LoadBossStatus(bossObject); // ���{�X�̂ݏ�����
            }
            wallObject.SetActive(true);

            bossActivated = true;
        }
    }
}
