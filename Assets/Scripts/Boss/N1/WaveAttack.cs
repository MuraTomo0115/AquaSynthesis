using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveAttack : MonoBehaviour
{
    private Transform _target;
    private int _attackPower = 10;
    private float _speed = 2f;
    private float _acceleration = 3f;
    private Vector2 _moveDir;
    private bool _initialized = false;
    private Character sharedBossCharacter;

    /// <summary>
    /// �^�[�Q�b�g��ݒ肵�A�������������s��
    /// </summary>
    /// <param name="t">�ڕW�I�u�W�F�N�g</param>
    public void SetTarget(Transform t)
    {
        _target = t;
        if (_target != null)
        {
            _moveDir = ((Vector2)(_target.position - transform.position)).normalized;
            _initialized = true;

            // �v���C���[�����ɏ㑤�iY+�j��������
            float angle = Mathf.Atan2(_moveDir.y, _moveDir.x) * Mathf.Rad2Deg;
            // Y+���^�[�Q�b�g�����ɍ��킹�邽�߁AZ����]��-90�x�␳
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    /// <summary>
    /// �U���͂�ݒ肷�郁�\�b�h
    /// </summary>
    /// <param name="power">�U����</param>
    public void SetAttackPower(int power)
    {
        _attackPower = power;
    }

    /// <summary>
    /// �������x�Ɖ����x��ݒ肷�郁�\�b�h
    /// </summary>
    /// <param name="initialSpeed">�����x</param>
    /// <param name="accel">�����x</param>
    public void SetSpeedAndAcceleration(float initialSpeed, float accel)
    {
        _speed = initialSpeed;
        _acceleration = accel;
    }

    /// <summary>
    /// ���ʑ̗͂��Z�b�g
    /// </summary>
    /// <param name="shared">���ʑ̗̓I�u�W�F�N�g</param>
    public void SetSharedBossCharacter(Character shared)
    {
        sharedBossCharacter = shared;
    }

    /// <summary>
    /// ���t���[���̍X�V�����B�^�[�Q�b�g�����ɐi�݁A����
    /// </summary>
    void Update()
    {
        if (!_initialized || _target == null) return;

        // �������Ȃ���^�[�Q�b�g�����ɐi��
        _speed += _acceleration * Time.deltaTime;
        // Y+�����itransform.up�j�ɐi��
        transform.position += transform.up * _speed * Time.deltaTime;
    }

    /// <summary>
    /// �Փˎ��̏����B�v���C���[�Ƀ_���[�W��^����
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerChar = collision.gameObject.GetComponent<Character>();
            if (playerChar != null)
            {
                playerChar.HitAttack(_attackPower);
            }
        }
        // �����ɏՓ˂�����K��������
        Destroy(gameObject);
    }
}
