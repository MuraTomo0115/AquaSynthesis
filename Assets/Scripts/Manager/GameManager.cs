using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuContents;
    [SerializeField] private GameObject _menuUI;
    private string _currentScene;
    private MenuInputActions _inputActions;
    private Menu _menu;

    private void Awake()
    {
        _inputActions = new MenuInputActions();
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(_menuContents.gameObject);
    }

    private void Start()
    {
        _currentScene = SceneManager.GetActiveScene().name;
        _menu = _menuUI.GetComponent<Menu>();

        if (_currentScene.Contains("Stage"))
        {
            // ステージシーンならMenuInputActionsアクションマップを有効化
            _inputActions.Menu.Enable();
            _inputActions.Menu.Open.performed += ctx => OpenMenu();
        }
        else
        {
            // メニュー画面などではMenuInputActionsアクションマップを無効化
            _inputActions.Menu.Disable();
            _menuUI.SetActive(false);
        }
    }

    private void OpenMenu()
    {
        // メニューを表示し、アニメーションなどの処理を呼び出す
        _menuUI.SetActive(true);
        _menu.ToggleMenu();
    }
}
