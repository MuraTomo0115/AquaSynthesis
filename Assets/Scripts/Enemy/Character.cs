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
    private SpriteRenderer[] _spriteRenderers;
    private Color[] _defaultColors;
    private GameObject _player;
    private PlayerMovement _playerMovement;
    private bool _isDead = false; // ���S�������ǂ����̃t���O
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
        
        // 自分とすべての子オブジェクトからSpriteRendererを取得
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        // デフォルトカラーを保存
        if (_spriteRenderers != null && _spriteRenderers.Length > 0)
        {
            _defaultColors = new Color[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _defaultColors[i] = _spriteRenderers[i].color;
            }
        }
        
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerMovement = _player.GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    public void HitAttack(int damage)
    {
        if (_isDead) return;

        if (CompareTag("Player"))
        {
            var playerList = DatabaseManager.GetAllCharacters();
            var playerData = playerList.Find(c => c.name == "Shizuku");
            AudioManager.Instance.PlaySE("Player", playerData.damage_se);

            // �_���[�W�v�Z
            _currentHealth -= damage;
        }
        else if (CompareTag("Enemy"))
        {
            // HP�o�[���������Ȃ琶��
            if (_hpBar == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                var prefab = Resources.Load<EnemyHPBar>("Prefab/HPBar/EnemyHealth");
                if (prefab == null)
                {
                    Debug.LogError("EnemyHPBar�v���n�u�����[�h�ł��܂���B�p�X��X�N���v�g�̃A�^�b�`���m�F���Ă��������B");
                }
                else
                {
                    _hpBar = Instantiate(prefab, canvas.transform);
                    _hpBar.Init(transform);
                }
            }

            // �_���[�W�v�Z
            _currentHealth -= damage;

            // HP�o�[�X�V���\��
            _hpBar.SetHP(_currentHealth, _maxHealth);
            _hpBar.ShowForSeconds(_hpBarShowTime);
        }
        else if (CompareTag("Boss"))
        {
            var boss = DatabaseManager.GetBossByName("SeaDemon");
            AudioManager.Instance.PlaySE("Boss", boss.idle_voice, "N1");

            // �_���[�W�v�Z
            _currentHealth -= damage;

            // �{�XHP�o�[���X�V
            BossHPBar bossHpBar = FindObjectOfType<BossHPBar>();
            if (bossHpBar != null)
            {
                bossHpBar.SetHP(_currentHealth, _maxHealth);
                bossHpBar.AppearHPBar(true);
            }
        }

        UnityEngine.Debug.Log($"{_characterName} �̓_���[�W�� {damage} �H�炢�܂����B�c��HP: {_currentHealth}");

        if (_spriteRenderers != null && _spriteRenderers.Length > 0 && _currentHealth > 0)
            StartCoroutine(FlashRed());
        else if (_spriteRenderers != null && _spriteRenderers.Length > 0 && _currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// 赤く点滅させるコルーチン
    /// </summary>
    private IEnumerator FlashRed()
    {
        // すべてのSpriteRendererを赤くする
        if (_spriteRenderers != null)
        {
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                    _spriteRenderers[i].color = Color.red;
            }
        }
        
        yield return new WaitForSeconds(0.15f);
        
        // すべてのSpriteRendererを元の色に戻す
        if (_spriteRenderers != null && _defaultColors != null)
        {
            for (int i = 0; i < _spriteRenderers.Length && i < _defaultColors.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                    _spriteRenderers[i].color = _defaultColors[i];
            }
        }
    }

    /// <summary>
    /// ���S���̏���
    /// </summary>
    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // HP�o�[��j��
        if (_hpBar != null)
        {
            _hpBar.FadeOutAndDestroy(0.3f);
            _hpBar = null;
        }

        // �{�XHP�o�[���\��
        if (CompareTag("Boss"))
        {
            BossHPBar bossHpBar = FindObjectOfType<BossHPBar>();
            if (bossHpBar != null)
            {
                bossHpBar.AppearHPBar(false);
            }
        }

        // �j��\�I�u�W�F�N�g�̏ꍇ�����𕪂���
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
        // _currentHealth���񕜁i�ő�l�𒴂��Ȃ��j
        _currentHealth = Mathf.Min(_currentHealth + Mathf.RoundToInt(amount), _maxHealth);

        // HP�v���p�e�B�������ifloat�^�Ŏg���ꍇ�̂݁j
        HP = _currentHealth;
        MaxHP = _maxHealth;

        Debug.Log($"{_characterName} healed! Current HP: {_currentHealth}");
    }

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

    public void SetSE(string path)
    {
        _seFile = path;
    }

    /// <summary>
    /// ����Character����X�e�[�^�X���R�s�[����
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
    /// �j��\�I�u�W�F�N�g�̏���
    /// </summary>
    private void DestructibleObj()
    {
        AudioManager.Instance.PlaySE("StageObj", _seFile);
        float dropChance = 0.8f; // 80%�̊m��
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
