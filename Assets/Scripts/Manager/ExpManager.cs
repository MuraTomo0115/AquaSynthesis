using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance;

    [SerializeField] private TextMeshProUGUI _expText; // �o���l�\���p��UI�e�L�X�g
    private int _currentExp = 0;

    public int CurrentExp => _currentExp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _currentExp = 0; // ������
        UpdateExpText();
    }

    /// <summary>
    /// �o���l��ǉ����郁�\�b�h
    /// </summary>
    /// <param name="exp">�ǉ�����l</param>
    public void AddExp(int exp)
    {
        _currentExp += exp;
        UpdateExpText();
        Debug.Log($"�o���l��ǉ�: {exp} (���݂̌o���l: {_currentExp})");
    }

    /// <summary>
    /// ���݂̌o���l���擾���郁�\�b�h
    /// </summary>
    /// <returns></returns>
    public int GetCurrentExp()
    {
        return _currentExp;
    }

    /// <summary>
    /// �o���l�\��UI���X�V
    /// </summary>
    private void UpdateExpText()
    {
        if (_expText != null)
            _expText.text = _currentExp.ToString();
    }
}
