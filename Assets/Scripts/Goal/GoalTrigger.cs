using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }
    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    private int _addExp = 0; // クリア時に追加する経験値

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
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            _advManager.StartScenario(_scenarioFileName);
            StageClear();
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
        Destroy(gameObject);
    }
}
