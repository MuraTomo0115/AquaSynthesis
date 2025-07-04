using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private string _characterName;
    [SerializeField] private GameObject _expPrefab;
    [SerializeField] private float _hpBarShowTime = 2f;
    private EnemyHPBar _hpBar;


    private int _maxHealth;
    private int _currentHealth;
    private int _attackPower;
    private int _pistolPower = 0;
    protected Animator _animator;
    protected Animation _animation;
    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;
    private GameObject _player;
    private PlayerMovement _playerMovement;
    private bool _isDead = false; // 死亡したかどうかのフラグ
    private string _seFile;

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
        _animation = GetComponent<Animation>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
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
        if (_isDead) return;

        if (CompareTag("Player"))
        {
            var playerList = DatabaseManager.GetAllCharacters();
            var playerData = playerList.Find(c => c.name == "Shizuku");
            AudioManager.Instance.PlaySE("Player", playerData.damage_se);

            // ダメージ計算
            _currentHealth -= damage;
        }
        else if (CompareTag("Enemy"))
        {
            // HPバーが未生成なら生成
            if (_hpBar == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                var prefab = Resources.Load<EnemyHPBar>("Prefab/HPBar/EnemyHealth");
                if (prefab == null)
                {
                    Debug.LogError("EnemyHPBarプレハブがロードできません。パスやスクリプトのアタッチを確認してください。");
                }
                else
                {
                    _hpBar = Instantiate(prefab, canvas.transform);
                    _hpBar.Init(transform);
                }
            }

            // ダメージ計算
            _currentHealth -= damage;

            // HPバー更新＆表示
            _hpBar.SetHP(_currentHealth, _maxHealth);
            _hpBar.ShowForSeconds(_hpBarShowTime);
        }
        else if (CompareTag("Boss"))
        {
            // ダメージ計算
            _currentHealth -= damage;

            // ボスHPバーを更新
            BossHPBar bossHpBar = FindObjectOfType<BossHPBar>();
            if (bossHpBar != null)
            {
                bossHpBar.SetHP(_currentHealth, _maxHealth);
                bossHpBar.AppearHPBar(true);
            }
        }

        UnityEngine.Debug.Log($"{_characterName} はダメージを {damage} 食らいました。残りHP: {_currentHealth}");

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
        if (_isDead) return;
        _isDead = true;

        // HPバーを破棄
        if (_hpBar != null)
        {
            _hpBar.FadeOutAndDestroy(0.3f);
            _hpBar = null;
        }

        // ボスHPバーを非表示
        if (CompareTag("Boss"))
        {
            BossHPBar bossHpBar = FindObjectOfType<BossHPBar>();
            if (bossHpBar != null)
            {
                bossHpBar.AppearHPBar(false);
            }
        }

        // 破壊可能オブジェクトの場合処理を分ける
        if (CompareTag("Destructible"))
        {
            DestructibleObj();
            return;
        }

        if(_animator != null)
            _animator?.SetTrigger("Die");

        if (CompareTag("Player"))
        {
            InputActionHolder.Instance.playerInputActions.Player.Disable();

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Destroy(gameObject, 10.0f);
            return;
        }

        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp != this)
                comp.enabled = false;
        }

        Destroy(gameObject, 1.0f);
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

    public void SetSE(string path)
    {
        _seFile = path;
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

    /// <summary>
    /// 破壊可能オブジェクトの処理
    /// </summary>
    private void DestructibleObj()
    {
        AudioManager.Instance.PlaySE("StageObj", _seFile);
        float dropChance = 0.8f; // 80%の確率
        if (Random.value < dropChance && _expPrefab != null)
        {
            GameObject exp = Instantiate(_expPrefab, transform.position, Quaternion.identity, this.transform);
            var anim = exp.GetComponent<Animation>();
            if (anim != null) anim.Play();

            int _expMin = 500;
            int _expMax = 1000;
            int _expStep = 100;
            int stepCount = (_expMax - _expMin) / _expStep + 1;
            int expCount = _expMin + _expStep * Random.Range(0, stepCount);

            var popup = exp.GetComponent<ExpPopup>();
            if (popup != null)
            {
                popup.SetExp(expCount);
            }

            ExpManager.Instance.AddExp(expCount);
        }
        _animator?.SetTrigger("Destroy");
        Destroy(gameObject, 1.0f);
    }
}
