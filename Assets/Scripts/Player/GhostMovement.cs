using System.Collections;
using UnityEngine;

/// <summary>
/// ゴーストの動作・攻撃・アクション再現を行うクラス
/// </summary>
public class GhostMovement : MonoBehaviour
{
    [Header("攻撃関連")]
    private GameObject _attackSensorInstance;                 // 実際に使う攻撃センサー

    [SerializeField] private GameObject _bullet;              // ピストルの弾プレハブ（Inspector用・実際はInitializeで上書き）
    [SerializeField] private Transform _firePoint;            // 弾の発射位置

    // 記録された各種データ
    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;
    private bool _recordedFacingLeft;

    // 内部参照
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    /// <summary>
    /// ゴーストの初期化（記録データ・攻撃センサー・弾プレハブをセット）
    /// </summary>
    public void Initialize(
        Vector2 position, Vector2 input, bool jump, bool attack, bool pistol, bool summonA, bool summonB, bool facingLeft,
        GameObject playerAttackSensor = null,
        GameObject bulletPrefab = null
    )
    {
        _recordedPosition = position;
        _recordedInput = input;
        _recordedJump = jump;
        _recordedAttack = attack;
        _recordedPistol = pistol;
        _recordedSummonA = summonA;
        _recordedSummonB = summonB;
        _recordedFacingLeft = facingLeft;

        // 攻撃センサーをプレイヤーから複製
        if (playerAttackSensor != null)
        {
            _attackSensorInstance = Instantiate(playerAttackSensor, transform);
            _attackSensorInstance.SetActive(false);
        }

        // 弾プレハブをセット
        if (bulletPrefab != null)
        {
            _bullet = bulletPrefab;
        }
    }

    /// <summary>
    /// ゴーストの初期化処理
    /// </summary>
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();

        // プレイヤーのステータスをコピー
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerChar = playerObj.GetComponent<Character>();
            if (playerChar != null && _charaState != null)
            {
                _charaState.CopyStatsFrom(playerChar);
            }
        }

        // 記録された初期位置に移動
        transform.position = _recordedPosition;

        // 向き調整
        _spriteRenderer.flipX = _recordedFacingLeft;

        // 記録されたアクションを再現（初期フレームのみ）
        if (_recordedAttack)
            _animator.SetTrigger("AttackSword");
        if (_recordedPistol)
            _animator.SetTrigger("AttackPistol");
        if (_recordedJump)
            _animator.SetTrigger("Jump");
        if (_recordedSummonA)
            _animator.SetTrigger("SummonA");
        if (_recordedSummonB)
            _animator.SetTrigger("SummonB");
    }

    /// <summary>
    /// 移動アニメーションの再現
    /// </summary>
    private void FixedUpdate()
    {
        // 入力値に応じてアニメーション状態を切り替え
        if (Mathf.Abs(_recordedInput.x) > Mathf.Epsilon)
            _animator.SetInteger("AnimState", 1); // 歩き
        else
            _animator.SetInteger("AnimState", 0); // 待機
    }

    /// <summary>
    /// ピストル発射処理（ゴースト用）
    /// </summary>
    public void ShootPistol()
    {
        if (_bullet == null || _firePoint == null) return;

        // 弾を生成
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        // 進行方向を決定
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // 弾のパラメータをセット
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
            bulletScript.SetDamage(_charaState != null ? _charaState.PistolPower : 1);
            bulletScript.SetIsGhostBullet(true);
        }
    }

    /// <summary>
    /// 近接攻撃開始（攻撃センサー有効化）
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
    /// 近接攻撃終了（攻撃センサー無効化）
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
        if (_charaState == null) return;

        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    /// <summary>
    /// 記録された入力値をセット（再生時に呼ばれる）
    /// </summary>
    public void SetRecordedInput(Vector2 input)
    {
        _recordedInput = input;
    }

    // ダミーメソッド
    public void trueAttack() { }
    public void falseAttack() { }
}
