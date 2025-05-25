using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string id;            // �L�����N�^�[ID�iplayer, enemy1, etc.�j
    public int maxHealth;        // �ő�HP
    public int attackPower;      // �U����
    public int pistolPower;     // �v���C���[�݂̂̃X�e�[�^�X�B�s�X�g���U����
}

[System.Serializable]
public class CharacterDatabase
{
    public List<CharacterData> characters;  // �L�����N�^�[���̃��X�g
}

public class CharacterManager : MonoBehaviour
{
    private string characterName;
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void LoadCharacterStatus()
    {
        // Enemy�^�O���t�������ׂẴI�u�W�F�N�g���擾
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Player�^�O���t�����I�u�W�F�N�g��1�����擾
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (enemies.Length == 0)
        {
            Debug.LogError("Enemy�^�O���t�����I�u�W�F�N�g��������܂���I");
            return;
        }
        if (player == null)
        {
            Debug.LogError("Player�^�O�������I�u�W�F�N�g��������܂���I");
            return;
        }

        // ���ׂĂ�Enemy�I�u�W�F�N�g�ɑ΂��ď������s��
        foreach (GameObject enemyObject in enemies)
        {
            Character enemy = enemyObject.GetComponent<Character>();

            if (enemy != null)
            {
                characterName = enemy.CharacterName;
                Debug.Log($"�擾�����G�L�����N�^�[��: {characterName}");

                if (string.IsNullOrEmpty(characterName))
                {
                    Debug.LogError("�L�����N�^�[������ł��I");
                    continue;
                }

                // �e�[�u������f�[�^��ǂݍ���
                LoadCharacterDataFromTable(characterName, enemy);
            }
            else
            {
                Debug.LogError("Enemy�R���|�[�l���g��������܂���I");
            }
        }

        // Player�I�u�W�F�N�g�ɑ΂��ď���
        Character playerCharacter = player.GetComponent<Character>();

        if (playerCharacter != null)
        {
            characterName = playerCharacter.CharacterName;
            Debug.Log($"�擾�����v���C���[�L�����N�^�[��: {characterName}");

            if (string.IsNullOrEmpty(characterName))
            {
                Debug.LogError("�v���C���[�̃L�����N�^�[ID����ł��I");
            }
            else
            {
                // �e�[�u������f�[�^��ǂݍ���
                LoadCharacterDataFromTable(characterName, playerCharacter, true);
            }
        }
        else
        {
            Debug.LogError("Player��Character�R���|�[�l���g��������܂���I");
        }
    }

    private void LoadCharacterDataFromTable(string name, Character character, bool isPlayer = false)
    {
        if (isPlayer)
        {
            var playerList = DatabaseManager.GetAllCharacters();
            var playerData = playerList.Find(c => c.Name == name);
            var pistolList = DatabaseManager.GetAllPistols();
            var pistolData = pistolList.Find(c => c.Id == 1);
			if (playerData != null)
            {
                character.SetStats(playerData.HP, playerData.AttackPower, pistolData.AttackPower);
                Debug.Log($"�v���C���[�̃X�e�[�^�X��ݒ�: HP={playerData.HP}, AttackPower={playerData.AttackPower}");
                Debug.Log($"�s�X�g���̍U���͂�ݒ�: PistolPower={pistolData.AttackPower}");
			}
            else
            {
                Debug.LogError($"CharacterStatus�e�[�u����Name '{name}' �̃f�[�^������܂���");
            }
        }
        else
        {
            var enemyList = DatabaseManager.GetAllEnemies();
            var enemyData = enemyList.Find(e => e.Name == name);
            if (enemyData != null)
            {
                character.SetStats(enemyData.HP, enemyData.AttackPower);
                Debug.Log($"�G�L�����N�^�[�̃X�e�[�^�X��ݒ�: HP={enemyData.HP}, AttackPower={enemyData.AttackPower}");
			}
            else
            {
                Debug.LogError($"EnemyStatus�e�[�u����Name '{name}' �̃f�[�^������܂���");
            }
        }
    }
}
