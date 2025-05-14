using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    private Transform _target; // プレイヤーのTransformを格納する変数

    [SerializeField] private float _smoothTime; // カメラの移動のスムーズさを決定する時間
    [SerializeField] private Vector3 _offset = new Vector3(0, 2f, -10f); // プレイヤーとの相対位置

    [Header("ズーム設定")]
    [SerializeField] private float _startZoomY = 5f; // ズームが開始されるY座標
    [SerializeField] private float _zoomRange = 10f; // ズームを適用する高さの範囲
    [SerializeField] private float _zoomOutMax = 5f; // 最大ズームアウト量
    [SerializeField] private float _zoomSpeed = 2f; // ズーム速度
    [SerializeField] private float _normalSize = 5f; // 通常のカメラサイズ

    [Header("Y追従開始高さ")]
    [SerializeField] private float _followStartY = 5f; // プレイヤーがこの高さを超えるとY方向の追従を開始

    [Header("カメラ制限・下限")]
    [SerializeField] private float _minCameraBottomY = 0f; // カメラの下限Y座標

    [Header("下回ったときに戻すY座標")]
    [SerializeField] private float _fixedReturnY = 5f; // プレイヤーがY座標を下回ったときにカメラを戻すY座標

    private Vector3 _velocity = Vector3.zero; // カメラのスムーズな移動を計算するための補間用変数
    private Camera _cam; // メインカメラを格納する変数

    private bool _isFollowingY = false; // Y方向の追従を始めたかどうかを判断するフラグ

    private float _currentYVelocity = 0f; // Y方向の補間速度

    /// <summary>
    /// プレイヤーを検索し、ターゲットとして設定、カメラの初期位置を設定
    /// </summary>
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
            transform.position = _target.position + _offset;
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません！");
        }

        _cam = Camera.main;
        if (_cam == null)
        {
            Debug.LogError("Main Camera が見つからない！");
        }
    }

    /// <summary>
    /// プレイヤー位置に基づいてズーム、Y方向の追従、カメラ制限を適用し、カメラ位置を更新
    /// </summary>
    private void FixedUpdate()
    {
        if (_target == null || _cam == null) return;

        float playerY = _target.position.y;

        // ズーム補間処理
        HandleZoom(playerY);

        // Y方向の追従開始・停止処理
        HandleYFollow(playerY);

        // カメラY座標のターゲット位置を決定
        float desiredY = _isFollowingY ? _target.position.y + _offset.y : _fixedReturnY;

        // カメラ下端制限処理
        desiredY = ApplyCameraBottomLimit(desiredY);

        // Y座標を補間
        float smoothedY = Mathf.SmoothDamp(transform.position.y, desiredY, ref _currentYVelocity, _smoothTime);

        // カメラ位置の更新
        UpdateCameraPosition(smoothedY);
    }

    /// <summary>
    /// プレイヤーのY座標に基づいてカメラのズームをスムーズに補間
    /// </summary>
    private void HandleZoom(float playerY)
    {
        float excess = Mathf.Max(0f, playerY - _startZoomY);
        float zoomRate = Mathf.Clamp01(excess / _zoomRange);
        float targetZoom = Mathf.Lerp(_normalSize, _normalSize + _zoomOutMax, zoomRate);
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetZoom, Time.fixedDeltaTime * _zoomSpeed);
    }

    /// <summary>
    /// プレイヤーのY座標に応じて、カメラのY方向の追従を開始・停止
    /// </summary>
    private void HandleYFollow(float playerY)
    {
        if (!_isFollowingY && playerY > _followStartY)
        {
            _isFollowingY = true;
        }
        else if (_isFollowingY && playerY < _followStartY)
        {
            _isFollowingY = false;
        }
    }

    /// <summary>
    /// カメラのY座標が画面外に出ないように、下端制限を適用
    /// </summary>
    private float ApplyCameraBottomLimit(float desiredY)
    {
        float cameraBottomY = desiredY - _cam.orthographicSize;
        float minAllowedCenterY = _minCameraBottomY + _cam.orthographicSize;
        if (cameraBottomY < _minCameraBottomY)
        {
            desiredY = minAllowedCenterY;
        }

        return desiredY;
    }

    /// <summary>
    /// 補間されたY座標を基にカメラの位置をスムーズに更新
    /// </summary>
    private void UpdateCameraPosition(float smoothedY)
    {
        Vector3 targetPosition = new Vector3(
            _target.position.x + _offset.x,
            smoothedY,
            _offset.z
        );

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
    }
}
