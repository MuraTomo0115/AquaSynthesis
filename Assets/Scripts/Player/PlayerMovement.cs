using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float      _moveSpeed = 5f;  // �ړ����x
    [SerializeField] private float      _jumpForce = 5f;  // �W�����v��
    [SerializeField] private float      _ray = 1f;        // �n�ʂ����o���郌�C�̒���
    [SerializeField] private Transform  _groundCheck;     // �����̋�I�u�W�F�N�g
    [SerializeField] private LayerMask  _groundLayer;     // �n�ʂ̃^�O
    [SerializeField] private GameObject _attackSensor;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform  _firePoint;
    [SerializeField] private float      _pistolCoolTime = 1f;
    [SerializeField] float              _offset = 0.2f;
    private Rigidbody2D                 _rb;
    private Vector2                     _movement;
    private PlayerInputActions          _inputActions;
    private Character                   _charaState;
    private Animator                    _animator;
    private SpriteRenderer              _spriteRenderer;
    private bool                        _is_CanJump = true;
    private bool                        _canAdjacentAttack = true;
    private bool                        _canPistolAttack = true;
    public Character CharaState =>      _charaState;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _attackSensor.gameObject.SetActive(false);
    }

    /// <summary>
    /// InputSystem�ݒ�
    /// </summary>
    public void OnEnable()
    {
        // InputSystem��L���ɂ���
        _inputActions.Player.Enable();
        _inputActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        // ���͂��Ȃ��Ȃ����ꍇ������͂��O�ɂ���
        _inputActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        // �W�����v����
        _inputActions.Player.Jump.performed += ctx => Jump();
        // �U��
        _inputActions.Player.Attack.performed += ctx => Attack();
        _inputActions.Player.Pistol.performed += ctx => Pistol();
    }

    /// <summary>
    /// InputSystem�𖳌��ɂ���
    /// </summary>
    public void DisableInput()
    {
        // InputSystem�𖳌��ɂ���
        _inputActions.Player.Disable();
    }

    private void Start()
    {
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
        //_animator.SetTrigger("AttackPistol");

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
    /// �{�f�ޓ������폜�B�s�X�gكN�[���^�C��
    /// </summary>
    private void CanPistol()
    {
        _canPistolAttack = true;
    }

    public void trueAttack()
    {
        _canAdjacentAttack = true;
        _animator.SetBool("isAttack", true );
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

    public void EndAttack()
    {
        // �U������𖳌���
        _attackSensor.transform.localScale = new Vector3(0, 0, 0); // �X�P�[�������Z�b�g
        _attackSensor.gameObject.SetActive(false); // ��\��
    }
}
