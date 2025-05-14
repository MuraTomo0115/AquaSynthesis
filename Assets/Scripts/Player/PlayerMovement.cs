using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float      _moveSpeed = 5f;  // 移動速度
    [SerializeField] private float      _jumpForce = 5f;  // ジャンプ力
    [SerializeField] private float      _ray = 1f;        // 地面を検出するレイの長さ
    [SerializeField] private Transform  _groundCheck;     // 足元の空オブジェクト
    [SerializeField] private LayerMask  _groundLayer;     // 地面のタグ
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
    /// InputSystem設定
    /// </summary>
    public void OnEnable()
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
        _inputActions.Player.Pistol.performed += ctx => Pistol();
    }

    /// <summary>
    /// InputSystemを無効にする
    /// </summary>
    public void DisableInput()
    {
        // InputSystemを無効にする
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
            _attackSensor.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }
        else if (_movement.x < -0.01f)
        {
            _spriteRenderer.flipX = true; // 左向き
            _attackSensor.transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(-Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }

        // 地面チェック
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
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // フラグが無効の場合ジャンプしない
        if (!_is_CanJump) return;

        // ForceMode2Dを使用し、瞬発的にジャンプ
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _is_CanJump = false;
    }

    private void Attack()
    {
        if (!_canAdjacentAttack) return; // 攻撃が許可されていなければ中断

        _animator.SetTrigger("AttackSword");
    }

    private void Pistol()
    {
        if (!_canPistolAttack) return;

        // 本素材導入時までコメントアウト
        //_animator.SetTrigger("AttackPistol");

        //本素材導入時、アニメーションパスで発火させる
        ShootPistol();
    }

    /// <summary>
    /// 本素材導入時、アニメーションパスで発火させる
    /// </summary>
    private void ShootPistol()
    {
        // Bullet を生成
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        // 弾に向きを伝える
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);

        // Bullet に PlayerMovement を渡す
        bulletScript.SetPlayerMovement(this);

        _canPistolAttack = false;

        Invoke(nameof(CanPistol), _pistolCoolTime);
    }

    /// <summary>
    /// 本素材導入時削除。ピストﾙクールタイム
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
        // 敵のCharacterコンポーネントを取得
        Character hitObject = other.GetComponent<Character>();

        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
            //Debug.Log(_charaState.AttackPower + " 敵は " + hitObject.AttackPower);
        }
    }

    public void StartAttack()
    {
        _attackSensor.gameObject.SetActive(true);

        // プレイヤーの向きに合わせて攻撃判定のスケールを変更
        if (_spriteRenderer.flipX)
        {
            // 左向き（反転）
            _attackSensor.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // 右向き
            _attackSensor.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void EndAttack()
    {
        // 攻撃判定を無効化
        _attackSensor.transform.localScale = new Vector3(0, 0, 0); // スケールをリセット
        _attackSensor.gameObject.SetActive(false); // 非表示
    }
}
