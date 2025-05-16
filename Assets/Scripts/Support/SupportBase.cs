using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportBase : MonoBehaviour
{
    protected SupportStatus status;

    public virtual void Initialize(SupportStatus status)
    {
        this.status = status;
        Debug.Log($"{status.name} initialized with HP: {status.hp}, ATK: {status.attack}");
    }

    public abstract void Act();  // サブクラスでオーバーライド
}
