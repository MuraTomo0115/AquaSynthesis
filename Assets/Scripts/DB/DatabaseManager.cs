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
	private const bool _debugMode = false; // �f�o�b�O���[�h�t���O�i�e�[�u���̍폜���Ɏg�p�j

	private static SQLiteConnection _connection;

	public static SQLiteConnection Connection
	{
		get
		{
			if (_connection == null)
			{
				// StreamingAssets��DB�t�@�C�����쐬
				string dbPath = Path.Combine(Application.streamingAssetsPath, "game.db");

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
        if(_debugMode)
        {
            //ResetCharacterStatusTable();
		}

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

		// Exp�J���������݂��Ȃ��ꍇ�A�ǉ����鏈���̗�
		// �Ⴆ�΁A�o���l��ǉ��������ꍇ�ȂǂɎg�p
		if (!columns.Contains("Exp"))
        {
            //Connection.Execute("ALTER TABLE CharacterStatus ADD COLUMN Exp INTEGER DEFAULT 100;");
		}
    }

    /// CharacterStatus�e�[�u���̑S���R�[�h���擾
    /// </summary>
    public static List<CharacterStatus> GetAllCharacters()
	{
		return Connection.Query<CharacterStatus>("SELECT * FROM CharacterStatus");
	}

	/// <summary>
	/// EnemyStatus�e�[�u���̑S���R�[�h���擾
	/// </summary>
	public static List<EnemyStatus> GetAllEnemies()
	{
		return Connection.Query<EnemyStatus>("SELECT * FROM EnemyStatus");
	}

	/// <summary>
	/// PistolStatus�e�[�u���̑S���R�[�h���擾
	/// </summary>
	public static List<PistolStatus> GetAllPistols()
	{
		return Connection.Query<PistolStatus>("SELECT * FROM PistolStatus");
	}

	/// <summary>
	/// �T�|�[�g�X�e�[�^�X�̑S���R�[�h���擾
	/// </summary>
	/// <returns></returns>
	public static List<SupportStatus> GetAllSupportStatuses()
	{
		return Connection.Query<SupportStatus>("SELECT * FROM SupportStatus");
	}

	/// <summary>
	/// �T�|�[�g�X�e�[�^�X�𖼑O�Ŏ擾
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static SupportStatus GetSupportStatusByName(string name)
	{
		var list = Connection.Query<SupportStatus>("SELECT * FROM SupportStatus WHERE Name = ?", name);
		return list.FirstOrDefault();
	}

	/// <summary>
	/// EnemyStatus�e�[�u���ɐV�����G�f�[�^��}��
	/// </summary>
	/// <param name="enemy">�}������EnemyStatus�I�u�W�F�N�g</param>
	public static void InsertEnemy(EnemyStatus enemy)
	{
		// INSERT�`:�ǂ̃e�[�u���̂ǂ̃J�����Ƀf�[�^�����邩�w�� VALUES:���ۂɂǂ�Ȓl�����邩�w��(?�̓v���[�X�z���_�[)
		Connection.Execute(
			"INSERT INTO EnemyStatus (HP, AttackPower) VALUES (?, ?)",
			enemy.HP, enemy.AttackPower);
	}

	/// <summary>
	/// PistolStatus�e�[�u���ɐV�����s�X�g���f�[�^��}��
	/// </summary>
	/// <param name="pistol">�}������PistolStatus�I�u�W�F�N�g</param>
	public static void InsertPistol(PistolStatus pistol)
	{
		Connection.Execute(
			"INSERT INTO PistolStatus (AttackPower, DisableTime) VALUES (?, ?)",
			pistol.AttackPower, pistol.DisableTime);
	}

	/// <summary>
	/// CharacterStatus�e�[�u���̃v���C���[�X�e�[�^�X���X�V
	/// </summary>
	/// <param name="id">�X�V����v���C���[��ID</param>
	/// <param name="newHP">�V����HP</param>
	/// <param name="newAttackPower">�V�����U����</param>
	/// <param name="newLevel">�V�������x��</param>
	public static void UpdatePlayerStatus(int id, int newHP, int newAttackPower, int newLevel)
	{
		Connection.Execute(
			"UPDATE CharacterStatus SET HP = ?, AttackPower = ?, Level = ? WHERE Id = ?",
			newHP, newAttackPower, newLevel, id);
	}

	/// <summary>
	/// �f�o�b�O�p�Ƀe�[�u�������Z�b�g���鏈��
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
		UnityEngine.Debug.Log("CharacterStatus�e�[�u�������Z�b�g���܂����B�����f�[�^��}�����܂����B");
	}
}
