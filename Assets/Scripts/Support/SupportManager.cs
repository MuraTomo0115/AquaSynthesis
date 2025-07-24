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
        // DB����S�T�|�[�g�f�[�^���擾
        var supportList = DatabaseManager.GetAllSupportStatuses();
        statusTable = new Dictionary<string, SupportStatus>();
        foreach (var data in supportList)
        {
            if (string.IsNullOrEmpty(data.Name))
            {
                Debug.LogWarning("SupportStatus��name���ݒ肳��Ă��Ȃ��f�[�^������܂��B�X�L�b�v���܂��B");
                continue;
            }
            statusTable[data.Name] = data;
        }
        // �f�t�H���g�̃T�|�[�g�L������ݒ�i��: "Kasumi", "Drone" �Ȃ�DB��name�ƈ�v����l�j
        SetSelectedSupports("Kasumi", "Kasumi");
    }

    /// <summary>
    /// �O������I�����ꂽ�T�|�[�g�L�����̖��O��ݒ肷��
    /// </summary>
    public void SetSelectedSupports(string name1, string name2)
    {
        supportId1 = name1;
        supportId2 = name2;
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
    /// �w�薼�̃T�|�[�g�L����������
    /// </summary>
    private void SummonSupport(string supportName)
    {
        if (string.IsNullOrEmpty(supportName))
        {
            Debug.LogWarning("���������ݒ肳��Ă��܂���");
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
            Debug.LogWarning($"SupportBase��������Ȃ��A�܂��̓X�e�[�^�X���s��: {supportName}");
        }
    }
}
