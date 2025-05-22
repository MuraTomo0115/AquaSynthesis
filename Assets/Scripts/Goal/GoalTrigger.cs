using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;

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
        Destroy(gameObject);
    }
}
