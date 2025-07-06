using SQLite4Unity3d;

public class Bosses
{
    [PrimaryKey]
    public string name { get; set; } // �X�e�[�WID�i��: "stage1" �Ȃǁj

    public int hp { get; set; } // �{�X�̗̑�

    public int attack_power { get; set; } // �{�X�̍U����

    public int exp { get; set; } // �o���l

    public string flag { get; set; } // �t���O��

    public string idle_voice { get; set; } // ���S�����v
}
