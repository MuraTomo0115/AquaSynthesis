using System.Collections;
using UnityEngine;

public class Enemy1Movement : EnemyBase
{
    [Header("Settings")]
    [SerializeField] private GameObject _attackSensor;

    [Header("Ground Check")]
    [SerializeField] private float _ray = 0.3f;
    [SerializeField] private float _offset = 0.3f;
    [SerializeField] private LayerMask _groundLayer;

    private bool _isCoolingDown = false;
    private SpriteRenderer _spriteRenderer;
    private Character _character;

    /// <summary>
    /// コンポーネントの初期化
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _character = GetComponent<Character>();
        _attackSensor.SetActive(false);
    }

    /// <summary>
    /// アニメーション管理・行動制御
    /// </summary>
    private void Update()
    {
        CheckGrounded();

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        bool isWithinRange = distanceToPlayer <= detectionRange;
        bool isCloseEnoughToAttack = distanceToPlayer <= attackRange;

        animator.SetBool("isDiscoveryPlayer", isWithinRange);
        animator.SetBool("isWithinRange", isCloseEnoughToAttack);
        animator.SetBool("isCoolTime", _isCoolingDown);

        if (!_isCoolingDown && isWithinRange)
        {
            if (isCloseEnoughToAttack)
            {
                StartCoroutine(Attack());
            }
            else
            {
                Move();
            }
        }
        else
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 地面に接地しているか
    /// </summary>
    private void CheckGrounded()
    {
        Vector2 center = _groundCheck.position;
        Vector2 left = center + Vector2.left * _offset;
        Vector2 right = center + Vector2.right * _offset;

        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, _ray, _groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, _ray, _groundLayer);

        bool isGrounded = (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null);
        animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// 攻撃判定を有効化
    /// </summary>
    public void StartAttack()
    {
        _attackSensor.SetActive(true);

        if (_spriteRenderer.flipX)
        {
            _attackSensor.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _attackSensor.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    /// <summary>
    /// 攻撃判定を無効化
    /// </summary>
    public void EndAttack()
    {
        _attackSensor.transform.localScale = new Vector3(0, 0, 0);
        _attackSensor.SetActive(false);
    }

    /// <summary>
    /// EnemyBaseの抽象メソッドをオーバーライド：プレイヤー方向へ移動
    /// </summary>
    public override void Move()
    {
        float diffX = _player.position.x - transform.position.x;
        float deadZone = 0.1f; // デッドゾーン

        float directionX = 0f;
        if (Mathf.Abs(diffX) > deadZone)
        {
            directionX = Mathf.Sign(diffX);
        }

        _rigidbody2D.velocity = new Vector2(directionX * speed, _rigidbody2D.velocity.y);

        if (directionX != 0)
        {
            float scaleX = directionX > 0 ? -1f : 1f;
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    /// <summary>
    /// EnemyBaseの抽象メソッドをオーバーライド：攻撃処理
    /// </summary>
    public override IEnumerator Attack()
    {
        _rigidbody2D.velocity = Vector2.zero;
        animator.SetTrigger("AttackSword");
        animator.SetBool("isAttacking", true);

        _isCoolingDown = true;
        yield return new WaitForSeconds(coolTime);
        _isCoolingDown = false;
    }
}
