using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Common Settings")]
    [SerializeField] protected float speed;                // 移動速度
    [SerializeField] protected float coolTime;             // 攻撃後のクールタイム（秒）
    [SerializeField] protected float detectionRange;       // プレイヤーを発見する距離
    [SerializeField] protected float attackRange;          // 攻撃可能な距離
    [SerializeField] protected LayerMask _playerLayer;     // プレイヤー判定用レイヤーマスク
    [SerializeField] protected Transform _groundCheck;     // 地面判定用のTransform
    [SerializeField] protected Transform _player;          // プレイヤーのTransform参照

    protected Animator animator;           // アニメーターコンポーネント
    protected Rigidbody2D _rigidbody2D;   // 2Dリジッドボディ

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 敵の移動処理（継承先で実装）
    /// </summary>
    public abstract void Move();

    /// <summary>
    /// 敵の攻撃処理（継承先で実装）
    /// </summary>
    public abstract IEnumerator Attack();
}
