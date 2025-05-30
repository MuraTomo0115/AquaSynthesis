using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    //================ UI関連 ====================
    [Header("UI関連")]
    [SerializeField] private Image _coolDownImage;
    [SerializeField] private TextMeshProUGUI _coolDownText;
    [SerializeField] private Image _playCoolDownImage;
    [SerializeField] private TextMeshProUGUI _playCoolDownText;
    [SerializeField] private GameObject _playIcon;

    //================ 画面暗転用 ====================
    [Header("画面暗転用オーバーレイ")]
    [SerializeField] private Image _darkOverlay;

    //================ 能力制御 ====================
    [Header("能力制御")]
    [SerializeField] private RecordAbility _recordAbility;

    //================ プレイヤー関連 ====================
    [Header("プレイヤー")]
    [SerializeField] private Transform _playerTransform;

    //================ キー設定 ====================
    [Header("キー設定")]
    [SerializeField] private Key _recordKey = Key.Q;
    [SerializeField] private Key _playKey = Key.P;

    //================ 時間設定 ====================
    [Header("各種時間設定")]
    [SerializeField] private float _recordDuration = 10f;
    [SerializeField] private float _recordCoolTime = 5f;
    [SerializeField] private float _playDuration = 10f;
    [SerializeField] private float _playCoolTime = 5f;

    //================ 記録中に止めたいコンポーネント ====================
    [Header("記録中に止めたいコンポーネント")]
    [SerializeField] private List<MonoBehaviour> componentsToDisable;

    //================ 記録中に止めたい親オブジェクト ====================
    [Header("記録中に止めたい親オブジェクト")]
    [SerializeField] private GameObject parentToDisable;

    //================ 記録中に無効化したいプレイヤーのColliderや攻撃判定スクリプト ====================
    [Header("記録中に無効化したいプレイヤーのColliderや攻撃判定スクリプト")]
    [SerializeField] private List<Behaviour> playerComponentsToDisable;

    //================ 内部状態 ====================
    private bool _isRecording = false;
    private bool _isRecordCoolingDown = false;
    private bool _isPlaying = false;
    private bool _isPlayCoolingDown = false;
    private bool _hasRecorded = false;

    private Coroutine _recordCoroutine = null;
    private Coroutine _playCoroutine = null;
    private Coroutine _playCoolDownCoroutine = null;

    private int _playRepeatCount = 0;
    private float _playCoolTimeCurrent = 0f;

    private Vector3 _playerStartPos;

    // 一時的に無効化したコンポーネントを保持
    private List<MonoBehaviour> _disabledComponents = new List<MonoBehaviour>();
    private List<Behaviour> _playerDisabledComponents = new List<Behaviour>();

    private void Start()
    {
        _coolDownImage.gameObject.SetActive(false);
        _coolDownText.gameObject.SetActive(false);
        _playCoolDownImage.gameObject.SetActive(false);
        _playCoolDownText.gameObject.SetActive(false);
        _coolDownImage.fillAmount = 0f;
        _playCoolDownImage.fillAmount = 0f;
        _playIcon.SetActive(true);

        if (_darkOverlay != null)
        {
            SetOverlayAlpha(0f);
        }
    }

    private void Update()
    {
        // Qキー押下で記録開始
        if (Keyboard.current[_recordKey].wasPressedThisFrame)
        {
            if (!_isRecording && !_isRecordCoolingDown && !_isPlaying && !_isPlayCoolingDown)
            {
                if (_recordCoroutine != null) StopCoroutine(_recordCoroutine);
                _recordCoroutine = StartCoroutine(_StartRecordMode());
            }
        }

        // Pキー押下で再生開始 or 中断
        if (Keyboard.current[_playKey].wasPressedThisFrame)
        {
            if (_isPlaying)
            {
                _recordAbility?.StopPlayback();
                if (_playCoroutine != null) StopCoroutine(_playCoroutine);
                _isPlaying = false;

                _playRepeatCount++;
                _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);
                StartCoroutine(_StartPlayCoolDown());
            }
            else if (_hasRecorded && !_isPlayCoolingDown)
            {
                _playCoroutine = StartCoroutine(_StartPlay());
            }
        }
    }

    /// <summary>
    /// 記録モードを開始する
    /// </summary>
    private IEnumerator _StartRecordMode()
    {
        _isRecording = true;

        // プレイヤーのコライダーや攻撃判定を無効化
        foreach (var comp in playerComponentsToDisable)
        {
            if (comp != null && comp.enabled)
            {
                comp.enabled = false;
                _playerDisabledComponents.Add(comp);
            }
        }

        // プレイヤー以外の動きを止める（個別指定分）
        foreach (var comp in componentsToDisable)
        {
            if (comp != null && comp.enabled)
            {
                comp.enabled = false;
                _disabledComponents.Add(comp);
            }
        }

        // 親オブジェクト配下の全MonoBehaviourを無効化
        if (parentToDisable != null)
        {
            var components = parentToDisable.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                // プレイヤー自身や記録に必要なものは除外（必要なら条件追加）
                if (comp != null && comp.enabled && comp.gameObject != _playerTransform.gameObject && !_disabledComponents.Contains(comp))
                {
                    comp.enabled = false;
                    _disabledComponents.Add(comp);
                }
            }
        }

        _recordAbility?.StartRecording();

        if (_darkOverlay != null)
        {
            SetOverlayAlpha(0.5f);
        }

        _playerStartPos = _playerTransform.position;

        float timer = 0f;
        _coolDownImage.gameObject.SetActive(true);
        _coolDownText.gameObject.SetActive(true);

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

        _recordAbility?.StopRecording();

        _hasRecorded = true;
        _isRecording = false;

        _playRepeatCount = 0;

        _playerTransform.position = _playerStartPos;

        if (_darkOverlay != null)
        {
            SetOverlayAlpha(0f);
        }

        // 止めていたコンポーネントを再度有効化
        foreach (var comp in _disabledComponents)
        {
            if (comp != null) comp.enabled = true;
        }
        _disabledComponents.Clear();

        // プレイヤーのコライダーや攻撃判定を再度有効化
        foreach (var comp in _playerDisabledComponents)
        {
            if (comp != null) comp.enabled = true;
        }
        _playerDisabledComponents.Clear();

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

        _recordAbility?.StartPlayback();

        _playCoolDownImage.gameObject.SetActive(true);
        _playCoolDownText.gameObject.SetActive(true);

        float playTimer = _playDuration;

        while (playTimer > 0f)
        {
            playTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(playTimer / _playDuration);
            _playCoolDownImage.fillAmount = ratio;
            _playCoolDownText.text = playTimer.ToString("F1");

            yield return null;
        }

        _isPlaying = false;

        _recordAbility?.StopPlayback();

        _playRepeatCount++;
        _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);

        _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());
    }

    /// <summary>
    /// 再生モード終了後のクールタイム処理
    /// </summary>
    private IEnumerator _StartPlayCoolDown()
    {
        _isPlayCoolingDown = true;

        float coolTimer = _playCoolTimeCurrent;

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
    private void SetOverlayAlpha(float alpha)
    {
        if (_darkOverlay == null) return;
        Color c = _darkOverlay.color;
        c.a = alpha;
        _darkOverlay.color = c;
    }
}