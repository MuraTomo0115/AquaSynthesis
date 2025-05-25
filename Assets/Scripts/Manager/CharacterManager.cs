using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string id;            // キャラクターID（player, enemy1, etc.）
    public int maxHealth;        // 最大HP
    public int attackPower;      // 攻撃力
    public int pistolPower;     // プレイヤーのみのステータス。ピストル攻撃力
}

[System.Serializable]
public class CharacterDatabase
{
    public List<CharacterData> characters;  // キャラクター情報のリスト
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
        // Enemyタグが付いたすべてのオブジェクトを取得
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Playerタグが付いたオブジェクトを1つだけ取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (enemies.Length == 0)
        {
            Debug.LogError("Enemyタグが付いたオブジェクトが見つかりません！");
            return;
        }
        if (player == null)
        {
            Debug.LogError("Playerタグがついたオブジェクトが見つかりません！");
            return;
        }

        // すべてのEnemyオブジェクトに対して処理を行う
        foreach (GameObject enemyObject in enemies)
        {
            Character enemy = enemyObject.GetComponent<Character>();

            if (enemy != null)
            {
                characterName = enemy.CharacterName;
                Debug.Log($"取得した敵キャラクター名: {characterName}");

                if (string.IsNullOrEmpty(characterName))
                {
                    Debug.LogError("キャラクター名が空です！");
                    continue;
                }

                // テーブルからデータを読み込む
                LoadCharacterDataFromTable(characterName, enemy);
            }
            else
            {
                Debug.LogError("Enemyコンポーネントが見つかりません！");
            }
        }

        // Playerオブジェクトに対して処理
        Character playerCharacter = player.GetComponent<Character>();

        if (playerCharacter != null)
        {
            characterName = playerCharacter.CharacterName;
            Debug.Log($"取得したプレイヤーキャラクター名: {characterName}");

            if (string.IsNullOrEmpty(characterName))
            {
                Debug.LogError("プレイヤーのキャラクターIDが空です！");
            }
            else
            {
                // テーブルからデータを読み込む
                LoadCharacterDataFromTable(characterName, playerCharacter, true);
            }
        }
        else
        {
            Debug.LogError("PlayerのCharacterコンポーネントが見つかりません！");
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
                Debug.Log($"プレイヤーのステータスを設定: HP={playerData.HP}, AttackPower={playerData.AttackPower}");
                Debug.Log($"ピストルの攻撃力を設定: PistolPower={pistolData.AttackPower}");
			}
            else
            {
                Debug.LogError($"CharacterStatusテーブルにName '{name}' のデータがありません");
            }
        }
        else
        {
            var enemyList = DatabaseManager.GetAllEnemies();
            var enemyData = enemyList.Find(e => e.Name == name);
            if (enemyData != null)
            {
                character.SetStats(enemyData.HP, enemyData.AttackPower);
                Debug.Log($"敵キャラクターのステータスを設定: HP={enemyData.HP}, AttackPower={enemyData.AttackPower}");
			}
            else
            {
                Debug.LogError($"EnemyStatusテーブルにName '{name}' のデータがありません");
            }
        }
    }
}
