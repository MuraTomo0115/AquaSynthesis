using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject _menuContents;
    private Animation _menuAnim;
    private bool _is_open = false;

    private void Awake()
    {
        _menuAnim = GetComponent<Animation>();  // 自身のアニメーションコンポーネントを取得
    }

    public void ToggleMenu()
    {
        _menuContents.SetActive(true);

        if (_is_open)
        {
            CloseMenu();
        }
        else
        {
            // 開く処理
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        // 逆再生の影響をリセット
        ResetAnimationState();

        if (_menuAnim.clip != null)
            _menuAnim.Play(); // 自身のアニメーション

        _is_open = true;
    }

    private void CloseMenu()
    {
        // アニメーションを逆再生するために一度停止
        _menuAnim.Stop();

        // 自身のアニメーションも逆再生
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = stateMenu.length;  // アニメーションの終わりから始める
        stateMenu.speed = -1f;  // 逆再生

        // アニメーションを逆再生
        _menuAnim.Play();

        _is_open = false;

        // アニメーションが終了したらメニューを非表示にする
        StartCoroutine(WaitForAnimationToEnd());
    }

    private IEnumerator WaitForAnimationToEnd()
    {
        yield return new WaitForSeconds(_menuAnim.clip.length);
        _menuContents.SetActive(false);  // アニメーションが終了したらメニューを非表示にする
    }

    private void ResetAnimationState()
    {
        // アニメーションの時間をリセットして逆再生の影響をリセット
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;  // アニメーションの先頭から
        stateMenu.speed = 1f;  // 通常再生
    }
}
