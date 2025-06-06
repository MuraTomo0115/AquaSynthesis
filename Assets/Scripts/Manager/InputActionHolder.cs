using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputActionHolder : MonoBehaviour
{
    public static InputActionHolder Instance { get; private set; }

    public PlayerInputActions playerInputActions { get; private set; }
    public MenuInputActions menuInputActions { get; private set; }
    public StageSelectInputActions stageSelectInputActions { get; private set; }
    public OptionInputActions optionInputActions { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("InputActionHolder Awake");
            Instance = this;
            playerInputActions = new PlayerInputActions();
            menuInputActions = new MenuInputActions();
            stageSelectInputActions = new StageSelectInputActions();
            optionInputActions = new OptionInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
