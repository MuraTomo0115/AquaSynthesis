using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class NozzleController : MonoBehaviour, IGimmickActivatable
{
    [SerializeField] private WaterWallController waterWall;
    [SerializeField] public static float activationRadius = 1.0f;
    private Animation _animation;
    private SpriteRenderer _renderer;
    private bool isActivated = false;
    [SerializeField] private TextMeshProUGUI actionPromptText;

    public bool CanActivateByEcho => true;

    private int _echoLayer;
    private int _playerLayer;

    // トリガーで範囲内のアクターを管理
    private HashSet<GameObject> _actorsInRange = new HashSet<GameObject>();

    private void Start()
    {
        _animation = GetComponent<Animation>();
        _renderer = GetComponent<SpriteRenderer>();
        _echoLayer = LayerMask.NameToLayer("Echo");
        _playerLayer = LayerMask.NameToLayer("Player");

        // CircleCollider2Dがあれば自動で設定
        var col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = activationRadius;
        }
    }

    /// <summary>
    /// プレイヤーまたはEchoがノズルのアクティベーション範囲内にいるかチェック
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public bool IsPlayerInRange(GameObject actor)
    {
        return _actorsInRange.Contains(actor);
    }

    /// <summary>
    /// ノズルをアクティブ化する
    /// </summary>
    /// <param name="actor"></param>
    public void Activate(GameObject actor)
    {
        if (isActivated) return;
        isActivated = true;

        _animation.Play();

        // テキスト非表示
        if (actionPromptText != null)
            actionPromptText.enabled = false;

        StartCoroutine(WaitAnimationAndFade());
    }

    /// <summary>
    /// アニメーションの再生が終わるまで待機し、NozzleとWaterWallをフェードアウト
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAnimationAndFade()
    {
        AnimationClip clip = _animation.clip;
        float waitTime = (clip != null) ? clip.length : 0.5f;
        yield return new WaitForSeconds(waitTime);

        // ノズル自身のフェードアウト
        if (_renderer != null)
        {
            _renderer.DOFade(0f, 1.0f)
                .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }

        // WaterWallも同時にフェードアウト
        if (waterWall != null)
        {
            waterWall.FadeOutAndDisable(1.0f);
        }
    }

    private void Update()
    {
        if (isActivated)
        {
            if (actionPromptText != null)
                actionPromptText.enabled = false;
            return;
        }

        // 範囲内のアクターを探す
        GameObject actor = null;
        foreach (var obj in _actorsInRange)
        {
            if (obj != null)
            {
                actor = obj;
                break;
            }
        }

        if (actor != null)
        {
            if (actionPromptText != null)
            {
                actionPromptText.enabled = true;
                actionPromptText.text = "回す";
            }
        }
        else
        {
            if (actionPromptText != null)
                actionPromptText.enabled = false;
        }
    }

    /// <summary>
    /// トリガーに入ったアクターを管理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == _echoLayer || other.gameObject.layer == _playerLayer)
        {
            _actorsInRange.Add(other.gameObject);
        }
    }

    /// <summary>
    /// トリガーから出たアクターを管理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (_actorsInRange.Contains(other.gameObject))
        {
            _actorsInRange.Remove(other.gameObject);
        }
    }
}
