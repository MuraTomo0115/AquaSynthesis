using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private string _characterId;

    private int                  _maxHealth;
    private int                  _currentHealth;
    private int                  _attackPower;
    private int                  _pistolPower = 0;
    protected Animator           _animator;
    private SpriteRenderer       _spriteRenderer;
    private Color                _defaultColor;
    private GameObject           _player;
    private PlayerMovement       _playerMovement;

    public string CharacterId => _characterId;
    public int AttackPower =>    _attackPower;
    public int CurrentHealth =>  _currentHealth;
    public int MaxHealth =>      _maxHealth;
    public int PistolPower =>    _pistolPower;

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
        _currentHealth -= damage;
        Debug.Log($"{_characterId} はダメージを {damage} 食らいました。残りHP: {_currentHealth}");

        // 赤くする
        if (_spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (_currentHealth <= 0)
        {
            Die();
        }
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
}
