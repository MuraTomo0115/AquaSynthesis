using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// キーコンフィグ管理クラス
/// 主制作者：村田智哉
/// </summary>
public enum KeyDeviceType
{
    Keyboard = 0,
    Gamepad = 1
}

/// <summary>
/// キー設定データクラス
/// </summary>
[System.Serializable]
public class KeyConfigData
{
    public string               label;                  // 識別用ラベル
    public Button               keyButton;              // キー設定用ボタン
    public TextMeshProUGUI      keyText;                // キー表示用テキスト
    public Button               resetButton;            // リセットボタン
    public InputActionReference action;                 // InputAction参照
    public int                  keyboardBindingIndex;   // キーボード用バインディングインデックス
    public int                  gamepadBindingIndex;    // ゲームパッド用バインディングインデックス
    public string               saveKey;                // 保存用キー

    /// <summary>
    /// デバイスタイプに応じたバインディングインデックスを取得する
    /// </summary>
    /// <param name="type">デバイスタイプ</param>
    /// <returns>バインディングインデックス</returns>
    public int GetBindingIndex(KeyDeviceType type)
    {
        // キーボードとゲームパッドのバインディングインデックスを返す
        return type == KeyDeviceType.Keyboard ? keyboardBindingIndex : gamepadBindingIndex;
    }
}

/// <summary>
/// キー設定のバインディングデータ
/// </summary>
[System.Serializable]
public class KeyBindingData
{
    public string        saveKey;        // 保存用キー
    public string        overridePath;   // オーバーライドパス
    public KeyDeviceType deviceType;     // デバイスタイプ
}

/// <summary>
/// キー設定の保存データクラス
/// </summary>
[System.Serializable]
public class KeyConfigSaveData
{
    public List<KeyBindingData> bindings = new List<KeyBindingData>(); // バインディングリスト
}

public class KeyConfig : MonoBehaviour
{
    [Header("キー設定データ")]
    [SerializeField] private KeyConfigData[] _keyConfigs;        // キー設定データ配列
    [SerializeField] private TextMeshProUGUI _errorText;         // エラーメッセージ表示用テキスト
    [SerializeField] private Button          _keyboardButton;    // キーボード切替ボタン
    [SerializeField] private Button          _gamepadButton;     // ゲームパッド切替ボタン
    [SerializeField] private TextMeshProUGUI _deviceLabelText;   // デバイス名表示用テキスト

    [Header("選択状態を示す枠画像")]
    [SerializeField] private GameObject _keyboardFrameSelected;  // キーボード選択枠
    [SerializeField] private GameObject _keyboardFrameActive;    // キーボード決定枠
    [SerializeField] private GameObject _gamepadFrameSelected;   // ゲームパッド選択枠
    [SerializeField] private GameObject _gamepadFrameActive;     // ゲームパッド決定枠

    private int           _selectedIndex = 0;         // 選択中のキー設定インデックス
    private int           _selectedDeviceIndex = -1;  // 選択中のデバイスインデックス
    private bool          _isResetSelected = false;   // リセットボタン選択状態
    private Animation     _optionAnim;                // オプション画面アニメーション
    private Coroutine     _errorCoroutine;            // エラー表示用コルーチン

    // 決定済みデバイスタイプ
    private KeyDeviceType _activeDeviceType = KeyDeviceType.Keyboard;
    // オーバーライドパスの保存用ディクショナリ
    private readonly Dictionary<(string saveKey, KeyDeviceType deviceType), string> _overridePathDict
        = new Dictionary<(string, KeyDeviceType), string>();

    /// <summary>
    /// ゲームパッドのラベルを日本語表記に変換するためのマッピング
    /// </summary>
    private static readonly Dictionary<string, string> GamepadLabelMap = new Dictionary<string, string>
    {
        { "Button South", "Aボタン" },
        { "Button West",  "Xボタン" },
        { "Button North","Yボタン" },
        { "Button East", "Bボタン" },
        { "Left Shoulder", "L1" },
        { "Right Shoulder", "R1" },
        { "leftTriggerButton", "L2" },
        { "rightTriggerButton", "R2" }
    };

    /// <summary>
    /// 各種設定のロードやUI初期化を行う。
    /// </summary>
    private void Start()
    {
        var optionActions = InputActionHolder.Instance.optionInputActions;
        optionActions.Option.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        optionActions.Option.Click.performed += ctx => OnSubmit();
        optionActions.Option.Close.performed += ctx => OnClose();

        Debug.Log("KeyConfigのJSON保存先: " + GetKeyConfigSavePath());
        _optionAnim = GetComponent<Animation>();

        LoadAllKeyConfigsFromJson();

        foreach (var config in _keyConfigs)
        {
            config.keyButton.onClick.AddListener(() => StartRebind(config));
            config.resetButton.onClick.AddListener(() => ResetBinding(config));
        }

        _selectedDeviceIndex = 0;
        _selectedIndex = 0;
        _isResetSelected = false;
        _activeDeviceType = KeyDeviceType.Keyboard;
        UpdateKeyTexts();
        UpdateSelectionVisual();
        OnDisable(); // 初期状態では無効化
    }

    /// <summary>
    /// 無効化時にOptionアクションマップを無効化
    /// </summary>
    private void OnDisable()
    {
        InputActionHolder.Instance.optionInputActions.Option.Disable();
    }

