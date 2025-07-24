using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ゲームオーバー画面の管理クラス
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [SerializeField] private RectTransform[] _gameOverItems;         // ゲームオーバー項目（ボタンなど）の配列
    [SerializeField] private Image           _fadeImage;             // フェード用Image（Inspectorでアサイン）
    [SerializeField] private float           _fadeDuration = 0.5f;   // フェード時間
    
    private int                              _currentIndex = 0;      // 現在選択中のゲームオーバー項目インデックス
    private bool                             _canInput = true;       // 入力可能かどうか
    private bool                             _isProcessing = false;  // 処理中フラグ
    private Vector3[]                        _originalScales;        // 各項目の元のスケール
    private MenuInputActions                 _menuInputActions;      // 入力アクション

    private const float                      GameOverItemSelectedScale = 1.2f;    // 選択中ゲームオーバー項目の拡大率
    private const float                      GameOverItemTweenDuration = 0.2f;    // ゲームオーバー項目の拡大・縮小アニメーション時間

    private static GameOverManager           _instance;              // シングルトンインスタンス

    private void Awake()
    {
        // 既に他のインスタンスが存在する場合は破棄
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        // 重複チェック
        if (_instance != this)
        {
            return;
        }

        // 配列が空の場合はエラーログを出力して終了
        if (_gameOverItems.Length == 0)
        {
            return;
        }

        // 各項目の元のスケールを保存
        _originalScales = new Vector3[_gameOverItems.Length];
        for (int i = 0; i < _gameOverItems.Length; i++)
        {
            _originalScales[i] = _gameOverItems[i].localScale;
        }

        // 入力アクションを作成
        _menuInputActions = new MenuInputActions();
        
        // イベント登録
        _menuInputActions.Menu.Move.performed += OnMovePerformed;
        _menuInputActions.Menu.Click.performed += OnClickPerformed;

        // 入力アクションを有効化
        _menuInputActions.Menu.Enable();
        
        // 初期選択状態を更新
        UpdateSelection();

        // フェードイン
        if (_fadeImage != null)
        {
            _fadeImage.color = new Color(0, 0, 0, 1); // 黒い画面からAlpha=1
            _fadeImage.DOFade(0f, _fadeDuration).SetUpdate(true);
        }
    }

    /// <summary>
    /// 無効化時に入力アクションを無効にする
    /// </summary>
    private void OnDisable()
    {
        // 入力アクションの無効化と解除
        if (_menuInputActions != null)
        {
            _menuInputActions.Menu.Move.performed -= OnMovePerformed;
            _menuInputActions.Menu.Click.performed -= OnClickPerformed;
            _menuInputActions.Menu.Disable();
        }
        
        // このオブジェクトに関連するTweenをすべて停止
        if (_fadeImage != null)
        {
            _fadeImage.DOKill();
        }
        
        // ゲームオーバー項目のTweenも停止
        if (_gameOverItems != null)
        {
            for (int i = 0; i < _gameOverItems.Length; i++)
            {
                if (_gameOverItems[i] != null)
                {
                    _gameOverItems[i].DOKill();
                }
            }
        }
    }

    /// <summary>
    /// オブジェクト破棄時にTweenを確実に停止
    /// </summary>
    private void OnDestroy()
    {
        // インスタンスリセット
        if (_instance == this)
        {
            _instance = null;
        }

        // 入力アクションの破棄
        if (_menuInputActions != null)
        {
            _menuInputActions.Menu.Move.performed -= OnMovePerformed;
            _menuInputActions.Menu.Click.performed -= OnClickPerformed;
            _menuInputActions.Dispose();
            _menuInputActions = null;
        }
        
        // このオブジェクトに関連するすべてのTweenを停止
        DOTween.Kill(this);
        
        if (_fadeImage != null)
        {
            _fadeImage.DOKill();
        }
        
        if (_gameOverItems != null)
        {
            for (int i = 0; i < _gameOverItems.Length; i++)
            {
                if (_gameOverItems[i] != null)
                {
                    _gameOverItems[i].DOKill();
                }
            }
        }
    }

    /// <summary>
    /// 移動入力の処理
    /// </summary>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!_canInput || _isProcessing || _gameOverItems.Length == 0) return;

        float direction = context.ReadValue<Vector2>().x;

        if (direction > 0) MoveRight();
        else if (direction < 0) MoveLeft();

        AudioManager.Instance.PlaySE("Menu", "5413MenuChoice");
    }

    /// <summary>
    /// クリック入力の処理
    /// </summary>
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (!_canInput || _isProcessing || _gameOverItems.Length == 0) return;
        
        _isProcessing = true;
        AudioManager.Instance.PlaySE("Menu", "5412MenuDecision");

        // 入力アクションを無効化
        if (_menuInputActions != null)
        {
            _menuInputActions.Menu.Disable();
        }

        switch (_currentIndex)
        {
            case 0:
                // RETRY - 前に挑戦していたステージに戻る
                StartCoroutine(RetryStageWithFade());
                break;
            case 1:
                // STAGE SELECT - ステージセレクトシーンを読み込み
                StartCoroutine(LoadStageSelectWithFade());
                break;
            default:
                _isProcessing = false;
                break;
        }
    }

    /// <summary>
    /// ゲームオーバー項目を右に移動（ループあり）
    /// </summary>
    private void MoveRight()
    {
        if (_gameOverItems.Length == 0) return;
        _currentIndex = (_currentIndex + 1) % _gameOverItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// ゲームオーバー項目を左に移動（ループあり）
    /// </summary>
    private void MoveLeft()
    {
        if (_gameOverItems.Length == 0) return;
        _currentIndex = (_currentIndex - 1 + _gameOverItems.Length) % _gameOverItems.Length;
        UpdateSelection();
    }

    /// <summary>
    /// ゲームオーバー項目の選択状態に応じて拡大・縮小アニメーションを行う
    /// </summary>
    private void UpdateSelection()
    {
        for (int i = 0; i < _gameOverItems.Length; i++)
        {
            bool isSelected = (i == _currentIndex);
            Vector3 targetScale = isSelected ? _originalScales[i] * GameOverItemSelectedScale : _originalScales[i];
            
            _gameOverItems[i].DOScale(targetScale, GameOverItemTweenDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    /// <summary>
    /// フェードアウトしてからステージをリトライ
    /// </summary>
    private IEnumerator RetryStageWithFade()
    {
        _canInput = false;

        // フェードアウト
        if (_fadeImage != null)
        {
            Tween fadeTween = _fadeImage.DOFade(1f, _fadeDuration).SetUpdate(true);
            yield return fadeTween.WaitForCompletion();
            
            // Tweenを完全に停止
            fadeTween.Kill();
        }

        // TimeScaleを戻しておく（ゲームオーバー時に0の場合があるため）
        Time.timeScale = 1f;

        // すべてのDOTweenを停止してからシーン遷移
        DOTween.KillAll();

        // 保存されたシーン名を取得してリトライ
        string lastPlayedScene = PlayerPrefs.GetString("LastPlayedScene", "");
        
        if (!string.IsNullOrEmpty(lastPlayedScene))
        {
            // 保存されたシーンをロード
            SceneManager.LoadScene(lastPlayedScene);
        }
        else
        {
            // 保存されたシーンがない場合は現在のシーンを再読み込み
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    /// <summary>
    /// フェードアウトしてからステージセレクトシーンを読み込み
    /// </summary>
    private IEnumerator LoadStageSelectWithFade()
    {
        _canInput = false;

        // フェードアウト
        if (_fadeImage != null)
        {
            Tween fadeTween = _fadeImage.DOFade(1f, _fadeDuration).SetUpdate(true);
            yield return fadeTween.WaitForCompletion();

            // Tweenを完全に停止
            fadeTween.Kill();
        }

        // TimeScaleを戻しておく
        Time.timeScale = 1f;

        // すべてのDOTweenを停止してからシーン遷移
        DOTween.KillAll();

        // ステージセレクトシーンを読み込み
        SceneManager.LoadScene("StageSelect");
    }
}
