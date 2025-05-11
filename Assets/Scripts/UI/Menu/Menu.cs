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
		ResetAnimationState();

		if (_menuAnim.clip != null)
			_menuAnim.Play();

		_is_open = true;  // 先にtrueにしておく

		StartCoroutine(WaitForAnimationToEndAndPause());  // 別のコルーチン
	}

	private void CloseMenu()
	{
		_menuAnim.Stop();

		AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
		stateMenu.time = stateMenu.length;
		stateMenu.speed = -1f;
		_menuAnim.Play();

		_is_open = false;

		StartCoroutine(WaitForAnimationToEndAndResume());  // 別のコルーチン
	}

	// 時間を止める処理
	private IEnumerator WaitForAnimationToEndAndPause()
	{
		yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
		Time.timeScale = 0f;  // ← アニメーションが終わってから止める
	}

	// 時間を再開する処理
	private IEnumerator WaitForAnimationToEndAndResume()
	{
		yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
		_menuContents.SetActive(false);
		Time.timeScale = 1f;
	}

	private void ResetAnimationState()
    {
        // アニメーションの時間をリセットして逆再生の影響をリセット
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;  // アニメーションの先頭から
        stateMenu.speed = 1f;  // 通常再生
    }
}
