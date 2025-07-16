using System.Collections;
using UnityEngine;

public class MusselMovement : MonoBehaviour
{
    [Header("攻撃設定")]
    [SerializeField] private float _attackRange = 5f;       // 攻撃範囲
    
    [Header("落下設定")]
    [SerializeField] private float _fallAcceleration = 10f; // 落下加速度
    [SerializeField] private float _maxFallSpeed = 20f;     // 最大落下速度
    [SerializeField] private float _detectionWidth = 2f;    // プレイヤー検知幅
    [SerializeField] private float _returnThreshold = 0.1f; // 元の位置に戻る際の閾値
    
    [Header("復帰設定")]
    [SerializeField] private float _returnSpeed = 5f;       // 元の位置への復帰速度
    [SerializeField] private float _waitTimeAfterFall = 2f; // 落下後の待機時間

    [Header("攻撃倍率")]
    [SerializeField] private int _attackMultiplier = 1; // 攻撃倍率

    [Header("カメラの振動設定")]
    [SerializeField] private float _shakeIntensity = 1f;    // 振動の強さ
    [SerializeField] private float _shakeDuration = 0.5f;   // 振動の持続時間
    
    

    private int         _attackPower;           // 攻撃力
    private Rigidbody2D _rigidbody2D;
    private Transform   _player;
    private Vector3     _initialPosition;

    private Character   _character;
    
    // 状態管理用enum
    private enum MusselState
    {
        Idle,       // 待機状態
        Falling,    // 落下中
        Waiting,    // 落下後の待機
        Returning   // 元の位置に戻る
    }
    
    private MusselState _currentState = MusselState.Idle;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;

        _character = GetComponent<Character>();

        _attackPower = _character.AttackPower;
        
        // プレイヤーを検索
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        
        // 初期状態では重力を無効にする
        _rigidbody2D.gravityScale = 0f;
        
