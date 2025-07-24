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
    public Transform stagePoint;  // �X�e�[�W�ʒu
}

public class StageSelector : MonoBehaviour
{
    [Header("�v���C���[�I�u�W�F�N�g")]
    [SerializeField] private GameObject _playerObject; // �v���C���[��GameObject

    [Header("�w�i�摜UI")]
    [SerializeField] private Image _backgroundImage; // �w�i�摜��\������UI

    [Header("�ړ����x")]
    [SerializeField] private float _moveSpeed; // �X�e�[�W�Ԃ̈ړ��ɂ�����b��

    [SerializeField] private float _playerYOffset; // �v���C���[��Y���I�t�Z�b�g

    private List<StageData> _allStages; // �i�s���őS�X�e�[�W��o�^

    // N/A/G���[�g�̑S���
    [Header("N���[�g�X�e�[�W���X�g (N1~N8)")]
    [SerializeField] private List<StageData> _nStages;
    [Header("A���[�g�X�e�[�W���X�g (A2~A8)")]
    [SerializeField] private List<StageData> _aStages;
    [Header("G���[�g�X�e�[�W���X�g (G4~G8)")]
    [SerializeField] private List<StageData> _gStages;

    private int _currentIndex = 0; // ���ݑI�𒆂̃X�e�[�W�ԍ�
    private bool _isMoving = false; // �v���C���[���ړ������ǂ���
    private int _moveDirection = 1; // -1:��, 1:�E�i�ړ������j

    // ���[�g����t���O
    private bool _isARoute = false;
    private bool _isGRoute = false;
    private string _currentRoute;

    private Transform _player; // �v���C���[��Transform
    private Animator _playerAnimator; // �v���C���[��Animator

    private void Awake()
    {
        // InputActions������
        var inputActions = InputActionHolder.Instance.stageSelectInputActions;
        inputActions.StageSelect.Move.performed += OnMove;
        inputActions.StageSelect.Submit.performed += OnSubmit;
    }

    private void OnEnable()
    {
        InputActionHolder.Instance.stageSelectInputActions?.Enable();
    }

    private void OnDisable()
    {
        InputActionHolder.Instance.stageSelectInputActions?.Disable();
    }

    private void Start()
    {
        _currentRoute = DatabaseManager.GetCurrentRouteById(1);

        switch (_currentRoute)
        {
            case "A":
                // N1 + A2~A8
                _allStages = new List<StageData>();
                if (_nStages.Count > 0) _allStages.Add(_nStages[0]); // N1
                _allStages.AddRange(_aStages); // A2~A8
                _isARoute = true;
                break;
            case "G":
                // N1 + A2~A3 + G4~G8
                _allStages = new List<StageData>();
                if (_nStages.Count > 0) _allStages.Add(_nStages[0]); // N1
                if (_aStages.Count > 0) _allStages.Add(_aStages[0]); // A2
                if (_aStages.Count > 1) _allStages.Add(_aStages[1]); // A3
                _allStages.AddRange(_gStages); // G4~G8
                _isARoute = true;
                _isGRoute = true;
                break;
            case "N":
            default:
                _allStages = new List<StageData>(_nStages);
                break;
        }

        _currentIndex = 0;

        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }
        UpdateStageView();
        MovePlayerInstant();
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
        SceneManager.LoadScene(_allStages[_currentIndex].sceneName);
    }

    /// <summary>
    /// �X�e�[�W���ړ����鏈��
    /// </summary>
    /// <param name="newIndex">�ړ���̃X�e�[�W�ԍ�</param>
    /// <param name="direction">�ړ������i-1:��, 1:�E�j</param>
    private void MoveToStage(int newIndex, int direction)
    {
        if (newIndex < 0 || newIndex >= _allStages.Count) return;
        _currentIndex = newIndex;
        _moveDirection = direction;
        UpdateStageView();
        SetPlayerFacing(_moveDirection);
        StartCoroutine(MovePlayerCoroutine());
    }

    /// <summary>
    /// �w�i�摜�����݂̃X�e�[�W�̂��̂ɍX�V
    /// </summary>
    private void UpdateStageView()
    {
        if (_backgroundImage != null && _allStages[_currentIndex].stageImage != null)
            _backgroundImage.sprite = _allStages[_currentIndex].stageImage;
    }

    /// <summary>
    /// �v���C���[�𑦍��Ɍ��݂̃X�e�[�W�ʒu�Ɉړ�
    /// </summary>
    private void MovePlayerInstant()
    {
        if (_player != null && _allStages[_currentIndex].stagePoint != null)
        {
            Vector3 pos = _allStages[_currentIndex].stagePoint.position;
            pos.y += _playerYOffset;
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
        Vector3 end = _allStages[_currentIndex].stagePoint.position;
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

    // �X�e�[�W�N���A���╪��t���O�𓥂񂾎��ɌĂ�
    public void OnStageFlagTriggered(string flag)
    {
        if (flag == "A" && !_isARoute && _currentIndex == 0)
        {
            // N1��A���[�g����
            _isARoute = true;
            // N1 + A2~A8
            var newStages = new List<StageData>();
            if (_nStages.Count > 0) newStages.Add(_nStages[0]); // N1
            newStages.AddRange(_aStages); // A2~A8
            _allStages = newStages;
            _currentIndex = 1; // A2�Ɉړ�
            UpdateStageView();
            MovePlayerInstant();
        }
        else if (flag == "G" && _isARoute && !_isGRoute && _currentIndex == 3)
        {
            // A3��G���[�g����
            _isGRoute = true;
            // N1 + A2~A3 + G4~G8
            var newStages = new List<StageData>();
            if (_nStages.Count > 0) newStages.Add(_nStages[0]); // N1
            if (_aStages.Count > 0) newStages.Add(_aStages[0]); // A2
            if (_aStages.Count > 1) newStages.Add(_aStages[1]); // A3
            newStages.AddRange(_gStages); // G4~G8
            _allStages = newStages;
            _currentIndex = 4; // G4�Ɉړ�
            UpdateStageView();
            MovePlayerInstant();
        }
    }
}
