using UnityEngine;
using System.IO;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    /// <summary>
    /// シングルトンのインスタンスを初期化し、ゲームオブジェクトを永続化する
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも消えない
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定された量のコインを追加し、セーブデータに反映する
    /// </summary>
    /// <param name="amount">追加するコインの数</param>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("コイン取得！現在のコイン枚数: " + _coinCount);
        SaveCoinData();
    }

    /// <summary>
    /// 現在のコイン枚数をJSON形式で保存する
    /// </summary>
    private void SaveCoinData()
    {
        CoinData data = new CoinData
        {
            totalCoins = _coinCount
        };

        string json = JsonUtility.ToJson(data, true);

        // 仮パス（実際には Application.persistentDataPath を使うといい）
        string path = Application.dataPath + "/coin.json";
        File.WriteAllText(path, json);
    }
}