    /// <summary>
    /// 全てのキー設定をJSONファイルに保存
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
        var json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(GetKeyConfigSavePath(), json);
    }

    /// <summary>
    /// JSONファイルから全てのキー設定を読み込む
    /// </summary>
    private void LoadAllKeyConfigsFromJson()
    {
        _overridePathDict.Clear();
        var path = GetKeyConfigSavePath();
        if (!System.IO.File.Exists(path)) return;

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
    }

    /// <summary>
    /// キーコンフィグの保存先パスを取得
    /// </summary>
    /// <returns>保存先パス</returns>
    private string GetKeyConfigSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "keyconfig.json");
    }

    /// <summary>
    /// キー設定の選択インデックスを移動
    /// </summary>
    /// <param name="dir">移動方向</param>
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
    /// 入力デバイスの移動入力を処理
    /// </summary>
    /// <param name="direction">移動ベクトル</param>
    private void OnMove(Vector2 direction)
    {
        if (_selectedDeviceIndex >= 0)
        {
            // 左右で状態ボタン切り替え
            if (Mathf.Abs(direction.x) > 0.5f)
            {
                _selectedDeviceIndex = 1 - _selectedDeviceIndex;
                UpdateSelectionVisual();
                return;
            }
            // 下でコンフィグボタンに移動
            if (direction.y < -0.5f)
            {
                _selectedDeviceIndex = -1;
                _selectedIndex = 0;
                _isResetSelected = false;
                UpdateSelectionVisual();
                return;
            }
            return;
        }

        // キー設定選択中の処理
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
            if (_selectedIndex < _keyConfigs.Length - 1)
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

        // 左右でリセットボタン切り替え
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

    /// <summary>
    /// 決定入力時の処理を行う
    /// </summary>
    private void OnSubmit()
    {
        if (_selectedDeviceIndex == 0 || _selectedDeviceIndex == 1)
        {
            // 状態ボタン選択時
            _activeDeviceType = (KeyDeviceType)_selectedDeviceIndex;
            UpdateSelectionVisual();
            _deviceLabelText.text = (_activeDeviceType == KeyDeviceType.Keyboard) ? "キーボード" : "ゲームパッド";
            UpdateKeyTexts();
            // バインディングも切り替え
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

        if (_isResetSelected)
            ResetBinding(_keyConfigs[_selectedIndex]);
        else
            StartRebind(_keyConfigs[_selectedIndex]);
    }

    /// <summary>
    /// メニューを閉じる処理を行う
    /// </summary>
    private void OnClose()
    {
        StartCoroutine(PlayAnimationUnscaled(_optionAnim, "OptionClose"));
    }

    /// <summary>
    /// アニメーションをスケール無視で再生するコルーチン
    /// </summary>
    /// <param name="anim">アニメーション</param>
    /// <param name="clipName">クリップ名</param>
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
    /// 選択状態のUIを更新
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

        for (int i = 0; i < _keyConfigs.Length; i++)
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

        _deviceLabelText.text = (_activeDeviceType == KeyDeviceType.Keyboard) ? "キーボード" : "ゲームパッド";
    }

    /// <summary>
    /// ゲームパッドのラベルを日本語表記に変換
    /// </summary>
    /// <param name="raw">元のラベル</param>
    /// <returns>日本語ラベル</returns>
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
    /// キー設定テキストを更新
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
    /// キーのリバインドを開始
    /// </summary>
    /// <param name="config">対象のキー設定データ</param>
    private void StartRebind(KeyConfigData config)
    {
        // オプション入力を一時的に無効化
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
                ShowErrorMessage($"{(_activeDeviceType == KeyDeviceType.Keyboard ? "キーボード" : "ゲームパッド")}のみ割り当て可能です");
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
                // 失敗してもオプション入力を再度有効化
                StartCoroutine(EnableOptionActionWithDelay(0.1f));
                ShowErrorMessage("選択されたキーは、既に他の項目で使用されています");
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
            // リバインド完了後、1秒待って有効化
            StartCoroutine(EnableOptionActionWithDelay(0.1f));
        })
        .Start();
    }

    /// <summary>
    /// オプションアクションを一定時間後に有効化するコルーチン
    /// </summary>
    /// <param name="delay">待機する時間</param>
    /// <returns></returns>
    private IEnumerator EnableOptionActionWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        InputActionHolder.Instance.optionInputActions.Option.Enable();
    }

    /// <summary>
    /// キー設定をデフォルトにリセット
    /// </summary>
    /// <param name="config">対象のキー設定データ</param>
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
    /// 他のキー設定と重複していないか判定
    /// </summary>
    /// <param name="current">現在の設定</param>
    /// <param name="overridePath">割り当てパス</param>
    /// <param name="bindingIndex">バインディングインデックス</param>
    /// <returns>重複していればtrue</returns>
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
    /// 全てのInputActionHolderにバインディングオーバーライドを適用
    /// </summary>
    /// <param name="config">対象のキー設定データ</param>
    /// <param name="overridePath">オーバーライドパス</param>
    /// <param name="bindingIndex">バインディングインデックス</param>
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
    /// エラーメッセージを表示する
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="duration">表示時間</param>
    private void ShowErrorMessage(string message, float duration = 2f)
    {
        if (_errorCoroutine != null)
            StopCoroutine(_errorCoroutine);
        _errorCoroutine = StartCoroutine(ShowErrorCoroutine(message, duration));
    }

    /// <summary>
    /// エラーメッセージを一定時間表示するコルーチン
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="duration">表示時間</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator ShowErrorCoroutine(string message, float duration)
    {
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        _errorText.gameObject.SetActive(false);
    }
}
