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
        _menuAnim = GetComponent<Animation>();  // ���g�̃A�j���[�V�����R���|�[�l���g���擾
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
            // �J������
            OpenMenu();
        }
    }

	private void OpenMenu()
	{
		ResetAnimationState();

		if (_menuAnim.clip != null)
			_menuAnim.Play();

		_is_open = true;  // ���true�ɂ��Ă���

		StartCoroutine(WaitForAnimationToEndAndPause());  // �ʂ̃R���[�`��
	}

	private void CloseMenu()
	{
		_menuAnim.Stop();

		AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
		stateMenu.time = stateMenu.length;
		stateMenu.speed = -1f;
		_menuAnim.Play();

		_is_open = false;

		StartCoroutine(WaitForAnimationToEndAndResume());  // �ʂ̃R���[�`��
	}

	// ���Ԃ��~�߂鏈��
	private IEnumerator WaitForAnimationToEndAndPause()
	{
		yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
		Time.timeScale = 0f;  // �� �A�j���[�V�������I����Ă���~�߂�
	}

	// ���Ԃ��ĊJ���鏈��
	private IEnumerator WaitForAnimationToEndAndResume()
	{
		yield return new WaitForSecondsRealtime(_menuAnim.clip.length);
		_menuContents.SetActive(false);
		Time.timeScale = 1f;
	}

	private void ResetAnimationState()
    {
        // �A�j���[�V�����̎��Ԃ����Z�b�g���ċt�Đ��̉e�������Z�b�g
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;  // �A�j���[�V�����̐擪����
        stateMenu.speed = 1f;  // �ʏ�Đ�
    }
}
