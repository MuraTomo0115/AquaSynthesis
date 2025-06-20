using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 主制作者：村田智哉
/// </summary>

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject      _menuContents;           // メニュー全体のUIオブジェクト
    [SerializeField] private GameObject      _menuUI;                 // メニューUIオブジェクト
    [SerializeField] private GameObject      _player;                 // プレイヤーオブジェクト
    [SerializeField] private Image           _fadeImage;              // フェード用Image（Inspectorでアサイン）
    [SerializeField] private float           _fadeDuration = 0.5f;    // フェード時間
    private Animation                        _menuAnim;               // メニュー開閉用のAnimationコンポーネント
    private bool                             _isOpen = false;         // メニューが開いているかどうか

    [SerializeField] private RectTransform[] _menuItems;              // メニュー項目（ボタン等）の配列
    [SerializeField] private Animation       _optionAnim;             // オプションのアニメーション
    [SerializeField] private GameObject      _backButton;             // 戻るボタンのGameObject
    private int                              _currentIndex = 0;       // 現在選択中のメニュー項目インデックス
    private bool                             _isInCarousel = true;    // メニュー項目選択中か（true:メニュー項目, false:戻るボタン）
    private Outline                          _backButtonOutline;      // 戻るボタンのOutlineコンポーネント
    private Vector3                          _originalBackButtonScale;// 戻るボタンの元のスケール
    private Tween                            _outlineTween;           // Outline点滅用のDOTweenインスタンス
    private Menu                             _menu;                   // メニュークラスのインスタンス
    private PlayerMovement _playerMovement;                           // プレイヤーの移動クラスのインスタンス

    private const float                      MenuItemSelectedScale = 1.2f;       // 選択中メニュー項目の拡大率
    private const float                      MenuItemTweenDuration = 0.2f;       // メニュー項目の拡大・縮小アニメーション時間
    private const float                      BackButtonTweenDuration = 0.2f;     // 戻るボタンの拡大・縮小アニメーション時間
    private const float                      StartAnimScale = 1.2f;              // 開始時の拡大率
    private const float                      StartAnimDuration = 0.4f;           // 開始時の拡大アニメーション時間
    private const float                      OutlineBlinkDuration = 0.5f;        // Outline点滅の周期
    private static readonly Color            OutlineDefaultColor = Color.yellow; // Outlineのデフォルト色
    private static readonly Color            OutlineBlinkColor = Color.white;    // Outline点滅時の色

    private void Awake()
    {
        _menuAnim = _menuUI.GetComponent<Animation>();
        _backButtonOutline = _backButton.GetComponent<Outline>();
        _backButtonOutline.enabled = false;
        _backButtonOutline.effectColor = OutlineDefaultColor;
        _originalBackButtonScale = _backButton.GetComponent<RectTransform>().localScale;

        // InputActionHolderからMenuInputActionsを取得してイベント登録
        var menuActions = InputActionHolder.Instance.menuInputActions;
        menuActions.Menu.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>().x);
        menuActions.Menu.Vertical.performed += ctx => OnVertical(ctx.ReadValue<Vector2>().y);
        menuActions.Menu.Click.performed += ctx => OnClick();

        if (_fadeImage != null)
        {
            _fadeImage.color = new Color(0, 0, 0, 1); // 念のためAlpha=1
            _fadeImage.DOFade(0f, _fadeDuration).SetUpdate(true);
        }
    }

    /// <summary>
    /// 開始時にメニュー項目の選択状態を初期化
    /// </summary>
    private void Start()
    {
        UpdateSelection();

        // プレイヤーオブジェクトを取得
        _player = GameObject.FindGameObjectWithTag("Player");
        // プレイヤーの移動クラスを取得
        _playerMovement = _player.GetComponent<PlayerMovement>();
        // ステージシーンならMenuInputActionsアクションマップを有効化
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
        InputActionHolder.Instance.menuInputActions.Menu.Open.performed += ctx => ToggleMenu();
    }

    /// <summary>
    /// メニューの開閉状態を切り替え
    /// </summary>
    public void ToggleMenu()
    {
        _menuContents.SetActive(true);

        if (_isOpen)
        {
            CloseMenu();

            // プレイヤーの移動を有効化
            //_playerMovement.OnEnableInput();
            //InputActionHolder.Instance.playerInputActions.Player.Enable();
        }
        else
        {
            // メニューを開く前にプレイヤーの移動を無効化
            //_playerMovement.ChangeInputActions();
            InputActionHolder.Instance.playerInputActions.Player.Disable();
            OpenMenu();
        }
    }

    /// <summary>
    /// メニューを開く処理
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
    /// メニューを閉じる処理
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
    /// アニメーション終了後にTimeScaleを0にする
    /// </summary>
    private IEnumerator WaitForAnimationToEndAndPause()
    {
        yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// アニメーション終了後にTimeScaleを1に戻す
    /// </summary>
    private IEnumerator WaitForAnimationToEndAndResume()
    {
        yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
        _isOpen = false;
        Time.timeScale = 1f;
        InputActionHolder.Instance.playerInputActions.Player.Enable();
    }

    /// <summary>
    /// アニメーション状態をリセット
    /// </summary>
    private void ResetAnimationState()
    {
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;
        stateMenu.speed = 1f;
    }

    /// <summary>
    /// 有効化時に入力アクションを有効にする
    /// </summary>
    private void OnEnable()
    {
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
    }

    /// <summary>
    /// 無効化時に入力アクションを無効にする
    /// </summary>
    private void OnDisable()
    {
        InputActionHolder.Instance.menuInputActions.Menu.Disable();
    }

    /// <summary>
    /// 横方向の入力でメニュー項目を移動
    /// </summary>
    private void OnMove(float direction)
    {
        if (!_isInCarousel) return;

        if (direction > 0) MoveRight();
        else if (direction < 0) MoveLeft();
    }

    /// <summary>
    /// 縦方向の入力でカーソル位置を切り替え
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
    /// 戻るボタンのOutlineを点滅させるアニメーションを開始
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
    /// 戻るボタンのOutline点滅アニメーションを停止し色を元に戻す
    /// </summary>
    private void StopOutlineBlink()
    {
        _outlineTween?.Kill();
        _backButtonOutline.effectColor = OutlineDefaultColor;
    }

    /// <summary>
    /// 決定ボタンが押されたときの処理
    /// </summary>
    private void OnClick()
    {
        if(!_isOpen) return;

        if(_isInCarousel)
        {
            switch (_currentIndex)
            {
                case 0:
                    // 1つ目のメニュー項目の処理
                    Debug.Log("メニュー1の処理");
                    break;
                case 1:
                    // 2つ目のメニュー項目の処理
                    Debug.Log("メニュー2の処理");
                    OnDisable();
                    SelectItem(_optionAnim, "Option");
                    InputActionHolder.Instance.optionInputActions.Option.Enable();
                    break;
                case 2:
                    // 3つ目のメニュー項目の処理
                    StartCoroutine(RetryStageWithFade());
                    break;
                case 3:
                    // 4つ目のメニュー項目の処理
                    Debug.Log("メニュー4の処理");
                    break;
                default:
                    Debug.Log("未定義のメニュー項目");
                    break;
            }
        }
        else
        {
            Debug.Log("ゲームへ戻るボタン押下");
        }
    }

    /// <summary>
    /// 選択されたメニュー項目のアニメーションを再生する
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="animName"></param>
    private void SelectItem(Animation anim,string animName)
    {
        StartCoroutine(PlayAnimationUnscaled(anim, animName));
    }

    /// <summary>
    /// スクロール入力でメニュー項目を移動
    /// </summary>
    private void OnScroll(float scroll)
    {
        if (!_isInCarousel) return;

        if (scroll > 0) MoveLeft();
        else if (scroll < 0) MoveRight();
    }

    /// <summary>
    /// メニュー項目を右に移動（ループあり）
    /// </summary>
    private void MoveRight()
    {
        if (_menuItems.Length == 0) return;
        _currentIndex = (_currentIndex + 1) % _menuItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// メニュー項目を左に移動（ループあり）
    /// </summary>
    private void MoveLeft()
    {
        if (_menuItems.Length == 0) return;
        _currentIndex = (_currentIndex - 1 + _menuItems.Length) % _menuItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// メニュー項目の選択状態に応じて拡大・縮小アニメーションを行う
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
    /// アニメーションを再生する（TimeScaleを無視してUnscaledで再生）
    /// </summary>
    /// <param name="anim">再生するアニメーション</param>
    /// <param name="clipName">アニメーション名</param>
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
        // フェードアウト
        if (_fadeImage != null)
        {
            // TimeScale=0でも動くようSetUpdate(true)を指定
            Tween fadeTween = _fadeImage.DOFade(1f, _fadeDuration).SetUpdate(true);
            yield return fadeTween.WaitForCompletion();
        }

        // TimeScaleを戻しておく（リトライ時に0のままだと止まるため）
        Time.timeScale = 1f;

        // シーン再読み込み
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}