using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 主製作者：村田智哉

public class SeaDemon : MonoBehaviour
{
    // ボスの状態
    private enum State
    {
        Hovering,
        Attacking
    }

    [Header("ホバリング移動設定")]
    public float hoverHeight = 3.0f;
    public float hoverSpeed = 2.0f;
    public float hoverAmplitude = 0.5f;

    [Header("移動設定")]
    public float swimSpeed = 2.0f;
    public float directionChangeInterval = 3.0f;

    [Header("攻撃タイミング")]
    public float timeBetweenAttacks = 2.0f;

    [Header("波攻撃設定")]
    [SerializeField] private GameObject wavePrefab; // WaveObjectのプレハブ
    [SerializeField] private float waveInitialSpeed = 2f;
    [SerializeField] private float waveAcceleration = 3f;

    [Header("共通キャラクターオブジェクト")]
    public Character _sharedCharacter;

    private State currentState = State.Hovering;
    private float hoverBaseY;
    private float attackTimer;

    [Header("攻撃倍率")]
    [SerializeField] private float _rushMagnification = 1.5f; // 突進攻撃の倍率
    [SerializeField] private float _waveMagnification = 1.1f; // 波攻撃倍率

    [Header("突進攻撃設定")]
    [SerializeField] private float _rushSpeed = 5f;     // 突進速度
    [SerializeField] private float _rushDuration = 1f;  // 突進持続時間
    [SerializeField] private float _rushStop = 2f;      // 突進後の停止時間

    // 攻撃パターンCで使う侵入方向の設定
    public enum EntrySide { Left, Right }

    [Header("攻撃パターンC設定")]
    [SerializeField] private EntrySide attackCEntrySide = EntrySide.Left; // 画面外退避方向
    [SerializeField] private float attackCOutDistance = 10f;              // 画面外へ退避する距離
    [SerializeField] private float attackCOutSpeed = 5f;                  // 画面外へ出るスピード
    [SerializeField] private float attackCYAlignSpeed = 5f;               // Y合わせのスピード
    [SerializeField] private float attackCPauseDuration = 1f;             // Y合わせ後の停止時間
    [SerializeField] private float attackCRushDistance = 8f;              // 突進する距離
    [SerializeField] private float attackCRushSpeed = 10f;                // 突進スピード
    [SerializeField] private float attackCReturnSpeed = 5f;               // 戻りスピード
    [SerializeField] private string attackCLayerDuringRush = "EnemyRush"; // 突進中のレイヤー
    [SerializeField] private string defaultLayer = "Enemy";               // 元のレイヤー

    [Header("攻撃パターン確率 (合計100にしてください)")]
    [Range(0, 100)]
    [SerializeField] private int attackPatternAProbability = 33;
    [Range(0, 100)]
    [SerializeField] private int attackPatternBProbability = 33;
    [Range(0, 100)]
    [SerializeField] private int attackPatternCProbability = 34;

    [Header("音声設定")]
    [SerializeField] private float _voiceMinInterval = 3f;  // 音声再生の最小間隔
    [SerializeField] private float _voiceMaxInterval = 8f;  // 音声再生の最大間隔

    [Header("突進攻撃設定")]
    private int     _moveDirection = 1;         // 1: 右, -1: 左
    private int     _attackPower;               // 攻撃倍率
    private float   _directionChangeTimer;      // 方向転換のタイマー
    private float   _nextVoiceTime;             // 次の音声再生時間

    private bool _isPerformingAttack = false;   // 攻撃中フラグ
    private bool _isDashing = false;            // 突進中フラグ
    private bool _isDashInterrupted = false;    // 突進中に衝突したか
    private bool _isPatternCActive = false;     // 攻撃パターンCがアクティブかどうか
    private bool _isPlayingVoice = false;       // 音声再生中フラグ
    private Vector3 _originalPosition;          // 突進前の位置
    private Vector3 _dashInterruptPosition;     // 衝突時の位置

    private Rigidbody2D _rb2d;                  // Rigidbody2Dコンポーネント
    private Collider2D  _collider2D;            // コライダー2D
    private Transform playerTransform;          // プレイヤーのTransform
    private LineRenderer _dashLineRenderer;     // 突進軌道を描画するためのLineRenderer
    private System.Random random = new System.Random(); // ランダム生成用

