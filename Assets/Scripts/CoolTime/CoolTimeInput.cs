using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// クールタイム付きの記録・再生入力管理クラス。
/// 記録・再生のUI表示、クールタイム管理、入力受付、記録中の一時停止処理などを担当。
/// </summary>
public class CoolTimeInput : MonoBehaviour
{
    //================ UI関連 ====================
    [Header("UI関連")]
    [SerializeField] private Image _coolDownImage; // 記録クールタイム用イメージ
    [SerializeField] private TextMeshProUGUI _coolDownText; // 記録クールタイム用テキスト
    [SerializeField] private Image _playCoolDownImage; // 再生クールタイム用イメージ
    [SerializeField] private TextMeshProUGUI _playCoolDownText; // 再生クールタイム用テキスト
    [SerializeField] private GameObject _playIcon; // 再生アイコン

    //================ 画面暗転用 ====================
    [Header("画面暗転用オーバーレイ")]
    [SerializeField] private Image _darkOverlay; // 画面暗転用オーバーレイ

    //================ 能力制御 ====================
    [Header("能力制御")]
    [SerializeField] private RecordAbility _recordAbility; // 記録・再生機能

    //================ プレイヤー関連 ====================
    [Header("プレイヤー")]
    [SerializeField] private Transform _playerTransform; // プレイヤーTransform

    //================ キー設定 (Input System) ====================
    [Header("キー設定 (Input System)")]
    [SerializeField] private InputActionReference _recordAction; // 記録アクション
    [SerializeField] private InputActionReference _playAction;   // 再生アクション

    //================ 時間設定 ====================
    [Header("各種時間設定")]
    [SerializeField] private float _recordDuration = 10f; // 記録時間
    [SerializeField] private float _recordCoolTime = 5f; // 記録クールタイム
    [SerializeField] private float _playDuration = 10f; // 再生時間
    [SerializeField] private float _playCoolTime = 5f; // 再生クールタイム

    //================ 記録中に止めたいコンポーネント ====================
    [Header("記録中に止めたいコンポーネント")]
    [SerializeField] private List<MonoBehaviour> componentsToDisable; // 記録中に停止するコンポーネント

    //================ 記録中に止めたい親オブジェクト ====================
    [Header("記録中に止めたい親オブジェクト")]
    [SerializeField] private GameObject parentToDisable; // 記録中に停止する親オブジェクト

    //================ 記録中に無効化したいプレイヤーのColliderや攻撃判定スクリプト ====================
    [Header("記録中に無効化したいプレイヤーのColliderや攻撃判定スクリプト")]
    [SerializeField] private List<Behaviour> playerComponentsToDisable; // 記録中に無効化するプレイヤーのコンポーネント

    //================ 内部状態 ====================
    private bool _isRecording = false; // 記録中フラグ
    private bool _isRecordCoolingDown = false; // 記録クールタイム中フラグ
    private bool _isPlaying = false; // 再生中フラグ
    private bool _isPlayCoolingDown = false; // 再生クールタイム中フラグ
    private bool _hasRecorded = false; // 記録済みフラグ

    private Coroutine _recordCoroutine = null; // 記録コルーチン
    private Coroutine _playCoroutine = null; // 再生コルーチン
    private Coroutine _playCoolDownCoroutine = null; // 再生クールタイムコルーチン

    private int _playRepeatCount = 0; // 再生繰り返し回数
    private float _playCoolTimeCurrent = 0f; // 現在の再生クールタイム

    private Vector3 _playerStartPos; // 記録開始時のプレイヤー位置

    // 一時的に無効化したコンポーネントを保持
    private readonly List<MonoBehaviour> _disabledComponents = new List<MonoBehaviour>();
    private readonly List<Behaviour> _playerDisabledComponents = new List<Behaviour>();

    /// <summary>
    /// UIやオーバーレイの初期化
    /// </summary>
    private void Start()
    {
        // UI初期化
        _coolDownImage.gameObject.SetActive(false);
        _coolDownImage.fillAmount = 0f;
        _coolDownText.gameObject.SetActive(false);
        _playCoolDownImage.gameObject.SetActive(false);
        _playCoolDownImage.fillAmount = 0f;
        _playCoolDownText.gameObject.SetActive(false);
        _playIcon.SetActive(true);

        // 画面暗転初期化
        SetOverlayAlpha(0f);
    }

