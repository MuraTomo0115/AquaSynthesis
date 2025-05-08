using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private string _characterId;

    private int _maxHealth;
    private int _currentHealth;
    private int _attackPower;
    private int _pistolPower = 0; // �f�t�H���g 0 �ɂ��Ă����ƓG�ɂ����S

    public string CharacterId => _characterId;
    public int AttackPower => _attackPower;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public int PistolPower => _pistolPower;  // �O������Q�Ƃł���悤��

    // �G�p�Ȃ� pistolPower �������ꍇ
    public void SetStats(int maxHp, int atk)
    {
        _maxHealth = maxHp;
        _attackPower = atk;
        _currentHealth = maxHp;
        _pistolPower = 0;
    }

    // �v���C���[�p�Ȃ� pistolPower ��n���ꍇ
    public void SetStats(int maxHp, int atk, int pistol)
    {
        _maxHealth = maxHp;
        _attackPower = atk;
        _pistolPower = pistol;
        _currentHealth = maxHp;
    }

    public void HitAttack(int damage)
    {
        _currentHealth -= damage;
        Debug.Log($"{_characterId} �̓_���[�W�� {damage} �H�炢�܂����B�c��HP: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{_characterId} died.");
        Destroy(gameObject); // �Q�[���I�u�W�F�N�g�폜
    }
}
