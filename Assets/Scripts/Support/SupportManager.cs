using UnityEngine;
using System.Collections.Generic;

public class SupportManager : MonoBehaviour
{
    [SerializeField] private Transform summonPosition;

    private Dictionary<string, SupportStatus> statusTable;

    // 選択されたサポートキャラのID（外部UIなどからセット）
    private string supportId1;
    private string supportId2;

    private void Start()
    {
        LoadSupportData();
    }

    private void LoadSupportData()
    {
        string resourcesPath = "Status/SupportStatus";
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcesPath);
        if (jsonAsset == null)
        {
            Debug.LogError($"SupportStatus JSON not found at Resources/{resourcesPath}.json");
            statusTable = new Dictionary<string, SupportStatus>();
            return;
        }

        string json = jsonAsset.text;
        SupportStatus[] dataArray = JsonUtility.FromJson<SupportStatusArray>("{\"array\":" + json + "}").array;

        statusTable = new Dictionary<string, SupportStatus>();
        foreach (var data in dataArray)
        {
            statusTable[data.id] = data;
        }
        SetSelectedSupports("SupportTank", "SupportKasumi"); // デフォルトのサポートキャラを設定
    }

    /// <summary>
    /// 外部から選択されたサポートキャラのIDを設定する
    /// </summary>
    public void SetSelectedSupports(string id1, string id2)
    {
        supportId1 = id1;
        supportId2 = id2;
        Debug.Log($"サポート選択: 1={id1}, 2={id2}");
    }

    /// <summary>
    /// サポート1体目を召喚
    /// </summary>
    public void Summon1()
    {
        SummonSupport(supportId1);
    }

    /// <summary>
    /// サポート2体目を召喚
    /// </summary>
    public void Summon2()
    {
        SummonSupport(supportId2);
    }

    /// <summary>
    /// 指定IDのサポートキャラを召喚
    /// </summary>
    private void SummonSupport(string supportId)
    {
        if (string.IsNullOrEmpty(supportId))
        {
            Debug.LogWarning("召喚IDが設定されていません");
            return;
        }

		string path = $"Prefab/Support/{supportId}";
		GameObject prefab = Resources.Load<GameObject>(path);
		if (prefab == null)
        {
            Debug.LogError($"Prefab not found for {supportId}");
            return;
        }

        GameObject instance = Instantiate(prefab, summonPosition.position, Quaternion.identity);
        var support = instance.GetComponent<SupportBase>();

        if (support != null && statusTable.ContainsKey(supportId))
        {
            support.Initialize(statusTable[supportId]);
            support.Act();
        }
        else
        {
            Debug.LogWarning($"SupportBaseが見つからない、またはステータスが不明: {supportId}");
        }
    }

    [System.Serializable]
    private class SupportStatusArray
    {
        public SupportStatus[] array;
    }
}
