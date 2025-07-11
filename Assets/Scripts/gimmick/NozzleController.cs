using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NozzleController : MonoBehaviour, IGimmickActivatable
{
    [SerializeField] private WaterWallController waterWall;
    [SerializeField] private float activationRadius = 1.0f;
    private Animation _animation; // Animation�R���|�[�l���g�Q��

    public bool CanActivateByEcho => true;

    private void Start()
    {
        _animation = GetComponent<Animation>();
    }

    public bool IsPlayerInRange(GameObject actor)
    {
        int echoLayer = LayerMask.NameToLayer("Echo");
        return actor.layer == echoLayer && Vector2.Distance(transform.position, actor.transform.position) < activationRadius;
    }

    public void Activate(GameObject actor)
    {
        _animation.Play();
        int echoLayer = LayerMask.NameToLayer("Echo");
        if (actor.layer != echoLayer) return;


        if (waterWall != null)
        {
            waterWall.DisableWall();
        }
    }

    /// <summary>
    /// �A�j���[�V�����I����Ɏ��g���폜
    /// </summary>
    public void OnDestroy()
    {
        Destroy(gameObject);
    }
}
