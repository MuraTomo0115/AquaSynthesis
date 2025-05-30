using System.Collections;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("�U���֘A")]
    [SerializeField] private GameObject _attackSensorPrefab;  // �U���Z���T�[�v���n�u
    private GameObject _attackSensorInstance;

    [SerializeField] private GameObject _bullet;     // �s�X�g���̒e�v���n�u
    [SerializeField] private Transform _firePoint;   // �e�̔��ˈʒu

    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;
    private bool _recordedFacingLeft; // �ǉ�

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    // �L�^�f�[�^���󂯎��
    public void Initialize(Vector2 position, Vector2 input, bool jump, bool attack, bool pistol, bool summonA, bool summonB, bool facingLeft)
    {
        _recordedPosition = position;
        _recordedInput = input;
        _recordedJump = jump;
        _recordedAttack = attack;
        _recordedPistol = pistol;
        _recordedSummonA = summonA;
        _recordedSummonB = summonB;
        _recordedFacingLeft = facingLeft; // �ǉ�
    }

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
        _spriteRenderer.flipX = _recordedFacingLeft; // �����Ō����𔽉f

        // �A�N�V�����Č�
        if (_recordedAttack)
        {
            _animator.SetTrigger("AttackSword");
        }

        if (_recordedPistol)
        {
            _animator.SetTrigger("AttackPistol");
            ShootPistol();
            Debug.LogWarning("AttackPistol triggered by ghost!");
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

    // �s�X�g�����ˏ���
    public void ShootPistol()
    {
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);
        bulletScript.SetDamage(_charaState != null ? _charaState.PistolPower : 1); // �����ōU���͂�n��
    }

    // �ߐڍU���J�n
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

    // �ߐڍU���I��
    public void EndAttack()
    {
        if (_attackSensorInstance != null)
        {
            _attackSensorInstance.transform.localScale = Vector3.zero;
            _attackSensorInstance.SetActive(false);
        }
    }

    public void OwnAttackHit(Collider2D other)
    {
        if (_charaState == null)
        {
            Debug.LogError("GhostMovement: _charaState is null!");
            return;
        }
        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    // �A�j���[�V�����C�x���g�p�̃_�~�[�֐��i���g�p�Ȃ��̂܂܂�OK�j
    public void trueAttack() { }
    public void falseAttack() { }
}
