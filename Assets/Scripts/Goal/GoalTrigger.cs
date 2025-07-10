using UnityEngine;
using System.Collections; 
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

public class GoalTrigger : MonoBehaviour
{
    public static GoalTrigger Instance { get; private set; }

    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private string _nextSceneName; // 遷移先シーン名
    [SerializeField] private TextMeshProUGUI _getExpName; // 獲得経験値数
    [SerializeField] private CanvasGroup _fadeCanvasGroup; // フェード用
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText; // 「Press Any Key」用
    [SerializeField] private float _resultDelay = 3f;      // リザルト表示時間（秒）
    [SerializeField] private float _fadeDuration = 1f;     // フェードアウト時間（秒）
    [SerializeField] private float _stageClearDelay = 0.25f; // ADV終了後の遅延秒数

    private int _addExp = 0; // クリア時に取得する経験値
    private string _route;
    private PlayerInputActions _inputActions;
    private bool _waitForAdvEnd = false;
    private bool _isResultOpen = false;
    private bool _waitForAnyKey = false;

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

        if (_pressAnyKeyText != null)
            _pressAnyKeyText.gameObject.SetActive(false);
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
    /// ゲームクリア時の処理
    /// </summary>
    private void StageClear()
    {
        _addExp = ExpManager.Instance.CurrentExp;

        _getExpName.text = _addExp.ToString();
        _addExp = 0;
        DatabaseManager.GetExp(1,ExpManager.Instance.GetCurrentExp());

        if(_route != null)
        {
            DatabaseManager.UpdateCurrentRoute(1, _route);
            _route = null;
        }

        // ゴールしたシーン名を取得
        string sceneName = SceneManager.GetActiveScene().name;

        // シーン名を保存
        PlayerPrefs.SetString("LastClearedStage", sceneName);
        PlayerPrefs.Save();

        // 進行状況をクリアに更新
        var status = DatabaseManager.GetStageStatus(sceneName);
        if (status != null)
        {
            DatabaseManager.UpdateStage(sceneName, 1, status.support1, status.support2, status.support3);
        }

        // トリガーの当たり判定を無効化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // リザルト表示
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
            _isResultOpen = true;
            Time.timeScale = 0f;
            StartCoroutine(ResultAndWaitForAnyKey());
        }
    }

    /// <summary>
    /// リザルト表示→「Press Any Key」→キー入力待ち→フェードアウト→シーン遷移
    /// </summary>
    private IEnumerator ResultAndWaitForAnyKey()
    {
        yield return new WaitForSecondsRealtime(_resultDelay); // 3秒リザルト表示

        // 「Press Any Key」表示
        if (_pressAnyKeyText != null)
            _pressAnyKeyText.gameObject.SetActive(true);

        _waitForAnyKey = true;

        // ここではキー入力待ちをUpdateで判定
        while (_waitForAnyKey)
        {
            yield return null;
        }

        // フェードアウト
        yield return StartCoroutine(FadeOut(_fadeDuration));

        // フェード後にリザルトパネルとテキストを非表示
        if (_resultPanel != null && _isResultOpen)
        {
            _resultPanel.SetActive(false);
            _isResultOpen = false;
        }
        if (_pressAnyKeyText != null)
            _pressAnyKeyText.gameObject.SetActive(false);

        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
    }

    /// <summary>
    /// フェードアウト演出
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
        // ADV終了待ち状態でADVが終わったらリザルト処理
        if (_waitForAdvEnd && _advManager != null && !_advManager.IsPlaying)
        {
            _waitForAdvEnd = false;
            Invoke(nameof(StageClear), _stageClearDelay);

            // 念のため当たり判定を無効化
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // 「Press Any Key」待ち
        if (_waitForAnyKey)
        {
            if (Keyboard.current == null) return;

            foreach (KeyControl key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    _waitForAnyKey = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ゴールのルートを設定
    /// </summary>
    /// <param name="routeFlag"></param>
    public void SetRoute(string routeFlag)
    {
        _route = routeFlag;
    }
}
