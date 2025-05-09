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
            // �X�e�[�W�V�[���Ȃ�MenuInputActions�A�N�V�����}�b�v��L����
            _inputActions.Menu.Enable();
            _inputActions.Menu.Open.performed += ctx => OpenMenu();
        }
        else
        {
            // ���j���[��ʂȂǂł�MenuInputActions�A�N�V�����}�b�v�𖳌���
            _inputActions.Menu.Disable();
            _menuUI.SetActive(false);
        }
    }

    private void OpenMenu()
    {
        // ���j���[��\�����A�A�j���[�V�����Ȃǂ̏������Ăяo��
        _menuUI.SetActive(true);
        _menu.ToggleMenu();
    }
}
