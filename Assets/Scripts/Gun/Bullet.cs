using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float    _speed = 10f;
    private int               _damage = 0;
    private Vector2           _moveDirection;

    private PlayerMovement _playerMovement;  // PlayerMovement 参照

    // PlayerMovement を設定するメソッド
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    public void SetDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    private void Start()
    {
        // PlayerMovement からピストルの攻撃力を取得
        if (_playerMovement != null)
        {
            _damage = _playerMovement.CharaState.PistolPower;
        }
        Destroy(gameObject, 5f); // 5秒後に自動破棄
    }

    private void FixedUpdate()
    {
        transform.Translate(_moveDirection * _speed * Time.deltaTime);
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    //Character hitObject = other.GetComponent<Character>();

    //    //if (hitObject == null) return;

    //    //hitObject.HitAttack(_damage);
    //    //Destroy(this.gameObject);

    //    //// Ground レイヤーなら弾を消す
    //    //if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
    //    //{
    //    //    Destroy(this.gameObject);
    //    //}
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Character hitObject = collision.gameObject.GetComponent<Character>();

            if (hitObject == null) return;

            hitObject.HitAttack(_damage);
        }

        Destroy(this.gameObject);
        Debug.Log("Bullet hit: " + collision.gameObject.name);
    }
}
