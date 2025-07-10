using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public enum GameState
{
    Title,
    Stage,
    Menu,
    GameOver,
    Clear
}

public class GameManager : MonoBehaviour
{
    private GameState                   _gameState;          // ゲームの状態を管理する変数
    public static GameManager Instance { get; private set; } // シングルトンインスタンス
    public GameState GameState => _gameState;
    [SerializeField] private CharacterManager _characterManager; // キャラクター管理用の参照

	private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        // DB接続・初期化
        var db = DatabaseManager.Connection;
        DatabaseManager.Initialize();
        _characterManager.LoadPlayerStatus();

        // 例：player_statusテーブルの最初のプレイヤー情報を取得してログ表示
        var player = db.Table<player_status>().FirstOrDefault();
        if (player != null)
        {
            //Debug.Log($"Player Info - Id:{player.Id}, HP:{player.HP}, AttackPower:{player.AttackPower}, Coin:{player.Coin}, Level:{player.Level}, WeaponId:{player.WeaponId}");
        }
        else
        {
            Debug.Log("PlayerStatus table is empty.");
        }
        // 現状タイトルがないためステージに設定
        _gameState = GameState.Stage;
    }

    private void Update()
    {
        // InputSystemでJキー押下を検知
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            Debug.Log("Jキーが押されました（デバッグ用レベルアップ）");
            LevelUpPlayer();
        }
    }

    private void LevelUpPlayer()
    {
        // プレイヤーの現在データ取得
        var player = DatabaseManager.GetAllCharacters().Find(c => c.name == "Shizuku"); // 例: 名前で検索
        if (player != null)
        {
            // ステータス上昇例（HP+10, 攻撃力+2, レベル+1）
            int newHP = player.hp + 10;
            int newAtk = player.attack_power + 2;
            int newLevel = player.level + 1;

            // DBを更新
            //DatabaseManager.UpdatePlayerStatus(player.id, newHP, newAtk, newLevel);

            Debug.Log($"レベルアップ！ 新HP:{newHP}, 新攻撃力:{newAtk}, 新レベル:{newLevel}");

            // 画面上のキャラクターにも反映したい場合は、再読込やSetStats呼び出しを追加
            _characterManager.LoadPlayerStatus();
        }
        else
        {
            Debug.LogWarning("プレイヤーデータが見つかりません");
        }
    }

    /// <summary>
    /// ゲームのステータスを変更する
    /// </summary>
    /// <param name="state">変更するステータス</param>
    public void ChangeState(GameState state)
    {
        _gameState = state;
    }
}
