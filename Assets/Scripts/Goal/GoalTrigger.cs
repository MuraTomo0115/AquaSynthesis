using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("何かに触れた: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("ステージクリア！");
            StageClear();
        }
    }

    void StageClear()
    {
        Destroy(gameObject);
    }
}
