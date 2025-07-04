using SQLite4Unity3d;

public class Bosses
{
    [PrimaryKey]
    public string name { get; set; } // ステージID（例: "stage1" など）

    public int hp { get; set; } // ボスの体力

    public int attack_power { get; set; } // ボスの攻撃力

    public int exp { get; set; } // 経験値

    public string flag { get; set; } // フラグ名
}
