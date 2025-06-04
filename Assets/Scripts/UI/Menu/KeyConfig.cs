using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// �吧��ҁF���c�q��
/// </summary>

public enum KeyDeviceType
{
    Keyboard,
    Gamepad
}

[System.Serializable]
public class KeyConfigData
{
    public string label; // ���ʗp
    public Button keyButton;
    public TextMeshProUGUI keyText;
    public Button resetButton;
    public InputActionReference action;
    public int bindingIndex;
    public string saveKey;
    public KeyDeviceType deviceType; // �L�[�̃f�o�C�X�^�C�v�iKeyboard/Gamepad�j
}

[System.Serializable]
public class KeyConfigSaveData
{
    public List<KeyBindingData> bindings = new List<KeyBindingData>();
}

[System.Serializable]
public class KeyBindingData
{
    public string saveKey;
    public string overridePath;
}

public class KeyConfig : MonoBehaviour
{
    [SerializeField] private KeyConfigData[] _keyConfigs;
    [SerializeField] private TextMeshProUGUI _errorText;
    private Coroutine _errorCoroutine;

    private int _selectedIndex = 0;
    private bool _isResetSelected = false;
    private Animation _optionAnim;
    private Dictionary<string, string> _overridePathDict = new Dictionary<string, string>();

    /// <summary>
    /// �����������BJSON���畜�����A�{�^���C�x���g��o�^�AUI���X�V�B
    /// </summary>
    private void Start()
    {
        Debug.Log("KeyConfig��JSON�ۑ���: " + GetKeyConfigSavePath());
        _optionAnim = GetComponent<Animation>();

        LoadAllKeyConfigsFromJson(); // JSON���畜��

        foreach (var config in _keyConfigs)
        {
            config.keyButton.onClick.AddListener(() => StartRebind(config));
            config.resetButton.onClick.AddListener(() => ResetBinding(config));
        }

        UpdateKeyTexts();
        UpdateSelectionVisual();
        OnDisable(); // ������Ԃł͖��������Ă���
    }

