using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("�����ɐG�ꂽ: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("�X�e�[�W�N���A�I");
            StageClear();
        }
    }

    void StageClear()
    {
        Destroy(gameObject);
    }
}
