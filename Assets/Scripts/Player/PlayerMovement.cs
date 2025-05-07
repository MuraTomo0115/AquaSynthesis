using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float     _moveSpeed = 5f;  // 移動速度
    [SerializeField]
    private float     _jumpForce = 5f;  // ジャンプ力
    [SerializeField]
    private float     _ray = 1f;        // 地面を検出するレイの長さ
    [SerializeField]
    private Transform _groundCheck; // 足元の空オブジェクト
    [SerializeField]
    private           LayerMask _groundLayer; // 地面のタグ
    private           Rigidbody2D _rb;
    private           Vector2 _movement;      // 自機
    private           PlayerInputActions _inputActions;
    private bool      _isCanJump = true;
    private Animator  _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _canAttack = true;
    [SerializeField]
    private GameObject attackSensor;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    /// <summary>
    /// InputSystem設定
    /// </summary>
    private void OnEnable()
    {
        // InputSystemを有効にする
        _inputActions.Player.Enable();
        _inputActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        // 入力がなくなった場合加える力を０にする
        _inputActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        // ジャンプする
        _inputActions.Player.Jump.performed += ctx => Jump();
        // 攻撃
        _inputActions.Player.Attack.performed += ctx => Attack();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 移動、ジャンプ処理
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

        // 移動方向に応じて向きを変える
        if (_movement.x > 0.01f)
        {
            _spriteRenderer.flipX = false; // 右向き
            attackSensor.transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
        else if (_movement.x < -0.01f)
        {
            _spriteRenderer.flipX = true; // 左向き
            attackSensor.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
        }

        // 地面チェック
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
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // フラグが無効の場合ジャンプしない
        if (!_isCanJump) return;

        // ForceMode2Dを使用し、瞬発的にジャンプ
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _isCanJump = false;
    }

    private void Attack()
    {
        if (!_canAttack) return; // 攻撃が許可されていなければ中断

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

    public void StartAttack()
    {
        attackSensor.gameObject.SetActive(true);
    }

    public void EndAttack()
    {
        attackSensor.gameObject.SetActive(false);
    }
}
