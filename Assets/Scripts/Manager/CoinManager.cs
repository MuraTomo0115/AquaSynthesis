using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    // ����Z�[�u�f�[�^���ƂɊ���U����\���ID�i���͉���1���g�p�j
    private int _currentPlayerId = 1;

    [SerializeField] private TextMeshProUGUI CoinText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ł��c��
            LoadCoinDataFromDatabase();     // �R�C���ǂݍ���
            UpdateCoinUI();                 // UI�X�V
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ����Z�[�u�f�[�^�I����ʂł���ID���ݒ肳���z��
    /// </summary>
    public void SetPlayerId(int id)
    {
        _currentPlayerId = id;
    }

    /// <summary>
    /// �f�[�^�x�[�X���猻�݂̃v���C���[ID�̃R�C���f�[�^��ǂݍ���
    /// </summary>
    private void LoadCoinDataFromDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        var character = characters.Find(c => c.Id == _currentPlayerId);

        if (character != null)
        {
            _coinCount = character.Coin;
        }
        else
        {
            Debug.LogWarning($"[Load] CharacterStatus �� ID={_currentPlayerId} �̃��R�[�h��������܂���B");
        }
    }

    /// <summary>
    /// �R�C�������Z���AUI�ƃf�[�^�x�[�X�ɔ��f
    /// </summary>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        UpdateCoinUI();
        SaveCoinDataToDatabase();
    }

    /// <summary>
    /// ���݂̃R�C���f�[�^���f�[�^�x�[�X�ɕۑ�
    /// </summary>
    private void SaveCoinDataToDatabase()
    {
        List<CharacterStatus> characters = DatabaseManager.GetAllCharacters();
        var character = characters.Find(c => c.Id == _currentPlayerId);

        if (character != null)
        {
            DatabaseManager.Connection.Execute(
                "UPDATE CharacterStatus SET Coin = ? WHERE Id = ?",
                _coinCount, character.Id);
        }
        else
        {
            Debug.LogWarning($"[Save] CharacterStatus �� ID={_currentPlayerId} �̃��R�[�h��������܂���B");
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
