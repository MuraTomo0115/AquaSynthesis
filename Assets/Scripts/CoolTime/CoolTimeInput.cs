using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    [SerializeField] private Image coolDownImage;
    [SerializeField] private Image playCoolDownImage;
    [SerializeField] private TextMeshProUGUI coolDownText;
    [SerializeField] private TextMeshProUGUI playCoolDownText;
    [SerializeField] private float recordCoolTime = 5f;
    [SerializeField] private float playDuration = 10f;
    [SerializeField] private float playCoolTime = 5f;

    [SerializeField] private Key recordKey = Key.Q;
    [SerializeField] private Key playKey = Key.E;

    [SerializeField] private GameObject playIcon;

    private bool isRecordCoolingDown = false;
    private bool isPlaying = false;
    private bool isPlayCoolingDown = false;
    private bool hasRecorded = false;

    private Coroutine playCoroutine = null;
    private Coroutine playCoolDownCoroutine = null;

    private int playRepeatCount = 0;
    private float playCoolTimeCurrent = 0f;

    private void Start()
    {
        coolDownImage.gameObject.SetActive(false);
        coolDownText.gameObject.SetActive(false);

        playCoolDownImage.gameObject.SetActive(false);
        playCoolDownText.gameObject.SetActive(false);

        coolDownImage.fillAmount = 0f;
        playCoolDownImage.fillAmount = 0f;

        playIcon.SetActive(true);
    }

    private void Update()
    {
        if (Keyboard.current[recordKey].wasPressedThisFrame)
        {
            if (!isRecordCoolingDown && !isPlaying)
            {
                Debug.Log("記録開始！");
                hasRecorded = true;

                // 新たな記録があれば再生クールタイムをキャンセル
                if (isPlayCoolingDown && playCoolDownCoroutine != null)
                {
                    StopCoroutine(playCoolDownCoroutine);
                    isPlayCoolingDown = false;

                    playCoolDownImage.fillAmount = 0f;
                    playCoolDownImage.gameObject.SetActive(false);
                    playCoolDownText.gameObject.SetActive(false);

                    playRepeatCount = 0; // カウントもリセット
                }

                StartCoroutine(StartRecordCoolDown());
            }
            else
            {
                Debug.Log("記録不可：クールタイム中または再生中");
            }
        }

        if (Keyboard.current[playKey].wasPressedThisFrame)
        {
            if (isPlaying)
            {
                Debug.Log("再生中断！");
                StopCoroutine(playCoroutine);
                isPlaying = false;

                // 再生UIリセット
                playCoolDownImage.fillAmount = 0f;
                playCoolDownText.text = "";

                // クールタイム開始
                playRepeatCount++;
                playCoolTimeCurrent = playCoolTime * Mathf.Pow(1.5f, playRepeatCount - 1);
                playCoolDownCoroutine = StartCoroutine(StartPlayCoolDown());
            }
            else if (hasRecorded && !isPlayCoolingDown)
            {
                Debug.Log("再生開始！");
                playCoroutine = StartCoroutine(StartPlay());
            }
            else
            {
                Debug.Log("再生不可：記録なし／再生中／クールタイム中");
            }
        }
    }

    private System.Collections.IEnumerator StartRecordCoolDown()
    {
        isRecordCoolingDown = true;

        coolDownImage.gameObject.SetActive(true);
        coolDownText.gameObject.SetActive(true);

        float timer = recordCoolTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / recordCoolTime);
            coolDownImage.fillAmount = ratio;
            coolDownText.text = timer.ToString("F1");

            yield return null;
        }

        coolDownImage.fillAmount = 0f;
        coolDownImage.gameObject.SetActive(false);
        coolDownText.gameObject.SetActive(false);

        isRecordCoolingDown = false;
    }

    private System.Collections.IEnumerator StartPlay()
    {
        isPlaying = true;

        playCoolDownImage.gameObject.SetActive(true);
        playCoolDownText.gameObject.SetActive(true);

        float playTimer = playDuration;

        while (playTimer > 0f)
        {
            playTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(playTimer / playDuration);
            playCoolDownImage.fillAmount = ratio;
            playCoolDownText.text = playTimer.ToString("F1");

            yield return null;
        }

        isPlaying = false;

        // クールタイム設定（繰り返し再生で増加）
        playRepeatCount++;
        playCoolTimeCurrent = playCoolTime * Mathf.Pow(1.5f, playRepeatCount - 1);

        playCoolDownCoroutine = StartCoroutine(StartPlayCoolDown());
    }

    private System.Collections.IEnumerator StartPlayCoolDown()
    {
        isPlayCoolingDown = true;

        float coolTimer = playCoolTimeCurrent;

        while (coolTimer > 0f)
        {
            coolTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(coolTimer / playCoolTimeCurrent);
            playCoolDownImage.fillAmount = ratio;
            playCoolDownText.text = coolTimer.ToString("F1");

            yield return null;
        }

        playCoolDownImage.fillAmount = 0f;
        playCoolDownImage.gameObject.SetActive(false);
        playCoolDownText.gameObject.SetActive(false);

        isPlayCoolingDown = false;
    }
}