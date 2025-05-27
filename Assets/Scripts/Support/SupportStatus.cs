using SQLite4Unity3d;

[System.Serializable]
public class SupportStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }                   // ��L�[�i�����̔ԁj

    public string Name { get; set; }              // �T�|�[�g���i��: "Kasumi", "Drone", "Grenade" �Ȃǁj
    public float AvailableTime { get; set; }      // �T�|�[�g���o�����Ă��鎞�ԁi�b�j

    // �����ݗp�X�e�[�^�X
    public float? HealRange { get; set; }         // �񕜔͈́i���a�j
    public float? HealAmount { get; set; }        // 1��̉񕜗�
    public float? HealInterval { get; set; }      // �񕜊Ԋu�i�b�j

    // �h���[���p�X�e�[�^�X
    public float? DroneAttackPower { get; set; }      // �h���[���̍U����
    public float? DroneAttackInterval { get; set; }   // �h���[���̍U���Ԋu�i�b�j
    public int? ItemSpawnCount { get; set; }          // �h���[�����o�����ɔz�u����A�C�e����

    // �O���|���p�X�e�[�^�X
    public float? GrenadePower { get; set; }      // �O���l�[�h�̍U����
    public float? GrenadeInterval { get; set; }   // �O���l�[�h���ˊԊu�i�b�j
    public float? ChargeSpeed { get; set; }       // �ˌ����̈ړ����x
}