using UnityEngine;
using System.Collections; 

public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }
    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    [SerializeField] private GameObject _resultPanel;
    private int _addExp = 0; // クリア時に追加する経験値
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
            // ADV再生開始
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
    /// ゲームクリア処理
    /// 今は仮でアタッチしているオブジェクトを消すのみ
    /// </summary>
    private void StageClear()
    {
        _addExp = ExpManager.Instance.CurrentExp;
        // ここでプレイヤーテーブルにEXPを追加する処理を実装する
        _addExp = 0;

        // トリガーの当たり判定を無効化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        Time.timeScale = 0f;
        // リザルト画面を表示
        if (_resultPanel != null)
        {
            _resultPanel.SetActive(true);
            _isResultOpen = true;
            StartCoroutine(HideResultPanelAfterDelay(5f)); // 5秒後に自動で非表示
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
        Destroy(gameObject); // ここで削除
    }

    private void Update()
    {
        // ADV終了待ち状態で、ADVが再生中でなくなったらリザルト表示
        if (_waitForAdvEnd && _advManager != null && !_advManager.IsPlaying)
        {
            _waitForAdvEnd = false;
            StageClear();
        }

        // スペースキー判定は不要なので削除
    }
}
