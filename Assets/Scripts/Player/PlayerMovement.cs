using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 5f;  // �ړ����x
    [SerializeField]
    private float _jumpForce = 5f;  // �W�����v��
    [SerializeField]
    private float _ray = 1f;        // �n�ʂ����o���郌�C�̒���
    [SerializeField]
    private Transform _groundCheck; // �����̋�I�u�W�F�N�g
    [SerializeField]
    private LayerMask _groundLayer; // �n�ʂ̃^�O
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

        //�n�ʃ`�F�b�N
        RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector2.down, _ray, _groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            _isCanJump = true;
        }
        else
        {
            _isCanJump= false;
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
