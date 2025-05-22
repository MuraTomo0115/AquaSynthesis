using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("何かに触れた: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("ステージクリア！");
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
