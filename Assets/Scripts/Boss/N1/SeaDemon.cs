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

    [Header("上下移動設定")]
    public float hoverHeight = 3.0f;
    public float hoverSpeed = 2.0f;
    public float hoverAmplitude = 0.5f;

    [Header("移動設定")]
    public float swimSpeed = 2.0f;
    public float directionChangeInterval = 3.0f;

    [Header("攻撃タイミング")]
    public float timeBetweenAttacks = 2.0f;

    [Header("衝撃波設定")]
    [SerializeField] private GameObject wavePrefab; // WaveObjectのプレファブ
    [SerializeField] private float waveInitialSpeed = 2f;
    [SerializeField] private float waveAcceleration = 3f;

    [Header("共通体力オブジェクト")]
    public Character _sharedCharacter;

    private State currentState = State.Hovering;
    private float hoverBaseY;
    private float attackTimer;

    [Header("攻撃力倍率")]
    [SerializeField] private float _rushMagnification = 1.5f; // 突進攻撃の倍率
    [SerializeField] private float _waveMagnification = 1.1f; // 波攻撃倍率

    [Header("突進攻撃設定")]
    [SerializeField] private float _rushSpeed = 5f;     // 突進速度
    [SerializeField] private float _rushDuration = 1f;  // 突進持続時間
    [SerializeField] private float _rushStop = 2f;      // 突進後の停止時間

    // --- Swimming movement variables ---
    private int _moveDirection = 1; // 1: 右, -1: 左
    private float _directionChangeTimer;
    private int _attackPower;

    private System.Random random = new System.Random();

    private Rigidbody2D rb2d;

    private Transform playerTransform;
    private bool isPerformingAttack = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private bool isDashing = false; // 突進中Y軸固定解除用
    private bool isDashInterrupted = false; // 突進中に衝突したか
    private Vector3 dashInterruptPosition;   // 衝突時の位置

    void Start()
    {
        hoverBaseY = transform.position.y;
        attackTimer = timeBetweenAttacks;
        _directionChangeTimer = directionChangeInterval;
        _attackPower = _sharedCharacter.AttackPower;

        // Rigidbody2DのfreezeRotationを有効化して回転を防止
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.freezeRotation = true;
            // Rigidbody2DのBody TypeをKinematicにする場合は、下記も推奨
            // rb2d.bodyType = RigidbodyType2D.Kinematic;
        }

        // プレイヤーのTransformを取得（タグ"Player"を想定）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("SeaDemon: Player object not found!");
        }
    }

    void Update()
    {
        if (isPerformingAttack)
            return;

        switch (currentState)
        {
            case State.Hovering:
                // 突進中でなければHover
                if (!isDashing)
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

    // 空中を漂う挙動
    private void Hover()
    {
        Vector3 pos = transform.position;
        pos.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = pos;
    }

    // 左右移動（泳ぐ挙動）
    private void Move()
    {
        Vector3 pos = transform.position;
        pos.x += _moveDirection * swimSpeed * Time.deltaTime;
        transform.position = pos;
    }

    // 方向転換
    private void ReverseDirection()
    {
        _moveDirection *= -1;
        // Optional: 反転時にスプライトを左右反転
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _moveDirection;
        transform.localScale = scale;
    }

    // ...existing code...

    // 衝突時に方向転換 or ダッシュ中断
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing)
        {
            // ダッシュ中なら中断フラグを立てて位置を記録（Yもそのまま保持）
            isDashInterrupted = true;
            dashInterruptPosition = transform.position; // ←Yもそのまま
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
                if (isDashing)
                {
                    damage = Mathf.RoundToInt(damage * _rushMagnification);
                }
                playerCharacter.HitAttack(damage);
            }
            return;
        }
    }

    // ランダムで攻撃パターンを選択して実行
    private void PerformRandomAttack()
    {
        int patternCount = 3; // 攻撃パターンの数
        int pattern = random.Next(patternCount);
        switch (pattern)
        {
            case 0:
                AttackPatternA();
                break;
            case 1:
                AttackPatternB();
                break;
            case 2:
                AttackPatternC();
                break;
        }
    }

    // 攻撃パターンA: プレイヤーに向けて突進
    private void AttackPatternA()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("SeaDemon: Player Transform not set for AttackPatternA");
            return;
        }
        StartCoroutine(DashAttackCoroutine());
    }

    // ...existing code...

    private IEnumerator DashAttackCoroutine()
    {
        isPerformingAttack = true;
        isDashInterrupted = false;

        // 1. その場に停止（2秒）: 落下しないようにrb2d.velocityは触らず、X移動だけ停止
        originalPosition = transform.position;
        originalScale = transform.localScale;

        // 2秒間、Hover（Y方向のみ）を維持し、X移動を止める
        float waitTime = 2f;
        float timer = 0f;
        while (timer < waitTime)
        {
            Vector3 pos = transform.position;
            pos.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            transform.position = pos;
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. プレイヤー方向に向く
        Vector3 toPlayer = playerTransform.position - transform.position;
        int dashDirection = toPlayer.x >= 0 ? 1 : -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dashDirection;
        transform.localScale = scale;

        // 3. 停止（2秒）: 同様にHoverのみ
        waitTime = 2f;
        timer = 0f;
        while (timer < waitTime)
        {
            Vector3 pos = transform.position;
            pos.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            transform.position = pos;
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = true;
        float elapsed = 0f;
        float startY = transform.position.y;
        float targetY = playerTransform.position.y;
        bool interrupted = false;
        while (elapsed < _rushDuration)
        {
            if (isDashInterrupted)
            {
                interrupted = true;
                break;
            }
            Vector3 pos = transform.position;
            pos.x += dashDirection * _rushSpeed * Time.deltaTime;
            pos.y = Mathf.Lerp(startY, targetY, elapsed / _rushDuration);
            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        isDashing = false;

        // 5. その場に停止（4秒）: 衝突時はその場、未衝突時はダッシュ終了位置
        Vector3 stopPosition = interrupted ? dashInterruptPosition : transform.position;
        timer = 0f;
        while (timer < _rushStop)
        {
            // 衝突時はYもそのまま維持
            transform.position = stopPosition;
            timer += Time.deltaTime;
            yield return null;
        }

        // 6. 元の位置に直線で戻る（YもLerpで自然に戻す）
        float returnSpeed = swimSpeed;
        Vector3 startPos = transform.position;
        float returnDistance = Vector3.Distance(startPos, originalPosition);
        float returnDuration = returnDistance / Mathf.Max(returnSpeed, 0.01f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(returnDuration, 0.01f);
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startPos.x, originalPosition.x, Mathf.Clamp01(t));
            pos.y = Mathf.Lerp(startPos.y, originalPosition.y, Mathf.Clamp01(t));
            transform.position = pos;
            yield return null;
        }
        transform.position = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z);
        transform.localScale = originalScale;

        isDashInterrupted = false;
        isPerformingAttack = false;
    }

    // 攻撃パターンB
    private void AttackPatternB()
    {
        // プレイヤーが存在しない場合は何もしない
        if (playerTransform == null || wavePrefab == null)
        {
            Debug.LogWarning("SeaDemon: Player or wavePrefab not set for AttackPatternB");
            return;
        }

        // 波オブジェクトを生成
        GameObject waveObj = Instantiate(wavePrefab, transform.position, Quaternion.identity);

        // WaveAttackスクリプトを取得し、ターゲット・攻撃力・加速パラメータを設定
        var waveAttack = waveObj.GetComponent<WaveAttack>();
        if (waveAttack != null)
        {
            waveAttack.SetTarget(playerTransform);
            waveAttack.SetAttackPower(Mathf.RoundToInt(_attackPower * _waveMagnification));
            waveAttack.SetSpeedAndAcceleration(waveInitialSpeed, waveAcceleration);
            // 共通体力を渡す
            waveAttack.SetSharedBossCharacter(_sharedCharacter);
        }
    }

    // 攻撃パターンC
    private void AttackPatternC()
    {
        // TODO: パターンCの攻撃処理を実装
        Debug.Log("SeaDemon: Attack Pattern C");
    }
}
