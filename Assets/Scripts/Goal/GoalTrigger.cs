using UnityEngine;
using System.Collections; 
using UnityEngine.SceneManagement;

/// <summary>
/// �S�[���n�_�̃g���K�[�BADV�Đ��E���U���g�\���E�V�[���J�ځE�t�F�[�h�A�E�g�𐧌�B
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }

    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private string _nextSceneName; // �J�ڐ�V�[����
    [SerializeField] private CanvasGroup _fadeCanvasGroup; // �t�F�[�h�p

    private int _addExp = 0; // �N���A���Ɏ擾����o���l
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
            return;
        }
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    private void OnDestroy()
    {
        _inputActions?.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

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

    /// <summary>
    /// �Q�[���N���A���̏����i���U���g�\���E�����蔻�薳�����E���~�߁E�J�ڃR���[�`���J�n�j
    /// </summary>
    private void StageClear()
    {
        _addExp = ExpManager.Instance.CurrentExp;
        _addExp = 0;

        // �g���K�[�̓����蔻��𖳌���
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // ���U���g�\��
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
            _isResultOpen = true;
            Time.timeScale = 0f;
            StartCoroutine(ResultAndSceneTransition());
        }
    }

    /// <summary>
    /// ���U���g�\�����t�F�[�h�A�E�g���V�[���J��
    /// </summary>
    private IEnumerator ResultAndSceneTransition()
    {
        yield return new WaitForSecondsRealtime(3f); // ���U���g3�b�\��

        // �t�F�[�h�A�E�g�i���U���g��\�������܂܍������Ԃ���j
        yield return StartCoroutine(FadeOut(1f));

        // �t�F�[�h��Ƀ��U���g�p�l�����\��
        if (_resultPanel != null && _isResultOpen)
        {
            _resultPanel.SetActive(false);
            _isResultOpen = false;
        }

        Time.timeScale = 1f; // �V�[���J�ڑO�Ɏ���߂�
        if (!string.IsNullOrEmpty(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
    }

    /// <summary>
    /// �t�F�[�h�A�E�g���o
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        if (_fadeCanvasGroup == null) yield break;
        float time = 0f;
        _fadeCanvasGroup.alpha = 0f;
        _fadeCanvasGroup.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            _fadeCanvasGroup.alpha = Mathf.Clamp01(time / duration);
            yield return null;
        }
        _fadeCanvasGroup.alpha = 1f;
    }

    private void Update()
    {
        // ADV�I���҂���Ԃ�ADV���I������烊�U���g����
        if (_waitForAdvEnd && _advManager != null && !_advManager.IsPlaying)
        {
            _waitForAdvEnd = false;
            Invoke(nameof(StageClear), 0.25f);

            // �O�̂��ߓ����蔻��𖳌���
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
