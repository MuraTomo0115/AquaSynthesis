using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGimmickActivatable
{
    bool IsPlayerInRange(GameObject actor);
    void Activate(GameObject actor); // actor: 実行者（Player or Echo）
    bool CanActivateByEcho { get; }  // Echoで発動可能か
}
