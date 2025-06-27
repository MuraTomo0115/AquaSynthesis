using UnityEngine;
using System.Collections; 
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

/// <summary>
/// ゴール地点のトリガー。ADV再生・リザルト表示・シーン遷移・フェードアウトを制御。
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }

    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private string _nextSceneName; // 遷移先シーン名
    [SerializeField] private CanvasGroup _fadeCanvasGroup; // フェード用
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText; // 「Press Any Key」用

    private int _addExp = 0; // クリア時に取得する経験値
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
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
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
        _addExp = 0;

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
        yield return new WaitForSecondsRealtime(3f); // 3秒リザルト表示

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
        yield return StartCoroutine(FadeOut(1f));

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
            Invoke(nameof(StageClear), 0.25f);

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
}
