using SQLite4Unity3d;

public class StageStatus
{
    [PrimaryKey]
    public string stage { get; set; } // �X�e�[�WID�i��: "stage1" �Ȃǁj

    public int is_clear { get; set; } // �N���A�t���O�i0:���N���A, 1:�N���A�j
    public int support1 { get; set; } // �T�|�[�g1�g�p��
    public int support2 { get; set; } // �T�|�[�g2�g�p��
    public int support3 { get; set; } // �T�|�[�g3�g�p��
}
