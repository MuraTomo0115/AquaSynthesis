using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 5f;  // �ړ����x
    [SerializeField]
    private float _jumpForce = 5f;  // �W�����v��
    private Rigidbody2D _rb;
    private Vector2 _movement;      // ���@
    private PlayerInputActions _inputActions;
    private bool _isCanJump = true;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    /// <summary>
    /// InputSystem�ݒ�
    /// </summary>
    private void OnEnable()
    {
        // InputSystem��L���ɂ���
        _inputActions.Player.Enable();
        _inputActions.Player.Move.performed += ctx => _movement = ctx.ReadValue<Vector2>();
        // ���͂��Ȃ��Ȃ����ꍇ������͂��O�ɂ���
        _inputActions.Player.Move.canceled += ctx => _movement = Vector2.zero;
        // �W�����v����
        _inputActions.Player.Jump.performed += ctx => Jump();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// �ړ��A�W�����v����
    /// </summary>
    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movement.x * _moveSpeed, _rb.velocity.y);
    }

    /// <summary>
    /// �n�ʂɒ��n������W�����v�\�ɂ���
    /// </summary>
    /// <param name="collision">�Փ˂����I�u�W�F�N�g</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isCanJump = true;
        }
    }

    /// <summary>
    /// �W�����v����
    /// </summary>
    private void Jump()
    {
        // �t���O�������̏ꍇ�W�����v���Ȃ�
        if (!_isCanJump)
            return;

        // ForceMode2D���g�p���A�u���I�ɃW�����v
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _isCanJump = false;
    }
}
