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
    }

    void Update()
    {
        if (isPerformingAttack)
            return;

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

    // ...existing code...

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

    // ...existing code...

    private IEnumerator DashAttackCoroutine()
    {
        isPerformingAttack = true;
        isDashInterrupted = false;

        // 1. ���̏�ɒ�~�i2�b�j: �������Ȃ��悤��rb2d.velocity�͐G�炸�AX�ړ�������~
        originalPosition = transform.position;
        originalScale = transform.localScale;

        // 2�b�ԁAHover�iY�����̂݁j���ێ����AX�ړ����~�߂�
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

        // 2. �v���C���[�����Ɍ���
        Vector3 toPlayer = playerTransform.position - transform.position;
        int dashDirection = toPlayer.x >= 0 ? 1 : -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dashDirection;
        transform.localScale = scale;

        // 3. ��~�i2�b�j: ���l��Hover�̂�
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

        // 5. ���̏�ɒ�~�i4�b�j: �Փˎ��͂��̏�A���Փˎ��̓_�b�V���I���ʒu
        Vector3 stopPosition = interrupted ? dashInterruptPosition : transform.position;
        timer = 0f;
        while (timer < _rushStop)
        {
            // �Փˎ���Y�����̂܂܈ێ�
            transform.position = stopPosition;
            timer += Time.deltaTime;
            yield return null;
        }

        // 6. ���̈ʒu�ɒ����Ŗ߂�iY��Lerp�Ŏ��R�ɖ߂��j
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
}