    void Start()
    {
        hoverBaseY = transform.position.y;
        attackTimer = timeBetweenAttacks;
        _directionChangeTimer = directionChangeInterval;
        _attackPower = _sharedCharacter.AttackPower;

        // Rigidbody2DのfreezeRotationを有効化
        _rb2d = GetComponent<Rigidbody2D>();
        if (_rb2d != null)
        {
            _rb2d.freezeRotation = true;
        }

        _collider2D = GetComponent<Collider2D>();
        if (_collider2D == null)
        {
            Debug.LogWarning("SeaDemon: Collider2D not found!");
        }

        // プレイヤーのTransformを取得（タグ"Player"を検索）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // 最初の音声再生タイミングを設定
        SetNextVoiceTime();

        // LineRendererの初期化
        _dashLineRenderer = gameObject.AddComponent<LineRenderer>();
        _dashLineRenderer.positionCount = 2;
        _dashLineRenderer.enabled = false;
        _dashLineRenderer.widthMultiplier = 0.12f;
        _dashLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _dashLineRenderer.sortingOrder = 100; // 前面に表示したい場合
        _dashLineRenderer.numCapVertices = 4;
    }

    /// <summary>
    /// 音声再生のタイミングを設定する。
    /// </summary>
    void Update()
    {
        if (_isPerformingAttack)
            return;

        // 音声再生チェック（攻撃中でない場合のみ）
        CheckAndPlayVoice();

        switch (currentState)
        {
            case State.Hovering:
                // 突進中でなければHover
                if (!_isDashing)
                    Hover();
                Move();
                attackTimer -= Time.deltaTime;
                _directionChangeTimer -= Time.deltaTime;
                if (_directionChangeTimer <= 0f)
                {
                    ReverseDirection();
                    _directionChangeTimer = directionChangeInterval;
                }
                if (attackTimer <= 0f)
                {
                    currentState = State.Attacking;
                }
                break;
            case State.Attacking:
                PerformRandomAttack();
                attackTimer = timeBetweenAttacks;
                currentState = State.Hovering;
                break;
        }
    }

    /// <summary>
    /// ホバリング移動
    /// </summary>
    private void Hover()
    {
        Vector3 pos = transform.position;
        pos.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = pos;
    }

    /// <summary>
    /// 横移動
    /// </summary>
    private void Move()
    {
        Vector3 pos = transform.position;
        pos.x += _moveDirection * swimSpeed * Time.deltaTime;
        transform.position = pos;
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    private void ReverseDirection()
    {
        _moveDirection *= -1;
        // Optional: 方向転換時にスプライトも反転
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _moveDirection;
        transform.localScale = scale;
    }

    /// <summary>
    /// 衝突時の処理
    /// 攻撃パターンC中のプレイヤー衝突時はコライダーを無効化
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // --- PatternC中にプレイヤーと衝突したらコライダーを無効化 ---
        if (_isPatternCActive && collision.gameObject.CompareTag("Player") && _collider2D != null)
        {
            _collider2D.enabled = false;
        }

        if (_isDashing)
        {
            // ダッシュ中なら衝突フラグを立てて位置を記録
            _isDashInterrupted = true;
            _dashInterruptPosition = transform.position;
        }
        else
        {
            ReverseDirection();
            _directionChangeTimer = directionChangeInterval;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーのCharacterコンポーネントを取得
            Character playerCharacter = collision.gameObject.GetComponent<Character>();
            if (playerCharacter != null)
            {
                int damage = _attackPower;
                // ダッシュ中なら倍率をかける
                if (_isDashing)
                {
                    damage = Mathf.RoundToInt(damage * _rushMagnification);
                }
                playerCharacter.HitAttack(damage);
            }
            return;
        }
    }

    /// <summary>
    /// 攻撃パターンを確率で選択し実行
    /// </summary>
    private void PerformRandomAttack()
    {
        // 確率の合計
        int total = attackPatternAProbability + attackPatternBProbability + attackPatternCProbability;
        if (total <= 0)
        {
            // どれも0ならA
            AttackPatternA();
            return;
        }
        int rand = UnityEngine.Random.Range(0, total);
        if (rand < attackPatternAProbability)
        {
            AttackPatternA();
        }
        else if (rand < attackPatternAProbability + attackPatternBProbability)
        {
            AttackPatternB();
        }
        else
        {
            AttackPatternC();
        }
    }

