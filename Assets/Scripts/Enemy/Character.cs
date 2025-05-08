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

    public string CharacterId => _characterId;
    public int AttackPower => _attackPower;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;

    // CharacterManagerから渡される
    public void SetStats(int maxHp, int atk)
    {
        _maxHealth = maxHp;
        _attackPower = atk;
        _currentHealth = maxHp;
    }

    public void HitAttack(int damage)
    {
        Debug.Log(_characterId + "はダメージを" + damage + "食らいました");
    }

    private void Die()
    {
        Debug.Log($"{_characterId} died.");
        Destroy(gameObject); // ゲームオブジェクト削除
    }
}
