using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    //追従対象のTransform（プレイヤー）
    private Transform _target;

    //カメラの追従の滑らかさ（値を大きくすると速く追いつく）
    [SerializeField] private float _smoothSpeed = 5f;

    //カメラの位置補正（Zは2Dでは-10固定が基本）
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10f);

    /// <summary>
    /// 最初にプレイヤーをタグから探して取得
    /// </summary>
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません！");
        }
    }

    /// <summary>
    /// 毎フレームの後にカメラを更新（LateUpdateの方がカメラ追従に適してる）
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null) return;

        //プレイヤーの位置にオフセットを加えた目標位置
        Vector3 desiredPosition = new Vector3(_target.position.x, _target.position.y, _offset.z);

        //現在のカメラ位置から目標位置へ滑らかに補間して移動
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
    }
}

