using SQLite4Unity3d;

public class PistolStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int AttackPower { get; set; }
    public int DisableTime { get; set; } // �e��������܂ł̎���
}
