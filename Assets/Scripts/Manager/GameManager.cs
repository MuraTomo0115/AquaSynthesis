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
    private GameState                   _gameState;          // �Q�[���̏�Ԃ��Ǘ�����ϐ�
    public static GameManager Instance { get; private set; } // �V���O���g���C���X�^���X
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

        // ����^�C�g�����Ȃ����߃X�e�[�W�ɐݒ�
        _gameState = GameState.Stage;
    }

    /// <summary>
    /// �Q�[���̃X�e�[�^�X��ύX����
    /// </summary>
    /// <param name="state">�ύX����X�e�[�^�X</param>
    public void ChangeState(GameState state)
    {
        _gameState = state;
    }
}
