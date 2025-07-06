using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �吻��ҁF���c�q��
public class SeaDemon : MonoBehaviour
{
    // �{�X�̏��
    private enum State
    {
        Hovering,
        Attacking
    }

    [Header("�㉺�ړ��ݒ�")]
    public float hoverHeight = 3.0f;
    public float hoverSpeed = 2.0f;
    public float hoverAmplitude = 0.5f;

    [Header("�ړ��ݒ�")]
    public float swimSpeed = 2.0f;
    public float directionChangeInterval = 3.0f;

    [Header("�U���^�C�~���O")]
    public float timeBetweenAttacks = 2.0f;

    [Header("�Ռ��g�ݒ�")]
    [SerializeField] private GameObject wavePrefab; // WaveObject�̃v���t�@�u
    [SerializeField] private float waveInitialSpeed = 2f;
    [SerializeField] private float waveAcceleration = 3f;

    [Header("���ʑ̗̓I�u�W�F�N�g")]
    public Character _sharedCharacter;

    private State currentState = State.Hovering;
    private float hoverBaseY;
    private float attackTimer;

    [Header("�U���͔{��")]
    [SerializeField] private float _rushMagnification = 1.5f; // �ːi�U���̔{��
    [SerializeField] private float _waveMagnification = 1.1f; // �g�U���{��

    [Header("�ːi�U���ݒ�")]
    [SerializeField] private float _rushSpeed = 5f;     // �ːi���x
    [SerializeField] private float _rushDuration = 1f;  // �ːi��������
    [SerializeField] private float _rushStop = 2f;      // �ːi��̒�~����

    [Header("音声設定")]
    [SerializeField] private float _voiceMinInterval = 3f;  // 音声再生の最小間隔
    [SerializeField] private float _voiceMaxInterval = 8f;  // 音声再生の最大間隔

    // --- Swimming movement variables ---
    private int _moveDirection = 1; // 1: �E, -1: ��
    private float _directionChangeTimer;
    private int _attackPower;

    private System.Random random = new System.Random();

    private Rigidbody2D rb2d;

    private Transform playerTransform;
    private bool isPerformingAttack = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private bool isDashing = false; // �ːi��Y���Œ�����p
    private bool isDashInterrupted = false; // �ːi���ɏՓ˂�����
    private Vector3 dashInterruptPosition;   // �Փˎ��̈ʒu

    // 音声関連
    private float _nextVoiceTime;
    private bool _isPlayingVoice = false;

    void Start()
    {
        hoverBaseY = transform.position.y;
        attackTimer = timeBetweenAttacks;
        _directionChangeTimer = directionChangeInterval;
        _attackPower = _sharedCharacter.AttackPower;

        // Rigidbody2D��freezeRotation��L�������ĉ�]��h�~
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.freezeRotation = true;
            // Rigidbody2D��Body Type��Kinematic�ɂ���ꍇ�́A���L������
            // rb2d.bodyType = RigidbodyType2D.Kinematic;
        }