    /// <summary>
    /// 入力アクションの有効化
    /// </summary>
    private void OnEnable()
    {
        _recordAction.action.Enable();
        _recordAction.action.performed += OnRecordPerformed;
        _playAction.action.Enable();
        _playAction.action.performed += OnPlayPerformed;
    }

    /// <summary>
    /// 入力アクションの無効化
    /// </summary>
    private void OnDisable()
    {
        _recordAction.action.performed -= OnRecordPerformed;
        _recordAction.action.Disable();
        _playAction.action.performed -= OnPlayPerformed;
        _playAction.action.Disable();
    }

    /// <summary>
    /// 記録ボタンが押された時の処理
    /// </summary>
    private void OnRecordPerformed(InputAction.CallbackContext context)
    {
        // 記録中なら記録を停止
        if (_isRecording)
        {
            if (_recordCoroutine != null) StopCoroutine(_recordCoroutine);
            _recordAbility.StopRecording();
            _isRecording = false;

            _hasRecorded = true;
            _playRepeatCount = 0;

            // UIや暗転解除、コンポーネント再有効化など
            _coolDownImage.fillAmount = 0f;
            _coolDownImage.gameObject.SetActive(false);
            _coolDownText.gameObject.SetActive(false);
            SetOverlayAlpha(0f);

            foreach (var comp in _disabledComponents)
            {
                comp.enabled = true;
            }
            _disabledComponents.Clear();

            foreach (var comp in _playerDisabledComponents)
            {
                comp.enabled = true;
            }
            _playerDisabledComponents.Clear();

            // プレイヤー位置を記録直前に戻す
            _playerTransform.position = _playerStartPos;

            // クールタイム開始
            StartCoroutine(_StartRecordCoolDown());
            return;
        }

        // 記録・再生・クールタイム中は記録開始不可
        if (!_isRecording && !_isRecordCoolingDown && !_isPlaying && !_isPlayCoolingDown)
        {
            if (_recordCoroutine != null) StopCoroutine(_recordCoroutine);
            _recordCoroutine = StartCoroutine(_StartRecordMode());
        }
    }

