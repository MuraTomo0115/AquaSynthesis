using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// �L�[�R���t�B�O�Ǘ��N���X
/// �吧��ҁF���c�q��
/// </summary>
public enum KeyDeviceType
{
    Keyboard = 0,
    Gamepad = 1
}

/// <summary>
/// �L�[�ݒ�f�[�^�N���X
/// </summary>
[System.Serializable]
public class KeyConfigData
{
    public string               label;                  // ���ʗp���x��
    public Button               keyButton;              // �L�[�ݒ�p�{�^��
    public TextMeshProUGUI      keyText;                // �L�[�\���p�e�L�X�g
    public Button               resetButton;            // ���Z�b�g�{�^��
    public InputActionReference action;                 // InputAction�Q��
    public int                  keyboardBindingIndex;   // �L�[�{�[�h�p�o�C���f�B���O�C���f�b�N�X
    public int                  gamepadBindingIndex;    // �Q�[���p�b�h�p�o�C���f�B���O�C���f�b�N�X
    public string               saveKey;                // �ۑ��p�L�[

    /// <summary>
    /// �f�o�C�X�^�C�v�ɉ������o�C���f�B���O�C���f�b�N�X���擾����
    /// </summary>
    /// <param name="type">�f�o�C�X�^�C�v</param>
    /// <returns>�o�C���f�B���O�C���f�b�N�X</returns>
    public int GetBindingIndex(KeyDeviceType type)
    {
        // �L�[�{�[�h�ƃQ�[���p�b�h�̃o�C���f�B���O�C���f�b�N�X��Ԃ�
        return type == KeyDeviceType.Keyboard ? keyboardBindingIndex : gamepadBindingIndex;
    }
}

/// <summary>
/// �L�[�ݒ�̃o�C���f�B���O�f�[�^
/// </summary>
[System.Serializable]
public class KeyBindingData
{
    public string        saveKey;        // �ۑ��p�L�[
    public string        overridePath;   // �I�[�o�[���C�h�p�X
    public KeyDeviceType deviceType;     // �f�o�C�X�^�C�v
}

/// <summary>
/// �L�[�ݒ�̕ۑ��f�[�^�N���X
/// </summary>
[System.Serializable]
public class KeyConfigSaveData
{
    public List<KeyBindingData> bindings = new List<KeyBindingData>(); // キーコンフィグ
    public float bgmVolume = 1.0f;
    public float seVolume = 1.0f;
}


[System.Serializable]
public class SoundButtonData
{
    public Button plusButton;      // プラスボタン
    public Button minusButton;     // マイナスボタン
    public TextMeshProUGUI label;  // 音量テキスト
    public string saveKey;         // 保存用のキー
}

public class KeyConfig : MonoBehaviour
{
    [Header("�L�[�ݒ�f�[�^")]
    [SerializeField] private KeyConfigData[] _keyConfigs;        // �L�[�ݒ�f�[�^�z��
    [SerializeField] private TextMeshProUGUI _errorText;         // �G���[���b�Z�[�W�\���p�e�L�X�g
    [SerializeField] private Button          _keyboardButton;    // �L�[�{�[�h�ؑփ{�^��
    [SerializeField] private Button          _gamepadButton;     // �Q�[���p�b�h�ؑփ{�^��
    [SerializeField] private SoundButtonData _bgmButton;         // BGM設定ボタン
    [SerializeField] private SoundButtonData _seButton;          // SE設定ボタン
    [SerializeField] private TextMeshProUGUI _deviceLabelText;   // �f�o�C�X���\���p�e�L�X�g

    [Header("�I����Ԃ������g�摜")]
    [SerializeField] private GameObject _keyboardFrameSelected;  // �L�[�{�[�h�I��g
    [SerializeField] private GameObject _keyboardFrameActive;    // �L�[�{�[�h����g
    [SerializeField] private GameObject _gamepadFrameSelected;   // �Q�[���p�b�h�I��g
    [SerializeField] private GameObject _gamepadFrameActive;     // �Q�[���p�b�h����g

