using SQLite4Unity3d;

public class DestructibleObjs
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string DestroySE { get; set; }
}
