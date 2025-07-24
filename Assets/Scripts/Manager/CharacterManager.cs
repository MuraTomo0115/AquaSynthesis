using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string id;            // �L�����N�^�[ID�iplayer, enemy1, etc.�j
    public int maxHealth;        // �ő�HP
    public int attackPower;      // �U����
    public int pistolPower;      // �v���C���[�݂̂̃X�e�[�^�X�B�s�X�g���U����
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

    public void LoadPlayerStatus()
    {
        // Enemy�^�O���t�������ׂẴI�u�W�F�N�g���擾
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Destructible�^�O���t�������ׂẴI�u�W�F�N�g���擾
        GameObject[] destructibleObjs = GameObject.FindGameObjectsWithTag("Destructible");

        // Player�^�O���t�����I�u�W�F�N�g��1�����擾
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Boss�^�O���t�������ׂẴI�u�W�F�N�g���擾
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

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

                if (string.IsNullOrEmpty(characterName))
                {
                    continue;
                }

                // �e�[�u������f�[�^��ǂݍ���
                LoadCharacterDataFromTable(characterName, enemy);
            }
        }

        // ���ׂĂ�Destructible�I�u�W�F�N�g�ɑ΂��ď������s��
        foreach (GameObject destructibleObject in destructibleObjs)
        {
            Character obj = destructibleObject.GetComponent<Character>();

            if (obj != null)
            {
                characterName = obj.CharacterName;

                if (string.IsNullOrEmpty(characterName))
                {
                    continue;
                }

                // �e�[�u������f�[�^��ǂݍ���
                LoadCharacterDataFromTable(characterName, obj);
            }
            else
            {
                Debug.LogError("Destructible��Character�R���|�[�l���g��������܂���I");
            }
        }

        // ���ׂĂ�Boss�I�u�W�F�N�g�ɑ΂��ď������s��
        foreach (GameObject bossObject in bosses)
        {
            Character bossCharacter = bossObject.GetComponent<Character>();

            if (bossCharacter != null)
            {
                characterName = bossCharacter.CharacterName;
                Debug.Log($"Found Boss Character: {characterName}");

                if (string.IsNullOrEmpty(characterName))
                {
                    Debug.LogError("�{�X�̃L�����N�^�[������ł��I");
                    continue;
                }

                // �e�[�u������f�[�^��ǂݍ���
                LoadCharacterDataFromTable(characterName, bossCharacter, false, true);
            }
        }

        // Player�I�u�W�F�N�g�ɑ΂��ď���
        Character playerCharacter = player.GetComponent<Character>();

        if (playerCharacter != null)
        {
            characterName = playerCharacter.CharacterName;

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
    }

    // isPlayer: �v���C���[���ǂ���, isBoss: �{�X���ǂ���
    private void LoadCharacterDataFromTable(string name, Character character, bool isPlayer = false, bool isBoss = false)
    {
        if (isPlayer)
        {
            var playerList = DatabaseManager.GetAllCharacters();
            var playerData = playerList.Find(c => c.name == name);
            var pistolList = DatabaseManager.GetAllPistols();
            var pistolData = pistolList.Find(c => c.Id == 1);
            if (playerData != null)
            {
                character.SetStats(playerData.hp, playerData.attack_power, pistolData.AttackPower);
            }
        }
        else if (isBoss)
        {
            var bossList = DatabaseManager.GetAllBosses();
            var bossData = bossList.Find(b => b.name == name);
            if (bossData != null)
            {
                character.SetStats(bossData.hp, bossData.attack_power);
            }
            else
            {
                Debug.LogError($"Boss data not found for Name '{name}' in bosses table");
            }
        }
        else if (character.CompareTag("Enemy"))
        {
            var enemyList = DatabaseManager.GetAllEnemies();
            var enemyData = enemyList.Find(e => e.name == name);
            if (enemyData != null)
            {
                character.SetStats(enemyData.hp, enemyData.attack_power);
            }
            else
            {
                Debug.LogError($"enemy_statusテーブルにName '{name}' のデータが見つかりませんでした");
            }
        }
        else if (character.CompareTag("Destructible"))
        {
            var destructibleList = DatabaseManager.GetAllDestructibleObjs();
            var destructibleData = destructibleList.Find(e => e.Name == name);
            character.SetSE(destructibleData.DestroySE);
        }
    }
}
