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
        // DBから全サポートデータを取得
        var supportList = DatabaseManager.GetAllSupportStatuses();
        statusTable = new Dictionary<string, SupportStatus>();
        foreach (var data in supportList)
        {
            if (string.IsNullOrEmpty(data.Name))
            {
                Debug.LogWarning("SupportStatusにnameが設定されていないデータがあります。スキップします。");
                continue;
            }
            statusTable[data.Name] = data;
        }
        // デフォルトのサポートキャラを設定（例: "Kasumi", "Drone" などDBのnameと一致する値）
        SetSelectedSupports("Kasumi", "Kasumi");
        Debug.Log("登録サポート名: " + string.Join(", ", statusTable.Keys));
    }

    /// <summary>
    /// 外部から選択されたサポートキャラの名前を設定する
    /// </summary>
    public void SetSelectedSupports(string name1, string name2)
    {
        supportId1 = name1;
        supportId2 = name2;
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
    /// 指定名のサポートキャラを召喚
    /// </summary>
    private void SummonSupport(string supportName)
    {
        if (string.IsNullOrEmpty(supportName))
        {
            Debug.LogWarning("召喚名が設定されていません");
            return;
        }

        string path = $"Prefab/Support/{supportName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found for {supportName}");
            return;
        }

        GameObject instance = Instantiate(prefab, summonPosition.position, Quaternion.identity);
        var support = instance.GetComponent<SupportBase>();

        if (support != null && statusTable.ContainsKey(supportName))
        {
            AudioManager.Instance.PlaySE("Support", "546SupportSE");
            support.Initialize(statusTable[supportName]);
            support.Act();
        }
        else
        {
            Debug.LogWarning($"SupportBaseが見つからない、またはステータスが不明: {supportName}");
        }
    }
}
