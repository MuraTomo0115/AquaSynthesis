using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// �吧��ҁF���c�q��
/// </summary>

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject      _menuContents;           // ���j���[�S�̂�UI�I�u�W�F�N�g
    [SerializeField] private GameObject      _menuUI;                 // ���j���[UI�I�u�W�F�N�g
    [SerializeField] private GameObject      _player;                 // �v���C���[�I�u�W�F�N�g
    [SerializeField] private Image           _fadeImage;              // �t�F�[�h�pImage�iInspector�ŃA�T�C���j
    [SerializeField] private float           _fadeDuration = 0.5f;    // �t�F�[�h����
    private Animation                        _menuAnim;               // ���j���[�J�p��Animation�R���|�[�l���g
    private bool                             _isOpen = false;         // ���j���[���J���Ă��邩�ǂ���

    [SerializeField] private RectTransform[] _menuItems;              // ���j���[���ځi�{�^�����j�̔z��
    [SerializeField] private Animation       _optionAnim;             // �I�v�V�����̃A�j���[�V����
    [SerializeField] private GameObject      _backButton;             // �߂�{�^����GameObject
    private int                              _currentIndex = 0;       // ���ݑI�𒆂̃��j���[���ڃC���f�b�N�X
    private bool                             _isInCarousel = true;    // ���j���[���ڑI�𒆂��itrue:���j���[����, false:�߂�{�^���j
    private Outline                          _backButtonOutline;      // �߂�{�^����Outline�R���|�[�l���g
    private Vector3                          _originalBackButtonScale;// �߂�{�^���̌��̃X�P�[��
    private Tween                            _outlineTween;           // Outline�_�ŗp��DOTween�C���X�^���X
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
    private const string MenuOpenAnimName = "MenuOpen";
    private const string MenuCloseAnimName = "MenuClose";
    private bool _isAnimating = false;

    private void Awake()
    {
        _menuAnim = _menuUI.GetComponent<Animation>();
        _backButtonOutline = _backButton.GetComponent<Outline>();
        _backButtonOutline.enabled = false;
        _backButtonOutline.effectColor = OutlineDefaultColor;
        _originalBackButtonScale = _backButton.GetComponent<RectTransform>().localScale;

        // InputActionHolder����MenuInputActions���擾���ăC�x���g�o�^
        var menuActions = InputActionHolder.Instance.menuInputActions;
        menuActions.Menu.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>().x);
        menuActions.Menu.Vertical.performed += ctx => OnVertical(ctx.ReadValue<Vector2>().y);
        menuActions.Menu.Click.performed += ctx => OnClick();

        if (_fadeImage != null)
        {
            _fadeImage.color = new Color(0, 0, 0, 1); // �O�̂���Alpha=1
            _fadeImage.DOFade(0f, _fadeDuration).SetUpdate(true);
        }
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
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
        InputActionHolder.Instance.menuInputActions.Menu.Open.performed += ctx => ToggleMenu();
    }

    /// <summary>
    /// ���j���[�̊J��Ԃ�؂�ւ�
    /// </summary>
    public void ToggleMenu()
    {
        if (_isAnimating) return; // アニメーション中は無効
        _menuContents.SetActive(true);

        if (_isOpen)
        {
            CloseMenu();
        }
        else
        {
            InputActionHolder.Instance.playerInputActions.Player.Disable();
            OpenMenu();
        }
    }

    /// <summary>
    /// ���j���[���J������
    /// </summary>
    private void OpenMenu()
    {
        AudioManager.Instance.PlaySE("Menu", "5411MenuOpen");
        ResetAnimationState(MenuOpenAnimName);
        _menuUI.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Menu);
        _isOpen = true;
        StartCoroutine(PlayMenuAnimationAndPause(MenuOpenAnimName));
    }

    /// <summary>
    /// ���j���[���J����
    /// </summary>
    private void CloseMenu()
    {
        AudioManager.Instance.PlaySE("Menu", "5412MenuClose");
        ResetAnimationState(MenuCloseAnimName);
        StartCoroutine(PlayMenuAnimationAndResume(MenuCloseAnimName));
    }

    private IEnumerator PlayMenuAnimationAndPause(string animName)
    {
        _isAnimating = true;
        InputActionHolder.Instance.menuInputActions.Menu.Open.Disable();
        _menuAnim.Play(animName);
        yield return new WaitForSecondsRealtime(_menuAnim[animName].length);
        Time.timeScale = 0f;
        _isAnimating = false;
        InputActionHolder.Instance.menuInputActions.Menu.Open.Enable();
    }

    /// <summary>
    /// ���j���[���J�����A�j���[�V�����ꂽ
    /// </summary>
    /// <param name="animName">�A�j���[�V����</param>
    /// <returns></returns>
    private IEnumerator PlayMenuAnimationAndResume(string animName)
    {
        _isAnimating = true;
        InputActionHolder.Instance.menuInputActions.Menu.Open.Disable();
        _menuAnim.Play(animName);
        yield return new WaitForSecondsRealtime(_menuAnim[animName].length);
        _isOpen = false;
        Time.timeScale = 1f;
        InputActionHolder.Instance.playerInputActions.Player.Enable();
        _isAnimating = false;
        InputActionHolder.Instance.menuInputActions.Menu.Open.Enable();
    }

    /// <summary>
    /// アニメーションの状態をリセット
    /// </summary>
    /// <param name="animName">リセットするアニメーションの名前</param
    private void ResetAnimationState(string animName)
    {
        if (_menuAnim[animName] != null)
        {
            AnimationState stateMenu = _menuAnim[animName];
            stateMenu.time = 0f;
            stateMenu.speed = 1f;
        }
    }

    /// <summary>
    /// �L�������ɓ��̓A�N�V������L���ɂ���
    /// </summary>
    private void OnEnable()
    {
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
    }

    /// <summary>
    /// ���������ɓ��̓A�N�V�����𖳌��ɂ���
    /// </summary>
    private void OnDisable()
    {
        InputActionHolder.Instance.menuInputActions.Menu.Disable();
    }

    /// <summary>
    /// �������̓��͂Ń��j���[���ڂ��ړ�
    /// </summary>
    private void OnMove(float direction)
    {
        if (!_isInCarousel || !_isOpen) return;

        if (direction > 0) MoveRight();
        else if (direction < 0) MoveLeft();

        AudioManager.Instance.PlaySE("Menu", "5413MenuChoice");
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
            EventSystem.current.SetSelectedGameObject(null);
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
        
        AudioManager.Instance.PlaySE("Menu","5412MenuDecision");

        if(_isInCarousel)
        {
            switch (_currentIndex)
            {
                case 0:
                    // 1�ڂ̃��j���[���ڂ̏���
                    Debug.Log("���j���[1�̏���");
                    break;
                case 1:
                    // 2�ڂ̃��j���[���ڂ̏���
                    Debug.Log("���j���[2�̏���");
                    OnDisable();
                    SelectItem(_optionAnim, "Option");
                    InputActionHolder.Instance.optionInputActions.Option.Enable();
                    break;
                case 2:
                    // 3�ڂ̃��j���[���ڂ̏���
                    StartCoroutine(RetryStageWithFade());
                    break;
                case 3:
                    // 4�ڂ̃��j���[���ڂ̏���
                    Debug.Log("���j���[4�̏���");
                    break;
                default:
                    Debug.Log("����`�̃��j���[����");
                    break;
            }
        }
        else
        {
            Debug.Log("�Q�[���֖߂�{�^������");
        }
    }

    /// <summary>
    /// �I�����ꂽ���j���[���ڂ̃A�j���[�V�������Đ�����
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="animName"></param>
    private void SelectItem(Animation anim,string animName)
    {
        StartCoroutine(PlayAnimationUnscaled(anim, animName));
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

    /// <summary>
    /// �A�j���[�V�������Đ�����iTimeScale�𖳎�����Unscaled�ōĐ��j
    /// </summary>
    /// <param name="anim">�Đ�����A�j���[�V����</param>
    /// <param name="clipName">�A�j���[�V������</param>
    /// <returns></returns>
    private IEnumerator PlayAnimationUnscaled(Animation anim, string clipName)
    {
        anim.Play(clipName);
        AnimationState state = anim[clipName];
        state.speed = 1f;

        while (state.time < state.length)
        {
            anim[clipName].time += Time.unscaledDeltaTime;
            anim.Sample();
            yield return null;
        }
        anim.Stop();
    }
    private IEnumerator RetryStageWithFade()
    {
        // �t�F�[�h�A�E�g
        if (_fadeImage != null)
        {
            // TimeScale=0�ł������悤SetUpdate(true)���w��
            Tween fadeTween = _fadeImage.DOFade(1f, _fadeDuration).SetUpdate(true);
            yield return fadeTween.WaitForCompletion();
        }

        // TimeScale��߂��Ă����i���g���C����0�̂܂܂��Ǝ~�܂邽�߁j
        Time.timeScale = 1f;

        // �V�[���ēǂݍ���
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}