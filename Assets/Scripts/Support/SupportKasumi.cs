using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SupportKasumiStatus
{
	public float healRange;
	public float healAmount;
	public float healInterval;
	public float healDuration;
}

public class SupportKasumi : SupportBase
{
	private float _healRange;
	private float _healAmount;
	private float _healInterval;
	private float _healDuration;
	private float _innerRadius;
    private CircleCollider2D _shieldCollider;

	[Header("遠距離攻撃")]
    [SerializeField] private LayerMask targetLayer;

	private Coroutine healCoroutine;
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();

    protected override void Awake()
	{
		base.Awake();
		LoadKasumiStatus();
        _shieldCollider = GetComponent<CircleCollider2D>();

        if (_shieldCollider != null)
        {
            _innerRadius = _healRange;
            _shieldCollider.radius = _innerRadius;
        }
    }

    /// <summary>
    /// JSONファイルからステータスを読み込む
    /// </summary>
    private void LoadKasumiStatus()
	{
		TextAsset json = Resources.Load<TextAsset>("Status/KasumiStatus");
		if (json != null)
		{
			SupportKasumiStatus status = JsonUtility.FromJson<SupportKasumiStatus>(json.text);
			_healRange = status.healRange;
			_healAmount = status.healAmount;
			_healInterval = status.healInterval;
			_healDuration = status.healDuration;
			Debug.Log(_healAmount + " " + _healInterval + " " + _healDuration + " " + _healRange);
		}
		else
		{
			Debug.LogWarning("SupportKasumiStatus.jsonが見つかりません。");
		}
	}

    /// <summary>
    /// 回復・シールド範囲に入ったときの処理
    /// </summary>
    /// <param name="other">衝突したオブジェクト</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        if (((1 << otherLayer) & targetLayer) == 0) return;

        Vector2 objPos = other.transform.position;
        Vector2 centerPos = transform.position;
        float dist = Vector2.Distance(objPos, centerPos);

        // 範囲の半径のしきい値（例えば半分の距離）を設定
        float threshold = _healRange * 0.9f;

        if (dist > threshold)
        {
            // 外側から入った → 即消し
            Destroy(other.gameObject);
        }
        else
        {
            // 内側から入った → 範囲外に出るまで追跡
            trackedObjects.Add(other.gameObject);
        }
    }

    /// <summary>
    /// 回復・シールド範囲から出たときの処理
    /// </summary>
    /// <param name="other">衝突したオブジェクト</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        if (((1 << otherLayer) & targetLayer) == 0) return;

        if (trackedObjects.Contains(other.gameObject))
        {
            trackedObjects.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// 回復アクションを開始する
    /// </summary>
    private void OnEnable()
	{
		healCoroutine = StartCoroutine(HealNearbyPlayers());
	}

    /// <summary>
    /// 回復アクションを停止する
    /// </summary>
    private void OnDisable()
	{
		if (healCoroutine != null)
		{
			StopCoroutine(healCoroutine);
		}
	}

    /// <summary>
    /// サポーター共通アクションを実行する
    /// </summary>
    public override void Act()
	{
		// Act自体では何もしない（OnEnableで自動開始）
	}

    /// <summary>
    /// 回復アクションを実行するコルーチン
    /// </summary>
    /// <returns>回復クールタイム</returns>
    private IEnumerator HealNearbyPlayers()
	{
		float elapsed = 0f;
		while (elapsed < _healDuration)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _healRange);
			foreach (var hit in hits)
			{
				if (hit.CompareTag("Player"))
				{
					PlayerMovement player = hit.GetComponent<PlayerMovement>();
					if (player != null)
					{
						player.Heal(_healAmount);
					}
				}
			}
			yield return new WaitForSeconds(_healInterval);
			elapsed += _healInterval;
		}
		EndAct();
	}

    /// <summary>
    /// Gizmosを描画する（エディタ上でのデバッグ用）
    /// </summary>
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _healRange);
	}
}