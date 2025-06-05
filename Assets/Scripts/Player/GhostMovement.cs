using System.Collections;
using UnityEngine;

/// <summary>
/// �S�[�X�g�̓���E�U���E�A�N�V�����Č����s���N���X
/// </summary>
public class GhostMovement : MonoBehaviour
{
    [Header("�U���֘A")]
    [SerializeField] private GameObject _attackSensorPrefab;  // �U���Z���T�[�v���n�u
    private GameObject _attackSensorInstance;

    [SerializeField] private GameObject _bullet;     // �s�X�g���̒e�v���n�u
    [SerializeField] private Transform _firePoint;   // �e�̔��ˈʒu

    // �L�^���ꂽ�e��f�[�^
    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;
    private bool _recordedFacingLeft; // ����

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    /// <summary>
    /// �L�^�f�[�^���󂯎���ď�����
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
    /// ����������
    /// </summary>
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();

        // �v���C���[��Character��T���ăX�e�[�^�X���R�s�[
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerChar = playerObj.GetComponent<Character>();
            if (playerChar != null && _charaState != null)
            {
                _charaState.CopyStatsFrom(playerChar);
            }
        }

        // �����ʒu�Ɉړ�
        transform.position = _recordedPosition;

        // �U���Z���T�[����
        if (_attackSensorPrefab != null)
        {
            _attackSensorInstance = Instantiate(_attackSensorPrefab, transform);
            _attackSensorInstance.SetActive(false);
        }

        // ��������
        _spriteRenderer.flipX = _recordedFacingLeft;

        // �A�N�V�����Č�
        if (_recordedAttack)
        {
            _animator.SetTrigger("AttackSword");
        }

        if (_recordedPistol)
        {
            _animator.SetTrigger("AttackPistol");
            // �s�v�ȃ��O�폜
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
    /// �ړ��A�j���[�V�����̍Č�
    /// </summary>
    private void FixedUpdate()
    {
        if (Mathf.Abs(_recordedInput.x) > Mathf.Epsilon)
        {
            _animator.SetInteger("AnimState", 1); // �������[�V����
        }
        else
        {
            _animator.SetInteger("AnimState", 0); // �ҋ@���[�V����
        }
    }

    /// <summary>
    /// �s�X�g�����ˏ���
    /// </summary>
    public void ShootPistol()
    {
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);
        bulletScript.SetDamage(_charaState != null ? _charaState.PistolPower : 1);
        bulletScript.SetIsGhostBullet(true); // ���S�[�X�g�e�t���O���Z�b�g
    }

    /// <summary>
    /// �ߐڍU���J�n
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
    /// �ߐڍU���I��
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
    /// �U�����q�b�g�������̏���
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

    // �N���X���ɒǉ�
    public void SetRecordedInput(Vector2 input)
    {
        _recordedInput = input;
    }
}
