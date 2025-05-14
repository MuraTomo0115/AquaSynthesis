using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject      _menuContents;           // ���j���[�S�̂�UI�I�u�W�F�N�g
    [SerializeField] private GameObject      _menuUI;                 // ���j���[UI�I�u�W�F�N�g
    [SerializeField] private GameObject      _player;                 // �v���C���[�I�u�W�F�N�g
    private Animation                        _menuAnim;               // ���j���[�J�p��Animation�R���|�[�l���g
    private bool                             _isOpen = false;         // ���j���[���J���Ă��邩�ǂ���

    [SerializeField] private RectTransform[] _menuItems;              // ���j���[���ځi�{�^�����j�̔z��
    [SerializeField] private GameObject      _backButton;             // �߂�{�^����GameObject
    private int                              _currentIndex = 0;       // ���ݑI�𒆂̃��j���[���ڃC���f�b�N�X
    private bool                             _isInCarousel = true;    // ���j���[���ڑI�𒆂��itrue:���j���[����, false:�߂�{�^���j
    private Outline                          _backButtonOutline;      // �߂�{�^����Outline�R���|�[�l���g
    private Vector3                          _originalBackButtonScale;// �߂�{�^���̌��̃X�P�[��
    private Tween                            _outlineTween;           // Outline�_�ŗp��DOTween�C���X�^���X
    private MenuInputActions                 _inputActions;           // ���̓A�N�V�����Ǘ��p
    private Menu                             _menu;                   // ���j���[�N���X�̃C���X�^���X
    private PlayerMovement _playerMovement;                           // �v���C���[�̈ړ��N���X�̃C���X�^���X

    private const float                      MenuItemSelectedScale = 1.2f;       // �I�𒆃��j���[���ڂ̊g�嗦
    private const float                      MenuItemTweenDuration = 0.2f;       // ���j���[���ڂ̊g��E�k���A�j���[�V��������
    private const float                      BackButtonTweenDuration = 0.2f;     // �߂�{�^���̊g��E�k���A�j���[�V��������
    private const float                      StartAnimScale = 1.2f;              // �J�n���̊g�嗦
    private const float                      StartAnimDuration = 0.4f;           // �J�n���̊g��A�j���[�V��������
    private const float                      OutlineBlinkDuration = 0.5f;        // Outline�_�ł̎���
    private static readonly Color            OutlineDefaultColor = Color.yellow; // Outline�̃f�t�H���g�F
    private static readonly Color            OutlineBlinkColor = Color.white;    // Outline�_�Ŏ��̐F

    private void Awake()
    {
        _menuAnim = _menuUI.GetComponent<Animation>();
        _backButtonOutline = _backButton.GetComponent<Outline>();
        _backButtonOutline.enabled = false;
        _backButtonOutline.effectColor = OutlineDefaultColor;
        _originalBackButtonScale = _backButton.GetComponent<RectTransform>().localScale;

        _inputActions = new MenuInputActions();
        _inputActions.Menu.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>().x);
        _inputActions.Menu.Vertical.performed += ctx => OnVertical(ctx.ReadValue<Vector2>().y);
        _inputActions.Menu.Click.performed += ctx => OnClick();
        _inputActions.Menu.Scroll.performed += ctx => OnScroll(ctx.ReadValue<Vector2>().y);
    }

    /// <summary>
    /// �J�n���Ƀ��j���[���ڂ̑I����Ԃ�������
    /// </summary>
    private void Start()
    {
        UpdateSelection();

        // �v���C���[�I�u�W�F�N�g���擾
        _player = GameObject.FindGameObjectWithTag("Player");
        // �v���C���[�̈ړ��N���X���擾
        _playerMovement = _player.GetComponent<PlayerMovement>();
        // �X�e�[�W�V�[���Ȃ�MenuInputActions�A�N�V�����}�b�v��L����
        _inputActions.Menu.Enable();
        _inputActions.Menu.Open.performed += ctx => ToggleMenu();
    }

    /// <summary>
    /// ���j���[�̊J��Ԃ�؂�ւ�
    /// </summary>
    public void ToggleMenu()
    {
        _menuContents.SetActive(true);

        if (_isOpen)
        {
            CloseMenu();

            // �v���C���[�̈ړ���L����
            _playerMovement.OnEnable();
        }
        else
        {
            // ���j���[���J���O�Ƀv���C���[�̈ړ��𖳌���
            _playerMovement.DisableInput();
            OpenMenu();
        }
    }

    /// <summary>
    /// ���j���[���J������
    /// </summary>
    private void OpenMenu()
    {
        ResetAnimationState();

        if (_menuAnim.clip != null)
            _menuAnim.Play();

        _menuUI.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Menu);

        _isOpen = true;
        StartCoroutine(WaitForAnimationToEndAndPause());
    }

    /// <summary>
    /// ���j���[����鏈��
    /// </summary>
    private void CloseMenu()
    {
        _menuAnim.Stop();

        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.speed = -1f;
        stateMenu.time = stateMenu.length;
        _menuAnim.Play(_menuAnim.clip.name);

        StartCoroutine(WaitForAnimationToEndAndResume());
    }

    /// <summary>
    /// �A�j���[�V�����I�����TimeScale��0�ɂ���
    /// </summary>
    private IEnumerator WaitForAnimationToEndAndPause()
    {
        yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// �A�j���[�V�����I�����TimeScale��1�ɖ߂�
    /// </summary>
    private IEnumerator WaitForAnimationToEndAndResume()
    {
        yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
        _isOpen = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// �A�j���[�V������Ԃ����Z�b�g
    /// </summary>
    private void ResetAnimationState()
    {
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;
        stateMenu.speed = 1f;
    }

    /// <summary>
    /// �L�������ɓ��̓A�N�V������L���ɂ���
    /// </summary>
    private void OnEnable()
    {
        _inputActions.Menu.Enable();
    }

    /// <summary>
    /// ���������ɓ��̓A�N�V�����𖳌��ɂ���
    /// </summary>
    private void OnDisable()
    {
        _inputActions.Menu.Disable();
    }

    /// <summary>
    /// �������̓��͂Ń��j���[���ڂ��ړ�
    /// </summary>
    private void OnMove(float direction)
    {
        if (!_isInCarousel) return;

        if (direction > 0) MoveRight();
        else if (direction < 0) MoveLeft();
    }

    /// <summary>
    /// �c�����̓��͂ŃJ�[�\���ʒu��؂�ւ�
    /// </summary>
    private void OnVertical(float direction)
    {
        var rect = _backButton.GetComponent<RectTransform>();

        if (direction < 0 && _isInCarousel)
        {
            _isInCarousel = false;
            _backButton.GetComponent<Button>().Select();

            rect.DOScale(Vector3.one * MenuItemSelectedScale, BackButtonTweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            _backButtonOutline.enabled = true;
            StartOutlineBlink();
            UpdateSelection();
        }
        else if (direction > 0 && !_isInCarousel)
        {
            _isInCarousel = true;

            rect.DOScale(_originalBackButtonScale, BackButtonTweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            UpdateSelection();
            StopOutlineBlink();
            _backButtonOutline.enabled = false;
        }
    }

    /// <summary>
    /// �߂�{�^����Outline��_�ł�����A�j���[�V�������J�n
    /// </summary>
    private void StartOutlineBlink()
    {
        _outlineTween?.Kill();

        _outlineTween = DOTween.To(
            () => _backButtonOutline.effectColor,
            c => _backButtonOutline.effectColor = c,
            OutlineBlinkColor,
            OutlineBlinkDuration
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine)
        .SetUpdate(true);
    }

    /// <summary>
    /// �߂�{�^����Outline�_�ŃA�j���[�V�������~���F�����ɖ߂�
    /// </summary>
    private void StopOutlineBlink()
    {
        _outlineTween?.Kill();
        _backButtonOutline.effectColor = OutlineDefaultColor;
    }

    /// <summary>
    /// ����{�^���������ꂽ�Ƃ��̏���
    /// </summary>
    private void OnClick()
    {
        if(!_isOpen) return;

        if (_isInCarousel)
        {
            Debug.Log("�I���F" + _menuItems[_currentIndex].name);
        }
        else
        {
            Debug.Log("�Q�[���֖߂�{�^������");
        }
    }

    /// <summary>
    /// �X�N���[�����͂Ń��j���[���ڂ��ړ�
    /// </summary>
    private void OnScroll(float scroll)
    {
        if (!_isInCarousel) return;

        if (scroll > 0) MoveLeft();
        else if (scroll < 0) MoveRight();
    }

    /// <summary>
    /// ���j���[���ڂ��E�Ɉړ��i���[�v����j
    /// </summary>
    private void MoveRight()
    {
        if (_menuItems.Length == 0) return;
        _currentIndex = (_currentIndex + 1) % _menuItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// ���j���[���ڂ����Ɉړ��i���[�v����j
    /// </summary>
    private void MoveLeft()
    {
        if (_menuItems.Length == 0) return;
        _currentIndex = (_currentIndex - 1 + _menuItems.Length) % _menuItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// ���j���[���ڂ̑I����Ԃɉ����Ċg��E�k���A�j���[�V�������s��
    /// </summary>
    private void UpdateSelection()
    {
        for (int i = 0; i < _menuItems.Length; i++)
        {
            Vector3 targetScale = (_isInCarousel && i == _currentIndex) ? Vector3.one * MenuItemSelectedScale : Vector3.one;
            _menuItems[i].DOScale(targetScale, MenuItemTweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }
}