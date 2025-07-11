using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGimmickActivatable
{
    bool IsPlayerInRange(GameObject actor);
    void Activate(GameObject actor); // actor: ���s�ҁiPlayer or Echo�j
    bool CanActivateByEcho { get; }  // Echo�Ŕ����\��
}
