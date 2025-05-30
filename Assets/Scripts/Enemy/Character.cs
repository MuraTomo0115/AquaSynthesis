using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private string _characterName;

    private int _maxHealth;
    private int _currentHealth;
    private int _attackPower;
    private int _pistolPower = 0;
    protected Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;
    private GameObject _player;
    private PlayerMovement _playerMovement;
    private bool _isDead = false; // 死亡したかどうかのフラグ

    public float HP { get; private set; }
    public float MaxHP { get; private set; }

    public string CharacterName => _characterName;
    public int AttackPower => _attackPower;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public int PistolPower => _pistolPower;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _defaultColor = _spriteRenderer.color;
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerMovement = _player.GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// ダメージを受けた時の処理
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    public void HitAttack(int damage)
    {
        if (_isDead) return; // 死亡してたら処理スキップ

        _currentHealth -= damage;
        UnityEngine.Debug.Log($"{_characterName} はダメージを {damage} 食らいました。残りHP: {_currentHealth}");

        // 赤くする
        if (_spriteRenderer != null && _currentHealth > 0)
            StartCoroutine(FlashRed());
        else if (_spriteRenderer != null && _currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// 赤く点滅させるコルーチン
    /// </summary>
    private IEnumerator FlashRed()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        _spriteRenderer.color = _defaultColor;
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    protected virtual void Die()
    {
        if (_isDead) return; // 念のため多重実行防止
        _isDead = true;      // フラグON（1回だけ死亡処理）

        _animator?.SetTrigger("Die");

        if (CompareTag("Player"))
        {
            _playerMovement.DisableInput();

            // Rigidbody2D があれば動きを停止
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            // Collider2D 無効化（攻撃判定など受けなくする）
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Destroy(gameObject, 10.0f); // 死亡アニメーション再生後に削除

            return;
        }

        // 全MonoBehaviourを無効化（自分以外）
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp != this)
                comp.enabled = false;
        }

        Destroy(gameObject, 1.0f); // 死亡アニメーション再生後に削除
    }

    public void Heal(float amount)
    {
        // _currentHealthを回復（最大値を超えない）
        _currentHealth = Mathf.Min(_currentHealth + Mathf.RoundToInt(amount), _maxHealth);

        // HPプロパティも同期（float型で使う場合のみ）
        HP = _currentHealth;
        MaxHP = _maxHealth;

        Debug.Log($"{_characterName} healed! Current HP: {_currentHealth}");
    }

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

    /// <summary>
    /// 他のCharacterからステータスをコピーする
    /// </summary>
    public void CopyStatsFrom(Character other)
    {
        if (other == null) return;
        _maxHealth = other.MaxHealth;
        _attackPower = other.AttackPower;
        _pistolPower = other.PistolPower;
        _currentHealth = other.CurrentHealth;
        HP = other.HP;
        MaxHP = other.MaxHP;
    }
}
