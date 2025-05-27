using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

public class TableInfo
{
    public int cid { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public int notnull { get; set; }
    public string dflt_value { get; set; }
    public int pk { get; set; }
}

/// <summary>
/// SQLiteの接続・初期化・テーブル作成を管理するクラス
/// StreamingAssets に .db を置かず、自動生成で対応
/// </summary>
public class DatabaseManager
{
	private const bool _debugMode = false; // デバッグモードフラグ（テーブルの削除等に使用）

	private static SQLiteConnection _connection;

	public static SQLiteConnection Connection
	{
		get
		{
			if (_connection == null)
			{
				// StreamingAssetsにDBファイルを作成
				string dbPath = Path.Combine(Application.streamingAssetsPath, "game.db");

				// SQLite 接続（読み書き・新規作成モード）
				_connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

				// テーブルが無ければ作成
				CreateTables();
			}

			return _connection;
		}
	}

	public static void Initialize()
    {
        if(_debugMode)
        {
            //ResetCharacterStatusTable();
		}

        Migrate();
    }

    /// <summary>
    /// アプリ初回起動時などに必要なテーブルを作成する処理
    /// </summary>
    private static void CreateTables()
    {
        // 敵キャラのステータステーブル
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS EnemyStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HP INTEGER NOT NULL,
                AttackPower INTEGER NOT NULL
            );
        ");

        // プレイヤーステータステーブル
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS CharacterStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HP INTEGER NOT NULL,
                AttackPower INTEGER NOT NULL,
                Coin INTEGER NOT NULL DEFAULT 0,
                Level INTEGER NOT NULL DEFAULT 1,
                WeaponId INTEGER
            );
        ");

        // ピストル（武器）テーブル
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS PistolStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AttackPower INTEGER NOT NULL,
                DisableTime INTEGER NOT NULL
            );
        ");

        // 初期データの挿入
        var playerCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM CharacterStatus");
        if(playerCount == 0)
        {
            _connection.Execute(
                "INSERT INTO CharacterStatus (HP, AttackPower) VALUES (?, ?)",
                10, 3, 0); // HP10、攻撃力3、コイン０で登録
        }
        var pistolCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM PistolStatus");
        if(pistolCount == 0)
        {
            _connection.Execute(
                "INSERT INTO PistolStatus (AttackPower, DisableTime) VALUES (?, ?)",
                2, 6); // 攻撃力２、弾が消えるまでの時間６で登録
        }
    }

    public static void Migrate()
    {
        var columns = Connection.Query<TableInfo>("PRAGMA table_info(CharacterStatus);")
                                .Select(c => c.name)
                                .ToList();

		// Expカラムが存在しない場合、追加する処理の例
		// 例えば、経験値を追加したい場合などに使用
		if (!columns.Contains("Exp"))
        {
            //Connection.Execute("ALTER TABLE CharacterStatus ADD COLUMN Exp INTEGER DEFAULT 100;");
		}
    }

    /// CharacterStatusテーブルの全レコードを取得
    /// </summary>
    public static List<CharacterStatus> GetAllCharacters()
	{
		return Connection.Query<CharacterStatus>("SELECT * FROM CharacterStatus");
	}

	/// <summary>
	/// EnemyStatusテーブルの全レコードを取得
	/// </summary>
	public static List<EnemyStatus> GetAllEnemies()
	{
		return Connection.Query<EnemyStatus>("SELECT * FROM EnemyStatus");
	}

	/// <summary>
	/// PistolStatusテーブルの全レコードを取得
	/// </summary>
	public static List<PistolStatus> GetAllPistols()
	{
		return Connection.Query<PistolStatus>("SELECT * FROM PistolStatus");
	}

	/// <summary>
	/// サポートステータスの全レコードを取得
	/// </summary>
	/// <returns></returns>
	public static List<SupportStatus> GetAllSupportStatuses()
	{
		return Connection.Query<SupportStatus>("SELECT * FROM SupportStatus");
	}

	/// <summary>
	/// サポートステータスを名前で取得
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static SupportStatus GetSupportStatusByName(string name)
	{
		var list = Connection.Query<SupportStatus>("SELECT * FROM SupportStatus WHERE Name = ?", name);
		return list.FirstOrDefault();
	}

	/// <summary>
	/// EnemyStatusテーブルに新しい敵データを挿入
	/// </summary>
	/// <param name="enemy">挿入するEnemyStatusオブジェクト</param>
	public static void InsertEnemy(EnemyStatus enemy)
	{
		// INSERT～:どのテーブルのどのカラムにデータを入れるか指定 VALUES:実際にどんな値を入れるか指定(?はプレースホルダー)
		Connection.Execute(
			"INSERT INTO EnemyStatus (HP, AttackPower) VALUES (?, ?)",
			enemy.HP, enemy.AttackPower);
	}

	/// <summary>
	/// PistolStatusテーブルに新しいピストルデータを挿入
	/// </summary>
	/// <param name="pistol">挿入するPistolStatusオブジェクト</param>
	public static void InsertPistol(PistolStatus pistol)
	{
		Connection.Execute(
			"INSERT INTO PistolStatus (AttackPower, DisableTime) VALUES (?, ?)",
			pistol.AttackPower, pistol.DisableTime);
	}

	/// <summary>
	/// CharacterStatusテーブルのプレイヤーステータスを更新
	/// </summary>
	/// <param name="id">更新するプレイヤーのID</param>
	/// <param name="newHP">新しいHP</param>
	/// <param name="newAttackPower">新しい攻撃力</param>
	/// <param name="newLevel">新しいレベル</param>
	public static void UpdatePlayerStatus(int id, int newHP, int newAttackPower, int newLevel)
	{
		Connection.Execute(
			"UPDATE CharacterStatus SET HP = ?, AttackPower = ?, Level = ? WHERE Id = ?",
			newHP, newAttackPower, newLevel, id);
	}

	/// <summary>
	/// デバッグ用にテーブルをリセットする処理
	/// </summary>
	private static void ResetCharacterStatusTable()
	{
		Connection.Execute("DROP TABLE IF EXISTS CharacterStatus;");
		Connection.Execute("DROP TABLE IF EXISTS EnemyStatus;");

		Connection.Execute(@"
        CREATE TABLE CharacterStatus (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL DEFAULT 'Shizuku',
            HP INTEGER NOT NULL,
            AttackPower INTEGER NOT NULL,
            Coin INTEGER NOT NULL DEFAULT 0,
            Level INTEGER NOT NULL DEFAULT 1,
            WeaponId INTEGER
        );

    ");
		Connection.Execute(@"
		CREATE TABLE EnemyStatus(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            HP INTEGER NOT NULL,
            AttackPower INTEGER NOT NULL
        );
    ");

		Connection.Execute(
			"INSERT INTO CharacterStatus (HP, AttackPower, Coin, Level, WeaponId) VALUES (?, ?, ?, ?, ?);",
			10, 3, 0, 1, null);

		Connection.Execute(
			"INSERT INTO EnemyStatus (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy1", 10, 2);

		Connection.Execute(
			"INSERT INTO EnemyStatus (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy2", 25, 5);

		Connection.Execute("DROP TABLE IF EXISTS SupportStatus;");
		UnityEngine.Debug.Log("CharacterStatusテーブルをリセットしました。初期データを挿入しました。");
	}
}
