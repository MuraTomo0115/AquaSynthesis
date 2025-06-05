using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputActionHolder : MonoBehaviour
{
    public static InputActionHolder Instance { get; private set; }

    public PlayerInputActions playerInputActions { get; private set; }
    public MenuInputActions menuInputActions { get; private set; }
    public StageSelectInputActions stageSelectInputActions { get; private set; }
    public OptionInputActions optionInputActions { get; private set; }
    public abstract void ChangeInputActions(); // 継承先でキーコンアクションフィグ設定を適用する

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerInputActions = new PlayerInputActions();
            menuInputActions = new MenuInputActions();
            stageSelectInputActions = new StageSelectInputActions();
            optionInputActions = new OptionInputActions();

            // 継承先のChangeInputActionsを呼び出す
            //ChangeInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
