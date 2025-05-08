using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float     _moveSpeed = 5f;  // �ړ����x
    [SerializeField]
    private float     _jumpForce = 5f;  // �W�����v��
    [SerializeField]
    private float     _ray = 1f;        // �n�ʂ����o���郌�C�̒���
    [SerializeField]
    private Transform _groundCheck; // �����̋�I�u�W�F�N�g
    [SerializeField]
    private           LayerMask _groundLayer; // �n�ʂ̃^�O
    private           Rigidbody2D _rb;
    private           Vector2 _movement;      // ���@
    private           PlayerInputActions _inputActions;
    private bool      _isCanJump = true;
    private Animator  _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _canAttack = true;
    [SerializeField]
    private GameObject attackSensor;
    private Character _charaState;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        attackSensor.gameObject.SetActive(false);
    }

    /// <summary>
    /// InputSystem�ݒ�
    /// </summary>
    private void OnEnable()
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
            attackSensor.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
        }
        else if (_movement.x < -0.01f)
        {
            _spriteRenderer.flipX = true; // ������
            attackSensor.transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }

        // �n�ʃ`�F�b�N
        RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector2.down, _ray, _groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            _isCanJump = true;
            _animator.SetBool("isGround", true);
        }
        else
        {
            _isCanJump = false;
            _animator.SetBool("isGround", false);
        }
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    private void Jump()
    {
        // �t���O�������̏ꍇ�W�����v���Ȃ�
        if (!_isCanJump) return;

        // ForceMode2D���g�p���A�u���I�ɃW�����v
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _isCanJump = false;
    }

    private void Attack()
    {
        if (!_canAttack) return; // �U����������Ă��Ȃ���Β��f

        _animator.SetTrigger("AttackSword");
    }

    public void trueAttack()
    {
        _canAttack = true;
        _animator.SetBool("isAttack", true );
    }

    public void falseAttack()
    {
        _canAttack = false;
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
        attackSensor.gameObject.SetActive(true);

        // �v���C���[�̌����ɍ��킹�čU������̃X�P�[����ύX
        if (_spriteRenderer.flipX)
        {
            // �������i���]�j
            attackSensor.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // �E����
            attackSensor.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void EndAttack()
    {
        // �U������𖳌���
        attackSensor.transform.localScale = new Vector3(0, 0, 0); // �X�P�[�������Z�b�g
        attackSensor.gameObject.SetActive(false); // ��\��
    }
}
