using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NozzleController : MonoBehaviour, IGimmickActivatable
{
    [SerializeField] private WaterWallController waterWall;
    [SerializeField] private float activationRadius = 1.0f;
    private Animation _animation;
    private SpriteRenderer _renderer;
    private bool isActivated = false;
    [SerializeField] private UnityEngine.UI.Text actionPromptText;

    public bool CanActivateByEcho => true;

    private int _echoLayer;
    private int _playerLayer;

    private void Start()
    {
        _animation = GetComponent<Animation>();
        _renderer = GetComponent<SpriteRenderer>();
        _echoLayer = LayerMask.NameToLayer("Echo");
        _playerLayer = LayerMask.NameToLayer("Player");
    }

    /// <summary>
    /// プレイヤーまたはEchoがノズルのアクティベーション範囲内にいるかチェック
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public bool IsPlayerInRange(GameObject actor)
    {
        int layer = actor.layer;
        return (layer == _echoLayer || layer == _playerLayer)
            && Vector2.Distance(transform.position, actor.transform.position) < activationRadius;
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

        GameObject player = FindPlayerOrEcho();
        if (player != null && IsPlayerInRange(player))
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
    /// プレイヤーまたはEchoを探す
    /// </summary>
    /// <returns></returns>
    private GameObject FindPlayerOrEcho()
    {
        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.layer == _echoLayer)
                return obj;
            if (obj.layer == _playerLayer)
                return obj;
        }
        return null;
    }
}
