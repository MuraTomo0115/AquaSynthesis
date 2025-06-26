using UnityEngine;
using System.Collections; 

public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }
    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    [SerializeField] private GameObject _resultPanel;
    private int _addExp = 0; // �N���A���ɒǉ�����o���l
    private PlayerInputActions _inputActions;
    private bool _waitForAdvEnd = false;
    private bool _isResultOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Disable();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ADV�Đ��J�n
            if (_advManager != null)
            {
                _advManager.StartScenario(_scenarioFileName);
                _waitForAdvEnd = true;
            }
            else
            {
                StageClear();
            }
        }
    }

    /// <summary>
    /// �Q�[���N���A����
    /// ���͉��ŃA�^�b�`���Ă���I�u�W�F�N�g�������̂�
    /// </summary>
    private void StageClear()
    {
        _addExp = ExpManager.Instance.CurrentExp;
        // �����Ńv���C���[�e�[�u����EXP��ǉ����鏈������������
        _addExp = 0;

        // �g���K�[�̓����蔻��𖳌���
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        Time.timeScale = 0f;
        // ���U���g��ʂ�\��
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
            _isResultOpen = true;
            StartCoroutine(HideResultPanelAfterDelay(5f)); // 5�b��Ɏ����Ŕ�\��
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator HideResultPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_resultPanel != null && _isResultOpen)
        {
            _resultPanel.SetActive(false);
            _isResultOpen = false;
        }
        Destroy(gameObject); // �����ō폜
    }

    private void Update()
    {
        // ADV�I���҂���ԂŁAADV���Đ����łȂ��Ȃ����烊�U���g�\��
        if (_waitForAdvEnd && _advManager != null && !_advManager.IsPlaying)
        {
            _waitForAdvEnd = false;
            StageClear();
        }

        // �X�y�[�X�L�[����͕s�v�Ȃ̂ō폜
    }
}
