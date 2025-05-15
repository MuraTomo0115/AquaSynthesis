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
    /// �_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    public void HitAttack(int damage)
    {
        _currentHealth -= damage;
        Debug.Log($"{_characterId} �̓_���[�W�� {damage} �H�炢�܂����B�c��HP: {_currentHealth}");

        // �Ԃ�����
        if (_spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (_currentHealth <= 0)
        {
            Die();
        }
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
}
