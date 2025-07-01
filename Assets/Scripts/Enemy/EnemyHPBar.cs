using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _fadeTime = 0.3f;
    private Transform _target;
    private Vector3 _offset = new Vector3(0, 1.5f, 0);
    private Coroutine _hideCoroutine;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Init(Transform target, Vector3? offset = null)
    {
        _target = target;
        if (offset.HasValue) _offset = offset.Value;
        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void SetHP(float hp, float maxHp)
    {
        float rate = hp / maxHp;
        _slider.value = rate;

        // êFÇäÑçáÇ≈ïœâª
        if (rate > 0.5f)
        {
            // óŒÅ®â©
            float t = (rate - 0.5f) / 0.5f; // 0.5Å`1.0Å®0Å`1
            _fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            // â©Å®ê‘
            float t = rate / 0.5f; // 0Å`0.5Å®0Å`1
            _fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
        }
    }

    public void ShowForSeconds(float seconds)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay(seconds));
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        yield return FadeOutAndDisable(_fadeTime);
    }

    private IEnumerator FadeOutAndDisable(float fadeTime)
    {
        float t = 0f;
        float startAlpha = _canvasGroup.alpha;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeTime);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void FadeOutAndDestroy(float fadeTime = 0.3f)
    {
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        StartCoroutine(FadeOutAndDestroyCoroutine(fadeTime));
    }

    private IEnumerator FadeOutAndDestroyCoroutine(float fadeTime)
    {
        float t = 0f;
        float startAlpha = _canvasGroup.alpha;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeTime);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (_target == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_target.position + _offset);
        transform.position = screenPos;
    }
}
