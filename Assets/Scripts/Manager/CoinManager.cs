using UnityEngine;
using System.IO;
using TMPro; // TextMeshPro�p

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    [SerializeField] private TextMeshProUGUI CoinText; // UI�\���p

    private void Start()
    {
        Debug.Log("CoinText �� null�H: " + (CoinText == null));
        AddCoin(1); // �e�X�g�p
    }


    /// <summary>
    /// �V���O���g���̃C���X�^���X�����������A�Q�[���I�u�W�F�N�g���i��������
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ł������Ȃ�

            LoadCoinData(); // �N�����ɕۑ��f�[�^����ǂݍ���
            UpdateCoinUI(); // UI�ɔ��f
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

        UpdateCoinUI();   // UI�X�V
        SaveCoinData();   // �ۑ�
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

    /// <summary>
    /// JSON�t�@�C������R�C������ǂݍ��ށi�N�����ɌĂ΂��j
    /// </summary>
    private void LoadCoinData()
    {
        string path = Application.dataPath + "/Data/coin.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CoinData data = JsonUtility.FromJson<CoinData>(json);
            _coinCount = data.totalCoins;
            Debug.Log("�ۑ����ꂽ�R�C����ǂݍ���: " + _coinCount);
        }
        else
        {
            Debug.Log("�R�C���f�[�^��������܂���B�V�K�쐬����܂��B");
        }
    }

    /// <summary>
    /// �R�C��UI�̕\�����X�V����
    /// </summary>
    private void UpdateCoinUI()
    {
        if (CoinText != null)
        {
            CoinText.text = "�~ " + _coinCount.ToString();
        }
    }
}
