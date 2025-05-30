using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    private int _damage = 0;
    private Vector2 _moveDirection;

    private PlayerMovement _playerMovement;  // PlayerMovement �Q��

    // PlayerMovement ��ݒ肷�郁�\�b�h�i�v���C���[���ˎ��p�j
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    // �U���͂𒼐ڐݒ肷�郁�\�b�h�i�S�[�X�g�⑼�̔��ˌ��p�j
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    // �e�̐i�s������ݒ�
    public void SetDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    private void Start()
    {
        // PlayerMovement ����s�X�g���̍U���͂��擾�i�v���C���[���ˎ��̂݁j
        if (_playerMovement != null)
        {
            _damage = _playerMovement.CharaState.PistolPower;
        }
        // �S�[�X�g�������SetDamage�Œ��ڃZ�b�g�����
        Destroy(gameObject, 5f); // 5�b��Ɏ����j��
    }

    private void FixedUpdate()
    {
        transform.Translate(_moveDirection * _speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Character hitObject = collision.gameObject.GetComponent<Character>();

            if (hitObject == null) return;

            hitObject.HitAttack(_damage);
        }

        Destroy(this.gameObject);
    }
}
