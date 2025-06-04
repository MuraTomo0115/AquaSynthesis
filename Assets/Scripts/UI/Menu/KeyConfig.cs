using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 主制作者：村田智哉
/// </summary>

public enum KeyDeviceType
{
    Keyboard,
    Gamepad
}

[System.Serializable]
public class KeyConfigData
{
    public string label; // 識別用
    public Button keyButton;
    public TextMeshProUGUI keyText;
    public Button resetButton;
    public InputActionReference action;
    public int bindingIndex;
    public string saveKey;
    public KeyDeviceType deviceType; // キーのデバイスタイプ（Keyboard/Gamepad）
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
    /// 初期化処理。JSONから復元し、ボタンイベントを登録、UIを更新。
    /// </summary>
    private void Start()
    {
        Debug.Log("KeyConfigのJSON保存先: " + GetKeyConfigSavePath());
        _optionAnim = GetComponent<Animation>();

        LoadAllKeyConfigsFromJson(); // JSONから復元

        foreach (var config in _keyConfigs)
        {
            config.keyButton.onClick.AddListener(() => StartRebind(config));
            config.resetButton.onClick.AddListener(() => ResetBinding(config));
        }

        UpdateKeyTexts();
        UpdateSelectionVisual();
        OnDisable(); // 初期状態では無効化しておく
    }

    private void Awake()
    {
        // InputActionHolderのインスタンスを取得
        var optionActions = InputActionHolder.Instance.optionInputActions;

        // イベント登録
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
    /// 全キー設定をJSONファイルに保存する
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
    /// JSONファイルから全キー設定を復元し、UIとランタイム両方に反映する
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

        // バインディングをUIとランタイム両方にApply
        foreach (var config in _keyConfigs)
        {
            if (_overridePathDict.TryGetValue(config.saveKey, out var overridePath) && !string.IsNullOrEmpty(overridePath))
            {
                // UI表示用
                config.action.action.ApplyBindingOverride(config.bindingIndex, overridePath);

                // 実際のゲーム操作用
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
    /// JSON保存先のフルパスを取得する
    /// </summary>
    private string GetKeyConfigSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "keyconfig.json");
    }

    /// <summary>
    /// 選択中のキー設定を移動する（端でループ）
    /// </summary>
    private void MoveSelection(int dir)
    {
        int prev = _selectedIndex;
        int len = _keyConfigs.Length;
        _selectedIndex = (_selectedIndex + dir + len) % len;
        if (prev != _selectedIndex)
        {
            _isResetSelected = false; // インデックス移動時はkeyButtonに戻す
            UpdateSelectionVisual();
        }
    }

    /// <summary>
    /// 横方向の入力を受け取り、選択中のキー設定を移動
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
    /// 選択中の項目を決定する処理
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
    /// オプション画面を閉じる処理
    /// </summary>
    private void OnClose()
    {
        StartCoroutine(PlayAnimationUnscaled(_optionAnim, "OptionClose"));
    }

    /// <summary>
    /// Time.scaleが０のため、AnimationをUnscaledで再生し、完了後にデバッグログを表示
    /// </summary>
    private IEnumerator PlayAnimationUnscaled(Animation anim, string clipName)
    {
        anim.Play(clipName);
        AnimationState state = anim[clipName];
        state.speed = 1f;

        // 手動でtimeを進める
        while (state.time < state.length)
        {
            state.time += Time.unscaledDeltaTime;
            anim.Sample();
            yield return null;
        }
        anim.Stop();

        // 再生完了後の処理
        InputActionHolder.Instance.menuInputActions.Menu.Enable();
        OnDisable(); // オプション画面を閉じたら無効化
    }

    /// <summary>
    /// 選択中のボタンの色を更新する
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
    /// 各キー設定のテキスト表示を更新する
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
    /// キーリバインドを開始し、完了時に保存・反映・UI更新を行う
    /// </summary>
    private void StartRebind(KeyConfigData config)
    {
        config.keyButton.interactable = false;
        config.keyText.text = "Press any key";

        var action = config.action.action;
        action.Disable();

        var rebind = action.PerformInteractiveRebinding(config.bindingIndex);

        // デバイス制限（エラー表示付き）
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
                ShowErrorMessage("この項目には" +
                    (config.deviceType == KeyDeviceType.Keyboard ? "キーボード" : "ゲームパッド") +
                    "のみ割り当て可能です");
                operation.Cancel(); // リバインドキャンセル
                operation.Dispose(); // Disposeする
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

            // 重複チェック
            if (IsDuplicateKey(config, overridePath))
            {
                ShowErrorMessage("選択されたキーは、既に他の項目で使用されています");
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
        // バインディングのオーバーライドを削除
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

        // リセット後もリセットボタンを有効化
        config.resetButton.interactable = true;
    }

    /// <summary>
    /// 他の項目とバインディングが重複していないか判定
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
    /// UIとランタイム両方のInputActionにバインディングオーバーライドを適用
    /// </summary>
    private void ApplyOverrideToAllHolders(KeyConfigData config, string overridePath)
    {
        // UI表示用
        config.action.action.ApplyBindingOverride(config.bindingIndex, overridePath);

        // 実際のゲーム操作用
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
    /// エラーメッセージを一定時間表示する
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
