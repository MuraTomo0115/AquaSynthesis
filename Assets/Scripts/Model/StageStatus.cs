using SQLite4Unity3d;

public class StageStatus
{
    [PrimaryKey]
    public string stage { get; set; } // ステージID（例: "stage1" など）

    public int is_clear { get; set; } // クリアフラグ（0:未クリア, 1:クリア）
    public int support1 { get; set; } // サポート1使用可否
    public int support2 { get; set; } // サポート2使用可否
    public int support3 { get; set; } // サポート3使用可否
}
