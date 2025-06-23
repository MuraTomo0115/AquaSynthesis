using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public GoalTrigger Instance { get; private set; }
    [SerializeField] private ADVManager _advManager;
    [SerializeField] private string _scenarioFileName;
    private int _addExp = 0; // �N���A���ɒǉ�����o���l

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
    /// �Q�[���N���A����
    /// ���͉��ŃA�^�b�`���Ă���I�u�W�F�N�g�������̂�
    /// </summary>
    private void StageClear()
    {
        _addExp = ExpManager.Instance.CurrentExp;
        // �����Ńv���C���[�e�[�u����EXP��ǉ����鏈������������
        _addExp = 0;
        Destroy(gameObject);
    }
}
