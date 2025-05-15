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
    /// �ۑ��ꏊ�FAssets/Data/coin.json�i���J���p�j
    /// </summary>
    private void SaveCoinData()
    {
        // �R�C���������f�[�^�N���X�ɃZ�b�g���AJSON������ɕϊ�
        CoinData data = new CoinData
        {
            totalCoins = _coinCount
        };
        string json = JsonUtility.ToJson(data, true);

        // Assets/Data �t�H���_���쐬�i�Ȃ���΁j
        string dir = Application.dataPath + "/Data";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // coin.json �����̃t�H���_���ɕۑ�
        string path = dir + "/coin.json";

        // �ۑ��p�X���f�o�b�O�\���i�m�F�p�j
        Debug.Log("�R�C���f�[�^�ۑ���: " + path);

        // ���ۂɃt�@�C���ɏ�������
        File.WriteAllText(path, json);
    }
}