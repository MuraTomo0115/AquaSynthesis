using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    [SerializeField] private TextMeshProUGUI CoinText; // UI�\���p

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ł������Ȃ�

            LoadCoinDataFromDatabase(); // �f�[�^�x�[�X����ǂݍ���
            UpdateCoinUI(); // UI�ɔ��f
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �R�C���̐����f�[�^�x�[�X����擾���Ĕ��f����
    /// </summary>
    private void LoadCoinDataFromDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        if (characters != null && characters.Count > 0)
        {
            _coinCount = characters[0].Coin;
            Debug.Log("DB����R�C���ǂݍ���: " + _coinCount);
        }
        else
        {
            Debug.LogWarning("CharacterStatus �e�[�u���Ƀ��R�[�h�����݂��܂���B");
        }
    }

    /// <summary>
    /// �w�肳�ꂽ�ʂ̃R�C����ǉ����A�f�[�^�x�[�X�ɕۑ�����
    /// </summary>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("�R�C���擾�I���݂̃R�C������: " + _coinCount);

        UpdateCoinUI();
        SaveCoinDataToDatabase();
    }

    /// <summary>
    /// ���݂̃R�C������ CharacterStatus �e�[�u���ɕۑ�����
    /// </summary>
    private void SaveCoinDataToDatabase()
    {
        var characters = DatabaseManager.GetAllCharacters();
        if (characters != null && characters.Count > 0)
        {
            var character = characters[0];
            // �v���C���[�����X�V�i�R�C���ȊO�͂��̂܂܁j
            DatabaseManager.Connection.Execute(
                "UPDATE CharacterStatus SET Coin = ? WHERE Id = ?",
                _coinCount, character.Id);
            Debug.Log("DB�ɃR�C����ۑ�: " + _coinCount);
        }
        else
        {
            Debug.LogWarning("�R�C���ۑ��Ɏ��s�F�v���C���[�f�[�^�����݂��܂���B");
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
