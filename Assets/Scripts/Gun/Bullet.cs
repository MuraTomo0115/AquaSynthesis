using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    private int _damage = 0;
    private Vector2 _moveDirection;

    private PlayerMovement _playerMovement;  // PlayerMovement 参照

    // PlayerMovement を設定するメソッド（プレイヤー発射時用）
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    // 攻撃力を直接設定するメソッド（ゴーストや他の発射元用）
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    // 弾の進行方向を設定
    public void SetDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    private void Start()
    {
        // PlayerMovement からピストルの攻撃力を取得（プレイヤー発射時のみ）
        if (_playerMovement != null)
        {
            _damage = _playerMovement.CharaState.PistolPower;
        }
        // ゴースト等からはSetDamageで直接セットされる
        Destroy(gameObject, 5f); // 5秒後に自動破棄
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
