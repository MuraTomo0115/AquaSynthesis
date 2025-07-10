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
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _ray = 1f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _spikeLayer;
    [SerializeField] private GameObject _attackSensor;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _pistolCoolTime = 1f;
    [SerializeField] private float _invincibleTime = 1f;
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
    private bool _isInvincible = false;
    private bool _isOnSpike = false;
    private AudioSource _audioSource;
    private bool _wasMoving = false;

    // InputActionsをフィールドで保持
    private PlayerInputActions playerActions;

    public bool DidAttack { get; private set; }
    public bool DidPistol { get; private set; }

    public Character CharaState => _charaState;
    public bool IsRecording { get; internal set; }
    public Vector2 MovementInput => _movement;
    public GameObject AttackSensorGameObject => _attackSensor;
    public GameObject BulletPrefab => _bullet;

    private void Awake()
    {
        _attackSensor.gameObject.SetActive(false);
    }

    private void SummonSupport(int num)
    {
        if (_supportManager == null)
        {
            Debug.LogError("SupportManager is not assigned in PlayerMovement.");
            return;
        }
        switch (num)
        {
            case 1:
                _supportManager.Summon1();
                break;
            case 2:
                _supportManager.Summon2();
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        playerActions = InputActionHolder.Instance.playerInputActions;
        playerActions.Player.Enable();
        playerActions.Support.Enable();

        playerActions.Player.Move.performed += OnMovePerformed;
        playerActions.Player.Move.canceled += OnMoveCanceled;
        playerActions.Player.Jump.performed += OnJumpPerformed;
        playerActions.Player.Attack.performed += OnAttackPerformed;
        playerActions.Player.Pistol.performed += OnPistolPerformed;
        playerActions.Support.SummonA.performed += OnSummonASupport;
        playerActions.Support.SummonB.performed += OnSummonBSupport;

        _attackSensor.gameObject.SetActive(false);
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (playerActions != null)
        {
            playerActions.Player.Move.performed -= OnMovePerformed;
            playerActions.Player.Move.canceled -= OnMoveCanceled;
            playerActions.Player.Jump.performed -= OnJumpPerformed;
            playerActions.Player.Attack.performed -= OnAttackPerformed;
            playerActions.Player.Pistol.performed -= OnPistolPerformed;
            playerActions.Support.SummonA.performed -= OnSummonASupport;
            playerActions.Support.SummonB.performed -= OnSummonBSupport;
        }
    }

    // 各InputActionのコールバック
    private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => _movement = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => _movement = Vector2.zero;
    private void OnJumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => Jump();
    private void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => Attack();
    private void OnPistolPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => Pistol();
    private void OnSummonASupport(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => SummonSupport(1);
    private void OnSummonBSupport(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => SummonSupport(2);

    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movement.x * _moveSpeed, _rb.velocity.y);

        bool isMoving = Mathf.Abs(_movement.x) > Mathf.Epsilon;

        if (isMoving && !_wasMoving)
        {
            AudioManager.Instance.PlayLoopSE("Player", PlayerSE.Dash);
            _animator.SetInteger("AnimState", 1);
        }
        else if (!isMoving && _wasMoving)
        {
            AudioManager.Instance.StopLoopSE("Player", PlayerSE.Dash);
            _animator.SetInteger("AnimState", 0);
        }

        _wasMoving = isMoving;

        _animator.SetFloat("FallSpeed", _rb.velocity.y);
        if (_movement.x > 0.01f)
        {
            _spriteRenderer.flipX = false;
            _attackSensor.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }
        else if (_movement.x < -0.01f)
        {
            _spriteRenderer.flipX = true;
            _attackSensor.transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            _firePoint.localPosition = new Vector2(-Mathf.Abs(_firePoint.localPosition.x), _firePoint.localPosition.y);
        }

        Vector2 center = _groundCheck.position;
        Vector2 left = center + Vector2.left * _offset;
        Vector2 right = center + Vector2.right * _offset;
        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, _ray, _groundLayer);

        bool isGrounded = (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null);

        if (!_is_CanJump && isGrounded)
        {
            PlaySE(PlayerSE.Landing);
        }

        _is_CanJump = isGrounded;
        _animator.SetBool("isGround", isGrounded);

        if (!isGrounded)
        {
            AudioManager.Instance.StopLoopSE("Player", PlayerSE.Dash);
        }

        DidAttack = false;
        DidPistol = false;
    }

    private void PlaySE(string sePath)
    {
        AudioManager.Instance.PlaySE("Player", sePath);
    }

    private void Jump()
    {
        if (!_is_CanJump) return;

        PlaySE(PlayerSE.Jump);
        _animator.SetTrigger("Jump");
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _is_CanJump = false;
    }

    private void Attack()
    {
        if (!_canAdjacentAttack) return;

        _animator.SetTrigger("AttackSword");
        DidAttack = true;
    }

    private void Pistol()
    {
        if (!_canPistolAttack) return;

        _animator.SetTrigger("AttackPistol");
        ShootPistol();
        DidPistol = true;
    }

    private void ShootPistol()
    {
        PlaySE(PlayerSE.Pistol);

        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);

        bulletScript.SetPlayerMovement(this);

        _canPistolAttack = false;

        bulletScript.SetIsRecording(IsRecording);
        Invoke(nameof(CanPistol), _pistolCoolTime);
    }

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
        if (((1 << other.gameObject.layer) & _spikeLayer) != 0)
        {
            return;
        }

        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
        else
        {
            // 親または子にアタッチされている場合も考慮  
            Character parentHitObject = other.GetComponentInParent<Character>();
            if (parentHitObject != null)
            {
                parentHitObject.HitAttack(_charaState.AttackPower);
            }
        }
    }

    public void StartAttack()
    {
        _attackSensor.gameObject.SetActive(true);
        PlaySE(PlayerSE.Attack);

        if (_spriteRenderer.flipX)
        {
            _attackSensor.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _attackSensor.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsRecording) return;
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsRecording) return;
        if (other.CompareTag("Spike"))
        {
            _isOnSpike = false;
        }
    }

    private void Update()
    {
        if (IsRecording) return;

        if (_isOnSpike && !_isInvincible)
        {
            _charaState.HitAttack(3);
            _isInvincible = true;
            StartCoroutine(ResetInvincible());
        }
    }

    private IEnumerator ResetInvincible()
    {
        yield return new WaitForSeconds(_invincibleTime);
        _isInvincible = false;
    }

    public void EndAttack()
    {
        _attackSensor.transform.localScale = new Vector3(0, 0, 0);
        _attackSensor.gameObject.SetActive(false);
    }

    public void Heal(float healAmount)
    {
        _charaState.Heal(healAmount);
    }
}
