using System.Collections;
using UnityEngine;

public class BourBonMissile : MonoBehaviour
{
    [Header("発射設定")]
    [SerializeField] private float _launchSpeed = 12f;            // 発射時の初速
    [SerializeField] private float _launchDuration = 0.5f;        // 発射フェーズの時間
    [SerializeField] private float _decelerationRate = 0.8f;      // 減速率
    [SerializeField] private float _rotationSpeed = 120f;         // 旋回速度（度/秒）
    [SerializeField] private float _minSpeed = 1f;                // 最低速度
    
    [Header("突撃設定")]
    [SerializeField] private float _chargeSpeed = 15f;            // 突撃時の最大速度
    [SerializeField] private float _aimThreshold = 30f;           // プレイヤーに向く角度の閾値（度）
    [SerializeField] private float _accelerationRate = 1.05f;     // 加速率（毎フレーム）
    
    [Header("爆発設定")]
    [SerializeField] private float _lifeTime = 10f;               // ミサイルの生存時間
    [SerializeField] private GameObject _explosionPrefab;         // 爆発エフェクトプレハブ
    [SerializeField] private float _explosionForce = 500f;        // 爆発の力
    [SerializeField] private float _explosionRadius = 2f;         // 爆発の範囲

    [Header("攻撃倍率")]
    [SerializeField] private float _attackMultiplier = 1.4f;      // 攻撃倍率

    private float _attackPower; // 攻撃力

    public BourBonMissile Instance { get; private set; }          // シングルトンインスタンス
    
    private enum MissileState
    {
        Launch,         // 発射フェーズ
        Deceleration,   // 減速・旋回フェーズ
        Charge          // 突撃フェーズ
    }
    
