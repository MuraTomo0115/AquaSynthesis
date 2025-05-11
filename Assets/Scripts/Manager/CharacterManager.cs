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
    private string characterId;
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
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
            // Characterコンポーネントを取得
            Character enemy = enemyObject.GetComponent<Character>();

            if (enemy != null)
            {
                // IDを取得してデバッグログに出力
                characterId = enemy.CharacterId;
                Debug.Log($"取得した敵キャラクターID: {characterId}");

                if (string.IsNullOrEmpty(characterId))
                {
                    Debug.LogError("キャラクターIDが空です！");
                    continue;
                }

                // IDでデータを読み込む
                LoadCharacterDataFromJson(characterId, enemy);
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
            characterId = playerCharacter.CharacterId;
            Debug.Log($"取得したプレイヤーキャラクターID: {characterId}");

            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogError("プレイヤーのキャラクターIDが空です！");
            }
            else
            {
                // プレイヤーのIDでデータを読み込む
                LoadCharacterDataFromJson(characterId, playerCharacter);
            }
        }
        else
        {
            Debug.LogError("PlayerのCharacterコンポーネントが見つかりません！");
        }
    }

    private void LoadCharacterDataFromJson(string id, Character character)
    {
        // JSONの読み込み
        TextAsset json = Resources.Load<TextAsset>("Status/Status");
        if (json == null)
        {
            Debug.LogError("Status.json が Resources/JSON フォルダ内に見つかりません！");
            return;
        }

        // JSONからキャラクターデータをパース
        CharacterDatabase db = JsonUtility.FromJson<CharacterDatabase>(json.text);

        // IDに一致するキャラクターを探す
        CharacterData data = db.characters.Find(c => c.id == id);

        if (data == null)
        {
            Debug.LogError($"キャラクターID '{id}' に対応するデータがありません");
            return;
        }

        // デバッグログにデータを表示
        Debug.Log($"{data.id} : 最大HP = {data.maxHealth}, 攻撃力 = {data.attackPower}, ピストル = {data.pistolPower}");

        // キャラクターにデータを設定
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
