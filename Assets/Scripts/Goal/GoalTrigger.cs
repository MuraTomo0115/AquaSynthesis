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
    /// �Q�[���N���A����
    /// ���͉��ŃA�^�b�`���Ă���I�u�W�F�N�g�������̂�
    /// </summary>
    private void StageClear()
    {
        Destroy(gameObject);
    }
}
