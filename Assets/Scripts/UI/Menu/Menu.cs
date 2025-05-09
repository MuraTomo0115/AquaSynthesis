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
        // �t�Đ��̉e�������Z�b�g
        ResetAnimationState();

        if (_menuAnim.clip != null)
            _menuAnim.Play(); // ���g�̃A�j���[�V����

        _is_open = true;
    }

    private void CloseMenu()
    {
        // �A�j���[�V�������t�Đ����邽�߂Ɉ�x��~
        _menuAnim.Stop();

        // ���g�̃A�j���[�V�������t�Đ�
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = stateMenu.length;  // �A�j���[�V�����̏I��肩��n�߂�
        stateMenu.speed = -1f;  // �t�Đ�

        // �A�j���[�V�������t�Đ�
        _menuAnim.Play();

        _is_open = false;

        // �A�j���[�V�������I�������烁�j���[���\���ɂ���
        StartCoroutine(WaitForAnimationToEnd());
    }

    private IEnumerator WaitForAnimationToEnd()
    {
        yield return new WaitForSeconds(_menuAnim.clip.length);
        _menuContents.SetActive(false);  // �A�j���[�V�������I�������烁�j���[���\���ɂ���
    }

    private void ResetAnimationState()
    {
        // �A�j���[�V�����̎��Ԃ����Z�b�g���ċt�Đ��̉e�������Z�b�g
        AnimationState stateMenu = _menuAnim[_menuAnim.clip.name];
        stateMenu.time = 0f;  // �A�j���[�V�����̐擪����
        stateMenu.speed = 1f;  // �ʏ�Đ�
    }
}