    private Vector3 _targetPosition;     // ターゲット位置（固定）
    private Vector3 _launchDirection;    // 発射方向
    private Vector3 _chargeDirection;    // 突撃方向（固定）
    private MissileState _currentState;  // 現在の状態
    private float _currentSpeed;         // 現在の速度
    private Rigidbody2D _rb;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this; // シングルトンインスタンスの設定
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 既存のインスタンスがある場合は削除
            return;
        }

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 重力を無効にする
        _rb.gravityScale = 0f;
        
        // 一定時間後に自動削除
        Destroy(gameObject, _lifeTime);
    }
    
    private void Start()
    {
        // 発射フェーズから開始
        _currentState = MissileState.Launch;
        _currentSpeed = _launchSpeed;
        
        StartCoroutine(MissileSequence());
    }

    public void SetAttackPower(float power)
    {
        _attackPower = power * _attackMultiplier; // 攻撃力を設定（倍率を適用）
    }
    
    /// <summary>
    /// ミサイルの初期化
    /// </summary>
    /// <param name="targetPos">ターゲット位置</param>
    public void Initialize(Vector3 targetPos)
    {
        _targetPosition = targetPos;

        // 発射方向を決定（敵の向きとは逆方向に発射）
        BourBonMovement enemy = FindObjectOfType<BourBonMovement>();
        if (enemy != null)
        {
            // 敵の向きとは逆方向に発射
            bool enemyFacingRight = enemy.transform.localScale.x > 0;
            if (enemyFacingRight)
            {
                _launchDirection = Vector3.left; // 敵が右向きなら左方向に発射
            }
            else
            {
                _launchDirection = Vector3.right; // 敵が左向きなら右方向に発射
            }
        }
        else
        {
            // 敵が見つからない場合はターゲット方向
            _launchDirection = (_targetPosition - transform.position).normalized;
        }

        // 少しランダム性を追加（上下に±10度）
        float randomAngle = Random.Range(-10f, 10f);
        _launchDirection = Quaternion.Euler(0, 0, randomAngle) * _launchDirection;
    }
    
    /// <summary>
    /// ミサイルの動作シーケンス
    /// </summary>
    private IEnumerator MissileSequence()
    {
        float timer = 0f;
        
        // 1. 発射フェーズ
        while (_currentState == MissileState.Launch && timer < _launchDuration)
        {
            LaunchPhase();
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 2. 減速・旋回フェーズに移行
        _currentState = MissileState.Deceleration;
        
        while (_currentState == MissileState.Deceleration)
        {
            DecelerationPhase();
            
            // 目標方向に向いているかチェック
            if (IsAimingAtPlayer())
            {
                // 突撃方向を固定（この時点でのプレイヤー方向）
                _chargeDirection = (_targetPosition - transform.position).normalized;
                _currentState = MissileState.Charge;
            }
            
            yield return null;
        }
        
        // 3. 突撃フェーズ
        while (_currentState == MissileState.Charge)
        {
            // 突撃フェーズでは目標位置を更新しない（発射時の位置に向かって直進）
            ChargePhase();
            yield return null;
        }
    }
    
    /// <summary>
    /// 発射フェーズの処理
    /// </summary>
    private void LaunchPhase()
    {
        // 発射方向に勢いよく移動
        _rb.velocity = _launchDirection * _currentSpeed;
        
        // 移動方向に向きを合わせる（スプライトが右向きの場合）
        float angle = Mathf.Atan2(_launchDirection.y, _launchDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    /// <summary>
    /// 減速・旋回フェーズの処理
    /// </summary>
    private void DecelerationPhase()
    {
        // 減速
        _currentSpeed = Mathf.Max(_currentSpeed * _decelerationRate, _minSpeed);
        
        // プレイヤー方向への旋回（通過点として扱う）
        Vector3 directionToTarget = (_targetPosition - transform.position).normalized;
        
        // 目標角度を計算
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        
        // 角度差を計算（-180～180度の範囲に正規化）
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        // 旋回速度を制限
        float rotationStep = _rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(angleDifference) < rotationStep)
        {
            transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        }
        else
        {
            float rotationDirection = Mathf.Sign(angleDifference);
            transform.Rotate(0, 0, rotationDirection * rotationStep);
        }
        
        // 現在の向きに向かって移動（transform.rightを使用）
        Vector3 currentDirection = new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), 
                                             Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0);
        _rb.velocity = currentDirection * _currentSpeed;
    }
    
    /// <summary>
    /// 突撃フェーズの処理
    /// </summary>
    private void ChargePhase()
    {
        // 徐々に加速
        _currentSpeed = Mathf.Min(_currentSpeed * _accelerationRate, _chargeSpeed);
        
        // 固定された方向に直進（ホーミングしない）
        _rb.velocity = _chargeDirection * _currentSpeed;
        
        // 突撃方向に向きを固定
        float angle = Mathf.Atan2(_chargeDirection.y, _chargeDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    /// <summary>
    /// 目標方向に向いているかチェック
    /// </summary>
    private bool IsAimingAtPlayer()
    {
        Vector3 directionToTarget = (_targetPosition - transform.position).normalized;
        // 現在のミサイルの向きを正確に取得
        Vector3 missileDirection = new Vector3(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), 
                                             Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), 0);
        
        float angle = Vector3.Angle(missileDirection, directionToTarget);
        return angle <= _aimThreshold;
    }
    
    /// <summary>
    /// 衝突処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 自分自身以外のすべてのオブジェクトとの衝突で爆発
        if (other.gameObject != gameObject && other.GetComponent<BourBonMissile>() == null)
        {
            Explode();
        }
    }
    
    /// <summary>
    /// 物理衝突処理
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 自分自身以外のすべてのオブジェクトとの衝突で爆発
        if (collision.gameObject != gameObject && collision.gameObject.GetComponent<BourBonMissile>() == null)
        {
            Explode();
        }
    }
    
    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode()
    {
        // 爆発エフェクト生成
        if (_explosionPrefab != null)
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // 周囲のオブジェクトに爆発の影響を与える
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            // プレイヤーにダメージを与える処理
            if (hitCollider.CompareTag("Player"))
            {
                // プレイヤーのダメージ処理
                Character playerCharacter = hitCollider.gameObject.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    // 攻撃力に倍率を適用してダメージを計算
                    int damage = Mathf.RoundToInt(_attackPower * _attackMultiplier);
                    playerCharacter.HitAttack(damage);
                }
                
                // 物理的な力を加える
                Rigidbody2D playerRb = hitCollider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 explosionDirection = (hitCollider.transform.position - transform.position).normalized;
                    playerRb.AddForce(explosionDirection * _explosionForce);
                }
            }
            
            // その他のオブジェクトへの影響
            Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();
            if (rb != null && rb != _rb)
            {
                Vector2 explosionDirection = (hitCollider.transform.position - transform.position).normalized;
                rb.AddForce(explosionDirection * _explosionForce * 0.5f);
            }
        }
        
        // ミサイル自体を削除
        Destroy(gameObject);
    }
    
    /// <summary>
    /// デバッグ用：爆発範囲を表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        
        if (_targetPosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetPosition);
            
            // 現在の状態を表示
            Gizmos.color = Color.blue;
            Vector3 labelPos = transform.position + Vector3.up * 1f;
            
            switch (_currentState)
            {
                case MissileState.Launch:
                    Gizmos.DrawWireCube(labelPos, Vector3.one * 0.2f);
                    break;
                case MissileState.Deceleration:
                    Gizmos.DrawWireSphere(labelPos, 0.1f);
                    break;
                case MissileState.Charge:
                    // 突撃方向を表示
                    if (_chargeDirection != Vector3.zero)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawRay(transform.position, _chargeDirection * 3f);
                    }
                    break;
            }
        }
    }
}
