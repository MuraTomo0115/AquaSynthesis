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
    /// 保存場所：Assets/Data/coin.json（※開発用）
    /// </summary>
    private void SaveCoinData()
    {
        // コイン枚数をデータクラスにセットし、JSON文字列に変換
        CoinData data = new CoinData
        {
            totalCoins = _coinCount
        };
        string json = JsonUtility.ToJson(data, true);

        // Assets/Data フォルダを作成（なければ）
        string dir = Application.dataPath + "/Data";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // coin.json をそのフォルダ内に保存
        string path = dir + "/coin.json";

        // 保存パスをデバッグ表示（確認用）
        Debug.Log("コインデータ保存先: " + path);

        // 実際にファイルに書き込み
        File.WriteAllText(path, json);
    }
}