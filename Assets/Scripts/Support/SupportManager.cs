using UnityEngine;
using System.Collections.Generic;

public class SupportManager : MonoBehaviour
{
    [SerializeField] private Transform summonPosition;

    private Dictionary<string, SupportStatus> statusTable;

    // �I�����ꂽ�T�|�[�g�L������ID�i�O��UI�Ȃǂ���Z�b�g�j
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
        SetSelectedSupports("SupportTank", "SupportKasumi"); // �f�t�H���g�̃T�|�[�g�L������ݒ�
    }

    /// <summary>
    /// �O������I�����ꂽ�T�|�[�g�L������ID��ݒ肷��
    /// </summary>
    public void SetSelectedSupports(string id1, string id2)
    {
        supportId1 = id1;
        supportId2 = id2;
        Debug.Log($"�T�|�[�g�I��: 1={id1}, 2={id2}");
    }

    /// <summary>
    /// �T�|�[�g1�̖ڂ�����
    /// </summary>
    public void Summon1()
    {
        SummonSupport(supportId1);
    }

    /// <summary>
    /// �T�|�[�g2�̖ڂ�����
    /// </summary>
    public void Summon2()
    {
        SummonSupport(supportId2);
    }

    /// <summary>
    /// �w��ID�̃T�|�[�g�L����������
    /// </summary>
    private void SummonSupport(string supportId)
    {
        if (string.IsNullOrEmpty(supportId))
        {
            Debug.LogWarning("����ID���ݒ肳��Ă��܂���");
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
            Debug.LogWarning($"SupportBase��������Ȃ��A�܂��̓X�e�[�^�X���s��: {supportId}");
        }
    }

    [System.Serializable]
    private class SupportStatusArray
    {
        public SupportStatus[] array;
    }
}
