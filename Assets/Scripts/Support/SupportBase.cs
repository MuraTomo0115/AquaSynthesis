using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportBase : MonoBehaviour
{
	protected SupportStatus status;
	protected Animator _animator;

	protected virtual void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public virtual void Initialize(SupportStatus status)
	{
		this.status = status;
		//Debug.Log($"{status.name} initialized with HP: {status.hp}, ATK: {status.attack}");
	}

	public void EndAct()
	{
		Invoke("DestroySelf", 2.0f); // 0.5秒後にアクション終了	
	}

	private void DestroySelf()
	{
		Debug.Log($"{gameObject.name} destroyed after action.");
		Destroy(this.gameObject);
		/*_animator.SetTrigger("End");*/
	}

	public abstract void Act();  // サブクラスでオーバーライド
}