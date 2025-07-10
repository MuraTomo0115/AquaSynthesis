using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [SerializeField] private Slider _bossHpSlider;

    public BossHPBar Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_bossHpSlider != null)
        {
            //_bossHpSlider.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// HP�o�[�̕\�����X�V
    /// </summary>
    /// <param name="appear">true:�\�� false:��\��</param>
    public void AppearHPBar(bool appear)
    {
        if (_bossHpSlider != null)
        {
            _bossHpSlider.gameObject.SetActive(appear);
        }
    }

    /// <summary>
    /// �{�XHP�o�[�̍ő�l�ƌ��ݒl��ݒ�
    /// </summary>
    /// <param name="current">���݂�HP</param>
    /// <param name="max">�ő�HP</param>
    public void SetHP(float current, float max)
    {
        if (_bossHpSlider != null)
        {
            _bossHpSlider.maxValue = max;
            _bossHpSlider.value = current;
        }
    }
}
