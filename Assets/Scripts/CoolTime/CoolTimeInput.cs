using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    [SerializeField] private Image _coolDownImage;
    [SerializeField] private Image _playCoolDownImage;
    [SerializeField] private TextMeshProUGUI _coolDownText;
    [SerializeField] private TextMeshProUGUI _playCoolDownText;
    [SerializeField] private float _recordCoolTime = 5f;
    [SerializeField] private float _playDuration = 10f;
    [SerializeField] private float _playCoolTime = 5f;

    [SerializeField] private Key _recordKey = Key.Q;
    [SerializeField] private Key _playKey = Key.E;

    [SerializeField] private GameObject _playIcon;

    private bool _isRecordCoolingDown = false;
    private bool _isPlaying = false;
    private bool _isPlayCoolingDown = false;
    private bool _hasRecorded = false;

    private Coroutine _playCoroutine = null;
    private Coroutine _playCoolDownCoroutine = null;

    private int _playRepeatCount = 0;
    private float _playCoolTimeCurrent = 0f;

    /// <summary>
    /// 初期化処理：UIの非表示と初期状態の設定
    /// </summary>
    private void Start()
    {
        _coolDownImage.gameObject.SetActive(false);
        _coolDownText.gameObject.SetActive(false);

        _playCoolDownImage.gameObject.SetActive(false);
        _playCoolDownText.gameObject.SetActive(false);

        _coolDownImage.fillAmount = 0f;
        _playCoolDownImage.fillAmount = 0f;

        _playIcon.SetActive(true);
    }

    /// <summary>
    /// 入力チェック：録画と再生のキー入力を監視
    /// </summary>
    private void Update()
    {
        if (Keyboard.current[_recordKey].wasPressedThisFrame)
        {
            if (!_isRecordCoolingDown && !_isPlaying)
            {
                _hasRecorded = true;

                // 新たに記録された場合、再生のクールタイムを中断してリセット
                if (_isPlayCoolingDown && _playCoolDownCoroutine != null)
                {
                    StopCoroutine(_playCoolDownCoroutine);
                    _isPlayCoolingDown = false;

                    _playCoolDownImage.fillAmount = 0f;
                    _playCoolDownImage.gameObject.SetActive(false);
                    _playCoolDownText.gameObject.SetActive(false);

                    _playRepeatCount = 0;
                }

                StartCoroutine(_StartRecordCoolDown());
            }
        }

        if (Keyboard.current[_playKey].wasPressedThisFrame)
        {
            if (_isPlaying)
            {
                StopCoroutine(_playCoroutine);
                _isPlaying = false;

                _playCoolDownImage.fillAmount = 0f;
                _playCoolDownText.text = "";

                _playRepeatCount++;
                _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);
                _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());
            }
            else if (_hasRecorded && !_isPlayCoolingDown)
            {
                _playCoroutine = StartCoroutine(_StartPlay());
            }
        }
    }

    /// <summary>
    /// 記録開始後のクールタイム処理
    /// </summary>
    private System.Collections.IEnumerator _StartRecordCoolDown()
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
    /// 再生処理：指定時間の間、再生状態を維持
    /// </summary>
    private System.Collections.IEnumerator _StartPlay()
    {
        _isPlaying = true;

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

        _playRepeatCount++;
        _playCoolTimeCurrent = _playCoolTime * Mathf.Pow(1.5f, _playRepeatCount - 1);

        _playCoolDownCoroutine = StartCoroutine(_StartPlayCoolDown());
    }

    /// <summary>
    /// 再生後のクールタイム処理（回数に応じて時間増加）
    /// </summary>
    private System.Collections.IEnumerator _StartPlayCoolDown()
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
}