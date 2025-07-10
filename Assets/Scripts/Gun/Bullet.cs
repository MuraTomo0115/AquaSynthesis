using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    private int _damage = 0;
    private Vector2 _moveDirection;

    private PlayerMovement _playerMovement;  // PlayerMovement 参照
    private bool _isRecording = false; // 記録中フラグ（記録中の弾かどうか）
    private bool _isGhostBullet = false; // ゴースト弾フラグ

    /// <summary>
    /// プレイヤー発射時にPlayerMovement参照を設定する
    /// </summary>
    /// <param name="playerMovement">発射元のPlayerMovement</param>
    public void SetPlayerMovement(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    /// <summary>
    /// 弾の攻撃力を直接設定する（ゴーストや他の発射元用）
    /// </summary>
    /// <param name="damage">攻撃力</param>
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    /// <summary>
    /// 弾の進行方向を設定する
    /// </summary>
    /// <param name="direction">進行方向ベクトル</param>
    public void SetDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    /// <summary>
    /// 弾生成時の初期化処理。攻撃力の設定と自動破棄タイマーのセット
    /// </summary>
    private void Start()
    {
        // プレイヤーから発射された場合は攻撃力を設定
        if (_playerMovement != null)
        {
            _damage = _playerMovement.CharaState.PistolPower;
        }
        // 5秒後に自動で弾を破棄
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// 弾の移動処理
    /// </summary>
    private void FixedUpdate()
    {
        // 弾を進行方向に移動
        transform.Translate(_moveDirection * _speed * Time.deltaTime);
    }

    /// <summary>
    /// 衝突時の処理。記録中は攻撃せず消滅、敵に当たればダメージを与える
    /// </summary>
    /// <param name="collision">衝突情報</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 記録中の弾は攻撃判定を行わず、即座に消滅させる
        if (_isRecording)
        {
            Destroy(this.gameObject);
            return;
        }

        // 敵に当たった場合のみ攻撃判定を行う
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
            // Destructibleオブジェクトに当たった場合はダメージを与える
            Character destructible = collision.gameObject.GetComponent<Character>();
            if (destructible != null)
            {
                destructible.HitAttack(_damage);
            }
        }
        else if (collision.gameObject.tag == "Boss")
        {
            // ボスに当たった場合はダメージを与える
            Character boss = collision.gameObject.GetComponent<Character>();
            if (boss != null)
            {
                boss.HitAttack(_damage);
            }
            else
            {
                // 親または子にアタッチされている場合も考慮  
                Character parentHitObject = collision.gameObject.GetComponentInParent<Character>();
                if (parentHitObject != null)
                {
                    parentHitObject.HitAttack(_damage);
                }
            }
        }

        // 何かに当たったら弾を破棄
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 記録中フラグをセットする
    /// </summary>
    /// <param name="isRecording">記録中かどうか</param>
    public void SetIsRecording(bool isRecording)
    {
        _isRecording = isRecording;
    }

    /// <summary>
    /// ゴースト弾フラグをセットする
    /// </summary>
    /// <param name="isGhost">ゴースト弾かどうか</param>
    public void SetIsGhostBullet(bool isGhost)
    {
        _isGhostBullet = isGhost;
    }
}
