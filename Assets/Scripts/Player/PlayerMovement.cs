using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//　主製作者：村田智哉
//  編集者：秋葉朋輝

public struct PlayerSE
{
    public const string Attack = "540AttackSE";
    public const string Pistol = "541PistolSE";
    public const string Jump = "542JumpSE";
    public const string Landing = "543LandingSE";
    public const string Dash = "544DashSE";
    public const string Hit = "545DamageSE";
}

/// <summary>
/// プレイヤーの移動・攻撃・トゲダメージ・記録中フラグ管理
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;  // 移動速度
    [SerializeField] private float _jumpForce = 5f;  // ジャンプ力
    [SerializeField] private float _ray = 1f;        // 地面を検出するレイの長さ
    [SerializeField] private Transform _groundCheck;     // 足元の空オブジェクト
    [SerializeField] private LayerMask _groundLayer;     // 地面のタグ
    [SerializeField] private LayerMask _spikeLayer;      // Spike用LayerMask
    [SerializeField] private GameObject _attackSensor;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _pistolCoolTime = 1f;
    [SerializeField] private float _invincibleTime = 1f;   // ダメージ後の無敵時間
    [SerializeField] float _offset = 0.2f;
    [SerializeField] private SupportManager _supportManager;
    private Rigidbody2D _rb;
    private Vector2 _movement;
    private Character _charaState;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _is_CanJump = true;
    private bool _canAdjacentAttack = true;
    private bool _canPistolAttack = true;
    private bool _isInvincible = false; // 無敵状態かどうか
    private bool _isOnSpike = false; // トゲにいるかどうか
    private AudioSource _audioSource;
    private bool _wasMoving = false; // 前フレームの移動状態

    // 追加: 攻撃・ピストル発射フラグ
    public bool DidAttack { get; private set; }
    public bool DidPistol { get; private set; }

    public Character CharaState => _charaState;

    /// <summary>
    /// 記録中かどうか（RecordAbilityのみ書き換え可）
    /// </summary>
    public bool IsRecording { get; internal set; } // internal setでRecordAbilityのみ操作

    public Vector2 MovementInput => _movement;

    private void Awake()
    {
        _attackSensor.gameObject.SetActive(false);
    }

    private void summonsupport1()
    {
        _supportManager.Summon1();
    }

    private void Start()
    {
        var playerActions = InputActionHolder.Instance.playerInputActions;
        playerActions.Player.Enable();
        playerActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        playerActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        playerActions.Player.Jump.performed += ctx => Jump();
        playerActions.Player.Attack.performed += ctx => Attack();
        playerActions.Player.Pistol.performed += ctx => Pistol();
        playerActions.Support.SummonA.performed += ctx => summonsupport1();
        playerActions.Support.SummonB.performed += ctx => _supportManager.Summon2();

        _attackSensor.gameObject.SetActive(false);
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 移動、ジャンプ処理
    /// </summary>
    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movement.x * _moveSpeed, _rb.velocity.y);

        bool isMoving = Mathf.Abs(_movement.x) > Mathf.Epsilon;

        if (isMoving && !_wasMoving)
        {
            AudioManager.Instance.PlayLoopSE("Player", PlayerSE.Dash); // 移動開始時にループSE再生
            _animator.SetInteger("AnimState", 1);
        }
        else if (!isMoving && _wasMoving)
        {
            AudioManager.Instance.StopLoopSE("Player", PlayerSE.Dash); // 移動停止時にSE停止
            _animator.SetInteger("AnimState", 0);
        }

        _wasMoving = isMoving; // 状態を記録

        _animator.SetFloat("FallSpeed", _rb.velocity.y);
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

        // 着地判定
        if (!_is_CanJump && isGrounded)
        {
            PlaySE(PlayerSE.Landing);
        }

        _is_CanJump = isGrounded;
        _animator.SetBool("isGround", isGrounded);

        // 攻撃・ピストル発射フラグをリセット
        DidAttack = false;
        DidPistol = false;
    }

    /// <summary>
    /// SEを再生するメソッド
    /// </summary>
    /// <param name="sePath">再生するファイル名</param>
    private void PlaySE(string sePath)
    {
        AudioManager.Instance.PlaySE("Player", sePath);
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // フラグが無効の場合ジャンプしない
        if (!_is_CanJump) return;

        PlaySE(PlayerSE.Jump);
        // ForceMode2Dを使用し、瞬発的にジャンプ
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _is_CanJump = false;
    }

    private void Attack()
    {
        if (!_canAdjacentAttack) return; // 攻撃が許可されていなければ中断

        _animator.SetTrigger("AttackSword");
        DidAttack = true;
    }

    private void Pistol()
    {
        if (!_canPistolAttack) return;

        _animator.SetTrigger("AttackPistol");
        ShootPistol();
        DidPistol = true; // 追加
    }

    /// <summary>
    /// 本素材導入時、アニメーションパスで発火させる
    /// </summary>
    private void ShootPistol()
    {
        PlaySE(PlayerSE.Pistol);

        // Bullet を生成
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        // 弾に向きを伝える
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);

        // Bullet に PlayerMovement を渡す
        bulletScript.SetPlayerMovement(this);

        _canPistolAttack = false;
        
        // 記録中フラグを渡す
        bulletScript.SetIsRecording(IsRecording);
        Invoke(nameof(CanPistol), _pistolCoolTime);
    }

    /// <summary>
    /// 本素材導入時削除。ピストルクールタイム
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

    public void Landing()
    {
        PlaySE(PlayerSE.Attack);
    }

    public void OwnAttackHit(Collider2D other)
    {
        // スパイクなら攻撃判定をスキップ
        if (((1 << other.gameObject.layer) & _spikeLayer) != 0)
        {
            return;
        }

        // 敵のCharacterコンポーネントを取得
        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    public void StartAttack()
    {
        _attackSensor.gameObject.SetActive(true);
        PlaySE(PlayerSE.Attack);

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

    // トゲに触れた瞬間に呼ばれる。トゲに乗っていることを記録
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsRecording) return;
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = true; // トゲの上にいるフラグを立てる
        }
    }

    // トゲから離れた瞬間に呼ばれる。トゲに乗っていないことを記録
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsRecording) return;
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = false; // トゲの上にいないフラグを立てる
        }
    }

    private void Update()
    {
        // 記録中はトゲダメージ処理をスキップ
        if (IsRecording) return;

        // トゲの上にいて無敵じゃなければダメージを受ける処理
        if (_isOnSpike && !_isInvincible)
        {
            _charaState.HitAttack(3);  // ダメージを与える
            _isInvincible = true;      // 無敵状態に切り替え
            StartCoroutine(ResetInvincible());  // 一定時間後に無敵解除
        }
    }

    /// <summary>
    /// 無敵状態を一定時間後に解除する
    /// </summary>
    private IEnumerator ResetInvincible()
    {
        yield return new WaitForSeconds(_invincibleTime);
        _isInvincible = false;
    }

    public void EndAttack()
    {
        // 攻撃判定を無効化
        _attackSensor.transform.localScale = new Vector3(0, 0, 0); // スケールをリセット
        _attackSensor.gameObject.SetActive(false); // 非表示
    }

    /// <summary>
    /// プレイヤーのHPを回復する
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void Heal(float healAmount)
    {
        _charaState.Heal(healAmount);
    }
}
