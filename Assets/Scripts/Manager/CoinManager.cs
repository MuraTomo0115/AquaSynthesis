using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    // 今後セーブデータごとに割り振られる予定のID（今は仮に1を使用）
    private int _currentPlayerId = 1;

    [SerializeField] private TextMeshProUGUI CoinText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも残す
            LoadCoinDataFromDatabase();     // コイン読み込み
            UpdateCoinUI();                 // UI更新
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 今後セーブデータ選択画面でこのIDが設定される想定
    /// </summary>
    public void SetPlayerId(int id)
    {
        _currentPlayerId = id;
    }

    /// <summary>
    /// データベースから現在のプレイヤーIDのコインデータを読み込む
    /// </summary>
    private void LoadCoinDataFromDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        var character = characters.Find(c => c.Id == _currentPlayerId);

        if (character != null)
        {
            _coinCount = character.Coin;
        }
        else
        {
            Debug.LogWarning($"[Load] CharacterStatus に ID={_currentPlayerId} のレコードが見つかりません。");
        }
    }

    /// <summary>
    /// コインを加算し、UIとデータベースに反映
    /// </summary>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        UpdateCoinUI();
        SaveCoinDataToDatabase();
    }

    /// <summary>
    /// 現在のコインデータをデータベースに保存
    /// </summary>
    private void SaveCoinDataToDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        var character = characters.Find(c => c.Id == _currentPlayerId);

        if (character != null)
        {
            DatabaseManager.Connection.Execute(
                "UPDATE CharacterStatus SET Coin = ? WHERE Id = ?",
                _coinCount, character.Id);
        }
        else
        {
            Debug.LogWarning($"[Save] CharacterStatus に ID={_currentPlayerId} のレコードが見つかりません。");
        }
    }

    /// <summary>
    /// コインUIの表示を更新する
    /// </summary>
    private void UpdateCoinUI()
    {
        if (CoinText != null)
        {
            CoinText.text = "× " + _coinCount.ToString();
        }
    }
}
