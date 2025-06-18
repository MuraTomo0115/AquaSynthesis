using SQLite4Unity3d;

public class CharacterStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
	public int HP { get; set; } // HP
    public int AttackPower { get; set; } // 攻撃力
    public int Coin { get; set; } // コインの数
    public int Level { get; set; } // レベル
    public int? WeaponId { get; set; } // WeaponStatusへの外部キー
    public string DamageSE { get; set; } // ダメージSEファイル名
}
