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
/// SQLite�̐ڑ��E�������E�e�[�u���쐬���Ǘ�����N���X
/// StreamingAssets �� .db ��u�����A���������őΉ�
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
                // �������݉\�ȏꏊ�� DB �t�@�C�����쐬�i�Ȃ���΁j
                string dbPath = Path.Combine(Application.persistentDataPath, "game.db");

                // SQLite �ڑ��i�ǂݏ����E�V�K�쐬���[�h�j
                _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

                // �e�[�u����������΍쐬
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
    /// �A�v������N�����ȂǂɕK�v�ȃe�[�u�����쐬���鏈��
    /// </summary>
    private static void CreateTables()
    {
        // �G�L�����̃X�e�[�^�X�e�[�u��
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS EnemyStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                HP INTEGER NOT NULL,
                AttackPower INTEGER NOT NULL
            );
        ");

        // �v���C���[�X�e�[�^�X�e�[�u��
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

        // �s�X�g���i����j�e�[�u��
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS PistolStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AttackPower INTEGER NOT NULL,
                DisableTime INTEGER NOT NULL
            );
        ");

        // �����f�[�^�̑}��
        var playerCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM CharacterStatus");
        if(playerCount == 0)
        {
            _connection.Execute(
                "INSERT INTO CharacterStatus (HP, AttackPower) VALUES (?, ?)",
                10, 3, 0); // HP10�A�U����3�A�R�C���O�œo�^
        }
        var pistolCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM PistolStatus");
        if(pistolCount == 0)
        {
            _connection.Execute(
                "INSERT INTO PistolStatus (AttackPower, DisableTime) VALUES (?, ?)",
                2, 6); // �U���͂Q�A�e��������܂ł̎��ԂU�œo�^
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
        // INSERT�`:�ǂ̃e�[�u���̂ǂ̃J�����Ƀf�[�^�����邩�w�� VALUES:���ۂɂǂ�Ȓl�����邩�w��(?�̓v���[�X�z���_�[)
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
