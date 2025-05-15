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

        // Data�t�H���_�iAssets/Data�j���v���W�F�N�g���ɍ쐬���A���݂��Ȃ���ΐV�K�쐬
        string dir = Application.dataPath + "/Data";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // JSON�t�@�C���̕ۑ��p�X��ݒ肵�A�t�@�C���ɏ�������
        string path = dir + "/coin.json";
        Debug.Log("�R�C���f�[�^�ۑ���: " + path);

        File.WriteAllText(path, json);
    }
}