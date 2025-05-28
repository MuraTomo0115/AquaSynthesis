using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Common Settings")]
    [SerializeField] protected float speed;                // �ړ����x
    [SerializeField] protected float coolTime;             // �U����̃N�[���^�C���i�b�j
    [SerializeField] protected float detectionRange;       // �v���C���[�𔭌����鋗��
    [SerializeField] protected float attackRange;          // �U���\�ȋ���
    [SerializeField] protected LayerMask _playerLayer;     // �v���C���[����p���C���[�}�X�N
    [SerializeField] protected Transform _groundCheck;     // �n�ʔ���p��Transform
    [SerializeField] protected Transform _player;          // �v���C���[��Transform�Q��

    protected Animator animator;           // �A�j���[�^�[�R���|�[�l���g
    protected Rigidbody2D _rigidbody2D;   // 2D���W�b�h�{�f�B

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// �G�̈ړ������i�p����Ŏ����j
    /// </summary>
    public abstract void Move();

    /// <summary>
    /// �G�̍U�������i�p����Ŏ����j
    /// </summary>
    public abstract IEnumerator Attack();
}
