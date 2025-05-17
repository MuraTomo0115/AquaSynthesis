using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SupportHealerStatus
{
	public float healRange;
	public float healAmount;
	public float healInterval;
	public float healDuration;
}

public class SupportHealer : SupportBase
{
	private float healRange;
	private float healAmount;
	private float healInterval;
	private float healDuration;

	private Coroutine healCoroutine;

	protected override void Awake()
	{
		base.Awake();
		LoadHealerStatus();
	}

	private void LoadHealerStatus()
	{
		TextAsset json = Resources.Load<TextAsset>("Status/HealerStatus");
		if (json != null)
		{
			SupportHealerStatus status = JsonUtility.FromJson<SupportHealerStatus>(json.text);
			healRange = status.healRange;
			healAmount = status.healAmount;
			healInterval = status.healInterval;
			healDuration = status.healDuration;
			Debug.Log(healAmount + " " + healInterval + " " + healDuration + " " + healRange);
		}
		else
		{
			Debug.LogWarning("SupportHealerStatus.jsonが見つかりません。デフォルト値を使用します。");
		}
	}

	private void OnEnable()
	{
		healCoroutine = StartCoroutine(HealNearbyPlayers());
	}

	private void OnDisable()
	{
		if (healCoroutine != null)
		{
			StopCoroutine(healCoroutine);
		}
	}

	public override void Act()
	{
		Debug.Log("SupportHealer: 回復アクション実行！");
		// Act自体では何もしない（OnEnableで自動開始）
	}

	private IEnumerator HealNearbyPlayers()
	{
		float elapsed = 0f;
		while (elapsed < healDuration)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, healRange);
			foreach (var hit in hits)
			{
				if (hit.CompareTag("Player"))
				{
					PlayerMovement player = hit.GetComponent<PlayerMovement>();
					if (player != null)
					{
						player.Heal(healAmount);
						Debug.Log("SupportHealer: プレイヤーを回復！");
					}
				}
			}
			yield return new WaitForSeconds(healInterval);
			elapsed += healInterval;
		}
		EndAct();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, healRange);
	}
}