    private void Awake()
    {
        // InputActionHolder�̃C���X�^���X���擾
        var optionActions = InputActionHolder.Instance.optionInputActions;

        // �C�x���g�o�^
        optionActions.Option.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>().x);
        optionActions.Option.Click.performed += ctx => OnSubmit();
        optionActions.Option.Close.performed += ctx => OnClose();
    }

    private void OnEnable()
    {
        InputActionHolder.Instance.optionInputActions.Option.Enable();
    }

    private void OnDisable()
    {
        InputActionHolder.Instance.optionInputActions.Option.Disable();
    }

    /// <summary>
    /// �S�L�[�ݒ��JSON�t�@�C���ɕۑ�����
    /// </summary>
    private void SaveAllKeyConfigsToJson()
    {
        KeyConfigSaveData saveData = new KeyConfigSaveData();
        foreach (var config in _keyConfigs)
        {
            var action = config.action.action;
            var overridePath = action.bindings[config.bindingIndex].overridePath;
            saveData.bindings.Add(new KeyBindingData
            {
                saveKey = config.saveKey,
                overridePath = overridePath
            });
        }
        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(GetKeyConfigSavePath(), json);
    }

    /// <summary>
    /// JSON�t�@�C������S�L�[�ݒ�𕜌����AUI�ƃ����^�C�������ɔ��f����
    /// </summary>
    private void LoadAllKeyConfigsFromJson()
    {
        _overridePathDict.Clear();

        string path = GetKeyConfigSavePath();
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        var saveData = JsonUtility.FromJson<KeyConfigSaveData>(json);
        if (saveData == null) return;

        foreach (var binding in saveData.bindings)
        {
            if (!string.IsNullOrEmpty(binding.overridePath))
            {
                _overridePathDict[binding.saveKey] = binding.overridePath;
            }
        }

        // �o�C���f�B���O��UI�ƃ����^�C��������Apply
        foreach (var config in _keyConfigs)
        {
            if (_overridePathDict.TryGetValue(config.saveKey, out var overridePath) && !string.IsNullOrEmpty(overridePath))
            {
                // UI�\���p
                config.action.action.ApplyBindingOverride(config.bindingIndex, overridePath);

                // ���ۂ̃Q�[������p
                if (InputActionHolder.Instance != null)
                {
                    var runtimeAction = InputActionHolder.Instance.playerInputActions.asset.FindAction(config.action.action.name, true);
                    if (runtimeAction != null)
                    {
                        runtimeAction.ApplyBindingOverride(config.bindingIndex, overridePath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// JSON�ۑ���̃t���p�X���擾����
    /// </summary>
    private string GetKeyConfigSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "keyconfig.json");
    }

    /// <summary>
    /// �I�𒆂̃L�[�ݒ���ړ�����i�[�Ń��[�v�j
    /// </summary>
    private void MoveSelection(int dir)
    {
        int prev = _selectedIndex;
        int len = _keyConfigs.Length;
        _selectedIndex = (_selectedIndex + dir + len) % len;
        if (prev != _selectedIndex)
        {
            _isResetSelected = false; // �C���f�b�N�X�ړ�����keyButton�ɖ߂�
            UpdateSelectionVisual();
        }
    }

    /// <summary>
    /// �������̓��͂��󂯎��A�I�𒆂̃L�[�ݒ���ړ�
    /// </summary>
    /// <param name="direction"></param>
    private void OnMove(float direction)
    {
        if (direction < -0.5f)
        {
            if (_isResetSelected)
            {
                _isResetSelected = false;
                UpdateSelectionVisual();
            }
            else
            {
                MoveSelection(-1);
                _isResetSelected = true;
                UpdateSelectionVisual();
            }
        }
        else if (direction > 0.5f)
        {
            if (!_isResetSelected)
            {
                _isResetSelected = true;
                UpdateSelectionVisual();
            }
            else
            {
                MoveSelection(1);
                _isResetSelected = false;
                UpdateSelectionVisual();
            }
        }
    }

    /// <summary>
    /// �I�𒆂̍��ڂ����肷�鏈��
    /// </summary>
    private void OnSubmit()
    {
        if (_isResetSelected)
        {
            ResetBinding(_keyConfigs[_selectedIndex]);
        }
        else
        {
            StartRebind(_keyConfigs[_selectedIndex]);
        }
    }

    /// <summary>
    /// �I�v�V������ʂ���鏈��
    /// </summary>
    private void OnClose()
    {
        StartCoroutine(PlayAnimationUnscaled(_optionAnim, "OptionClose"));
    }

    /// <summary>
    /// Time.scale���O�̂��߁AAnimation��Unscaled�ōĐ����A������Ƀf�o�b�O���O��\��
    /// </summary>
    private IEnumerator PlayAnimationUnscaled(Animation anim, string clipName)
    {
        anim.Play(clipName);
        AnimationState state = anim[clipName];
        state.speed = 1f;

        // �蓮��time��i�߂�
        while (state.time < state.length)
        {
            state.time += Time.unscaledDeltaTime;
            anim.Sample();
            yield return null;
        }
        anim.Stop();

        // �Đ�������̏���
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
        OnDisable(); // �I�v�V������ʂ�����疳����
    }

    /// <summary>
    /// �I�𒆂̃{�^���̐F���X�V����
    /// </summary>
    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < _keyConfigs.Length; i++)
        {
            var keyColors = _keyConfigs[i].keyButton.colors;
            var resetColors = _keyConfigs[i].resetButton.colors;

            if (i == _selectedIndex)
            {
                keyColors.normalColor = !_isResetSelected ? Color.yellow : Color.white;
                resetColors.normalColor = _isResetSelected ? Color.yellow : Color.white;
            }
            else
            {
                keyColors.normalColor = Color.white;
                resetColors.normalColor = Color.white;
            }

            _keyConfigs[i].keyButton.colors = keyColors;
            _keyConfigs[i].resetButton.colors = resetColors;
        }
    }

    /// <summary>
    /// �e�L�[�ݒ�̃e�L�X�g�\�����X�V����
    /// </summary>
    private void UpdateKeyTexts()
    {
        foreach (var config in _keyConfigs)
        {
            string displayPath = null;
            if (_overridePathDict.TryGetValue(config.saveKey, out var overridePath) && !string.IsNullOrEmpty(overridePath))
            {
                displayPath = overridePath;
            }
            else
            {
                displayPath = config.action.action.bindings[config.bindingIndex].effectivePath;
            }

            config.keyText.text = InputControlPath.ToHumanReadableString(
                displayPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }

    /// <summary>
    /// �L�[���o�C���h���J�n���A�������ɕۑ��E���f�EUI�X�V���s��
    /// </summary>
    private void StartRebind(KeyConfigData config)
    {
        config.keyButton.interactable = false;
        config.keyText.text = "Press any key";

        var action = config.action.action;
        action.Disable();

        var rebind = action.PerformInteractiveRebinding(config.bindingIndex);

        // �f�o�C�X�����i�G���[�\���t���j
        rebind.OnPotentialMatch(operation =>
        {
            var control = operation.selectedControl;
            bool isValid = false;
            if (config.deviceType == KeyDeviceType.Keyboard && control.device is Keyboard)
                isValid = true;
            else if (config.deviceType == KeyDeviceType.Gamepad && control.device is Gamepad)
                isValid = true;

            if (!isValid)
            {
                ShowErrorMessage("���̍��ڂɂ�" +
                    (config.deviceType == KeyDeviceType.Keyboard ? "�L�[�{�[�h" : "�Q�[���p�b�h") +
                    "�̂݊��蓖�ĉ\�ł�");
                operation.Cancel(); // ���o�C���h�L�����Z��
                operation.Dispose(); // Dispose����
                config.keyButton.interactable = true;
                config.keyText.text = InputControlPath.ToHumanReadableString(
                    config.action.action.bindings[config.bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
        });

        rebind.OnComplete(op =>
        {
            action.Enable();
            config.keyButton.interactable = true;
            op.Dispose();

            var overridePath = action.bindings[config.bindingIndex].overridePath;

            // �d���`�F�b�N
            if (IsDuplicateKey(config, overridePath))
            {
                ShowErrorMessage("�I�����ꂽ�L�[�́A���ɑ��̍��ڂŎg�p����Ă��܂�");
                action.RemoveBindingOverride(config.bindingIndex);
                _overridePathDict.Remove(config.saveKey);
                LoadAllKeyConfigsFromJson();
                UpdateKeyTexts();
                return;
            }

            if (!string.IsNullOrEmpty(overridePath))
                _overridePathDict[config.saveKey] = overridePath;
            else
                _overridePathDict.Remove(config.saveKey);

            ApplyOverrideToAllHolders(config, overridePath);

            SaveAllKeyConfigsToJson();
            UpdateKeyTexts();

            InputActionHolder.Instance.ChangeInputActions();
        })
        .Start();
    }

    private void ResetBinding(KeyConfigData config)
    {
        // �o�C���f�B���O�̃I�[�o�[���C�h���폜
        config.action.action.RemoveBindingOverride(config.bindingIndex);

        if (InputActionHolder.Instance != null)
        {
            var actions = InputActionHolder.Instance.playerInputActions;
            var action = actions.asset.FindAction(config.action.action.name, true);
            if (action != null)
            {
                action.RemoveBindingOverride(config.bindingIndex);
            }
        }

        _overridePathDict.Remove(config.saveKey);
        SaveAllKeyConfigsToJson();
        UpdateKeyTexts();

        // ���Z�b�g������Z�b�g�{�^����L����
        config.resetButton.interactable = true;
    }

    /// <summary>
    /// ���̍��ڂƃo�C���f�B���O���d�����Ă��Ȃ�������
    /// </summary>
    private bool IsDuplicateKey(KeyConfigData current, string overridePath)
    {
        if (string.IsNullOrEmpty(overridePath)) return false;
        foreach (var config in _keyConfigs)
        {
            if (config == current) continue;
            string otherPath = null;
            if (_overridePathDict.TryGetValue(config.saveKey, out var path) && !string.IsNullOrEmpty(path))
                otherPath = path;
            else
                otherPath = config.action.action.bindings[config.bindingIndex].effectivePath;

            if (overridePath == otherPath)
                return true;
        }
        return false;
    }

    /// <summary>
    /// UI�ƃ����^�C��������InputAction�Ƀo�C���f�B���O�I�[�o�[���C�h��K�p
    /// </summary>
    private void ApplyOverrideToAllHolders(KeyConfigData config, string overridePath)
    {
        // UI�\���p
        config.action.action.ApplyBindingOverride(config.bindingIndex, overridePath);

        // ���ۂ̃Q�[������p
        if (InputActionHolder.Instance != null)
        {
            var actions = InputActionHolder.Instance.playerInputActions;
            var action = actions.asset.FindAction(config.action.action.name, true);
            if (action != null)
            {
                if (!string.IsNullOrEmpty(overridePath))
                    action.ApplyBindingOverride(config.bindingIndex, overridePath);
                else
                    action.RemoveBindingOverride(config.bindingIndex);
            }
        }
    }

    /// <summary>
    /// �G���[���b�Z�[�W����莞�ԕ\������
    /// </summary>
    private void ShowErrorMessage(string message, float duration = 2f)
    {
        if (_errorCoroutine != null)
            StopCoroutine(_errorCoroutine);
        _errorCoroutine = StartCoroutine(ShowErrorCoroutine(message, duration));
    }

    private System.Collections.IEnumerator ShowErrorCoroutine(string message, float duration)
    {
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        _errorText.gameObject.SetActive(false);
    }
}
