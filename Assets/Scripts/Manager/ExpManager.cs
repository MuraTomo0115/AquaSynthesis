using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance;

    [SerializeField] private TextMeshProUGUI _expText; // 経験値表示用のUIテキスト
    private int _currentExp = 0;

    public int CurrentExp => _currentExp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _currentExp = 0; // 初期化
        UpdateExpText();
    }

    /// <summary>
    /// 経験値を追加するメソッド
    /// </summary>
    /// <param name="exp">追加する値</param>
    public void AddExp(int exp)
    {
        _currentExp += exp;
        UpdateExpText();
        Debug.Log($"経験値を追加: {exp} (現在の経験値: {_currentExp})");
    }

    /// <summary>
    /// 経験値表示UIを更新
    /// </summary>
    private void UpdateExpText()
    {
        if (_expText != null)
            _expText.text = _currentExp.ToString();
    }
}
