using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log("�����ɐG�ꂽ: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("�X�e�[�W�N���A�I");
            StageClear();
        }
    }

    /// <summary>
    /// �Q�[���N���A����
    /// ���͉��ŃA�^�b�`���Ă���I�u�W�F�N�g�������̂�
    /// </summary>
    private void StageClear()
    {
        Destroy(gameObject);
    }
}
