using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBattle : MonoBehaviour
{
    [Header("�{�X��GameObject�i��A�N�e�B�u�Ŕz�u�j")]
    public GameObject bossObject;

    [SerializeField] private GameObject _bossHPBar;

    [SerializeField] private MusselMovement _musselMovement; // MusselMovement script reference

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

        if (_musselMovement != null)
        {
            _musselMovement.enabled = false; // MusselMovement scriptを無効化
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
            AudioManager.Instance.StopBGM("N2BGM");
            AudioManager.Instance.PlayBGM("N2BossBGM");
            if (bossObject != null)
                bossObject.SetActive(true);
            if (_bossHPBar != null)
                _bossHPBar.SetActive(true);

            if (bossObject != null)
            {
                bossObject.SetActive(true);
                var manager = FindObjectOfType<CharacterManager>();
                if (manager != null)
                    manager.LoadBossStatus(bossObject); // ���{�X�̂ݏ�����
            }
            wallObject.SetActive(true);

            bossActivated = true;

            Invoke("OnEnableMussel", 3.0f); // MusselMovement scriptを3秒後に有効化
        }
    }

    private void OnEnableMussel()
    {
        if (_musselMovement != null)
        {
            _musselMovement.enabled = true; // MusselMovement scriptを有効化
        }
    }
}