    private int           _selectedIndex = 0;         // 選択中のインデックス（キーコンフィグ＋音量設定）
    private int           _selectedDeviceIndex = -1;  // デバイス選択中かどうか（-1:通常, 0:キーボード, 1:ゲームパッド）
    private const int     _soundButtonCount = 4;      // BGM-, BGM+, SE-, SE+
    private bool          _isResetSelected = false;   // リセットボタン選択中
    private Animation     _optionAnim;                // �I�v�V������ʃA�j���[�V����
    private Coroutine     _errorCoroutine;            // �G���[�\���p�R���[�`��
    private float        _bgmVolume = 1.0f;           // BGM音量
    private float        _seVolume = 1.0f;            // SE音量
    private const float  _volumeStep = 0.05f;         // 音量調整の間隔(5%ずつ調整)
    private const float  _volumeMin = 0.0f;           // 音量調整の最小値
    private const float  _volumeMax = 1.0f;           // 音量調整の最大値

    // ����ς݃f�o�C�X�^�C�v
    private KeyDeviceType _activeDeviceType = KeyDeviceType.Keyboard;
    // �I�[�o�[���C�h�p�X�̕ۑ��p�f�B�N�V���i��
    private readonly Dictionary<(string saveKey, KeyDeviceType deviceType), string> _overridePathDict
        = new Dictionary<(string, KeyDeviceType), string>();

    /// <summary>
    /// �Q�[���p�b�h�̃��x������{��\�L�ɕϊ����邽�߂̃}�b�s���O
    /// </summary>
    private static readonly Dictionary<string, string> GamepadLabelMap = new Dictionary<string, string>
    {
        { "Button South", "A�{�^��" },
        { "Button West",  "X�{�^��" },
        { "Button North","Y�{�^��" },
        { "Button East", "B�{�^��" },
        { "Left Shoulder", "L1" },
        { "Right Shoulder", "R1" },
        { "leftTriggerButton", "L2" },
        { "rightTriggerButton", "R2" }
    };

    /// <summary>
    /// �e��ݒ�̃��[�h��UI���������s���B
    /// </summary>
    private void Start()
    {
        var optionActions = InputActionHolder.Instance.optionInputActions;
        optionActions.Option.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        optionActions.Option.Click.performed += ctx => OnSubmit();
        optionActions.Option.Close.performed += ctx => OnClose();
        Debug.Log("KeyConfig JSON保存先: " + GetKeyConfigSavePath());
        _optionAnim = GetComponent<Animation>();
        LoadAllKeyConfigsFromJson();
        foreach (var config in _keyConfigs)
        {
            config.keyButton.onClick.AddListener(() => StartRebind(config));
            config.resetButton.onClick.AddListener(() => ResetBinding(config));
        }
        _selectedDeviceIndex = -1;
        _selectedIndex = 0;
        _isResetSelected = false;
        _activeDeviceType = KeyDeviceType.Keyboard;
        UpdateKeyTexts();
        UpdateSelectionVisual();
        OnDisable();
        // BGM
        if (_bgmButton.minusButton != null)
            _bgmButton.minusButton.onClick.AddListener(() => ChangeBGMVolume(-_volumeStep));
        if (_bgmButton.plusButton != null)
            _bgmButton.plusButton.onClick.AddListener(() => ChangeBGMVolume(_volumeStep));
        // SE
        if (_seButton.minusButton != null)
            _seButton.minusButton.onClick.AddListener(() => ChangeSEVolume(-_volumeStep));
        if (_seButton.plusButton != null)
            _seButton.plusButton.onClick.AddListener(() => ChangeSEVolume(_volumeStep));
    }

    /// <summary>
    /// BGM音量を増減し、AudioManagerとラベル、JSONに反映する
    /// </summary>
    private void ChangeBGMVolume(float delta)
    {
        _bgmVolume = Mathf.Clamp(_bgmVolume + delta, _volumeMin, _volumeMax);
        AudioManager.Instance?.SetBGMVolume(_bgmVolume);
        UpdateVolumeLabels();
        SaveAllKeyConfigsToJson();
    }

    /// <summary>
    /// SE音量を増減し、AudioManagerとラベル、JSONに反映する
    /// </summary>
    private void ChangeSEVolume(float delta)
    {
        _seVolume = Mathf.Clamp(_seVolume + delta, _volumeMin, _volumeMax);
        AudioManager.Instance?.SetSFXVolume(_seVolume);
        UpdateVolumeLabels();
        SaveAllKeyConfigsToJson();
    }

