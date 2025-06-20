using System.Collections;
using UnityEngine;

/// <summary>
/// �S�[�X�g�̓���E�U���E�A�N�V�����Č����s���N���X
/// </summary>
public class GhostMovement : MonoBehaviour
{
    [Header("�U���֘A")]
    private GameObject _attackSensorInstance;                 // ���ۂɎg���U���Z���T�[

    [SerializeField] private GameObject _bullet;              // �s�X�g���̒e�v���n�u�iInspector�p�E���ۂ�Initialize�ŏ㏑���j
    [SerializeField] private Transform _firePoint;            // �e�̔��ˈʒu

    // �L�^���ꂽ�e��f�[�^
    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;
    private bool _recordedFacingLeft;

    // �����Q��
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    /// <summary>
    /// �S�[�X�g�̏������i�L�^�f�[�^�E�U���Z���T�[�E�e�v���n�u���Z�b�g�j
    /// </summary>
    public void Initialize(
        Vector2 position, Vector2 input, bool jump, bool attack, bool pistol, bool summonA, bool summonB, bool facingLeft,
        GameObject playerAttackSensor = null,
        GameObject bulletPrefab = null
    )
    {
        _recordedPosition = position;
        _recordedInput = input;
        _recordedJump = jump;
        _recordedAttack = attack;
        _recordedPistol = pistol;
        _recordedSummonA = summonA;
        _recordedSummonB = summonB;
        _recordedFacingLeft = facingLeft;

        // �U���Z���T�[���v���C���[���畡��
        if (playerAttackSensor != null)
        {
            _attackSensorInstance = Instantiate(playerAttackSensor, transform);
            _attackSensorInstance.SetActive(false);
        }

        // �e�v���n�u���Z�b�g
        if (bulletPrefab != null)
        {
            _bullet = bulletPrefab;
        }
    }

    /// <summary>
    /// �S�[�X�g�̏���������
    /// </summary>
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();

        // �v���C���[�̃X�e�[�^�X���R�s�[
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerChar = playerObj.GetComponent<Character>();
            if (playerChar != null && _charaState != null)
            {
                _charaState.CopyStatsFrom(playerChar);
            }
        }

        // �L�^���ꂽ�����ʒu�Ɉړ�
        transform.position = _recordedPosition;

        // ��������
        _spriteRenderer.flipX = _recordedFacingLeft;

        // �L�^���ꂽ�A�N�V�������Č��i�����t���[���̂݁j
        if (_recordedAttack)
            _animator.SetTrigger("AttackSword");
        if (_recordedPistol)
            _animator.SetTrigger("AttackPistol");
        if (_recordedJump)
            _animator.SetTrigger("Jump");
        if (_recordedSummonA)
            _animator.SetTrigger("SummonA");
        if (_recordedSummonB)
            _animator.SetTrigger("SummonB");
    }

    /// <summary>
    /// �ړ��A�j���[�V�����̍Č�
    /// </summary>
    private void FixedUpdate()
    {
        // ���͒l�ɉ����ăA�j���[�V������Ԃ�؂�ւ�
        if (Mathf.Abs(_recordedInput.x) > Mathf.Epsilon)
            _animator.SetInteger("AnimState", 1); // ����
        else
            _animator.SetInteger("AnimState", 0); // �ҋ@
    }

    /// <summary>
    /// �s�X�g�����ˏ����i�S�[�X�g�p�j
    /// </summary>
    public void ShootPistol()
    {
        if (_bullet == null || _firePoint == null) return;

        // �e�𐶐�
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);

        // �i�s����������
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // �e�̃p�����[�^���Z�b�g
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
            bulletScript.SetDamage(_charaState != null ? _charaState.PistolPower : 1);
            bulletScript.SetIsGhostBullet(true);
        }
    }

    /// <summary>
    /// �ߐڍU���J�n�i�U���Z���T�[�L�����j
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
    /// �ߐڍU���I���i�U���Z���T�[�������j
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
        if (_charaState == null) return;

        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    /// <summary>
    /// �L�^���ꂽ���͒l���Z�b�g�i�Đ����ɌĂ΂��j
    /// </summary>
    public void SetRecordedInput(Vector2 input)
    {
        _recordedInput = input;
    }

    // �_�~�[���\�b�h
    public void trueAttack() { }
    public void falseAttack() { }
}
