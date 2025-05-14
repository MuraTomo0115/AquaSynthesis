using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine;

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

        // 現状タイトルがないためステージに設定
        _gameState = GameState.Stage;
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
