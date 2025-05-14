using UnityEngine;
using System.IO;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    /// <summary>
    /// �V���O���g���̃C���X�^���X�����������A�Q�[���I�u�W�F�N�g���i��������
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ł������Ȃ�
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �w�肳�ꂽ�ʂ̃R�C����ǉ����A�Z�[�u�f�[�^�ɔ��f����
    /// </summary>
    /// <param name="amount">�ǉ�����R�C���̐�</param>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("�R�C���擾�I���݂̃R�C������: " + _coinCount);
        SaveCoinData();
    }

    /// <summary>
    /// ���݂̃R�C��������JSON�`���ŕۑ�����
    /// </summary>
    private void SaveCoinData()
    {
        CoinData data = new CoinData
        {
            totalCoins = _coinCount
        };

        string json = JsonUtility.ToJson(data, true);

        // ���p�X�i���ۂɂ� Application.persistentDataPath ���g���Ƃ����j
        string path = Application.dataPath + "/coin.json";
        File.WriteAllText(path, json);
    }
}
