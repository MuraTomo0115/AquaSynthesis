using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class StageData
{
    public string stageName;      // �\����
    public string sceneName;      // �V�[����
    public Sprite stageImage;     // �w�i�摜
    public Transform stagePoint;     // �X�e�[�W�ʒu
}

public class StageSelector : MonoBehaviour
{
    [Header("�X�e�[�W�f�[�^���X�g")]
    [SerializeField] private List<StageData> _stages; // �X�e�[�W���̃��X�g

    [Header("�v���C���[�I�u�W�F�N�g")]
    [SerializeField] private GameObject _playerObject; // �v���C���[��GameObject

    [Header("�w�i�摜UI")]
    [SerializeField] private Image _backgroundImage; // �w�i�摜��\������UI

    [Header("�ړ����x")]
    [SerializeField] private float _moveSpeed; // �X�e�[�W�Ԃ̈ړ��ɂ�����b��

    [SerializeField] private float _playerYOffset; // �v���C���[��Y���I�t�Z�b�g

    private int _currentIndex = 0; // ���ݑI�𒆂̃X�e�[�W�ԍ�
    private bool _isMoving = false; // �v���C���[���ړ������ǂ���
    private int _moveDirection = 1; // -1:��, 1:�E�i�ړ������j

    private Transform _player; // �v���C���[��Transform
    private Animator _playerAnimator; // �v���C���[��Animator

    // InputSystem
    private StageSelectInputActions _inputActions;

    private void Awake()
    {
        // InputActions������
        _inputActions = new StageSelectInputActions();
        _inputActions.StageSelect.Move.performed += OnMove;
        _inputActions.StageSelect.Submit.performed += OnSubmit;
    }

    private void OnEnable()
    {
        _inputActions?.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Disable();
    }

    private void Start()
    {
        // �v���C���[��Transform��Animator���擾
        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }

        UpdateStageView();    // �w�i�摜��������
        MovePlayerInstant();  // �v���C���[�������ʒu�Ɉړ�
    }

    // ���̓C�x���g: �ړ�
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (_isMoving) return;
        Vector2 value = ctx.ReadValue<Vector2>();
        if (value.x < -0.5f)
        {
            MoveToStage(_currentIndex - 1, -1);
        }
        else if (value.x > 0.5f)
        {
            MoveToStage(_currentIndex + 1, 1);
        }
    }

    // ���̓C�x���g: ����
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_isMoving) return;
        SceneManager.LoadScene(_stages[_currentIndex].sceneName);
    }

    /// <summary>
    /// �X�e�[�W���ړ����鏈��
    /// </summary>
    /// <param name="newIndex">�ړ���̃X�e�[�W�ԍ�</param>
    /// <param name="direction">�ړ������i-1:��, 1:�E�j</param>
    private void MoveToStage(int newIndex, int direction)
    {
        if (newIndex < 0 || newIndex >= _stages.Count) return; // �͈͊O�͖���
        _currentIndex = newIndex;
        _moveDirection = direction;
        UpdateStageView();           // �w�i�摜���X�V
        SetPlayerFacing(_moveDirection); // �ړ������Ɍ�����ς���
        StartCoroutine(MovePlayerCoroutine()); // �v���C���[���A�j���[�V�����ňړ�
    }

    /// <summary>
    /// �w�i�摜�����݂̃X�e�[�W�̂��̂ɍX�V
    /// </summary>
    private void UpdateStageView()
    {
        if (_backgroundImage != null && _stages[_currentIndex].stageImage != null)
            _backgroundImage.sprite = _stages[_currentIndex].stageImage;
    }

    /// <summary>
    /// �v���C���[�𑦍��Ɍ��݂̃X�e�[�W�ʒu�Ɉړ�
    /// </summary>
    private void MovePlayerInstant()
    {
        if (_player != null && _stages[_currentIndex].stagePoint != null)
        {
            Vector3 pos = _stages[_currentIndex].stagePoint.position;
            pos.y += _playerYOffset; // Y���������グ��
            _player.position = pos;
        }
    }

    /// <summary>
    /// �v���C���[�̌�����ݒ�idirection: 1=�E, -1=���j
    /// </summary>
    private void SetPlayerFacing(int direction)
    {
        if (_player == null) return;
        Vector3 scale = _player.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        _player.localScale = scale;
    }

    /// <summary>
    /// �v���C���[���X���C�h�ړ�������R���[�`��
    /// </summary>
    private IEnumerator MovePlayerCoroutine()
    {
        _isMoving = true;
        Vector3 start = _player.position;
        Vector3 end = _stages[_currentIndex].stagePoint.position;
        end.y += _playerYOffset; // Y���I�t�Z�b�g��������
        float duration = _moveSpeed; // �ړ��ɂ�����b��
        float elapsed = 0f;

        // Run�A�j���[�V�����Đ�
        if (_playerAnimator != null)
            _playerAnimator.SetBool("isRunning", true);

        // �w�莞�Ԃ����ăX���C�h�ړ�
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _player.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        _player.position = end;

        SetPlayerFacing(1); // �������͉E�����ɂ���

        // Idle�A�j���[�V�����Đ�
        if (_playerAnimator != null)
            _playerAnimator.SetBool("isRunning", false);

        _isMoving = false;
    }
}
