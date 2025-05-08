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
    private int _pistolPower = 0; // デフォルト 0 にしておくと敵にも安全

    public string CharacterId => _characterId;
    public int AttackPower => _attackPower;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public int PistolPower => _pistolPower;  // 外部から参照できるように

    // 敵用など pistolPower が無い場合
    public void SetStats(int maxHp, int atk)
    {
        _maxHealth = maxHp;
        _attackPower = atk;
        _currentHealth = maxHp;
        _pistolPower = 0;
    }

    // プレイヤー用など pistolPower を渡す場合
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
        Debug.Log($"{_characterId} はダメージを {damage} 食らいました。残りHP: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{_characterId} died.");
        Destroy(gameObject); // ゲームオブジェクト削除
    }
}
