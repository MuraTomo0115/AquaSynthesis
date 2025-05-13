using UnityEngine;
using System.IO;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

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

    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("コイン取得！現在のコイン枚数: " + _coinCount);
        SaveCoinData();
    }

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
