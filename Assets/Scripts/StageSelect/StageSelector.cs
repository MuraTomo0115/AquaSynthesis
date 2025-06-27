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
    public Transform stagePoint;  // ステージ位置
}

public class StageSelector : MonoBehaviour
{
    [Header("プレイヤーオブジェクト")]
    [SerializeField] private GameObject _playerObject; // プレイヤーのGameObject

    [Header("背景画像UI")]
    [SerializeField] private Image _backgroundImage; // 背景画像を表示するUI

    [Header("移動速度")]
    [SerializeField] private float _moveSpeed; // ステージ間の移動にかかる秒数

    [SerializeField] private float _playerYOffset; // プレイヤーのY軸オフセット

    private List<StageData> _allStages; // 進行順で全ステージを登録

    // N/A/Gルートの全候補
    [Header("Nルートステージリスト (N1~N8)")]
    [SerializeField] private List<StageData> _nStages;
    [Header("Aルートステージリスト (A2~A8)")]
    [SerializeField] private List<StageData> _aStages;
    [Header("Gルートステージリスト (G4~G8)")]
    [SerializeField] private List<StageData> _gStages;

    private int _currentIndex = 0; // 現在選択中のステージ番号
    private bool _isMoving = false; // プレイヤーが移動中かどうか
    private int _moveDirection = 1; // -1:左, 1:右（移動方向）

    // ルート分岐フラグ
    private bool _isARoute = false;
    private bool _isGRoute = false;
    private string _currentRoute;

    private Transform _player; // プレイヤーのTransform
    private Animator _playerAnimator; // プレイヤーのAnimator

    private void Awake()
    {
        // InputActions初期化
        var inputActions = InputActionHolder.Instance.stageSelectInputActions;
        inputActions.StageSelect.Move.performed += OnMove;
        inputActions.StageSelect.Submit.performed += OnSubmit;
    }

    private void OnEnable()
    {
        InputActionHolder.Instance.stageSelectInputActions?.Enable();
    }

    private void OnDisable()
    {
        InputActionHolder.Instance.stageSelectInputActions?.Disable();
    }

    private void Start()
    {
        _currentRoute = DatabaseManager.GetCurrentRouteById(1);
        Debug.Log("現在のルート: " + _currentRoute);

        switch (_currentRoute)
        {
            case "A":
                // N1 + A2~A8
                _allStages = new List<StageData>();
                if (_nStages.Count > 0) _allStages.Add(_nStages[0]); // N1
                _allStages.AddRange(_aStages); // A2~A8
                _isARoute = true;
                break;
            case "G":
                // N1 + A2~A3 + G4~G8
                _allStages = new List<StageData>();
                if (_nStages.Count > 0) _allStages.Add(_nStages[0]); // N1
                if (_aStages.Count > 0) _allStages.Add(_aStages[0]); // A2
                if (_aStages.Count > 1) _allStages.Add(_aStages[1]); // A3
                _allStages.AddRange(_gStages); // G4~G8
                _isARoute = true;
                _isGRoute = true;
                break;
            case "N":
            default:
                _allStages = new List<StageData>(_nStages);
                break;
        }

        _currentIndex = 0;

        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }
        UpdateStageView();
        MovePlayerInstant();
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
        SceneManager.LoadScene(_allStages[_currentIndex].sceneName);
    }

    /// <summary>
    /// ステージを移動する処理
    /// </summary>
    /// <param name="newIndex">移動先のステージ番号</param>
    /// <param name="direction">移動方向（-1:左, 1:右）</param>
    private void MoveToStage(int newIndex, int direction)
    {
        if (newIndex < 0 || newIndex >= _allStages.Count) return;
        _currentIndex = newIndex;
        _moveDirection = direction;
        UpdateStageView();
        SetPlayerFacing(_moveDirection);
        StartCoroutine(MovePlayerCoroutine());
    }

    /// <summary>
    /// 背景画像を現在のステージのものに更新
    /// </summary>
    private void UpdateStageView()
    {
        if (_backgroundImage != null && _allStages[_currentIndex].stageImage != null)
            _backgroundImage.sprite = _allStages[_currentIndex].stageImage;
    }

    /// <summary>
    /// プレイヤーを即座に現在のステージ位置に移動
    /// </summary>
    private void MovePlayerInstant()
    {
        if (_player != null && _allStages[_currentIndex].stagePoint != null)
        {
            Vector3 pos = _allStages[_currentIndex].stagePoint.position;
            pos.y += _playerYOffset;
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
        Vector3 end = _allStages[_currentIndex].stagePoint.position;
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

    // ステージクリア時や分岐フラグを踏んだ時に呼ぶ
    public void OnStageFlagTriggered(string flag)
    {
        if (flag == "A" && !_isARoute && _currentIndex == 0)
        {
            // N1でAルート分岐
            _isARoute = true;
            // N1 + A2~A8
            var newStages = new List<StageData>();
            if (_nStages.Count > 0) newStages.Add(_nStages[0]); // N1
            newStages.AddRange(_aStages); // A2~A8
            _allStages = newStages;
            _currentIndex = 1; // A2に移動
            UpdateStageView();
            MovePlayerInstant();
        }
        else if (flag == "G" && _isARoute && !_isGRoute && _currentIndex == 3)
        {
            // A3でGルート分岐
            _isGRoute = true;
            // N1 + A2~A3 + G4~G8
            var newStages = new List<StageData>();
            if (_nStages.Count > 0) newStages.Add(_nStages[0]); // N1
            if (_aStages.Count > 0) newStages.Add(_aStages[0]); // A2
            if (_aStages.Count > 1) newStages.Add(_aStages[1]); // A3
            newStages.AddRange(_gStages); // G4~G8
            _allStages = newStages;
            _currentIndex = 4; // G4に移動
            UpdateStageView();
            MovePlayerInstant();
        }
    }
}
