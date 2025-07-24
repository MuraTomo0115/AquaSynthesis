using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class StageData
{
    public string stageName;            // �\����
    public string sceneName;            // �V�[����
    public Sprite stageImage;           // �w�i�摜
    public Transform stagePoint;        // �X�e�[�W�ʒu
    public GameObject pathObject;       // Inspector�ŃA�^�b�`
    public string animationTriggerName; // �g���K�[���iTrigger���g���ꍇ�̂݁j
}

public class StageSelector : MonoBehaviour
{
    [Header("�v���C���[�I�u�W�F�N�g")]
    [SerializeField] private GameObject _playerObject;  // �v���C���[��GameObject

    [Header("�w�i�摜UI")]
    [SerializeField] private Image _backgroundImage;    // �w�i�摜��\������UI

    [Header("�ړ����x")]
    [SerializeField] private float _moveSpeed;          // �X�e�[�W�Ԃ̈ړ��ɂ�����b��

    [SerializeField] private float _playerYOffset;      // �v���C���[��Y���I�t�Z�b�g

    private List<StageData> _allStages;                 // �i�s���őS�X�e�[�W��o�^

    // N/A/G���[�g�̑S���
    [Header("N���[�g�X�e�[�W���X�g (N1~N8)")]
    [SerializeField] private List<StageData> _nStages;
    [Header("A���[�g�X�e�[�W���X�g (A2~A8)")]
    [SerializeField] private List<StageData> _aStages;
    [Header("G���[�g�X�e�[�W���X�g (G4~G8)")]
    [SerializeField] private List<StageData> _gStages;

    private int _currentIndex = 0;      // ���ݑI�𒆂̃X�e�[�W�ԍ�
    private bool _isMoving = false;     // �v���C���[���ړ������ǂ���
    private int _moveDirection = 1;     // -1:��, 1:�E�i�ړ������j

    // ���[�g����t���O
    private bool _isARoute = false;
    private bool _isGRoute = false;
    private string _currentRoute;

    private Transform _player; // �v���C���[��Transform
    private Animator _playerAnimator; // �v���C���[��Animator

    // === �ǉ�: ���I�u�W�F�N�g�̎Q�� ===
    [Header("���I�u�W�F�N�g�i�X�e�[�W�Ԃ��ƂɃZ�b�g�j")]
    [SerializeField] private List<GameObject> _pathObjects;

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
        Debug.Log("���݂̃��[�g: " + _currentRoute);

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


        // �N���A�����V�[�������擾���A_currentIndex���X�V
        string lastClearedStage = PlayerPrefs.GetString("LastClearedStage", "");
        Debug.Log($"[StageSelector] lastClearedStage: {lastClearedStage}");

        if (!string.IsNullOrEmpty(lastClearedStage))
        {
            int stageIndex = _allStages.FindIndex(s => s.sceneName == lastClearedStage);
            Debug.Log($"[StageSelector] stageIndex: {stageIndex}");

            if (stageIndex >= 0)
            {
                _currentIndex = stageIndex;
            }

            // ���O�N���A�X�e�[�W�̏��N���A����Ɖ��o
            var status = DatabaseManager.GetStageStatus(lastClearedStage);
            Debug.Log($"[StageSelector] status: {(status != null ? $"is_clear={status.is_clear}" : "null")}");

            if (status != null && status.is_clear == 1 && !PlayerPrefs.HasKey("PathAnimationPlayed_" + lastClearedStage))
            {
                var stageData = _allStages[stageIndex];
                Debug.Log($"[StageSelector] pathObject: {(stageData.pathObject != null ? stageData.pathObject.name : "null")}, trigger: {stageData.animationTriggerName}");
                if (stageData.pathObject != null)
                {
                    PlayPathAnimation(stageData.pathObject, stageData.animationTriggerName);
                }
                PlayerPrefs.SetInt("PathAnimationPlayed_" + lastClearedStage, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("[StageSelector] PlayPathAnimation�̏����𖞂����Ă��܂���");
            }
        }
        else
        {
            Debug.Log("[StageSelector] lastClearedStage����ł�");
        }
        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }
        UpdateStageView();
        MovePlayerInstant();


        int pathCount = _pathObjects.Count;
        int stageCount = _allStages.Count - 1; // i+1�ŃA�N�Z�X���邽��-1
        int loopCount = Mathf.Min(pathCount, stageCount);

        // === �ǉ�: ���łɉ���ς݂̓��͏펞�\�� ===
        for (int i = 0; i < loopCount; i++)
        {
            // i�Ԗڂ̓��́Ai+1�Ԗڂ̃X�e�[�W���N���A�ς݂Ȃ�펞�\��
            var status = DatabaseManager.GetStageStatus(_allStages[i + 1].sceneName);
            if (status != null && status.is_clear == 1)
            {
                if (_pathObjects[i] != null)
                {
                    //_pathObjects[i].SetActive(true);
                    // Animator�ŉ��o�ς݂Ȃ�X�P�[���̓A�j���[�V�����ɔC����
                }
            }
        }
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
        // 0�Ԗځi�ŏ��̃X�e�[�W�j�͏�ɑI���\
        if (newIndex < 0 || newIndex >= _allStages.Count) return;
        if (newIndex > 0)
        {
            // ���O�̃X�e�[�W�����擾
            string prevStageName = _allStages[newIndex - 1].stageName;
            var prevStatus = DatabaseManager.GetStageStatus(prevStageName);
            // ���O�̃X�e�[�W�����N���A�Ȃ�i�߂Ȃ�
            if (prevStatus == null || prevStatus.is_clear == 0)
            {
                Debug.Log("�O�̃X�e�[�W���N���A���Ă��܂���B");
                return;
            }
        }
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
    /// ���̃A�j���[�V�������Đ�
    /// </summary>
    private void PlayPathAnimation(GameObject pathObject, string triggerName)
    {
        Debug.Log($"[PlayPathAnimation] �Ăяo��: pathObject={(pathObject != null ? pathObject.name : "null")}, triggerName={triggerName}");

        if (pathObject == null)
        {
            Debug.LogWarning("[PlayPathAnimation] pathObject��null�ł�");
            return;
        }
        var animator = pathObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[PlayPathAnimation] {pathObject.name} ��Animator���A�^�b�`����Ă��܂���");
            return;
        }
        Debug.Log($"[PlayPathAnimation] Animator�擾�����BTrigger��: {triggerName}");

        pathObject.SetActive(true);
        animator.ResetTrigger(triggerName); // �O�̂��߃��Z�b�g
        animator.SetTrigger(triggerName);
        Debug.Log($"[PlayPathAnimation] Trigger {triggerName} ���Z�b�g���܂���");
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
