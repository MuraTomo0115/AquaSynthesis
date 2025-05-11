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
    private string characterId;
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
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
            // Character�R���|�[�l���g���擾
            Character enemy = enemyObject.GetComponent<Character>();

            if (enemy != null)
            {
                // ID���擾���ăf�o�b�O���O�ɏo��
                characterId = enemy.CharacterId;
                Debug.Log($"�擾�����G�L�����N�^�[ID: {characterId}");

                if (string.IsNullOrEmpty(characterId))
                {
                    Debug.LogError("�L�����N�^�[ID����ł��I");
                    continue;
                }

                // ID�Ńf�[�^��ǂݍ���
                LoadCharacterDataFromJson(characterId, enemy);
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
            characterId = playerCharacter.CharacterId;
            Debug.Log($"�擾�����v���C���[�L�����N�^�[ID: {characterId}");

            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogError("�v���C���[�̃L�����N�^�[ID����ł��I");
            }
            else
            {
                // �v���C���[��ID�Ńf�[�^��ǂݍ���
                LoadCharacterDataFromJson(characterId, playerCharacter);
            }
        }
        else
        {
            Debug.LogError("Player��Character�R���|�[�l���g��������܂���I");
        }
    }

    private void LoadCharacterDataFromJson(string id, Character character)
    {
        // JSON�̓ǂݍ���
        TextAsset json = Resources.Load<TextAsset>("Status/Status");
        if (json == null)
        {
            Debug.LogError("Status.json �� Resources/JSON �t�H���_���Ɍ�����܂���I");
            return;
        }

        // JSON����L�����N�^�[�f�[�^���p�[�X
        CharacterDatabase db = JsonUtility.FromJson<CharacterDatabase>(json.text);

        // ID�Ɉ�v����L�����N�^�[��T��
        CharacterData data = db.characters.Find(c => c.id == id);

        if (data == null)
        {
            Debug.LogError($"�L�����N�^�[ID '{id}' �ɑΉ�����f�[�^������܂���");
            return;
        }

        // �f�o�b�O���O�Ƀf�[�^��\��
        Debug.Log($"{data.id} : �ő�HP = {data.maxHealth}, �U���� = {data.attackPower}, �s�X�g�� = {data.pistolPower}");

        // �L�����N�^�[�Ƀf�[�^��ݒ�
        if (data.pistolPower > 0)
        {
            character.SetStats(data.maxHealth, data.attackPower, data.pistolPower);
        }
        else
        {
            character.SetStats(data.maxHealth, data.attackPower);
        }
    }
}
