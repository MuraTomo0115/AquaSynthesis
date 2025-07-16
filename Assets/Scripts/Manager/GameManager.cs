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
    private GameState                   _gameState;          // �Q�[���̏�Ԃ��Ǘ�����ϐ�
    public static GameManager Instance { get; private set; } // �V���O���g���C���X�^���X
    public GameState GameState => _gameState;
    [SerializeField] private CharacterManager _characterManager; // �L�����N�^�[�Ǘ��p�̎Q��

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

        // DB�ڑ��E������
        var db = DatabaseManager.Connection;
        DatabaseManager.Initialize();
        _characterManager.LoadPlayerStatus();

        // ��Fplayer_status�e�[�u���̍ŏ��̃v���C���[�����擾���ă��O�\��
        var player = db.Table<player_status>().FirstOrDefault();
        if (player != null)
        {
            //Debug.Log($"Player Info - Id:{player.Id}, HP:{player.HP}, AttackPower:{player.AttackPower}, Coin:{player.Coin}, Level:{player.Level}, WeaponId:{player.WeaponId}");
        }
        else
        {
            Debug.Log("PlayerStatus table is empty.");
        }
        // ����^�C�g�����Ȃ����߃X�e�[�W�ɐݒ�
        _gameState = GameState.Stage;
    }

    private void Update()
    {
        // InputSystem��J�L�[���������m
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            Debug.Log("J�L�[��������܂����i�f�o�b�O�p���x���A�b�v�j");
            LevelUpPlayer();
        }
    }

    private void LevelUpPlayer()
    {
        // �v���C���[�̌��݃f�[�^�擾
        var player = DatabaseManager.GetAllCharacters().Find(c => c.name == "Shizuku"); // ��: ���O�Ō���
        if (player != null)
        {
            // �X�e�[�^�X�㏸��iHP+10, �U����+2, ���x��+1�j
            int newHP = player.hp + 10;
            int newAtk = player.attack_power + 2;
            int newLevel = player.level + 1;

            // DB���X�V
            //DatabaseManager.UpdatePlayerStatus(player.id, newHP, newAtk, newLevel);

            Debug.Log($"���x���A�b�v�I �VHP:{newHP}, �V�U����:{newAtk}, �V���x��:{newLevel}");

            // ��ʏ�̃L�����N�^�[�ɂ����f�������ꍇ�́A�ēǍ���SetStats�Ăяo����ǉ�
            _characterManager.LoadPlayerStatus();
        }
        else
        {
            Debug.LogWarning("�v���C���[�f�[�^��������܂���");
        }
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
