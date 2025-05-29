using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    [Header("UI関連")]
    [SerializeField] private Image _coolDownImage;
    [SerializeField] private TextMeshProUGUI _coolDownText;

    [SerializeField] private Image _playCoolDownImage;
    [SerializeField] private TextMeshProUGUI _playCoolDownText;

    [SerializeField] private GameObject _playIcon;

    [Header("画面暗転用オーバーレイ")]
    [SerializeField] private Image _darkOverlay;

    [Header("能力制御")]
    [SerializeField] private RecordAbility _recordAbility;

    [Header("プレイヤー")]
    [SerializeField] private Transform _playerTransform;

    [Header("キー設定")]
    [SerializeField] private Key _recordKey = Key.Q;
    [SerializeField] private Key _playKey = Key.P;

    [Header("各種時間設定")]
    [SerializeField] private float _recordDuration = 10f;
    [SerializeField] private float _recordCoolTime = 5f;
    [SerializeField] private float _playDuration = 10f;
    [SerializeField] private float _playCoolTime = 5f;

    [Header("記録中でも動かしたいスクリプト")]
    [SerializeField] private List<MonoBehaviour> _objectsToKeepRunning = new List<MonoBehaviour>();

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
        if (Keyboard.current[_recordKey].wasPressedThisFrame)
        {
            if (!_isRecording && !_isRecordCoolingDown && !_isPlaying && !_isPlayCoolingDown)
            {
                if (_recordCoroutine != null) StopCoroutine(_recordCoroutine);
                _recordCoroutine = StartCoroutine(_StartRecordMode());
            }
        }

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

    private IEnumerator _StartRecordMode()
    {
        _isRecording = true;

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

        // 🔽 再生クールタイムのリセット処理
        _playRepeatCount = 0;

        _playerTransform.position = _playerStartPos;

        if (_darkOverlay != null)
        {
            SetOverlayAlpha(0f);
        }

        StartCoroutine(_StartRecordCoolDown());
    }

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

    private void SetOverlayAlpha(float alpha)
    {
        if (_darkOverlay == null) return;
        Color c = _darkOverlay.color;
        c.a = alpha;
        _darkOverlay.color = c;
    }
}
