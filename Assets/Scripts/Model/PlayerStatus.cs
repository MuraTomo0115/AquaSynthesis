using SQLite4Unity3d;

public class player_status
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string name { get; set; }
	public int hp { get; set; } // HP
    public int attack_power { get; set; } // 攻撃力

    public int level { get; set; } // レベル

    public int current_exp { get; set; } // 現在の経験値

    public int current_route { get; set; }

    public string damage_se { get; set; } // ダメージSEファイル名
}