        // 外部の力から動かないように設定
        SetRigidbodyConstraints(true);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case MusselState.Idle:
                CheckForPlayer();
                break;
            case MusselState.Falling:
                // 落下中は特に何もしない（物理演算に任せる）
                break;
            case MusselState.Waiting:
                // 待機中も特に何もしない
                break;
            case MusselState.Returning:
                ReturnToInitialPosition();
                break;
        }
    }

    /// <summary>
    /// プレイヤーが下にいるかチェックする
    /// </summary>
    private void CheckForPlayer()
    {
        if (_player == null) return;
        
        // プレイヤーが自分より下にいて、横方向の距離が検知幅内にいるかチェック
        bool isPlayerBelow = _player.position.y < transform.position.y;
        bool isPlayerInRange = Mathf.Abs(_player.position.x - transform.position.x) <= _detectionWidth;
        
        if (isPlayerBelow && isPlayerInRange)
        {
            StartFalling();
        }
    }

    /// <summary>
    /// 落下を開始する
    /// </summary>
    private void StartFalling()
    {
        _currentState = MusselState.Falling;
        _rigidbody2D.gravityScale = 1f;
        
        // 落下中はY軸のみ動けるように制約を設定
        SetRigidbodyConstraints(false);
        
        // 加速度を適用
        StartCoroutine(AccelerateFall());
    }

    /// <summary>
    /// 落下加速度を適用するコルーチン
    /// </summary>
    private IEnumerator AccelerateFall()
    {
        while (_currentState == MusselState.Falling)
        {
            // 現在の速度に加速度を加える
            Vector2 currentVelocity = _rigidbody2D.velocity;
            currentVelocity.y -= _fallAcceleration * Time.deltaTime;
            
            // 最大落下速度を制限
            currentVelocity.y = Mathf.Max(currentVelocity.y, -_maxFallSpeed);
            
            _rigidbody2D.velocity = currentVelocity;
            
            yield return null;
        }
    }

    /// <summary>
    /// 元の位置に戻る
    /// </summary>
    private void ReturnToInitialPosition()
    {
        Vector3 direction = (_initialPosition - transform.position).normalized;
        transform.position += direction * _returnSpeed * Time.deltaTime;
        
        // 元の位置に十分近づいたら待機状態に戻る
        if (Vector3.Distance(transform.position, _initialPosition) < _returnThreshold)
        {
            transform.position = _initialPosition;
            _currentState = MusselState.Idle;
            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.velocity = Vector2.zero;
            
            // 待機状態では完全に固定
            SetRigidbodyConstraints(true);
        }
    }

    /// <summary>
    /// Rigidbody2Dの制約を設定する
    /// </summary>
    /// <param name="freezeAll">全軸を固定するかどうか</param>
    private void SetRigidbodyConstraints(bool freezeAll)
    {
        if (freezeAll)
        {
            // 完全に固定（待機状態や復帰後）
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX | 
                                      RigidbodyConstraints2D.FreezePositionY | 
                                      RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            // X軸と回転のみ固定、Y軸は自由（落下時）
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX | 
                                      RigidbodyConstraints2D.FreezeRotation;
        }
    }

    /// <summary>
    /// 地面や障害物に衝突した時の処理
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        int damage = _attackPower;

        if (_currentState == MusselState.Falling)
        {
            // プレイヤーに当たった場合の処理（落下は継続）
            if (collision.gameObject.CompareTag("Player"))
            {
                // プレイヤーにダメージを与える
                Character playerCharacter = collision.gameObject.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    damage *= _attackMultiplier; // 落下攻撃中は攻撃倍率を適用
                    playerCharacter.HitAttack(damage);
                }

                // プレイヤーとの物理的な衝突を無視して通り抜ける
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);

                // 少し待ってから衝突を再び有効にする（プレイヤーが離れた後）
                StartCoroutine(ReEnablePlayerCollision(collision.collider));

                return; // 地面判定を行わずに処理を終了
            }

            // 地面に当たったら停止（プレイヤー以外の場合のみ）
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.gravityScale = 0f;

                // 地面に着地したら完全に固定
                SetRigidbodyConstraints(true);

                // カメラを振動させる
                FollowCamera2D.ShakeCamera(_shakeIntensity, _shakeDuration);

                StartCoroutine(WaitAndReturn());
            }
        }
        else
        {
            // 落下中でない場合は通常の攻撃処理
            Character playerCharacter = collision.gameObject.GetComponent<Character>();
            if (playerCharacter != null)
            {
                playerCharacter.HitAttack(damage);
            }
        }
    }

    /// <summary>
    /// プレイヤーとの衝突を再び有効にするコルーチン
    /// </summary>
    private IEnumerator ReEnablePlayerCollision(Collider2D playerCollider)
    {
        // 1秒後に衝突を再び有効にする
        yield return new WaitForSeconds(1f);
        
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), playerCollider, false);
        }
    }

    /// <summary>
    /// 待機してから元の位置に戻るコルーチン
    /// </summary>
    private IEnumerator WaitAndReturn()
    {
        _currentState = MusselState.Waiting;
        yield return new WaitForSeconds(_waitTimeAfterFall);
        _currentState = MusselState.Returning;
    }

    /// <summary>
    /// デバッグ用：検知範囲を表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 検知範囲を表示
        Gizmos.color = Color.red;
        Vector3 leftPoint = transform.position + Vector3.left * _detectionWidth;
        Vector3 rightPoint = transform.position + Vector3.right * _detectionWidth;
        Vector3 bottomPoint = transform.position + Vector3.down * _attackRange;
        
        Gizmos.DrawLine(leftPoint, leftPoint + Vector3.down * _attackRange);
        Gizmos.DrawLine(rightPoint, rightPoint + Vector3.down * _attackRange);
        Gizmos.DrawLine(leftPoint + Vector3.down * _attackRange, rightPoint + Vector3.down * _attackRange);
        
        // 初期位置を表示
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_initialPosition, 0.5f);
        }
    }
}
