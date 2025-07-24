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

        // ���ׂĂ�Destructible�I�u�W�F�N�g�ɑ΂��ď������s��
        foreach (GameObject destructibleObject in destructibleObjs)
        {
            Character obj = destructibleObject.GetComponent<Character>();

            if (obj != null)
            {
                characterName = obj.CharacterName;
                Debug.Log($"�擾�����G�L�����N�^�[��: {characterName}");

                if (string.IsNullOrEmpty(characterName))
                {
                    Debug.LogError("�L�����N�^�[������ł��I");
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
            else
            {
                Debug.LogError("Boss��Character�R���|�[�l���g��������܂���I");
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
                Debug.Log($"�v���C���[�̃X�e�[�^�X��ݒ�: HP={playerData.hp}, AttackPower={playerData.attack_power}, Level={playerData.level}");
                Debug.Log($"�s�X�g���̍U���͂�ݒ�: PistolPower={pistolData.AttackPower}");
            }
            else
            {
                Debug.LogError($"player_status�e�[�u����Name '{name}' �̃f�[�^������܂���");
            }
        }
        else if (isBoss)
        {
            var bossList = DatabaseManager.GetAllBosses();
            var bossData = bossList.Find(b => b.name == name);
            if (bossData != null)
            {
                character.SetStats(bossData.hp, bossData.attack_power);
                Debug.Log($"Boss Character Stats Set: HP={bossData.hp}, AttackPower={bossData.attack_power}");
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
                Debug.Log($"敵のステータスを取得: 名前={enemyData.name}, HP={enemyData.hp}, AttackPower={enemyData.attack_power}");
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
            Debug.Log($"Destructible�̃X�e�[�^�X��ݒ�: HP={0}, AttackPower={0}");
        }
    }

    public void LoadBossStatus(GameObject bossObject)
    {
        if (bossObject == null)
        {
            Debug.LogError("Bossオブジェクトがnullです");
            return;
        }

        Character bossCharacter = bossObject.GetComponent<Character>();
        if (bossCharacter == null)
        {
            Debug.LogError("BossオブジェクトにCharacterコンポーネントがありません");
            return;
        }

        string bossName = bossCharacter.CharacterName;
        if (string.IsNullOrEmpty(bossName))
        {
            Debug.LogError("BossのCharacterNameが空です");
            return;
        }

        var bossList = DatabaseManager.GetAllBosses();
        var bossData = bossList.Find(b => b.name == bossName);
        if (bossData != null)
        {
            bossCharacter.SetStats(bossData.hp, bossData.attack_power);
            Debug.Log($"Boss初期化: HP={bossData.hp}, AttackPower={bossData.attack_power}");
        }
        else
        {
            Debug.LogError($"Boss data not found for Name '{bossName}' in bosses table");
        }
    }
}
