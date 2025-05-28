using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    [SerializeField] private TextMeshProUGUI CoinText; // UI表示用

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも消えない

            LoadCoinDataFromDatabase(); // データベースから読み込み
            UpdateCoinUI(); // UIに反映
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// コインの数をデータベースから取得して反映する
    /// </summary>
    private void LoadCoinDataFromDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        if (characters != null && characters.Count > 0)
        {
            _coinCount = characters[0].Coin;
            Debug.Log("DBからコイン読み込み: " + _coinCount);
        }
        else
        {
            Debug.LogWarning("CharacterStatus テーブルにレコードが存在しません。");
        }
    }

    /// <summary>
    /// 指定された量のコインを追加し、データベースに保存する
    /// </summary>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("コイン取得！現在のコイン枚数: " + _coinCount);

        UpdateCoinUI();
        SaveCoinDataToDatabase();
    }

    /// <summary>
    /// 現在のコイン数を CharacterStatus テーブルに保存する
    /// </summary>
    private void SaveCoinDataToDatabase()
    {
        var characters = DatabaseManager.GetAllCharacters();
        if (characters != null && characters.Count > 0)
        {
            var character = characters[0];
            // プレイヤー情報を更新（コイン以外はそのまま）
            DatabaseManager.Connection.Execute(
                "UPDATE CharacterStatus SET Coin = ? WHERE Id = ?",
                _coinCount, character.Id);
            Debug.Log("DBにコインを保存: " + _coinCount);
        }
        else
        {
            Debug.LogWarning("コイン保存に失敗：プレイヤーデータが存在しません。");
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
