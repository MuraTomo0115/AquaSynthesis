using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 5f;  // 移動速度
    [SerializeField]
    private float _jumpForce = 5f;  // ジャンプ力
    private Rigidbody2D _rb;
    private Vector2 _movement;      // 自機
    private PlayerInputActions _inputActions;
    private bool _isCanJump = true;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    /// <summary>
    /// InputSystem設定
    /// </summary>
    private void OnEnable()
    {
        // InputSystemを有効にする
        _inputActions.Player.Enable();
        _inputActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        // 入力がなくなった場合加える力を０にする
        _inputActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        // ジャンプする
        _inputActions.Player.Jump.performed += ctx => Jump();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 移動、ジャンプ処理
    /// </summary>
    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movement.x * _moveSpeed, _rb.velocity.y);
    }

    /// <summary>
    /// 地面に着地したらジャンプ可能にする
    /// </summary>
    /// <param name="collision">衝突したオブジェクト</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isCanJump = true;
        }
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    private void Jump()
    {
        // フラグが無効の場合ジャンプしない
        if (!_isCanJump)
            return;

        // ForceMode2Dを使用し、瞬発的にジャンプ
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _isCanJump = false;
    }
}
