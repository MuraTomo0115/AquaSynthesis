using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    private int _damage = 0;
    private Vector2 _moveDirection;

    private PlayerMovement _playerMovement;  // PlayerMovement �Q��
    private bool _isRecording = false; // �L�^���t���O�i�L�^���̒e���ǂ����j
    private bool _isGhostBullet = false; // �S�[�X�g�e�t���O

    /// <summary>
    /// �v���C���[���ˎ���PlayerMovement�Q�Ƃ�ݒ肷��
    /// </summary>
    /// <param name="playerMovement">���ˌ���PlayerMovement</param>
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    /// <summary>
    /// �e�̍U���͂𒼐ڐݒ肷��i�S�[�X�g�⑼�̔��ˌ��p�j
    /// </summary>
    /// <param name="damage">�U����</param>
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    /// <summary>
    /// �e�̐i�s������ݒ肷��
    /// </summary>
    /// <param name="direction">�i�s�����x�N�g��</param>
    public void SetDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    /// <summary>
    /// �e�������̏����������B�U���͂̐ݒ�Ǝ����j���^�C�}�[�̃Z�b�g
    /// </summary>
    private void Start()
    {
        // �v���C���[���甭�˂��ꂽ�ꍇ�͍U���͂�ݒ�
        if (_playerMovement != null)
        {
            _damage = _playerMovement.CharaState.PistolPower;
        }
        // 5�b��Ɏ����Œe��j��
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// �e�̈ړ�����
    /// </summary>
    private void FixedUpdate()
    {
        // �e��i�s�����Ɉړ�
        transform.Translate(_moveDirection * _speed * Time.deltaTime);
    }

    /// <summary>
    /// �Փˎ��̏����B�L�^���͍U���������ŁA�G�ɓ�����΃_���[�W��^����
    /// </summary>
    /// <param name="collision">�Փˏ��</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �L�^���̒e�͍U��������s�킸�A�����ɏ��ł�����
        if (_isRecording)
        {
            Destroy(this.gameObject);
            return;
        }

        // �G�ɓ��������ꍇ�̂ݍU��������s��
        if (collision.gameObject.tag == "Enemy")
        {
            Character hitObject = collision.gameObject.GetComponent<Character>();
            if (hitObject != null)
            {
                hitObject.HitAttack(_damage);
            }
        }
        else if(collision.gameObject.tag == "Destructible")
        {
            // Destructible�I�u�W�F�N�g�ɓ��������ꍇ�̓_���[�W��^����
            Character destructible = collision.gameObject.GetComponent<Character>();
            if (destructible != null)
            {
                destructible.HitAttack(_damage);
            }
        }
        else if (collision.gameObject.tag == "Boss")
        {
            // �{�X�ɓ��������ꍇ�̓_���[�W��^����
            Character boss = collision.gameObject.GetComponent<Character>();
            if (boss != null)
            {
                boss.HitAttack(_damage);
            }
            else
            {
                // �e�܂��͎q�ɃA�^�b�`����Ă���ꍇ���l��  
                Character parentHitObject = collision.gameObject.GetComponentInParent<Character>();
                if (parentHitObject != null)
                {
                    parentHitObject.HitAttack(_damage);
                }
            }
        }

        // �����ɓ���������e��j��
        Destroy(this.gameObject);
    }

    /// <summary>
    /// �L�^���t���O���Z�b�g����
    /// </summary>
    /// <param name="isRecording">�L�^�����ǂ���</param>
    public void SetIsRecording(bool isRecording)
    {
        _isRecording = isRecording;
    }

    /// <summary>
    /// �S�[�X�g�e�t���O���Z�b�g����
    /// </summary>
    /// <param name="isGhost">�S�[�X�g�e���ǂ���</param>
    public void SetIsGhostBullet(bool isGhost)
    {
        _isGhostBullet = isGhost;
    }
}
