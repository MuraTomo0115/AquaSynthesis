using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class StageData
{
    public string stageName;      // 表示名
    public string sceneName;      // シーン名
    public Sprite stageImage;     // 背景画像
    public Transform stagePoint;     // ステージ位置
}

public class StageSelector : MonoBehaviour
{
    [Header("ステージデータリスト")]
    [SerializeField] private List<StageData> _stages; // ステージ情報のリスト

    [Header("プレイヤーオブジェクト")]
    [SerializeField] private GameObject _playerObject; // プレイヤーのGameObject

    [Header("背景画像UI")]
    [SerializeField] private Image _backgroundImage; // 背景画像を表示するUI

    [Header("移動速度")]
    [SerializeField] private float _moveSpeed; // ステージ間の移動にかかる秒数

    [SerializeField] private float _playerYOffset; // プレイヤーのY軸オフセット

    private int _currentIndex = 0; // 現在選択中のステージ番号
    private bool _isMoving = false; // プレイヤーが移動中かどうか
    private int _moveDirection = 1; // -1:左, 1:右（移動方向）

    private Transform _player; // プレイヤーのTransform
    private Animator _playerAnimator; // プレイヤーのAnimator

    // InputSystem
    private StageSelectInputActions _inputActions;

    private void Awake()
    {
        // InputActions初期化
        _inputActions = new StageSelectInputActions();
        _inputActions.StageSelect.Move.performed += OnMove;
        _inputActions.StageSelect.Submit.performed += OnSubmit;
    }

    private void OnEnable()
    {
        _inputActions?.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Disable();
    }

    private void Start()
    {
        // プレイヤーのTransformとAnimatorを取得
        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }

        UpdateStageView();    // 背景画像を初期化
        MovePlayerInstant();  // プレイヤーを初期位置に移動
    }

    // 入力イベント: 移動
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (_isMoving) return;
        Vector2 value = ctx.ReadValue<Vector2>();
        if (value.x < -0.5f)
        {
            MoveToStage(_currentIndex - 1, -1);
        }
        else if (value.x > 0.5f)
        {
            MoveToStage(_currentIndex + 1, 1);
        }
    }

    // 入力イベント: 決定
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_isMoving) return;
        SceneManager.LoadScene(_stages[_currentIndex].sceneName);
    }

    /// <summary>
    /// ステージを移動する処理
    /// </summary>
    /// <param name="newIndex">移動先のステージ番号</param>
    /// <param name="direction">移動方向（-1:左, 1:右）</param>
    private void MoveToStage(int newIndex, int direction)
    {
        if (newIndex < 0 || newIndex >= _stages.Count) return; // 範囲外は無視
        _currentIndex = newIndex;
        _moveDirection = direction;
        UpdateStageView();           // 背景画像を更新
        SetPlayerFacing(_moveDirection); // 移動方向に向きを変える
        StartCoroutine(MovePlayerCoroutine()); // プレイヤーをアニメーションで移動
    }

    /// <summary>
    /// 背景画像を現在のステージのものに更新
    /// </summary>
    private void UpdateStageView()
    {
        if (_backgroundImage != null && _stages[_currentIndex].stageImage != null)
            _backgroundImage.sprite = _stages[_currentIndex].stageImage;
    }

    /// <summary>
    /// プレイヤーを即座に現在のステージ位置に移動
    /// </summary>
    private void MovePlayerInstant()
    {
        if (_player != null && _stages[_currentIndex].stagePoint != null)
        {
            Vector3 pos = _stages[_currentIndex].stagePoint.position;
            pos.y += _playerYOffset; // Y軸を少し上げる
            _player.position = pos;
        }
    }

    /// <summary>
    /// プレイヤーの向きを設定（direction: 1=右, -1=左）
    /// </summary>
    private void SetPlayerFacing(int direction)
    {
        if (_player == null) return;
        Vector3 scale = _player.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        _player.localScale = scale;
    }

    /// <summary>
    /// プレイヤーをスライド移動させるコルーチン
    /// </summary>
    private IEnumerator MovePlayerCoroutine()
    {
        _isMoving = true;
        Vector3 start = _player.position;
        Vector3 end = _stages[_currentIndex].stagePoint.position;
        end.y += _playerYOffset; // Y軸オフセットを加える
        float duration = _moveSpeed; // 移動にかかる秒数
        float elapsed = 0f;

        // Runアニメーション再生
        if (_playerAnimator != null)
            _playerAnimator.SetBool("isRunning", true);

        // 指定時間かけてスライド移動
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _player.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        _player.position = end;

        SetPlayerFacing(1); // 到着時は右向きにする

        // Idleアニメーション再生
        if (_playerAnimator != null)
            _playerAnimator.SetBool("isRunning", false);

        _isMoving = false;
    }
}
