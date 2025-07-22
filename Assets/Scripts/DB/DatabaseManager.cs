using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

// �吻��ҁF���c�q��

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
				// ビルド後でも書き込み可能なpersistentDataPathを使用
				string dbPath = GetDatabasePath();

				// SQLite接続（読み書き・新規作成モード）
				_connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

				// テーブルが存在しなければ作成
				CreateTables();
			}

			return _connection;
		}
	}

	/// <summary>
	/// ビルド後のexeでも書き込み可能なデータベースパスを取得
	/// StreamingAssetsからpersistentDataPathにコピーして使用
	/// </summary>
	/// <returns>データベースファイルのパス</returns>
	private static string GetDatabasePath()
	{
		string fileName = "game.db";
		string persistentPath = Path.Combine(Application.persistentDataPath, fileName);
		
		// persistentDataPathにデータベースが存在しない場合
		if (!File.Exists(persistentPath))
		{
			string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
			
			// StreamingAssetsにデータベースが存在する場合はコピー
			if (File.Exists(streamingPath))
			{
				File.Copy(streamingPath, persistentPath);
			}
		}
		
		return persistentPath;
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
    /// �A�v������N�����ȂǂɕK�v�ȃe�[�u�����쐬���鏈��
    /// </summary>
    private static void CreateTables()
    {
        // �G�L�����̃X�e�[�^�X�e�[�u��
        Connection.Execute(@"
            CREATE TABLE IF NOT EXISTS enemy_status (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                hp INTEGER NOT NULL,
                attack_power INTEGER NOT NULL
            );
        ");

        //_connection.Execute("PRAGMA foreign_keys = ON;");
        //// �v���C���[�X�e�[�^�X�e�[�u��
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

        // �s�X�g���i����j�e�[�u��
        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS PistolStatus (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AttackPower INTEGER NOT NULL,
                DisableTime INTEGER NOT NULL
            );
        ");

        // �{�X�e�[�u��
        Connection.Execute(@"
            CREATE TABLE IF NOT EXISTS bosses (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                hp INTEGER NOT NULL,
                attack_power INTEGER NOT NULL,
                exp INTEGER NOT NULL DEFAULT 0,
                flag TEXT,
        		FOREIGN KEY (flag) REFERENCES route_flags(flag_name)
            );
        ");

        // �����f�[�^�̑}��
        var playerCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM player_status");
        if(playerCount == 0)
        {
            _connection.Execute(
                "INSERT INTO player_status (HP, attack_power) VALUES (?, ?)",
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
        var columns = Connection.Query<TableInfo>("PRAGMA table_info(player_status);")
                                .Select(c => c.name)
                                .ToList();

        // Exp�J���������݂��Ȃ��ꍇ�A�ǉ����鏈���̗�
        // �Ⴆ�΁A�o���l��ǉ��������ꍇ�ȂǂɎg�p
        if (!columns.Contains("Exp"))
        {
            //Connection.Execute("ALTER TABLE player_status ADD COLUMN Exp INTEGER DEFAULT 100;");
        }
    }

    /// player_status�e�[�u���̑S���R�[�h���擾
    /// </summary>
    public static List<player_status> GetAllCharacters()
    {
        return Connection.Query<player_status>("SELECT * FROM player_status");
    }

	/// <summary>
	/// enemy_status�e�[�u���̑S���R�[�h���擾
	/// </summary>
	public static List<EnemyStatus> GetAllEnemies()
	{
		return Connection.Query<EnemyStatus>("SELECT * FROM enemy_status");
	}

	/// <summary>
	/// enemy_status�e�[�u���̑S���R�[�h���擾
	/// </summary>
	public static List<DestructibleObjs> GetAllDestructibleObjs()
	{
		return Connection.Query<DestructibleObjs>("SELECT * FROM DestructibleObjs");
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
    /// player_status�e�[�u���̌��݂̃��[�g��id�Ŏ擾
    /// </summary>
    /// <param name="id"></param>
    /// <returns>���݂̃��[�g</returns>
    public static string GetCurrentRouteById(int id)
    {
        return Connection.ExecuteScalar<string>("SELECT current_route FROM player_status WHERE id = ?", id);
    }

	/// <summary>
	/// ボス名で単一のボスデータを取得
	/// </summary>
	/// <param name="name">ボス名</param>
	/// <returns>ボスデータ（見つからない場合はnull）</returns>
	public static Bosses GetBossByName(string name)
	{
		var list = Connection.Query<Bosses>("SELECT * FROM bosses WHERE name = ?", name);
		return list.FirstOrDefault();
	}

    /// <summary>
    /// �w�肵���v���C���[ID�̃��[�g��ύX����
    /// </summary>
    /// <param name="id">player_status�e�[�u����id</param>
    /// <param name="route">�V�������[�g���i��: "N", "A", "G"�j</param>
    public static void UpdateCurrentRoute(int id, string route)
    {
        Connection.Execute("UPDATE player_status SET current_route = ? WHERE id = ?", route, id);
    }

    /// <summary>
    /// enemy_status�e�[�u���ɐV�����G�f�[�^��}��
    /// </summary>
    /// <param name="enemy">�}������enemy_status�I�u�W�F�N�g</param>
    public static void InsertEnemy(string name, int hp, int attackPower)
	{
		// INSERT�`:�ǂ̃e�[�u���̂ǂ̃J�����Ƀf�[�^�����邩�w�� VALUES:���ۂɂǂ�Ȓl�����邩�w��(?�̓v���[�X�z���_�[)
		Connection.Execute(
			"INSERT INTO enemy_status (name, hp, attack_power) VALUES (?, ?, ?)",
			name, hp, attackPower);
	}

    /// <summary>
    /// Bosses�e�[�u���ɐV�����{�X�f�[�^��}��
    /// </summary>
    /// <param name="name">����</param>
    /// <param name="hp">�̗�</param>
    /// <param name="attackPower">�U����</param>
    /// <param name="exp">�o���l</param>
    /// <param name="flag">�t���O���iNULL�j</param>
    public static void InsertBoss(string name, int hp, int attackPower, int exp, string flag)
    {
        Connection.Execute(
            "INSERT INTO bosses (name, hp, attack_power, exp, flag) VALUES (?, ?, ?, ?, ?)",
            name, hp, attackPower, exp, flag);
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
    /// StageStatus�e�[�u���ɐV�����X�e�[�W�f�[�^��}��
    /// </summary>
    public static void InsertStage(string stageName,int is_clear,int support1,int support2,int support3)
	{
		Connection.Execute(
			"INSERT INTO stage_status (stage,is_clear,support1,support2,support3) VALUES (?, ?, ?, ?, ?)",
			stageName,is_clear, support1, support2,support3);
	}

    /// <summary>
    /// �X�e�[�W�N���A�֐�
    /// </summary>
    public static void ClearStage(string stageName,int is_clear)
	{
		Connection.Execute(
            "UPDATE stage_status SET is_clear = ? WHERE stage = ?",
            is_clear, stageName);
	}

    /// <summary>
    /// �w�肵���X�e�[�W�̐i�s�󋵂��X�V����
    /// </summary>
    /// <param name="stageName">�X�e�[�W��</param>
    /// <param name="is_clear">�N���A���</param>
    /// <param name="support1">�T�|�[�g1</param>
    /// <param name="support2">�T�|�[�g2</param>
    /// <param name="support3">�T�|�[�g3</param>
    public static void UpdateStage(string stageName, int is_clear, int support1, int support2, int support3)
    {
        Connection.Execute(
            "UPDATE stage_status SET is_clear = ?, support1 = ?, support2 = ?, support3 = ? WHERE stage = ?",
            is_clear, support1, support2, support3, stageName);
    }

    /// <summary>
    /// player_status�e�[�u���̃v���C���[�X�e�[�^�X���X�V
    /// </summary>
    /// <param name="id">�X�V����v���C���[��ID</param>
    /// <param name="newHP">�V����HP</param>
    /// <param name="newAttackPower">�V�����U����</param>
    /// <param name="newLevel">�V�������x��</param>
    public static void UpdatePlayerStatus(int id, int newHP, int newAttackPower, int newLevel)
	{
		Connection.Execute(
			"UPDATE player_status SET HP = ?, attack_power = ?, Level = ? WHERE Id = ?",
			newHP, newAttackPower, newLevel, id);
	}

    /// <summary>
    /// player_status�e�[�u���̃v���C���[�X�e�[�^�X���X�V
    /// </summary>
    /// <param name="id"></param>
    /// <param name="exp"></param>
    public static void GetExp(int id, int exp)
    {
        // 現在の経験値を取得
        var currentExp = Connection.ExecuteScalar<int>("SELECT current_exp FROM player_status WHERE id = ?", id);

        // 新しい経験値を計算
        var newExp = currentExp + exp;

        Connection.Execute("UPDATE player_status SET current_exp = ? WHERE id = ?", newExp, id);
    }

    /// <summary>
    /// �w�肵���X�e�[�WID�̐i�s�󋵁i�N���A�E�T�|�[�g�g�p�ۂȂǁj��stage_status�e�[�u������擾
    /// </summary>
    /// <param name="stage">�X�e�[�WID�i��: "stage1"�j</param>
    /// <returns>�Y���X�e�[�W��StageStatus�I�u�W�F�N�g�B���݂��Ȃ��ꍇ��null�B</returns>
    public static StageStatus GetStageStatus(string stage)
    {
        return Connection.Query<StageStatus>(
            "SELECT * FROM stage_status WHERE stage = ?", stage
        ).FirstOrDefault();
    }

    /// <summary>
    /// �{�X�̃��X�g���擾���郁�\�b�h��ǉ�
    /// </summary>
    public static List<Bosses> GetAllBosses()
    {
        return Connection.Query<Bosses>("SELECT * FROM bosses");
    }

    /// <summary>
    /// �f�o�b�O�p�Ƀe�[�u�������Z�b�g���鏈��
    /// </summary>
    private static void Resetplayer_statusTable()
	{
		Connection.Execute("DROP TABLE IF EXISTS player_status;");
		Connection.Execute("DROP TABLE IF EXISTS enemy_status;");

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
		CREATE TABLE enemy_status(
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
			"INSERT INTO enemy_status (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy1", 10, 2);

		Connection.Execute(
			"INSERT INTO enemy_status (Name, HP, AttackPower) VALUES (?, ?, ?);",
			"TestEnemy2", 25, 5);

		Connection.Execute("DROP TABLE IF EXISTS SupportStatus;");
		UnityEngine.Debug.Log("player_status�e�[�u�������Z�b�g���܂����B�����f�[�^��}�����܂����B");
	}

	/// <summary>
	/// データベース接続を適切に閉じる
	/// </summary>
	public static void CloseConnection()
	{
		if (_connection != null)
		{
			_connection.Close();
			_connection = null;
			Debug.Log("Database connection closed successfully.");
		}
	}
}
