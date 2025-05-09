using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    //追従対象のTransform（プレイヤー）
    private Transform target;

    //カメラの追従の滑らかさ（値を大きくすると速く追いつく）
    [SerializeField] private float smoothSpeed = 5f;

    //カメラの位置補正（Zは2Dでは-10固定が基本）
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    //最初にプレイヤーをタグから探して取得
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません！");
        }
    }

    //毎フレームの後にカメラを更新（LateUpdateの方がカメラ追従に適してる）
    private void LateUpdate()
    {
        if (target == null) return;

        //プレイヤーの位置にオフセットを加えた目標位置
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, offset.z);

        //現在のカメラ位置から目標位置へ滑らかに補間して移動
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}

