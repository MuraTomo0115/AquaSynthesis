using System.Collections;
using UnityEngine;

public class Enemy1Movement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private float _coolTime = 2.0f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _player;
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private GameObject _attackSensor;

    [Header("Ground Check")]
    [SerializeField] private float _ray = 0.3f;
    [SerializeField] private float _offset = 0.3f;
    [SerializeField] private LayerMask _groundLayer;

    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private bool _isCoolingDown = false;
    private SpriteRenderer _spriteRenderer;
    private Character _character;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _character = GetComponent<Character>();
        _attackSensor.gameObject.SetActive(false);
    }

    /// <summary>
    /// アニメーション管理
    /// </summary>
    private void Update()
    {
        CheckGrounded();

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        bool isWithinRange = distanceToPlayer <= _detectionRange;
        bool isCloseEnoughToAttack = distanceToPlayer <= _attackRange;

        _animator.SetBool("isDiscoveryPlayer", isWithinRange);
        _animator.SetBool("isWithinRange", isCloseEnoughToAttack);
        _animator.SetBool("isCoolTime", _isCoolingDown);

        // スケールの向きでアニメーション側に移動方向を伝える
        float scaleX = transform.localScale.x;
       
        if (!_isCoolingDown && isWithinRange)
        {
            if (isCloseEnoughToAttack)
            {
                StartCoroutine(Attack());
            }
            else
            {
                // ダッシュ移動
                MoveTowardsPlayer();
            }
        }
        else
        {
            // 停止
            _rigidbody2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 地面に接触しているかどうかを確認
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
        _animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// 当たり判定を出現
    /// </summary>
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

    /// <summary>
    /// 当たり判定を消去
    /// </summary>
    public void EndAttack()
    {
        // 攻撃判定を無効化
        _attackSensor.transform.localScale = new Vector3(0, 0, 0); // スケールをリセット
        _attackSensor.gameObject.SetActive(false); // 非表示
    }

    /// <summary>
    /// プレイヤーに向かって移動
    /// </summary>
    private void MoveTowardsPlayer()
    {
        Vector2 direction = (_player.position - transform.position).normalized;
        _rigidbody2D.velocity = new Vector2(direction.x * _speed, _rigidbody2D.velocity.y);

        // 向き調整（スケールで制御）
        if (direction.x != 0)
        {
            float scaleX = direction.x > 0 ? -1f : 1f; // 右が負、左が正になるよう調整
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    private IEnumerator Attack()
    {
        _rigidbody2D.velocity = Vector2.zero;
        _animator.SetTrigger("AttackSword");
        _animator.SetBool("isAttacking", true);

        _isCoolingDown = true;
        yield return new WaitForSeconds(_coolTime);
        _isCoolingDown = false;
    }
}