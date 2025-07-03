using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.MemoryProfiler;

// 主製作者：村田智哉

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
            //Resetplayer_statusTable();
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

        //_connection.Execute("PRAGMA foreign_keys = ON;");
        //// プレイヤーステータステーブル
        Connection.Execute(@"
        	CREATE TABLE IF NOT EXISTS player_status (
        		id INTEGER PRIMARY KEY AUTOINCREMENT,
        		name TEXT NOT NULL DEFAULT 'Shizuku',
        		hp INTEGER NOT NULL DEFAULT 5,
        		attack_power INTEGER NOT NULL DEFAULT 3,
        		level INTEGER NOT NULL DEFAULT 1,
        		current_exp INTEGER NOT NULL DEFAULT 0,
        		current_route TEXT NOT NULL DEFAULT 'A',
        		damage_se TEXT NOT NULL DEFAULT '545DamageSE',
        		FOREIGN KEY (current_route) REFERENCES route_flags(flag_name)
        	);
        ");

        Connection.Execute(@"
			CREATE TABLE IF NOT EXISTS route_flags (
				id INTEGER PRIMARY KEY AUTOINCREMENT,
				flag_name TEXT UNIQUE
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
        var playerCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM player_status");
        if(playerCount == 0)
        {
            _connection.Execute(
                "INSERT INTO player_status (HP, attack_power) VALUES (?, ?)",
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
		var columns = Connection.Query<TableInfo>("PRAGMA table_info(player_status);")
								.Select(c => c.name)
								.ToList();

		// Expカラムが存在しない場合、追加する処理の例
		// 例えば、経験値を追加したい場合などに使用
		if (!columns.Contains("Exp"))
		{
			//Connection.Execute("ALTER TABLE player_status ADD COLUMN Exp INTEGER DEFAULT 100;");
		}
    }

    /// player_statusテーブルの全レコードを取得
    /// </summary>
    public static List<player_status> GetAllCharacters()
	{
		return Connection.Query<player_status>("SELECT * FROM player_status");
	}

	/// <summary>
	/// EnemyStatusテーブルの全レコードを取得
	/// </summary>
	public static List<EnemyStatus> GetAllEnemies()
	{
		return Connection.Query<EnemyStatus>("SELECT * FROM EnemyStatus");
	}

	/// <summary>
	/// EnemyStatusテーブルの全レコードを取得
	/// </summary>
	public static List<DestructibleObjs> GetAllDestructibleObjs()
	{
		return Connection.Query<DestructibleObjs>("SELECT * FROM DestructibleObjs");
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
    /// player_statusテーブルの現在のルートをidで取得
    /// </summary>
    /// <param name="id"></param>
    /// <returns>現在のルート</returns>
    public static string GetCurrentRouteById(int id)
    {
        return Connection.ExecuteScalar<string>("SELECT current_route FROM player_status WHERE id = ?", id);
    }

    /// <summary>
    /// 指定したプレイヤーIDのルートを変更する
    /// </summary>
    /// <param name="id">player_statusテーブルのid</param>
    /// <param name="route">新しいルート名（例: "N", "A", "G"）</param>
    public static void UpdateCurrentRoute(int id, string route)
    {
        Connection.Execute("UPDATE player_status SET current_route = ? WHERE id = ?", route, id);
		Debug.Log($"プレイヤーID {id} のルートを '{route}' に更新しました。");
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
    /// StageStatusテーブルに新しいステージデータを挿入
    /// </summary>
    public static void InsertStage(string stageName,int is_clear,int support1,int support2,int support3)
	{
		Connection.Execute(
			"INSERT INTO stage_status (stage,is_clear,support1,support2,support3) VALUES (?, ?, ?, ?, ?)",
			stageName,is_clear, support1, support2,support3);
	}

    /// <summary>
    /// ステージクリア関数
    /// </summary>
    public static void ClearStage(string stageName,int is_clear)
	{
		Connection.Execute(
            "UPDATE stage_status SET is_clear = ? WHERE stage = ?",
            is_clear, stageName);
	}

    /// <summary>
    /// 指定したステージの進行状況を更新する
    /// </summary>
    /// <param name="stageName">ステージ名</param>
    /// <param name="is_clear">クリア状態</param>
    /// <param name="support1">サポート1</param>
    /// <param name="support2">サポート2</param>
    /// <param name="support3">サポート3</param>
    public static void UpdateStage(string stageName, int is_clear, int support1, int support2, int support3)
    {
        Connection.Execute(
            "UPDATE stage_status SET is_clear = ?, support1 = ?, support2 = ?, support3 = ? WHERE stage = ?",
            is_clear, support1, support2, support3, stageName);
    }

    /// <summary>
    /// player_statusテーブルのプレイヤーステータスを更新
    /// </summary>
    /// <param name="id">更新するプレイヤーのID</param>
    /// <param name="newHP">新しいHP</param>
    /// <param name="newAttackPower">新しい攻撃力</param>
    /// <param name="newLevel">新しいレベル</param>
    public static void UpdatePlayerStatus(int id, int newHP, int newAttackPower, int newLevel)
	{
		Connection.Execute(
			"UPDATE player_status SET HP = ?, attack_power = ?, Level = ? WHERE Id = ?",
			newHP, newAttackPower, newLevel, id);
	}

    /// <summary>
    /// 指定したステージIDの進行状況（クリア・サポート使用可否など）をstage_statusテーブルから取得
    /// </summary>
    /// <param name="stage">ステージID（例: "stage1"）</param>
    /// <returns>該当ステージのStageStatusオブジェクト。存在しない場合はnull。</returns>
    public static StageStatus GetStageStatus(string stage)
    {
        return Connection.Query<StageStatus>(
            "SELECT * FROM stage_status WHERE stage = ?", stage
        ).FirstOrDefault();
    }

    /// <summary>
    /// デバッグ用にテーブルをリセットする処理
    /// </summary>
    private static void Resetplayer_statusTable()
	{
		Connection.Execute("DROP TABLE IF EXISTS player_status;");
		Connection.Execute("DROP TABLE IF EXISTS EnemyStatus;");

		Connection.Execute(@"
        CREATE TABLE player_status (
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
			"INSERT INTO player_status (HP, AttackPower, Coin, Level, WeaponId) VALUES (?, ?, ?, ?, ?);",
			10, 3, 0, 1, null);

		Connection.Execute(
			"INSERT INTO EnemyStatus (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy1", 10, 2);

		Connection.Execute(
			"INSERT INTO EnemyStatus (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy2", 25, 5);

		Connection.Execute("DROP TABLE IF EXISTS SupportStatus;");
		UnityEngine.Debug.Log("player_statusテーブルをリセットしました。初期データを挿入しました。");
	}
}
