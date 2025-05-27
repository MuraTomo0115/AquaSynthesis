using SQLite4Unity3d;

[System.Serializable]
public class SupportStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }                   // 主キー（自動採番）

    public string Name { get; set; }              // サポート名（例: "Kasumi", "Drone", "Grenade" など）
    public float AvailableTime { get; set; }      // サポートが出現している時間（秒）

    // かすみ用ステータス
    public float? HealRange { get; set; }         // 回復範囲（半径）
    public float? HealAmount { get; set; }        // 1回の回復量
    public float? HealInterval { get; set; }      // 回復間隔（秒）

    // ドローン用ステータス
    public float? DroneAttackPower { get; set; }      // ドローンの攻撃力
    public float? DroneAttackInterval { get; set; }   // ドローンの攻撃間隔（秒）
    public int? ItemSpawnCount { get; set; }          // ドローンが出現時に配置するアイテム数

    // グレポン用ステータス
    public float? GrenadePower { get; set; }      // グレネードの攻撃力
    public float? GrenadeInterval { get; set; }   // グレネード発射間隔（秒）
    public float? ChargeSpeed { get; set; }       // 突撃時の移動速度
}