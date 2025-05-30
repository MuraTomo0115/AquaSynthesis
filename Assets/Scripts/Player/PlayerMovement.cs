using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : InputActionHolder
{
    [SerializeField] private float          _moveSpeed = 5f;  // �ړ����x
    [SerializeField] private float          _jumpForce = 5f;  // �W�����v��
    [SerializeField] private float          _ray = 1f;        // �n�ʂ����o���郌�C�̒���
    [SerializeField] private Transform      _groundCheck;     // �����̋�I�u�W�F�N�g
    [SerializeField] private LayerMask      _groundLayer;     // �n�ʂ̃^�O
    [SerializeField] private GameObject     _attackSensor;
    [SerializeField] private GameObject     _bullet;
    [SerializeField] private Transform      _firePoint;
    [SerializeField] private float          _pistolCoolTime = 1f;
    [SerializeField] private float          _invincibleTime = 1f;   // �_���[�W��̖��G����
    [SerializeField] private float          _offset = 0.2f;
    [SerializeField] private SupportManager _supportManager;
    private Rigidbody2D                     _rb;
    private Vector2                         _movement;
    private PlayerInputActions              _playerInputActions;
    private Character                       _charaState;
    private Animator                        _animator;
    private SpriteRenderer                  _spriteRenderer;
    private bool                            _is_CanJump = true;
    private bool                            _canAdjacentAttack = true;
    private bool                            _canPistolAttack = true;
    private bool                            _isInvincible = false; // ���G��Ԃ��ǂ���
    private bool                            _isOnSpike = false; // �g�Q�ɂ��邩�ǂ���
    private bool                            _inputEventsRegistered = false;

    public Character CharaState =>          _charaState;

    /// <summary>
    /// InputSystem�ݒ�
    /// </summary>
    public void OnEnableInput()
    {
        if (_inputEventsRegistered) return; // ���łɓo�^�ς݂Ȃ牽�����Ȃ�
        Debug.Log("Input actions changed for PlayerMovement.");

        _playerInputActions.Player.Enable();
        _playerInputActions.Support.Enable();
        _playerInputActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        _playerInputActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        _playerInputActions.Player.Jump.performed += ctx => Jump();
        _playerInputActions.Player.Attack.performed += ctx => Attack();
        _playerInputActions.Player.Pistol.performed += ctx => Pistol();
        _playerInputActions.Support.SummonA.performed += ctx => summonsupport1();
        _playerInputActions.Support.SummonB.performed += ctx => _supportManager.Summon2();

        _inputEventsRegistered = true;
    }

    public void OnDisable()
    {
        if (!_inputEventsRegistered) return; // �o�^����Ă��Ȃ���Ή������Ȃ�

        _playerInputActions.Player.Move.performed -= ctx => _movement = ctx.ReadValue<Vector2>();
        _playerInputActions.Player.Move.canceled -= ctx => _movement = Vector2.zero;
        _playerInputActions.Player.Jump.performed -= ctx => Jump();
        _playerInputActions.Player.Attack.performed -= ctx => Attack();
        _playerInputActions.Player.Pistol.performed -= ctx => Pistol();
        _playerInputActions.Support.SummonA.performed -= ctx => summonsupport1();
        _playerInputActions.Support.SummonB.performed -= ctx => _supportManager.Summon2();

        _playerInputActions.Player.Disable();
        _playerInputActions.Support.Disable();

        _inputEventsRegistered = false;
    }

    private void summonsupport1()
    {
        _supportManager.Summon1();
    }

    /// <summary>
    /// InputSystem�𖳌��ɂ���
    /// </summary>
    public void DisableInput()
    {
        _playerInputActions.Player.Disable();
    }

    private void Start()
    {
        // InputActionHolder���狤�L�C���X�^���X���擾
        _playerInputActions = InputActionHolder.Instance.playerInputActions;
        _attackSensor.gameObject.SetActive(false);
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();
    }

    /// <summary>
    /// �ړ��A�W�����v����
    /// </summary>
    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movement.x * _moveSpeed, _rb.velocity.y);

        if (Mathf.Abs(_movement.x) > Mathf.Epsilon)
        {
            _animator.SetInteger("AnimState", 1);
        }
        else
        {
            _animator.SetInteger("AnimState", 0);
        }

        _animator.SetFloat("FallSpeed", _rb.velocity.y);

        // �ړ������ɉ����Č�����ς���
        if (_movement.x > 0.01f)
        {
            _spriteRenderer.flipX = false; // �E����
            _attackSensor.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }
        else if (_movement.x < -0.01f)
        {
            _spriteRenderer.flipX = true; // ������
            _attackSensor.transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(-Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }

        // �n�ʃ`�F�b�N
        Vector2 center = _groundCheck.position;
        Vector2 left = center + Vector2.left * _offset;
        Vector2 right = center + Vector2.right * _offset;
        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, _ray, _groundLayer);

        bool isGrounded = (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null);

        _is_CanJump = isGrounded;
        _animator.SetBool("isGround", isGrounded);
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    private void Jump()
    {
        // �t���O�������̏ꍇ�W�����v���Ȃ�
        if (!_is_CanJump) return;

        // ForceMode2D���g�p���A�u���I�ɃW�����v
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _is_CanJump = false;
    }

    private void Attack()
    {
        if (!_canAdjacentAttack) return; // �U����������Ă��Ȃ���Β��f

        _animator.SetTrigger("AttackSword");
    }

    private void Pistol()
    {
        if (!_canPistolAttack) return;

        // �{�f�ޓ������܂ŃR�����g�A�E�g
        _animator.SetTrigger("AttackPistol");

        //�{�f�ޓ������A�A�j���[�V�����p�X�Ŕ��΂�����
        ShootPistol();
    }

    /// <summary>
    /// �{�f�ޓ������A�A�j���[�V�����p�X�Ŕ��΂�����
    /// </summary>
    private void ShootPistol()
    {
        // Bullet �𐶐�
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        // �e�Ɍ�����`����
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);

        // Bullet �� PlayerMovement ��n��
        bulletScript.SetPlayerMovement(this);

        _canPistolAttack = false;

        Invoke(nameof(CanPistol), _pistolCoolTime);
    }

    /// <summary>
    /// �{�f�ޓ������폜�B�s�X�g���N�[���^�C��
    /// </summary>
    private void CanPistol()
    {
        _canPistolAttack = true;
    }

    public void trueAttack()
    {
        _canAdjacentAttack = true;
        _animator.SetBool("isAttack", true);
    }

    public void falseAttack()
    {
        _canAdjacentAttack = false;
        _animator.SetBool("isAttack", false);
    }

    public void OwnAttackHit(Collider2D other)
    {
        // �G��Character�R���|�[�l���g���擾
        Character hitObject = other.GetComponent<Character>();

        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
            //Debug.Log(_charaState.AttackPower + " �G�� " + hitObject.AttackPower);
        }
    }

    public void StartAttack()
    {
        _attackSensor.gameObject.SetActive(true);

        // �v���C���[�̌����ɍ��킹�čU������̃X�P�[����ύX
        if (_spriteRenderer.flipX)
        {
            // �������i���]�j
            _attackSensor.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // �E����
            _attackSensor.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // �g�Q�ɐG�ꂽ�u�ԂɌĂ΂��B�g�Q�ɏ���Ă��邱�Ƃ��L�^
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = true; // �g�Q�̏�ɂ���t���O�𗧂Ă�
        }
    }

    // �g�Q���痣�ꂽ�u�ԂɌĂ΂��B�g�Q�ɏ���Ă��Ȃ����Ƃ��L�^
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = false; // �g�Q�̏�ɂ��Ȃ��t���O�𗧂Ă�
        }
    }

    private void Update()
    {
        // �g�Q�̏�ɂ��Ė��G����Ȃ���΃_���[�W���󂯂鏈��
        if (_isOnSpike && !_isInvincible)
        {
            _charaState.HitAttack(3);  // �_���[�W��^����
            _isInvincible = true;      // ���G��Ԃɐ؂�ւ�
            StartCoroutine(ResetInvincible());  // ��莞�Ԍ�ɖ��G����
        }
    }
    /// <summary>
    /// ���G��Ԃ���莞�Ԍ�ɉ�������
    /// </summary>
    private IEnumerator ResetInvincible()
    {
        yield return new WaitForSeconds(_invincibleTime);
        _isInvincible = false;
    }

    public void EndAttack()
    {
        // �U������𖳌���
        _attackSensor.transform.localScale = new Vector3(0, 0, 0); // �X�P�[�������Z�b�g
        _attackSensor.gameObject.SetActive(false); // ��\��
    }

    public void Heal(float healAmount)
    {
        _charaState.Heal(healAmount);
    }

    public override void ChangeInputActions()
    {
        OnDisable(); // �����C�x���g�����i�d���h�~�j
        _playerInputActions = InputActionHolder.Instance.playerInputActions; // �Ď擾
        OnEnableInput(); // �C�x���g�ēo�^
    }
}
