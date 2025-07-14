using System.Collections;
using UnityEngine;

public class BourBonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _verticalSpeed = 1f;        // 垂直移動速度
    [SerializeField] private float _verticalRange = 2f;        // 上下移動の範囲
    [SerializeField] private float _hoverDuration = 3f;        // Y座標反転までの時間
    [SerializeField] private float _idealDistance = 2f;        // プレイヤーとの理想的な距離
    [SerializeField] private float _minDistance = 1f;          // プレイヤーとの最小距離
    [SerializeField] private float _escapeSpeed = 3f;          // 逃げる時の速度
    
    [Header("Detection Settings")]
    [SerializeField] private float _detectionRange = 8f;       // プレイヤー検知範囲
    [SerializeField] private LayerMask _playerLayer = -1;      // プレイヤーのレイヤー
    [SerializeField] private float _loseTargetTime = 3f;       // 追跡を止めるまでの時間
    [SerializeField] private float _returnSpeed = 1.5f;        // 元の位置に戻る速度
    
    [Header("Attack Settings")]
    [SerializeField] private GameObject _missilePrefab;        // ミサイルプレハブ
    [SerializeField] private float _attackCooldown = 3f;       // 攻撃のクールダウン時間
    [SerializeField] private float _missileSpawnOffset = 1f;   // ミサイル発射位置のオフセット
    [SerializeField] private int _minMissileCount = 3;         // 最小ミサイル数
    [SerializeField] private int _maxMissileCount = 6;         // 最大ミサイル数
    [SerializeField] private float _missileSpawnDelay = 0.2f;  // ミサイル間の発射間隔
    [SerializeField] private float _targetSpread = 2f;         // ターゲット位置のばらつき範囲
    
    private Vector3 _startPosition;
    private bool _movingUp = true;
    private float _hoverTimer = 0f;
    private Transform _player;
    private bool _isCoolingDown = false;
    
    // 追跡・復帰システム用の変数
    private bool _isPlayerDetected = false;      // プレイヤーを検知中かどうか
    private float _loseTargetTimer = 0f;         // 追跡を止めるタイマー
    private bool _isReturningToPosition = false; // 元の位置に戻り中かどうか
    private Vector3 _discoveryPosition;          // 敵がプレイヤーを発見した時の敵自身の位置
    
    // 物理移動用
    private Rigidbody2D _rb;
    
    private void Start()
    {
        _startPosition = transform.position;
        
        // Rigidbody2Dを取得または追加
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Rigidbody2Dの設定
        _rb.gravityScale = 0f; // 重力を無効
        _rb.freezeRotation = true; // 回転を固定
        
        // Collider2Dの確認
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"{gameObject.name}: Collider2Dが見つかりません。壁の衝突判定のためにCollider2Dを追加してください。");
        }
        
        // プレイヤーを検索
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
    }
    
    private void Update()
    {
        UpdatePlayerDetection();
        HandleMovement();
        DetectPlayer();
    }
    
    /// <summary>
    /// プレイヤー検知状態の更新
    /// </summary>
    private void UpdatePlayerDetection()
    {
        if (_player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        bool playerInRange = distanceToPlayer <= _detectionRange;
        
        if (playerInRange)
        {
            // プレイヤーが範囲内にいる場合
            if (!_isPlayerDetected)
            {
                // 新たにプレイヤーを発見
                _isPlayerDetected = true;
                _discoveryPosition = transform.position; // 敵がプレイヤーを発見した時の敵自身の位置を記録
                _isReturningToPosition = false;
            }
            
            _loseTargetTimer = 0f; // タイマーをリセット
        }
        else
        {
            // プレイヤーが範囲外にいる場合
            if (_isPlayerDetected)
            {
                _loseTargetTimer += Time.deltaTime;
                
                if (_loseTargetTimer >= _loseTargetTime)
                {
                    // 追跡を停止
                    _isPlayerDetected = false;
                    _isReturningToPosition = true;
                    _loseTargetTimer = 0f;
                }
            }
        }
    }
    
    /// <summary>
    /// 敵の移動処理
    /// </summary>
    private void HandleMovement()
    {
        // 元の位置に戻る処理
        if (_isReturningToPosition)
        {
            HandleReturnMovement();
            return;
        }
        
        // プレイヤーとの距離を計算
        float distanceToPlayer = _player != null && _isPlayerDetected ? Vector2.Distance(transform.position, _player.position) : float.MaxValue;
        
        // 水平移動の決定
        Vector2 horizontalMovement = Vector2.zero;
        
        if (_player != null && _isPlayerDetected)
        {
            // プレイヤーとの位置関係で移動方向を決定
            float horizontalDirection = _player.position.x - transform.position.x;
            
            if (distanceToPlayer < _minDistance)
            {
                // 近すぎる場合は逃げる
                horizontalMovement = new Vector2(-Mathf.Sign(horizontalDirection) * _escapeSpeed, 0f);
            }
            else if (distanceToPlayer > _idealDistance)
            {
                // 遠すぎる場合は近づく（ただしゆっくり）
                horizontalMovement = new Vector2(Mathf.Sign(horizontalDirection) * _escapeSpeed * 0.5f, 0f);
            }
            else
            {
                // 理想的な距離なので静止
                horizontalMovement = Vector2.zero;
            }
        }
        else
        {
            // プレイヤーがいない場合は移動しない
            horizontalMovement = Vector2.zero;
        }
        
        // Rigidbody2Dを使用した物理移動
        Vector2 totalMovement = horizontalMovement;
        
        // 垂直移動（上下にゆっくり）
        Vector2 verticalMovement = HandleVerticalMovement();
        totalMovement += verticalMovement;
        
        // Rigidbody2Dに速度を設定
        _rb.velocity = totalMovement;
    }
    
    /// <summary>
    /// 元の位置に戻る移動処理
    /// </summary>
    private void HandleReturnMovement()
    {
        float distanceToDiscoveryPosition = Vector2.Distance(transform.position, _discoveryPosition);
        
        if (distanceToDiscoveryPosition < 0.5f)
        {
            // 発見位置に到着
            _isReturningToPosition = false;
            _startPosition = _discoveryPosition; // 発見位置を新しい基準位置に設定
            _rb.velocity = Vector2.zero; // 移動停止
        }
        else
        {
            // 発見位置に向かって移動
            Vector2 directionToDiscovery = (_discoveryPosition - transform.position).normalized;
            Vector2 horizontalMovement = directionToDiscovery * _returnSpeed;
            
            // 垂直移動も継続
            Vector2 verticalMovement = HandleVerticalMovement();
            
            // Rigidbody2Dに速度を設定
            _rb.velocity = horizontalMovement + verticalMovement;
        }
    }
    
    /// <summary>
    /// 垂直移動の処理
    /// </summary>
    private Vector2 HandleVerticalMovement()
    {
        Vector2 verticalMovement = Vector2.zero;
        
        // ホバー時間の管理
        _hoverTimer += Time.deltaTime;
        if (_hoverTimer >= _hoverDuration)
        {
            _movingUp = !_movingUp;
            _hoverTimer = 0f;
        }
        
        // Sine波を使った滑らかな上下移動（イージング）
        float progress = _hoverTimer / _hoverDuration; // 0から1の進行度
        float easedSpeed;
        
        if (_movingUp)
        {
            // 上向き移動：Sine波で滑らかな加減速
            easedSpeed = Mathf.Sin(progress * Mathf.PI) * _verticalSpeed;
            verticalMovement = Vector2.up * easedSpeed;
        }
        else
        {
            // 下向き移動：Sine波で滑らかな加減速
            easedSpeed = Mathf.Sin(progress * Mathf.PI) * _verticalSpeed;
            verticalMovement = Vector2.down * easedSpeed;
        }
        
        // 範囲制限チェック（強制的な方向転換）
        if (transform.position.y > _startPosition.y + _verticalRange)
        {
            _movingUp = false;
            _hoverTimer = 0f; // タイマーリセット
            verticalMovement = Vector2.down * _verticalSpeed;
        }
        else if (transform.position.y < _startPosition.y - _verticalRange)
        {
            _movingUp = true;
            _hoverTimer = 0f; // タイマーリセット
            verticalMovement = Vector2.up * _verticalSpeed;
        }
        
        return verticalMovement;
    }
    
    /// <summary>
    /// プレイヤー検知処理
    /// </summary>
    private void DetectPlayer()
    {
        if (_player == null || _isCoolingDown || !_isPlayerDetected) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        
        if (distanceToPlayer <= _detectionRange)
        {
            StartCoroutine(AttackPlayer());
        }
    }
    
    /// <summary>
    /// プレイヤーに攻撃する
    /// </summary>
    private IEnumerator AttackPlayer()
    {
        _isCoolingDown = true;
        
        // ランダムなミサイル数を決定
        int missileCount = Random.Range(_minMissileCount, _maxMissileCount + 1);
        
        for (int i = 0; i < missileCount; i++)
        {
            // ミサイル発射位置を計算（敵の後ろ側から発射）
            Vector3 spawnPosition = transform.position;
            if (_player.position.x > transform.position.x)
            {
                // プレイヤーが右側にいる場合、左側から発射
                spawnPosition.x = transform.position.x - _missileSpawnOffset;
            }
            else
            {
                // プレイヤーが左側にいる場合、右側から発射
                spawnPosition.x = transform.position.x + _missileSpawnOffset;
            }
            
            // ランダムなY軸オフセットを追加
            spawnPosition.y += Random.Range(-0.5f, 0.5f);
            
            // ミサイルを生成
            if (_missilePrefab != null)
            {
                GameObject missile = Instantiate(_missilePrefab, spawnPosition, Quaternion.identity);
                BourBonMissile missileScript = missile.GetComponent<BourBonMissile>();
                
                if (missileScript != null && _player != null)
                {
                    // ターゲット位置にランダムなズレを追加
                    Vector3 targetPosition = _player.position;
                    targetPosition.x += Random.Range(-_targetSpread, _targetSpread);
                    targetPosition.y += Random.Range(-_targetSpread, _targetSpread);
                    
                    missileScript.Initialize(targetPosition);
                }
            }
            
            // 次のミサイルまで少し待機
            if (i < missileCount - 1)
            {
                yield return new WaitForSeconds(_missileSpawnDelay);
            }
        }
        
        yield return new WaitForSeconds(_attackCooldown);
        _isCoolingDown = false;
    }
    
    /// <summary>
    /// デバッグ用：検知範囲を表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        
        // 理想的な距離を表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _idealDistance);
        
        // 最小距離を表示
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // オレンジ色
        Gizmos.DrawWireSphere(transform.position, _minDistance);
        
        // 発見位置を表示（復帰中の場合）
        if (_isReturningToPosition)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_discoveryPosition, Vector3.one * 0.7f);
        }
        
        // 巡回範囲を表示（垂直方向のみ）
        Gizmos.color = Color.yellow;
        Vector3 topPosition = new Vector3(transform.position.x, _startPosition.y + _verticalRange, transform.position.z);
        Vector3 bottomPosition = new Vector3(transform.position.x, _startPosition.y - _verticalRange, transform.position.z);
        Gizmos.DrawLine(topPosition, bottomPosition);
        
        // 状態表示
        if (_isReturningToPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _discoveryPosition);
        }
        
        if (_isPlayerDetected && _player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _player.position);
        }
    }
}
