using UnityEngine;
using System.IO;
using TMPro; // TextMeshPro用

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    [SerializeField] private TextMeshProUGUI CoinText; // UI表示用

    private void Start()
    {
        Debug.Log("CoinText は null？: " + (CoinText == null));
        AddCoin(1); // テスト用
    }


    /// <summary>
    /// シングルトンのインスタンスを初期化し、ゲームオブジェクトを永続化する
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも消えない

            LoadCoinData(); // 起動時に保存データから読み込み
            UpdateCoinUI(); // UIに反映
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

        UpdateCoinUI();   // UI更新
        SaveCoinData();   // 保存
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

    /// <summary>
    /// JSONファイルからコイン数を読み込む（起動時に呼ばれる）
    /// </summary>
    private void LoadCoinData()
    {
        string path = Application.dataPath + "/Data/coin.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CoinData data = JsonUtility.FromJson<CoinData>(json);
            _coinCount = data.totalCoins;
            Debug.Log("保存されたコインを読み込み: " + _coinCount);
        }
        else
        {
            Debug.Log("コインデータが見つかりません。新規作成されます。");
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
