using SQLite4Unity3d;

public class EnemyStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int HP { get; set; }
    public int AttackPower { get; set; }
}
