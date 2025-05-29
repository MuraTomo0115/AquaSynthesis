using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    [Header("UI関連")]
    [SerializeField] private Image _coolDownImage;
    [SerializeField] private TextMeshProUGUI _coolDownText;

    [SerializeField] private Image _playCoolDownImage;
    [SerializeField] private TextMeshProUGUI _playCoolDownText;

    [SerializeField] private GameObject _playIcon;

    [Header("記録中UI")]
    [SerializeField] private Image _recordProgressImage;
    [SerializeField] private TextMeshProUGUI _recordProgressText;

    [Header("画面暗転用オーバーレイ")]
    [SerializeField] private Image _darkOverlay;

    [Header("能力制御")]
    [SerializeField] private RecordAbility _recordAbility;

    [Header("キー設定")]
    [SerializeField] private Key _recordKey = Key.Q;
    [SerializeField] private Key _playKey = Key.E;

    [Header("各種時間設定")]
    [SerializeField] private float _recordDuration = 10f;
    [SerializeField] private float _recordCoolTime = 5f;
    [SerializeField] private float _playDuration = 10f;
    [SerializeField] private float _playCoolTime = 5f;

    private bool _isRecording = false;
    private bool _isRecordCoolingDown = false;

    private bool _isPlaying = false;
    private bool _isPlayCoolingDown = false;

    private bool _hasRecorded = false;

    private Coroutine _playCoroutine = null;
    private Coroutine _playCoolDownCoroutine = null;

    private Coroutine _recordCoroutine = null;
    private int _playRepeatCount = 0;
    private float _playCoolTimeCurrent = 0f;

    private void Start()
    {
        _coolDownImage.gameObject.SetActive(false);
        _coolDownText.gameObject.SetActive(false);

        _playCoolDownImage.gameObject.SetActive(false);
        _playCoolDownText.gameObject.SetActive(false);

        _recordProgressImage.gameObject.SetActive(false);
        _recordProgressText.gameObject.SetActive(false);

        _coolDownImage.fillAmount = 0f;
        _playCoolDownImage.fillAmount = 0f;
        _recordProgressImage.fillAmount = 0f;

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
            if (!_isRecording && !_isRecordCoolingDown && !_isPlaying)
            {
                if (_recordCoroutine != null)
                {
                    StopCoroutine(_recordCoroutine);
                }
                _recordCoroutine = StartCoroutine(_StartRecordMode());
            }
        }

        if (Keyboard.current[_playKey].wasPressedThisFrame)
        {
            if (_isPlaying)
            {
                if (_recordAbility != null)
                {
                    _recordAbility.StopPlayback();
                }

                if (_playCoroutine != null)
                {
                    StopCoroutine(_playCoroutine);
                    _playCoroutine = null;
                }

                _isPlaying = false;

                _playRepeatCount++;
                _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);
                _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());

                _playCoolDownImage.fillAmount = 1f;
                _playCoolDownText.text = _playCoolTimeCurrent.ToString("F1");
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

        if (_recordAbility != null)
        {
            _recordAbility.StartRecording();
        }

        if (_darkOverlay != null)
        {
            SetOverlayAlpha(0.5f);
        }

        // 記録中UI表示
        _recordProgressImage.gameObject.SetActive(true);
        _recordProgressText.gameObject.SetActive(true);

        float timer = _recordDuration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / _recordDuration);
            _recordProgressImage.fillAmount = ratio;
            _recordProgressText.text = timer.ToString("F1");
            yield return null;
        }

        // 記録中UI非表示
        _recordProgressImage.fillAmount = 0f;
        _recordProgressImage.gameObject.SetActive(false);
        _recordProgressText.gameObject.SetActive(false);

        _isRecording = false;

        if (_recordAbility != null)
        {
            _recordAbility.StopRecording();
        }

        _hasRecorded = true;

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

        if (_recordAbility != null)
        {
            _recordAbility.StartPlayback();
        }

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

        if (_recordAbility != null)
        {
            _recordAbility.StopPlayback();
        }

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
        Color c = _darkOverlay.color;
        c.a = alpha;
        _darkOverlay.color = c;
    }
}