    /// <summary>
    /// BGM/SE音量ラベルを現在の値で更新する
    /// </summary>
    private void UpdateVolumeLabels()
    {
        if (_bgmButton.label != null)
            _bgmButton.label.text = $"{Mathf.RoundToInt(_bgmVolume * 100)}%";
        if (_seButton.label != null)
            _seButton.label.text = $"{Mathf.RoundToInt(_seVolume * 100)}%";
    }

    /// <summary>
    /// ����������Option�A�N�V�����}�b�v�𖳌���
    /// </summary>
    private void OnDisable()
    {
        InputActionHolder.Instance.optionInputActions.Option.Disable();
    }

    /// <summary>
    /// �S�ẴL�[�ݒ��JSON�t�@�C���ɕۑ�
    /// </summary>
    private void SaveAllKeyConfigsToJson()
    {
        var saveData = new KeyConfigSaveData();
        foreach (var config in _keyConfigs)
        {
            int bindingIndex = config.GetBindingIndex(_activeDeviceType);
            var action = config.action.action;
            var overridePath = action.bindings[bindingIndex].overridePath;
            saveData.bindings.Add(new KeyBindingData
            {
                saveKey = config.saveKey,
                overridePath = overridePath,
                deviceType = _activeDeviceType
            });
        }
        saveData.bgmVolume = _bgmVolume;
        saveData.seVolume = _seVolume;
        var json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(GetKeyConfigSavePath(), json);
    }

    /// <summary>
    /// JSON�t�@�C������S�ẴL�[�ݒ��ǂݍ���
    /// </summary>
    private void LoadAllKeyConfigsFromJson()
    {
        _overridePathDict.Clear();
        var path = GetKeyConfigSavePath();
        if (!System.IO.File.Exists(path)) {
            _bgmVolume = 1.0f;
            _seVolume = 1.0f;
            AudioManager.Instance?.SetBGMVolume(_bgmVolume);
            AudioManager.Instance?.SetSFXVolume(_seVolume);
            UpdateVolumeLabels();
            return;
        }
        var json = System.IO.File.ReadAllText(path);
        var saveData = JsonUtility.FromJson<KeyConfigSaveData>(json);
        if (saveData == null) return;
        foreach (var binding in saveData.bindings)
        {
            if (!string.IsNullOrEmpty(binding.overridePath))
                _overridePathDict[(binding.saveKey, binding.deviceType)] = binding.overridePath;
        }
        foreach (var config in _keyConfigs)
        {
            int bindingIndex = config.GetBindingIndex(_activeDeviceType);
            if (_overridePathDict.TryGetValue((config.saveKey, _activeDeviceType), out var overridePath) && !string.IsNullOrEmpty(overridePath))
            {
                config.action.action.ApplyBindingOverride(bindingIndex, overridePath);
                if (InputActionHolder.Instance != null)
                {
                    var runtimeAction = InputActionHolder.Instance.playerInputActions.asset.FindAction(config.action.action.name, true);
                    runtimeAction?.ApplyBindingOverride(bindingIndex, overridePath);
                }
            }
        }
        // 音量も復元
        _bgmVolume = saveData.bgmVolume;
        _seVolume = saveData.seVolume;
        AudioManager.Instance?.SetBGMVolume(_bgmVolume);
        AudioManager.Instance?.SetSFXVolume(_seVolume);
        UpdateVolumeLabels();
    }

