using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Character : MonoBehaviour
{
    [SerializeField] private string _characterName;
    [SerializeField] private GameObject _expPrefab;
    [SerializeField] private float _hpBarShowTime = 2f;

    [Header("攻撃を受けるか")]
    [SerializeField] private bool _canHit = true;   // 攻撃を受けるかどうか

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
    private bool _isBoss = false;
    private string _seFile;
    private int _getExp = 0;
    private string _route;

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

    private void Start()
    {
        _isBoss = CompareTag("Boss");
    }

    /// <summary>
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    public void HitAttack(int damage)
    {
        if (_isDead || !_canHit) return;

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
            var boss = DatabaseManager.GetBossByName(_characterName);
            AudioManager.Instance.PlaySE("Boss", boss.idle_voice, "N1");

            // �_���[�W�v�Z
            _currentHealth -= damage;

            _getExp = boss.exp;

            // �{�XHP�o�[���X�V
            BossHPBar bossHpBar = FindObjectOfType<BossHPBar>();
            if (bossHpBar != null)
            {
                bossHpBar.SetHP(_currentHealth, _maxHealth);
                bossHpBar.AppearHPBar(true);
            }
        }

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

        if(_isBoss)
        {
            var boss = DatabaseManager.GetBossByName(_characterName);

            if (boss.flag != null)
                GoalTrigger.Instance.SetRoute(boss.flag);
        }

        ExpManager.Instance.AddExp(_getExp);

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

            // 現在のシーン名をPlayerPrefsに保存（ゲームオーバー時のリトライ用）
            PlayerPrefs.SetString("LastPlayedScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

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

            // ゲームオーバーシーンへの遷移を開始
            StartCoroutine(TransitionToGameOver());
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

    /// <summary>
    /// ゲームオーバーシーンへの遷移処理（フェードアウト→シーン遷移）
    /// </summary>
    private IEnumerator TransitionToGameOver()
    {
        // 死亡してから3秒待つ
        yield return new WaitForSeconds(3.0f);

        // フェード用のCanvasとImageを作成
        var fadeCanvas = new GameObject("FadeCanvas");
        var canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // 最前面に表示

        var canvasScaler = fadeCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvas.transform);
        
        var fadeImage = fadeImageObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // 透明な黒
        
        var rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // フェードアウト（黒くする）
        float fadeDuration = 1.0f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1); // 完全に黒

        // 少し待つ
        yield return new WaitForSecondsRealtime(0.5f);

        // ゲームオーバーシーンをロード
        SceneManager.LoadScene("GameOver");
    }
}
