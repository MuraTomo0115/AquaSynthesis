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
    private static SQLiteConnection _connection;

    public static SQLiteConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                // 書き込み可能な場所に DB ファイルを作成（なければ）
                string dbPath = Path.Combine(Application.persistentDataPath, "game.db");

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

        if (!columns.Contains("Exp"))
        {
            Connection.Execute("ALTER TABLE CharacterStatus ADD COLUMN Exp INTEGER DEFAULT 0;");
        }
    }

    public static List<CharacterStatus> GetAllCharacters()
    {
        return Connection.Query<CharacterStatus>("SELECT * FROM CharacterStatus");
    }

    public static List<EnemyStatus> GetAllEnemies()
    {
        return Connection.Query<EnemyStatus>("SELECT * FROM EnemyStatus");
    }

    public static List<PistolStatus> GetAllPistols()
    {
        return Connection.Query<PistolStatus>("SELECT * FROM PistolStatus");
    }

    public static void InsertEnemy(EnemyStatus enemy)
    {
        // INSERT～:どのテーブルのどのカラムにデータを入れるか指定 VALUES:実際にどんな値を入れるか指定(?はプレースホルダー)
        Connection.Execute(
            "INSERT INTO EnemyStatus (HP, AttackPower) VALUES (?, ?)",
            enemy.HP, enemy.AttackPower);
    }

    public static void InsertPistol(PistolStatus pistol)
    {
        Connection.Execute(
            "INSERT INTO PistolStatus (AttackPower, DisableTime)",
            pistol.AttackPower, pistol.DisableTime);
    }
}
