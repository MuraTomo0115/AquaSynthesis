using UnityEngine;

public class WaterWallController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;      // 壁の見た目用SpriteRenderer
    [SerializeField] private float _normalAlpha = 0.8f;     // 通常時の不透明度
    [SerializeField] private float _transparentAlpha = 0.2f;// 記録中の最小透明度
    [SerializeField] private float _flickerSpeed = 1.5f;    // 点滅（透明度変化）の速さ

    private bool _isFlicker = false;    // 記録中の点滅演出フラグ
    private bool _isDisabled = false;   // 壁が無効化されたかどうか
    private Collider2D _collider;       // 壁のコリジョン

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Inspectorで「Reset」した時にSpriteRendererを自動取得
    /// </summary>
    private void Reset()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 記録中などで壁を点滅（透明度変化）させるか切り替え
    /// </summary>
    /// <param name="isTransparent">trueで点滅開始、falseで通常表示</param>
    public void SetTransparent(bool isTransparent)
    {
        _isFlicker = isTransparent;
        if (!isTransparent && _renderer != null)
        {
            // 通常時は不透明度を戻す
            var color = _renderer.color;
            color.a = _normalAlpha;
            _renderer.color = color;
        }
    }

    /// <summary>
    /// 記録中は透明度を周期的に変化させて点滅演出
    /// </summary>
    private void Update()
    {
        // 無効化されていない＆点滅中のみ透明度を変化
        if (_isFlicker && _renderer != null && !_isDisabled)
        {
            // sin波で0.2～0.8の間を往復
            float t = (Mathf.Sin(Time.time * _flickerSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(_transparentAlpha, _normalAlpha, t);
            var color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }

    /// <summary>
    /// ノズル等から呼ばれると壁を無効化（透明＆コリジョン無効）
    /// </summary>
    public void DisableWall()
    {
        _isDisabled = true;
        if (_renderer != null)
        {
            // 完全に透明に
            var color = _renderer.color;
            color.a = 0f;
            _renderer.color = color;
        }
        if (_collider != null)
        {
            // コリジョンも無効化
            _collider.enabled = false;
        }
        // 必要ならゲームオブジェクト自体を非アクティブにしてもOK
        // gameObject.SetActive(false);
    }
}
