using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N2BGMController : MonoBehaviour
{
    public static N2BGMController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 既に存在する場合は新しいインスタンスを破棄
        }
    }
    private void Start()
    {
        AudioManager.Instance.PlayBGM("N2BGM");
    }

    public void PlayBossBGM()
    {
        AudioManager.Instance.PlayBGM("N2BossBGM");
    }
}