    /// <summary>
    /// 再生ボタンが押された時の処理
    /// </summary>
    private void OnPlayPerformed(InputAction.CallbackContext context)
    {
        // 記録中は再生不可
        if (_isRecording) return;

        // 再生中なら中断、そうでなければ再生開始
        if (_isPlaying)
        {
            _recordAbility?.StopPlayback();
            if (_playCoroutine != null) StopCoroutine(_playCoroutine);
            _isPlaying = false;

            // クールタイム計算
            _playRepeatCount++;
            _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);
            if (_playCoolDownCoroutine != null) StopCoroutine(_playCoolDownCoroutine);
            _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());
        }
        else if (_hasRecorded && !_isPlayCoolingDown)
        {
            if (_playCoroutine != null) StopCoroutine(_playCoroutine);
            _playCoroutine = StartCoroutine(_StartPlay());
        }
    }

    /// <summary>
    /// 記録モードを開始する
    /// UI・コンポーネント制御・暗転・クールタイム管理を行う
    /// </summary>
    private IEnumerator _StartRecordMode()
    {
        _isRecording = true;

        // プレイヤーのコライダーや攻撃判定を無効化
        foreach (var comp in playerComponentsToDisable)
        {
            comp.enabled = false;
            _playerDisabledComponents.Add(comp);
        }

        // プレイヤー以外の動きを止める（個別指定分）
        foreach (var comp in componentsToDisable)
        {
            comp.enabled = false;
            _disabledComponents.Add(comp);
        }

        // 親オブジェクト配下の全MonoBehaviourを無効化
        if (parentToDisable != null)
        {
            var components = parentToDisable.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                // プレイヤー自身や記録に必要なものは除外
                if (comp.gameObject != _playerTransform.gameObject && !_disabledComponents.Contains(comp))
                {
                    comp.enabled = false;
                    _disabledComponents.Add(comp);
                }
            }
        }

        // 記録開始
        _recordAbility.StartRecording();

        // 画面暗転
        SetOverlayAlpha(0.5f);

        // プレイヤー位置保存
        _playerStartPos = _playerTransform.position;

        float timer = 0f;
        _coolDownImage.gameObject.SetActive(true);
        _coolDownText.gameObject.SetActive(true);

        // 記録時間のカウントダウン
        while (timer < _recordDuration)
        {
            timer += Time.unscaledDeltaTime;

            float ratio = Mathf.Clamp01((_recordDuration - timer) / _recordDuration);
            _coolDownImage.fillAmount = ratio;
            _coolDownText.text = (_recordDuration - timer).ToString("F1");

            yield return null;
        }

        _coolDownImage.fillAmount = 0f;
        _coolDownImage.gameObject.SetActive(false);
        _coolDownText.gameObject.SetActive(false);

        // 記録停止
        _recordAbility.StopRecording();

        _hasRecorded = true;
        _isRecording = false;
        _playRepeatCount = 0;

        // プレイヤー位置を元に戻す
        _playerTransform.position = _playerStartPos;

        // 画面暗転解除
        SetOverlayAlpha(0f);

        // 止めていたコンポーネントを再度有効化
        foreach (var comp in _disabledComponents)
        {
            comp.enabled = true;
        }
        _disabledComponents.Clear();

        // プレイヤーのコライダーや攻撃判定を再度有効化
        foreach (var comp in _playerDisabledComponents)
        {
            comp.enabled = true;
        }
        _playerDisabledComponents.Clear();

        // 記録クールタイム開始
        StartCoroutine(_StartRecordCoolDown());
    }

    /// <summary>
    /// 記録モード終了後のクールタイム処理
    /// </summary>
    private IEnumerator _StartRecordCoolDown()
    {
        _isRecordCoolingDown = true;

        _coolDownImage.gameObject.SetActive(true);
        _coolDownText.gameObject.SetActive(true);

        float timer = _recordCoolTime;

        // クールタイムのカウントダウン
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / _recordCoolTime);
            _coolDownImage.fillAmount = ratio;
            _coolDownText.text = timer.ToString("F1");

            yield return null;
        }

        _coolDownImage.fillAmount = 0f;
        _coolDownImage.gameObject.SetActive(false);
        _coolDownText.gameObject.SetActive(false);

        _isRecordCoolingDown = false;
    }

    /// <summary>
    /// 再生モードを開始する
    /// </summary>
    private IEnumerator _StartPlay()
    {
        _isPlaying = true;

        // 再生開始
        _recordAbility.StartPlayback();

        _playCoolDownImage.gameObject.SetActive(true);
        _playCoolDownText.gameObject.SetActive(true);

        float playTimer = _playDuration;

        // 再生時間のカウントダウン
        while (playTimer > 0f)
        {
            playTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(playTimer / _playDuration);
            _playCoolDownImage.fillAmount = ratio;
            _playCoolDownText.text = playTimer.ToString("F1");

            yield return null;
        }

        _isPlaying = false;

        // 再生停止
        _recordAbility.StopPlayback();

        // クールタイム計算
        _playRepeatCount++;
        _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);

        // 再生クールタイム開始
        _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());
    }

    /// <summary>
    /// 再生モード終了後のクールタイム処理
    /// </summary>
    private IEnumerator _StartPlayCoolDown()
    {
        _isPlayCoolingDown = true;

        float coolTimer = _playCoolTimeCurrent;

        // クールタイムのカウントダウン
        while (coolTimer > 0f)
        {
            coolTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(coolTimer / _playCoolTimeCurrent);
            _playCoolDownImage.fillAmount = ratio;
            _playCoolDownText.text = coolTimer.ToString("F1");

            yield return null;
        }

        _playCoolDownImage.fillAmount = 0f;
        _playCoolDownImage.gameObject.SetActive(false);
        _playCoolDownText.gameObject.SetActive(false);

        _isPlayCoolingDown = false;
    }

    /// <summary>
    /// 画面暗転用の透明度を設定
    /// </summary>
    /// <param name="alpha">アルファ値（0～1）</param>
    private void SetOverlayAlpha(float alpha)
    {
        Color c = _darkOverlay.color;
        c.a = alpha;
        _darkOverlay.color = c;
    }
    public void StopRecording()
    {
        if (_recordCoroutine != null) StopCoroutine(_recordCoroutine);
        // _savedRecordは消さない
    }
}