using System.Collections;
using UnityEngine;

public class BourBonMissile : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float _launchSpeed = 12f;            // 発射時の初速
    [SerializeField] private float _launchDuration = 0.5f;        // 発射フェーズの時間
    
    [Header("Deceleration Settings")]
    [SerializeField] private float _decelerationRate = 0.8f;      // 減速率
    [SerializeField] private float _rotationSpeed = 120f;         // 旋回速度（度/秒）
    [SerializeField] private float _minSpeed = 1f;                // 最低速度
    
    [Header("Charge Settings")]
    [SerializeField] private float _chargeSpeed = 15f;            // 突撃時の最大速度
    [SerializeField] private float _aimThreshold = 30f;           // プレイヤーに向く角度の閾値（度）
    [SerializeField] private float _accelerationRate = 1.05f;     // 加速率（毎フレーム）
    
    [Header("General Settings")]
    [SerializeField] private float _lifeTime = 10f;               // ミサイルの生存時間
    [SerializeField] private GameObject _explosionPrefab;         // 爆発エフェクトプレハブ
    [SerializeField] private float _explosionForce = 500f;        // 爆発の力
    [SerializeField] private float _explosionRadius = 2f;         // 爆発の範囲
    
    private enum MissileState
    {
        Launch,         // 発射フェーズ
        Deceleration,   // 減速・旋回フェーズ
        Charge          // 突撃フェーズ
    }
    
    private Vector3 _targetPosition;     // ターゲット位置
    private Vector3 _launchDirection;    // 発射方向
    private MissileState _currentState;  // 現在の状態
    private float _currentSpeed;         // 現在の速度
    private Rigidbody2D _rb;
    
    private void Awake()
    {
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
    
    /// <summary>
    /// ミサイルの初期化
    /// </summary>
    /// <param name="targetPos">ターゲット位置</param>
    public void Initialize(Vector3 targetPos)
    {
        _targetPosition = targetPos;
        
        // 発射方向を決定（敵の向いている方向に基づく）
        // プレイヤー方向ではなく、発射時の向きを基準にする
        BourBonMovement enemy = FindObjectOfType<BourBonMovement>();
        if (enemy != null)
        {
            // 敵の向きに基づいて発射方向を決定
            bool enemyFacingRight = enemy.transform.localScale.x > 0;
            if (enemyFacingRight)
            {
                _launchDirection = Vector3.right; // 右方向に発射
            }
            else
            {
                _launchDirection = Vector3.left; // 左方向に発射
            }
        }
        else
        {
            // 敵が見つからない場合はターゲット方向
            _launchDirection = (_targetPosition - transform.position).normalized;
        }
        
        // 少しランダム性を追加（上下に±15度）
        float randomAngle = Random.Range(-15f, 15f);
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
            
            // プレイヤーに向いているかチェック
            if (IsAimingAtPlayer())
            {
                _currentState = MissileState.Charge;
            }
            
            yield return null;
        }
        
        // 3. 突撃フェーズ
        while (_currentState == MissileState.Charge)
        {
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
        
        // 移動方向に向きを合わせる
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
        
        // プレイヤー方向への旋回
        Vector3 directionToPlayer = (_targetPosition - transform.position).normalized;
        Vector3 currentDirection = transform.right; // ミサイルの向いている方向
        
        // 目標角度を計算
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
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
        
        // 現在の向きに向かって移動
        _rb.velocity = transform.right * _currentSpeed;
    }
    
    /// <summary>
    /// 突撃フェーズの処理
    /// </summary>
    private void ChargePhase()
    {
        // 徐々に加速
        _currentSpeed = Mathf.Min(_currentSpeed * _accelerationRate, _chargeSpeed);
        
        // プレイヤー方向に移動
        Vector3 directionToPlayer = (_targetPosition - transform.position).normalized;
        _rb.velocity = directionToPlayer * _currentSpeed;
        
        // 向きを更新
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    /// <summary>
    /// プレイヤーに向いているかチェック
    /// </summary>
    private bool IsAimingAtPlayer()
    {
        Vector3 directionToPlayer = (_targetPosition - transform.position).normalized;
        Vector3 missileDirection = transform.right;
        
        float angle = Vector3.Angle(missileDirection, directionToPlayer);
        return angle <= _aimThreshold;
    }
    
    /// <summary>
    /// 衝突処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーまたは地形との衝突で爆発
        if (other.CompareTag("Player") || other.CompareTag("Ground") || other.CompareTag("Wall"))
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
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // 周囲のオブジェクトに爆発の影響を与える
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            // プレイヤーにダメージを与える処理
            if (hitCollider.CompareTag("Player"))
            {
                // プレイヤーのダメージ処理をここに追加
                Debug.Log("Player hit by missile explosion!");
                
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
                    Gizmos.DrawRay(transform.position, transform.right * 2f);
                    break;
            }
        }
    }
}