    /// <summary>
    /// 攻撃パターンA: プレイヤーに向かって突進
    /// </summary>
    private void AttackPatternA()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("SeaDemon: Player Transform not set for AttackPatternA");
            return;
        }
        StartCoroutine(DashAttackCoroutine());
    }

    /// <summary>
    /// 攻撃パターンAの突進処理
    /// </summary>
    private IEnumerator DashAttackCoroutine()
    {
        _isPerformingAttack = true;
        _isDashing = false;
        _isDashInterrupted = false;
        
        // 1. その場で停止（位置固定）
        Vector3 startPosition = transform.position;
        _originalPosition = startPosition; // 戻る位置を記録
        
        // 空中でその場に完全停止（重力の影響を一時的に無効化）
        float originalGravityScale = 0f;
        if (_rb2d != null)
        {
            originalGravityScale = _rb2d.gravityScale;
            _rb2d.gravityScale = 0f; // 重力を無効化
            _rb2d.velocity = Vector2.zero;
        }
        
        // 2. プレイヤーの方を向く（2秒停止）
        Vector3 playerDirection = (playerTransform.position - transform.position).normalized;

        // --- Dash trajectory line: 突進前に線を描画 ---
        float waitDuration = 2f;
        float waitTimer = 0f;
        while (waitTimer < waitDuration)
        {
            // 位置を固定して落下を防ぐ
            transform.position = _originalPosition;

            // 線の色を黄色→赤に補間
            float t = waitTimer / waitDuration;
            Color color = Color.Lerp(Color.yellow, Color.red, t);

            // 線の始点・終点を更新
            if (_dashLineRenderer != null)
            {
                _dashLineRenderer.enabled = true;
                _dashLineRenderer.SetPosition(0, _originalPosition);
                _dashLineRenderer.SetPosition(1, playerTransform.position);
                _dashLineRenderer.startColor = color;
                _dashLineRenderer.endColor = color;
            }

            waitTimer += Time.deltaTime;
            yield return null;
        }

        // スプライトをプレイヤーの方向に向ける
        if (playerDirection.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // 突進開始時に線を非表示
        if (_dashLineRenderer != null)
            _dashLineRenderer.enabled = false;
        // --- End dash trajectory line ---

        // 3. プレイヤーに向け直線移動で突進
        _isDashing = true;
        Vector3 dashDirection = (playerTransform.position - transform.position).normalized;
        float dashTimer = 0f;
        
        while (dashTimer < _rushDuration && !_isDashInterrupted)
        {
            if (_rb2d != null)
            {
                _rb2d.velocity = dashDirection * _rushSpeed;
            }
            else
            {
                transform.Translate(dashDirection * _rushSpeed * Time.deltaTime, Space.World);
            }
            dashTimer += Time.deltaTime;
            yield return null;
        }
        
        // 突進後に速度を0にして停止
        if (_rb2d != null)
        {
            _rb2d.velocity = Vector2.zero;
        }
        
        // 4. 突進中に何かに衝突した場合はその場で数秒停止
        if (_isDashInterrupted)
        {
            // 衝突した位置で停止
            yield return new WaitForSeconds(_rushStop);
        }
        
        _isDashing = false;
        
        // 5. 突進する前の位置に戻る
        float returnSpeed = _rushSpeed * 0.7f; // 戻る速度は少し遅めに
        while (Vector3.Distance(transform.position, _originalPosition) > 0.1f)
        {
            Vector3 returnDirection = (_originalPosition - transform.position).normalized;
            
            if (_rb2d != null)
            {
                // Rigidbody2Dを使って戻る
                _rb2d.velocity = returnDirection * returnSpeed;
            }
            else
            {
                // Transform.Translateを使って戻る
                transform.Translate(returnDirection * returnSpeed * Time.deltaTime, Space.World);
            }
            yield return null;
        }
        
        // 正確に元の位置に戻して速度を0に
        if (_rb2d != null)
        {
            _rb2d.velocity = Vector2.zero;
            // 重力スケールを元に戻す
            _rb2d.gravityScale = originalGravityScale;
        }
        transform.position = _originalPosition;
        
        _isPerformingAttack = false;
        _isDashInterrupted = false;
    }

    /// <summary>
    /// 攻撃パターンB: 波動をプレイヤーに向け飛ばす
    /// </summary>
    private void AttackPatternB()
    {
        // プレイヤーまたはwavePrefabが未設定の場合は何もしない
        if (playerTransform == null || wavePrefab == null)
        {
            Debug.LogWarning("SeaDemon: Player or wavePrefab not set for AttackPatternB");
            return;
        }

        // 波オブジェクトを生成
        GameObject waveObj = Instantiate(wavePrefab, transform.position, Quaternion.identity);

        // WaveAttackスクリプトを取得し、ターゲット・攻撃力・速度パラメータを設定
        var waveAttack = waveObj.GetComponent<WaveAttack>();
        if (waveAttack != null)
        {
            waveAttack.SetTarget(playerTransform);
            waveAttack.SetAttackPower(Mathf.RoundToInt(_attackPower * _waveMagnification));
            waveAttack.SetSpeedAndAcceleration(waveInitialSpeed, waveAcceleration);
            // 共通キャラクターを渡す
            waveAttack.SetSharedBossCharacter(_sharedCharacter);
        }
    }

    /// <summary>
    /// 攻撃パターンC: 画面外退避・Y追従・突進・復帰
    /// </summary>
    private void AttackPatternC()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("SeaDemon: Player Transform not set for AttackPatternC");
            return;
        }
        StartCoroutine(AttackPatternCCoroutine());
    }

    /// <summary>
    /// 攻撃パターンCの段階的な動きを制御する
    /// </summary>
    private IEnumerator AttackPatternCCoroutine()
    {
        _isPerformingAttack = true;
        _isPatternCActive = true; // PatternC開始
        _rb2d.velocity = Vector2.zero;

        float originalGravity = _rb2d.gravityScale;
        _rb2d.gravityScale = 0f;

        Vector3 startPosition = transform.position;
        _originalPosition = startPosition;

        gameObject.layer = LayerMask.NameToLayer(attackCLayerDuringRush);

        // 1. 画面外へ移動前に向きをセット
        float outDirection = (attackCEntrySide == EntrySide.Left) ? -1f : 1f;
        SetFacingByDirection(outDirection);
        Vector3 outTarget = new Vector3(startPosition.x + outDirection * attackCOutDistance, startPosition.y, startPosition.z);
        yield return MoveToPosition(outTarget, attackCOutSpeed);

        // 2. プレイヤーのY座標を追従しながら待機＋線を描画
        float timer = 0f;
        while (timer < attackCPauseDuration)
        {
            // XはoutTarget.x、Yはプレイヤーの現在のY
            Vector3 followTarget = new Vector3(outTarget.x, playerTransform.position.y, outTarget.z);
            Vector3 dir = (followTarget - transform.position);
            float distance = dir.magnitude;
            if (distance > 0.05f)
            {
                dir.Normalize();
                _rb2d.MovePosition(transform.position + dir * attackCYAlignSpeed * Time.fixedDeltaTime);
            }

            // --- Dash trajectory line: プレイヤー追従中も線を描画 ---
            float rushDirection = (playerTransform.position.x > outTarget.x) ? 1f : -1f;
            float fixedRushY = transform.position.y;
            Vector3 dashLineStart = transform.position;
            Vector3 dashLineEnd = dashLineStart + new Vector3(rushDirection * attackCRushDistance, 0, 0);

            if (_dashLineRenderer != null)
            {
                _dashLineRenderer.enabled = true;
                _dashLineRenderer.SetPosition(0, dashLineStart);
                _dashLineRenderer.SetPosition(1, dashLineEnd);
                // 色は黄色で固定（追従中）
                _dashLineRenderer.startColor = Color.yellow;
                _dashLineRenderer.endColor = Color.yellow;
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 3. 突進前に向きをセット
        float rushDirectionFinal = (playerTransform.position.x > outTarget.x) ? 1f : -1f;
        SetFacingByDirection(rushDirectionFinal);

        // --- Dash trajectory line: 突進前に線を描画（突進距離に合わせて調整、Y固定） ---
        float waitDuration = 2f;
        float waitTimer = 0f;
        float fixedRushYFinal = transform.position.y; // 突進開始時のY座標を固定
        while (waitTimer < waitDuration)
        {
            // 線の色を黄色→赤に補間
            float t = waitTimer / waitDuration;
            Color color = Color.Lerp(Color.yellow, Color.red, t);

            Vector3 dashLineStart = transform.position;
            // Y座標を固定して直線突進
            Vector3 dashLineEnd = dashLineStart + new Vector3(rushDirectionFinal * attackCRushDistance, 0, 0);

            if (_dashLineRenderer != null)
            {
                _dashLineRenderer.enabled = true;
                _dashLineRenderer.SetPosition(0, dashLineStart);
                _dashLineRenderer.SetPosition(1, dashLineEnd);
                _dashLineRenderer.startColor = color;
                _dashLineRenderer.endColor = color;
            }

            waitTimer += Time.deltaTime;
            yield return null;
        }
        if (_dashLineRenderer != null)
            _dashLineRenderer.enabled = false;

        // 突進ターゲット座標を再計算（Y座標は固定）
        float rushTargetX = transform.position.x + rushDirectionFinal * attackCRushDistance;
        Vector3 rushTarget = new Vector3(rushTargetX, fixedRushYFinal, transform.position.z);

        yield return MoveToPosition(rushTarget, attackCRushSpeed);

        _rb2d.velocity = Vector2.zero;

        // 4. 戻り前に向きをセット
        SetFacingByDirection((_originalPosition.x > transform.position.x) ? 1f : -1f);
        yield return MoveToPosition(_originalPosition, attackCReturnSpeed);

        // 5. レイヤーと重力を元に戻す
        gameObject.layer = LayerMask.NameToLayer(defaultLayer);
        _rb2d.gravityScale = originalGravity;

        // --- PatternC終了時にコライダーを有効化 ---
        if (_collider2D != null)
            _collider2D.enabled = true;
        _isPatternCActive = false;

        _isPerformingAttack = false;
    }

    /// <summary>
    /// 汎用の直線移動処理（目標位置まで一定速度で移動）
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 dir = (target - transform.position).normalized;
            _rb2d.MovePosition(transform.position + dir * speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        _rb2d.velocity = Vector2.zero;
    }

    /// <summary>
    /// 指定方向にスプライトを向ける
    /// </summary>
    private void SetFacingByDirection(float dirX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dirX >= 0 ? 1 : -1);
        transform.localScale = scale;
    }

    /// <summary>
    /// 音声再生のタイミングをチェックし、必要なら再生
    /// </summary>
    private void CheckAndPlayVoice()
    {
        // 攻撃中や音声再生中は何もしない
        if (_isPerformingAttack || _isPlayingVoice)
            return;

        // 次の音声再生時間に達したかチェック
        if (Time.time >= _nextVoiceTime)
        {
            PlayVoice();
        }
    }

    /// <summary>
    /// ボイスを再生
    /// </summary>
    private void PlayVoice()
    {
        _isPlayingVoice = true;
        
        // AudioManagerで音声を再生
        AudioManager.Instance.PlaySE("Boss", "SeaDemonVoice1", "N1");
        
        // 音声終了後のコルーチンを開始
        StartCoroutine(VoiceEndCoroutine());
    }

    /// <summary>
    /// ボイス再生終了後の処理
    /// </summary>
    private IEnumerator VoiceEndCoroutine()
    {
        // 音声の長さを仮定（実際の音声ファイルの長さに合わせて調整）
        float voiceDuration = 2f; // 2秒と仮定
        yield return new WaitForSeconds(voiceDuration);
        
        _isPlayingVoice = false;
        SetNextVoiceTime();
    }

    /// <summary>
    /// 次の音声再生時間をランダムに設定
    /// </summary>
    private void SetNextVoiceTime()
    {
        // ランダムな間隔で次の音声再生時間を設定
        float interval = UnityEngine.Random.Range(_voiceMinInterval, _voiceMaxInterval);
        _nextVoiceTime = Time.time + interval;
    }
}
