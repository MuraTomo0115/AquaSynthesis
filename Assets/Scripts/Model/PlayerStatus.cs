using SQLite4Unity3d;

public class player_status
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string name { get; set; }
	public int hp { get; set; } // HP
    public int attack_power { get; set; } // �U����

    public int level { get; set; } // ���x��

    public int current_exp { get; set; } // ���݂̌o���l

    public int current_route { get; set; }

    public string damage_se { get; set; } // �_���[�WSE�t�@�C����
}
