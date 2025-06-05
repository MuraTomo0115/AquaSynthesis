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
    private bool _isDead = false; // ���S�������ǂ����̃t���O

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
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    public void HitAttack(int damage)
    {
        if (_isDead) return; // ���S���Ă��珈���X�L�b�v

        _currentHealth -= damage;
        UnityEngine.Debug.Log($"{_characterName} �̓_���[�W�� {damage} �H�炢�܂����B�c��HP: {_currentHealth}");

        // �Ԃ�����
        if (_spriteRenderer != null && _currentHealth > 0)
            StartCoroutine(FlashRed());
        else if (_spriteRenderer != null && _currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// �Ԃ��_�ł�����R���[�`��
    /// </summary>
    private IEnumerator FlashRed()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        _spriteRenderer.color = _defaultColor;
    }

    /// <summary>
    /// ���S���̏���
    /// </summary>
    protected virtual void Die()
    {
        if (_isDead) return; // �O�̂��ߑ��d���s�h�~
        _isDead = true;      // �t���OON�i1�񂾂����S�����j

        _animator?.SetTrigger("Die");

        if (CompareTag("Player"))
        {
            _playerMovement.DisableInput();

            // Rigidbody2D ������Γ������~
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            // Collider2D �������i�U������Ȃǎ󂯂Ȃ�����j
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Destroy(gameObject, 10.0f); // ���S�A�j���[�V�����Đ���ɍ폜

            return;
        }

        // �SMonoBehaviour�𖳌����i�����ȊO�j
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp != this)
                comp.enabled = false;
        }

        Destroy(gameObject, 1.0f); // ���S�A�j���[�V�����Đ���ɍ폜
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
}