    /// <summary>
    /// �L�[�R���t�B�O�̕ۑ���p�X���擾
    /// </summary>
    /// <returns>�ۑ���p�X</returns>
    private string GetKeyConfigSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "keyconfig.json");
    }

    /// <summary>
    /// �L�[�ݒ�̑I���C���f�b�N�X���ړ�
    /// </summary>
    /// <param name="dir">�ړ�����</param>
    private void MoveSelection(int dir)
    {
        int len = _keyConfigs.Length;
        int prev = _selectedIndex;
        _selectedIndex = (_selectedIndex + dir + len) % len;
        if (prev != _selectedIndex)
        {
            _isResetSelected = false;
            UpdateSelectionVisual();
        }
    }

    /// <summary>
    /// ���̓f�o�C�X�̈ړ����͂�����
    /// </summary>
    /// <param name="direction">�ړ��x�N�g��</param>
    private void OnMove(Vector2 direction)
    {
        int keyConfigCount = _keyConfigs.Length;
        int totalSelectable = keyConfigCount + _soundButtonCount;
        if (_selectedDeviceIndex >= 0)
        {
            if (Mathf.Abs(direction.x) > 0.5f)
            {
                _selectedDeviceIndex = 1 - _selectedDeviceIndex;
                UpdateSelectionVisual();
                return;
            }
            if (direction.y < -0.5f)
            {
                _selectedDeviceIndex = -1;
                _selectedIndex = 0;
                _isResetSelected = false;
                UpdateSelectionVisual();
                return;
            }
            if (direction.y > 0.5f)
            {
                _selectedDeviceIndex = -1;
                _selectedIndex = keyConfigCount; // BGM-
                _isResetSelected = false;
                UpdateSelectionVisual();
                return;
            }
            return;
        }
        // 音量設定群の範囲
        bool isSoundButton = (_selectedIndex >= keyConfigCount && _selectedIndex < keyConfigCount + _soundButtonCount);
        if (isSoundButton)
        {
            // 上下でキーコンフィグ群やデバイス選択に移動
            if (direction.y > 0.5f)
            {
                if (_selectedIndex == keyConfigCount) // BGM-
                {
                    _selectedIndex = keyConfigCount - 1; // 最後のキーコンフィグ
                }
                else
                {
                    _selectedIndex--;
                }
                UpdateSelectionVisual();
                return;
            }
            if (direction.y < -0.5f)
            {
                if (_selectedIndex == keyConfigCount + _soundButtonCount - 1) // SE+
                {
                    _selectedDeviceIndex = 0;
                    UpdateSelectionVisual();
                }
                else
                {
                    _selectedIndex++;
                    UpdateSelectionVisual();
                }
                return;
            }
            // 左右で音量設定群内を移動
            if (direction.x < -0.5f)
            {
                if (_selectedIndex == keyConfigCount)
                    _selectedIndex = keyConfigCount + _soundButtonCount - 1; // ループ
                else
                    _selectedIndex--;
                UpdateSelectionVisual();
                return;
            }
            if (direction.x > 0.5f)
            {
                if (_selectedIndex == keyConfigCount + _soundButtonCount - 1)
                    _selectedIndex = keyConfigCount; // ループ
                else
                    _selectedIndex++;
                UpdateSelectionVisual();
                return;
            }
            return;
        }
        // キーコンフィグ群
        if (direction.y > 0.5f)
        {
            if (_selectedIndex == 0)
            {
                _selectedDeviceIndex = 0;
                UpdateSelectionVisual();
            }
            else
            {
                _selectedIndex--;
                _isResetSelected = false;
                UpdateSelectionVisual();
            }
            return;
        }
        if (direction.y < -0.5f)
        {
            if (_selectedIndex < totalSelectable - 1)
            {
                _selectedIndex++;
                _isResetSelected = false;
                UpdateSelectionVisual();
            }
            else
            {
                _selectedDeviceIndex = 0;
                UpdateSelectionVisual();
            }
            return;
        }
        if (_selectedIndex < keyConfigCount)
        {
            if (direction.x < -0.5f && _isResetSelected)
            {
                _isResetSelected = false;
                UpdateSelectionVisual();
            }
            else if (direction.x > 0.5f && !_isResetSelected)
            {
                _isResetSelected = true;
                UpdateSelectionVisual();
            }
        }
    }

    /// <summary>
    ///  キーコンフィグのバインディングを開始
    ///  </summary>
    private void OnSubmit()
    {
        int keyConfigCount = _keyConfigs.Length;
        int totalSelectable = keyConfigCount + _soundButtonCount;
        if (_selectedDeviceIndex == 0 || _selectedDeviceIndex == 1)
        {
            // デバイス決定
            _activeDeviceType = (KeyDeviceType)_selectedDeviceIndex;
            UpdateSelectionVisual();
            _deviceLabelText.text = (_activeDeviceType == KeyDeviceType.Keyboard) ? "キーボード" : "ゲームパッド";
            UpdateKeyTexts();
            foreach (var config in _keyConfigs)
            {
                int bindingIndex = config.GetBindingIndex(_activeDeviceType);
                if (_overridePathDict.TryGetValue((config.saveKey, _activeDeviceType), out var overridePath) && !string.IsNullOrEmpty(overridePath))
                {
                    config.action.action.ApplyBindingOverride(bindingIndex, overridePath);
                    if (InputActionHolder.Instance != null)
                    {
                        var runtimeAction = InputActionHolder.Instance.playerInputActions.asset.FindAction(config.action.action.name, true);
                        runtimeAction?.ApplyBindingOverride(bindingIndex, overridePath);
                    }
                }
            }
            return;
        }
        if (_selectedIndex < keyConfigCount)
        {
            if (_isResetSelected)
                ResetBinding(_keyConfigs[_selectedIndex]);
            else
                StartRebind(_keyConfigs[_selectedIndex]);
        }
        else if (_selectedIndex == keyConfigCount) // BGM-
        {
            _bgmButton.minusButton.onClick.Invoke();
        }
        else if (_selectedIndex == keyConfigCount + 1) // BGM+
        {
            _bgmButton.plusButton.onClick.Invoke();
        }
        else if (_selectedIndex == keyConfigCount + 2) // SE-
        {
            _seButton.minusButton.onClick.Invoke();
        }
        else if (_selectedIndex == keyConfigCount + 3) // SE+
        {
            _seButton.plusButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// ���j���[����鏈�����s��
    /// </summary>
    private void OnClose()
    {
        StartCoroutine(PlayAnimationUnscaled(_optionAnim, "OptionClose"));
    }

    /// <summary>
    /// �A�j���[�V�������X�P�[�������ōĐ�����R���[�`��
    /// </summary>
    /// <param name="anim">�A�j���[�V����</param>
    /// <param name="clipName">�N���b�v��</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator PlayAnimationUnscaled(Animation anim, string clipName)
    {
        anim.Play(clipName);
        var state = anim[clipName];
        state.speed = 1f;

        while (state.time < state.length)
        {
            state.time += Time.unscaledDeltaTime;
            anim.Sample();
            yield return null;
        }
        anim.Stop();

        InputActionHolder.Instance.menuInputActions.Menu.Enable();
        OnDisable();
    }

    /// <summary>
    /// �I����Ԃ�UI���X�V
    /// </summary>
    private void UpdateSelectionVisual()
    {
        _keyboardFrameSelected.SetActive(false);
        _keyboardFrameActive.SetActive(false);
        _gamepadFrameSelected.SetActive(false);
        _gamepadFrameActive.SetActive(false);

        if (_selectedDeviceIndex == 0)
            _keyboardFrameSelected.SetActive(true);
        else if (_selectedDeviceIndex == 1)
            _gamepadFrameSelected.SetActive(true);

        if (_activeDeviceType == KeyDeviceType.Keyboard)
            _keyboardFrameActive.SetActive(true);
        else if (_activeDeviceType == KeyDeviceType.Gamepad)
            _gamepadFrameActive.SetActive(true);

        int keyConfigCount = _keyConfigs.Length;
        // キーコンフィグボタン群
        for (int i = 0; i < keyConfigCount; i++)
        {
            var keyColors = _keyConfigs[i].keyButton.colors;
            var resetColors = _keyConfigs[i].resetButton.colors;
            if (_selectedDeviceIndex == -1 && i == _selectedIndex)
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
        // 音量設定ボタン群
        Color bgmMinusColor = Color.white, bgmPlusColor = Color.white, seMinusColor = Color.white, sePlusColor = Color.white;
        if (_selectedDeviceIndex == -1 && _selectedIndex == keyConfigCount)
            bgmMinusColor = Color.yellow;
        if (_selectedDeviceIndex == -1 && _selectedIndex == keyConfigCount + 1)
            bgmPlusColor = Color.yellow;
        if (_selectedDeviceIndex == -1 && _selectedIndex == keyConfigCount + 2)
            seMinusColor = Color.yellow;
        if (_selectedDeviceIndex == -1 && _selectedIndex == keyConfigCount + 3)
            sePlusColor = Color.yellow;
        if (_bgmButton.minusButton != null)
            _bgmButton.minusButton.image.color = bgmMinusColor;
        if (_bgmButton.plusButton != null)
            _bgmButton.plusButton.image.color = bgmPlusColor;
        if (_seButton.minusButton != null)
            _seButton.minusButton.image.color = seMinusColor;
        if (_seButton.plusButton != null)
            _seButton.plusButton.image.color = sePlusColor;

        _deviceLabelText.text = (_activeDeviceType == KeyDeviceType.Keyboard) ? "キーボード" : "ゲームパッド";
    }

    /// <summary>
    /// �Q�[���p�b�h�̃��x������{��\�L�ɕϊ�
    /// </summary>
    /// <param name="raw">���̃��x��</param>
    /// <returns>���{�ꃉ�x��</returns>
    private string ToFriendlyGamepadLabel(string raw)
    {
        string lowerRaw = raw.ToLower();
        foreach (var pair in GamepadLabelMap)
        {
            if (lowerRaw.Contains(pair.Key.ToLower()))
                return pair.Value;
        }
        return raw;
    }

    /// <summary>
    /// �L�[�ݒ�e�L�X�g���X�V
    /// </summary>
    private void UpdateKeyTexts()
    {
        foreach (var config in _keyConfigs)
        {
            int bindingIndex = config.GetBindingIndex(_activeDeviceType);
            string displayPath = _overridePathDict.TryGetValue((config.saveKey, _activeDeviceType), out var overridePath) && !string.IsNullOrEmpty(overridePath)
                ? overridePath
                : config.action.action.bindings[bindingIndex].effectivePath;

            string label = InputControlPath.ToHumanReadableString(
                displayPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (_activeDeviceType == KeyDeviceType.Gamepad)
                label = ToFriendlyGamepadLabel(label);

            config.keyText.text = label;
        }
    }

    /// <summary>
    /// �L�[�̃��o�C���h���J�n
    /// </summary>
    /// <param name="config">�Ώۂ̃L�[�ݒ�f�[�^</param>
    private void StartRebind(KeyConfigData config)
    {
        // �I�v�V�������͂��ꎞ�I�ɖ�����
        var optionActions = InputActionHolder.Instance.optionInputActions;
        optionActions.Option.Disable();

        config.keyButton.interactable = false;
        config.keyText.text = "Press any key";

        var action = config.action.action;
        int bindingIndex = config.GetBindingIndex(_activeDeviceType);
        action.Disable();

        var rebind = action.PerformInteractiveRebinding(bindingIndex);

        rebind.OnPotentialMatch(operation =>
        {
            var control = operation.selectedControl;
            bool isValid = (_activeDeviceType == KeyDeviceType.Keyboard && control.device is Keyboard)
                        || (_activeDeviceType == KeyDeviceType.Gamepad && control.device is Gamepad);

            if (!isValid)
            {
                ShowErrorMessage($"{(_activeDeviceType == KeyDeviceType.Keyboard ? "キーボード" : "ゲームパッド")}の入力が無効です");
                operation.Cancel();
                operation.Dispose();
                config.keyButton.interactable = true;
                config.keyText.text = InputControlPath.ToHumanReadableString(
                    config.action.action.bindings[bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                StartCoroutine(EnableOptionActionWithDelay(0.1f));
            }
        });

        rebind.OnComplete(op =>
        {
            action.Enable();
            config.keyButton.interactable = true;
            op.Dispose();

            var overridePath = action.bindings[bindingIndex].overridePath;

            if (IsDuplicateKey(config, overridePath, bindingIndex))
            {
                // ���s���Ă��I�v�V�������͂��ēx�L����
                StartCoroutine(EnableOptionActionWithDelay(0.1f));
                ShowErrorMessage("�I�����ꂽ�L�[�́A���ɑ��̍��ڂŎg�p����Ă��܂�");
                action.RemoveBindingOverride(bindingIndex);
                _overridePathDict.Remove((config.saveKey, _activeDeviceType));
                LoadAllKeyConfigsFromJson();
                UpdateKeyTexts();
                return;
            }

            if (!string.IsNullOrEmpty(overridePath))
                _overridePathDict[(config.saveKey, _activeDeviceType)] = overridePath;
            else
                _overridePathDict.Remove((config.saveKey, _activeDeviceType));

            ApplyOverrideToAllHolders(config, overridePath, bindingIndex);

            SaveAllKeyConfigsToJson();
            UpdateKeyTexts();
            // ���o�C���h������A1�b�҂��ėL����
            StartCoroutine(EnableOptionActionWithDelay(0.1f));
        })
        .Start();
    }

    /// <summary>
    /// �I�v�V�����A�N�V��������莞�Ԍ�ɗL��������R���[�`��
    /// </summary>
    /// <param name="delay">�ҋ@���鎞��</param>
    /// <returns></returns>
    private IEnumerator EnableOptionActionWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        InputActionHolder.Instance.optionInputActions.Option.Enable();
    }

    /// <summary>
    /// �L�[�ݒ���f�t�H���g�Ƀ��Z�b�g
    /// </summary>
    /// <param name="config">�Ώۂ̃L�[�ݒ�f�[�^</param>
    private void ResetBinding(KeyConfigData config)
    {
        int bindingIndex = config.GetBindingIndex(_activeDeviceType);
        config.action.action.RemoveBindingOverride(bindingIndex);

        if (InputActionHolder.Instance != null)
        {
            var actions = InputActionHolder.Instance.playerInputActions;
            var action = actions.asset.FindAction(config.action.action.name, true);
            action?.RemoveBindingOverride(bindingIndex);
        }

        _overridePathDict.Remove((config.saveKey, _activeDeviceType));
        SaveAllKeyConfigsToJson();
        UpdateKeyTexts();

        config.resetButton.interactable = true;
    }

    /// <summary>
    /// ���̃L�[�ݒ�Əd�����Ă��Ȃ�������
    /// </summary>
    /// <param name="current">���݂̐ݒ�</param>
    /// <param name="overridePath">���蓖�ăp�X</param>
    /// <param name="bindingIndex">�o�C���f�B���O�C���f�b�N�X</param>
    /// <returns>�d�����Ă����true</returns>
    private bool IsDuplicateKey(KeyConfigData current, string overridePath, int bindingIndex)
    {
        if (string.IsNullOrEmpty(overridePath)) return false;
        foreach (var config in _keyConfigs)
        {
            if (config == current) continue;
            int otherIndex = config.GetBindingIndex(_activeDeviceType);
            string otherPath = _overridePathDict.TryGetValue((config.saveKey, _activeDeviceType), out var path) && !string.IsNullOrEmpty(path)
                ? path
                : config.action.action.bindings[otherIndex].effectivePath;

            if (overridePath == otherPath)
                return true;
        }
        return false;
    }

    /// <summary>
    /// �S�Ă�InputActionHolder�Ƀo�C���f�B���O�I�[�o�[���C�h��K�p
    /// </summary>
    /// <param name="config">�Ώۂ̃L�[�ݒ�f�[�^</param>
    /// <param name="overridePath">�I�[�o�[���C�h�p�X</param>
    /// <param name="bindingIndex">�o�C���f�B���O�C���f�b�N�X</param>
    private void ApplyOverrideToAllHolders(KeyConfigData config, string overridePath, int bindingIndex)
    {
        config.action.action.ApplyBindingOverride(bindingIndex, overridePath);

        if (InputActionHolder.Instance != null)
        {
            var actions = InputActionHolder.Instance.playerInputActions;
            var action = actions.asset.FindAction(config.action.action.name, true);
            if (action != null)
            {
                if (!string.IsNullOrEmpty(overridePath))
                    action.ApplyBindingOverride(bindingIndex, overridePath);
                else
                    action.RemoveBindingOverride(bindingIndex);
            }
        }
    }

    /// <summary>
    /// �G���[���b�Z�[�W��\������
    /// </summary>
    /// <param name="message">���b�Z�[�W</param>
    /// <param name="duration">�\������</param>
    private void ShowErrorMessage(string message, float duration = 2f)
    {
        if (_errorCoroutine != null)
            StopCoroutine(_errorCoroutine);
        _errorCoroutine = StartCoroutine(ShowErrorCoroutine(message, duration));
    }

    /// <summary>
    /// �G���[���b�Z�[�W����莞�ԕ\������R���[�`��
    /// </summary>
    /// <param name="message">���b�Z�[�W</param>
    /// <param name="duration">�\������</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator ShowErrorCoroutine(string message, float duration)
    {
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        _errorText.gameObject.SetActive(false);
    }
}
