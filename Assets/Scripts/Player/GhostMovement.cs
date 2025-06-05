using System.Collections;
using UnityEngine;

/// <summary>
/// ゴーストの動作・攻撃・アクション再現を行うクラス
/// </summary>
public class GhostMovement : MonoBehaviour
{
    [Header("攻撃関連")]
    [SerializeField] private GameObject _attackSensorPrefab;  // 攻撃センサープレハブ
    private GameObject _attackSensorInstance;

    [SerializeField] private GameObject _bullet;     // ピストルの弾プレハブ
    [SerializeField] private Transform _firePoint;   // 弾の発射位置

    // 記録された各種データ
    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;
    private bool _recordedFacingLeft; // 向き

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    /// <summary>
    /// 記録データを受け取って初期化
    /// </summary>
    public void Initialize(Vector2 position, Vector2 input, bool jump, bool attack, bool pistol, bool summonA, bool summonB, bool facingLeft)
    {
        _recordedPosition = position;
        _recordedInput = input;
        _recordedJump = jump;
        _recordedAttack = attack;
        _recordedPistol = pistol;
        _recordedSummonA = summonA;
        _recordedSummonB = summonB;
        _recordedFacingLeft = facingLeft;
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();

        // プレイヤーのCharacterを探してステータスをコピー
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerChar = playerObj.GetComponent<Character>();
            if (playerChar != null && _charaState != null)
            {
                _charaState.CopyStatsFrom(playerChar);
            }
        }

        // 初期位置に移動
        transform.position = _recordedPosition;

        // 攻撃センサー生成
        if (_attackSensorPrefab != null)
        {
            _attackSensorInstance = Instantiate(_attackSensorPrefab, transform);
            _attackSensorInstance.SetActive(false);
        }

        // 向き調整
        _spriteRenderer.flipX = _recordedFacingLeft;

        // アクション再現
        if (_recordedAttack)
        {
            _animator.SetTrigger("AttackSword");
        }

        if (_recordedPistol)
        {
            _animator.SetTrigger("AttackPistol");
            // 不要なログ削除
        }

        if (_recordedJump)
        {
            _animator.SetTrigger("Jump");
        }

        if (_recordedSummonA)
        {
            _animator.SetTrigger("SummonA");
        }

        if (_recordedSummonB)
        {
            _animator.SetTrigger("SummonB");
        }
    }

    /// <summary>
    /// 移動アニメーションの再現
    /// </summary>
    private void FixedUpdate()
    {
        if (Mathf.Abs(_recordedInput.x) > Mathf.Epsilon)
        {
            _animator.SetInteger("AnimState", 1); // 歩きモーション
        }
        else
        {
            _animator.SetInteger("AnimState", 0); // 待機モーション
        }
    }

    /// <summary>
    /// ピストル発射処理
    /// </summary>
    public void ShootPistol()
    {
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);
        bulletScript.SetDamage(_charaState != null ? _charaState.PistolPower : 1);
        bulletScript.SetIsGhostBullet(true); // ★ゴースト弾フラグをセット
    }

    /// <summary>
    /// 近接攻撃開始
    /// </summary>
    public void StartAttack()
    {
        if (_attackSensorInstance != null)
        {
            _attackSensorInstance.SetActive(true);
            _attackSensorInstance.transform.localScale = new Vector3(
                _spriteRenderer.flipX ? -1 : 1, 1, 1
            );
        }
    }

    /// <summary>
    /// 近接攻撃終了
    /// </summary>
    public void EndAttack()
    {
        if (_attackSensorInstance != null)
        {
            _attackSensorInstance.transform.localScale = Vector3.zero;
            _attackSensorInstance.SetActive(false);
        }
    }

    /// <summary>
    /// 攻撃がヒットした時の処理
    /// </summary>
    public void OwnAttackHit(Collider2D other)
    {
        if (_charaState == null)
        {
            return;
        }
        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    // クラス内に追加
    public void SetRecordedInput(Vector2 input)
    {
        _recordedInput = input;
    }
}
