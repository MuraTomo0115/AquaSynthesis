using SQLite4Unity3d;

public class CharacterStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
	public int HP { get; set; } // HP
    public int AttackPower { get; set; } // �U����
    public int Coin { get; set; } // �R�C���̐�
    public int Level { get; set; } // ���x��
    public int? WeaponId { get; set; } // WeaponStatus�ւ̊O���L�[
    public string DamageSE { get; set; } // �_���[�WSE�t�@�C����
}
