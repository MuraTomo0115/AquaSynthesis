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
    /// ターゲットを設定し、初期化処理を行う
    /// </summary>
    /// <param name="t">目標オブジェクト</param>
    public void SetTarget(Transform t)
    {
        _target = t;
        if (_target != null)
        {
            _moveDir = ((Vector2)(_target.position - transform.position)).normalized;
            _initialized = true;

            // プレイヤー方向に上側（Y+）を向ける
            float angle = Mathf.Atan2(_moveDir.y, _moveDir.x) * Mathf.Rad2Deg;
            // Y+をターゲット方向に合わせるため、Z軸回転で-90度補正
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    /// <summary>
    /// 攻撃力を設定するメソッド
    /// </summary>
    /// <param name="power">攻撃力</param>
    public void SetAttackPower(int power)
    {
        _attackPower = power;
    }

    /// <summary>
    /// 初期速度と加速度を設定するメソッド
    /// </summary>
    /// <param name="initialSpeed">初速度</param>
    /// <param name="accel">加速度</param>
    public void SetSpeedAndAcceleration(float initialSpeed, float accel)
    {
        _speed = initialSpeed;
        _acceleration = accel;
    }

    /// <summary>
    /// 共通体力をセット
    /// </summary>
    /// <param name="shared">共通体力オブジェクト</param>
    public void SetSharedBossCharacter(Character shared)
    {
        sharedBossCharacter = shared;
    }

    /// <summary>
    /// 毎フレームの更新処理。ターゲット方向に進み、加速
    /// </summary>
    void Update()
    {
        if (!_initialized || _target == null) return;

        // 加速しながらターゲット方向に進む
        _speed += _acceleration * Time.deltaTime;
        // Y+方向（transform.up）に進む
        transform.position += transform.up * _speed * Time.deltaTime;
    }

    /// <summary>
    /// 衝突時の処理。プレイヤーにダメージを与える
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
        // 何かに衝突したら必ず消える
        Destroy(gameObject);
    }
}
