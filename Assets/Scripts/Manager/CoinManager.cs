using UnityEngine;
using System.IO;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private int _coinCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ł������Ȃ�
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("�R�C���擾�I���݂̃R�C������: " + _coinCount);
        SaveCoinData();
    }

    private void SaveCoinData()
    {
        CoinData data = new CoinData
        {
            totalCoins = _coinCount
        };

        string json = JsonUtility.ToJson(data, true);

        // ���p�X�i���ۂɂ� Application.persistentDataPath ���g���Ƃ����j
        string path = Application.dataPath + "/coin.json";
        File.WriteAllText(path, json);
    }
}