        // �v���C���[��Transform���擾�i�^�O"Player"��z��j
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("SeaDemon: Player object not found!");
        }

        // 最初の音声再生タイミングを設定
        SetNextVoiceTime();
    }

    void Update()
    {
        if (isPerformingAttack)
            return;

        // 音声再生チェック（攻撃中でない場合のみ）
        CheckAndPlayVoice();

        switch (currentState)
        {
            case State.Hovering:
                // �ːi���łȂ����Hover
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

    // �󒆂�Y������
    private void Hover()
    {
        Vector3 pos = transform.position;
        pos.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = pos;
    }

    // ���E�ړ��i�j�������j
    private void Move()
    {
        Vector3 pos = transform.position;
        pos.x += _moveDirection * swimSpeed * Time.deltaTime;
        transform.position = pos;
    }

    // �����]��
    private void ReverseDirection()
    {
        _moveDirection *= -1;
        // Optional: ���]���ɃX�v���C�g�����E���]
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _moveDirection;
        transform.localScale = scale;
    }

    // �Փˎ��ɕ����]�� or �_�b�V�����f
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing)
        {
            // �_�b�V�����Ȃ璆�f�t���O�𗧂ĂĈʒu���L�^�iY�����̂܂ܕێ��j
            isDashInterrupted = true;
            dashInterruptPosition = transform.position; // ��Y�����̂܂�
        }
        else
        {
            ReverseDirection();
            _directionChangeTimer = directionChangeInterval;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // �v���C���[��Character�R���|�[�l���g���擾
            Character playerCharacter = collision.gameObject.GetComponent<Character>();
            if (playerCharacter != null)
            {
                int damage = _attackPower;
                // �_�b�V�����Ȃ�{����������
                if (isDashing)
                {
                    damage = Mathf.RoundToInt(damage * _rushMagnification);
                }
                playerCharacter.HitAttack(damage);
            }
            return;
        }
    }

    // �����_���ōU���p�^�[����I�����Ď��s
    private void PerformRandomAttack()
    {
        int patternCount = 3; // �U���p�^�[���̐�
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

    // �U���p�^�[��A: �v���C���[�Ɍ����ēːi
    private void AttackPatternA()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("SeaDemon: Player Transform not set for AttackPatternA");
            return;
        }
        StartCoroutine(DashAttackCoroutine());
    }

    private IEnumerator DashAttackCoroutine()
    {
        isPerformingAttack = true;
        isDashing = false;
        isDashInterrupted = false;
        
        // 1. その場で停止（位置固定）
        Vector3 startPosition = transform.position;
        originalPosition = startPosition; // 戻る位置を記録
        
        // 空中でその場に完全停止（重力の影響を一時的に無効化）
        float originalGravityScale = 0f;
        if (rb2d != null)
        {
            originalGravityScale = rb2d.gravityScale;
            rb2d.gravityScale = 0f; // 重力を無効化
            rb2d.velocity = Vector2.zero;
        }
        
        // 2. プレイヤーの方を向く（2秒停止）
        Vector3 playerDirection = (playerTransform.position - transform.position).normalized;
        
        // スプライトをプレイヤーの方向に向ける
        if (playerDirection.x > 0)
        {
            // プレイヤーが右側にいる場合
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            // プレイヤーが左側にいる場合
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
        // 2秒間停止（位置を固定）
        float waitTimer = 0f;
        while (waitTimer < 2f)
        {
            // 位置を固定して落下を防ぐ
            transform.position = originalPosition;
            waitTimer += Time.deltaTime;
            yield return null;
        }
        
        // 3. プレイヤーに向け直線移動で突進
        isDashing = true;
        Vector3 dashDirection = (playerTransform.position - transform.position).normalized;
        float dashTimer = 0f;
        
        while (dashTimer < _rushDuration && !isDashInterrupted)
        {
            if (rb2d != null)
            {
                // Rigidbody2Dを使って移動
                rb2d.velocity = dashDirection * _rushSpeed;
            }
            else
            {
                // Transform.Translateを使って移動
                transform.Translate(dashDirection * _rushSpeed * Time.deltaTime, Space.World);
            }
            dashTimer += Time.deltaTime;
            yield return null;
        }
        
        // 突進後に速度を0にして停止
        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
        }
        
        // 4. 突進中に何かに衝突した場合はその場で数秒停止
        if (isDashInterrupted)
        {
            // 衝突した位置で停止
            yield return new WaitForSeconds(_rushStop);
        }
        
        isDashing = false;
        
        // 5. 突進する前の位置に戻る
        float returnSpeed = _rushSpeed * 0.7f; // 戻る速度は少し遅めに
        while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            Vector3 returnDirection = (originalPosition - transform.position).normalized;
            
            if (rb2d != null)
            {
                // Rigidbody2Dを使って戻る
                rb2d.velocity = returnDirection * returnSpeed;
            }
            else
            {
                // Transform.Translateを使って戻る
                transform.Translate(returnDirection * returnSpeed * Time.deltaTime, Space.World);
            }
            yield return null;
        }
        
        // 正確に元の位置に戻して速度を0に
        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
            // 重力スケールを元に戻す
            rb2d.gravityScale = originalGravityScale;
        }
        transform.position = originalPosition;
        
        isPerformingAttack = false;
        isDashInterrupted = false;
    }

    // �U���p�^�[��B
    private void AttackPatternB()
    {
        // �v���C���[�����݂��Ȃ��ꍇ�͉������Ȃ�
        if (playerTransform == null || wavePrefab == null)
        {
            Debug.LogWarning("SeaDemon: Player or wavePrefab not set for AttackPatternB");
            return;
        }

        // �g�I�u�W�F�N�g�𐶐�
        GameObject waveObj = Instantiate(wavePrefab, transform.position, Quaternion.identity);

        // WaveAttack�X�N���v�g���擾���A�^�[�Q�b�g�E�U���́E�����p�����[�^��ݒ�
        var waveAttack = waveObj.GetComponent<WaveAttack>();
        if (waveAttack != null)
        {
            waveAttack.SetTarget(playerTransform);
            waveAttack.SetAttackPower(Mathf.RoundToInt(_attackPower * _waveMagnification));
            waveAttack.SetSpeedAndAcceleration(waveInitialSpeed, waveAcceleration);
            // ���ʑ̗͂�n��
            waveAttack.SetSharedBossCharacter(_sharedCharacter);
        }
    }

    // �U���p�^�[��C
    private void AttackPatternC()
    {
        // TODO: �p�^�[��C�̍U������������
        Debug.Log("SeaDemon: Attack Pattern C");
    }

    // 音声再生関連メソッド
    private void CheckAndPlayVoice()
    {
        // 攻撃中や音声再生中は何もしない
        if (isPerformingAttack || _isPlayingVoice)
            return;

        // 次の音声再生時間に達したかチェック
        if (Time.time >= _nextVoiceTime)
        {
            PlayVoice();
        }
    }

    private void PlayVoice()
    {
        _isPlayingVoice = true;
        
        // AudioManagerで音声を再生
        AudioManager.Instance.PlaySE("Boss", "SeaDemonVoice1", "N1");
        
        // 音声終了後のコルーチンを開始
        StartCoroutine(VoiceEndCoroutine());
    }

    private IEnumerator VoiceEndCoroutine()
    {
        // 音声の長さを仮定（実際の音声ファイルの長さに合わせて調整）
        float voiceDuration = 2f; // 2秒と仮定
        yield return new WaitForSeconds(voiceDuration);
        
        _isPlayingVoice = false;
        SetNextVoiceTime();
    }

    private void SetNextVoiceTime()
    {
        // ランダムな間隔で次の音声再生時間を設定
        float interval = UnityEngine.Random.Range(_voiceMinInterval, _voiceMaxInterval);
        _nextVoiceTime = Time.time + interval;
    }
}
