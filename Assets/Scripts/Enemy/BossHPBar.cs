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
    /// HPバーの表示を更新
    /// </summary>
    /// <param name="appear">true:表示 false:非表示</param>
    public void AppearHPBar(bool appear)
    {
        if (_bossHpSlider != null)
        {
            _bossHpSlider.gameObject.SetActive(appear);
        }
    }

    /// <summary>
    /// ボスHPバーの最大値と現在値を設定
    /// </summary>
    /// <param name="current">現在のHP</param>
    /// <param name="max">最大HP</param>
    public void SetHP(float current, float max)
    {
        if (_bossHpSlider != null)
        {
            _bossHpSlider.maxValue = max;
            _bossHpSlider.value = current;
        }
    }
}
