using SQLite4Unity3d;

public class EnemyStatus
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string name { get; set; }
    public int hp { get; set; }
    public int attack_power { get; set; }
}
