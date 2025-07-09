using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class StageData
{
    public string stageName;            // 表示名
    public string sceneName;            // シーン名
    public Sprite stageImage;           // 背景画像
    public Transform stagePoint;        // ステージ位置
    public GameObject pathObject;       // Inspectorでアタッチ
    public string animationTriggerName; // トリガー名（Triggerを使う場合のみ）
}

public class StageSelector : MonoBehaviour
{
    [Header("プレイヤーオブジェクト")]
    [SerializeField] private GameObject _playerObject;  // プレイヤーのGameObject

    [Header("背景画像UI")]
    [SerializeField] private Image _backgroundImage;    // 背景画像を表示するUI

    [Header("移動速度")]
    [SerializeField] private float _moveSpeed;          // ステージ間の移動にかかる秒数

    [SerializeField] private float _playerYOffset;      // プレイヤーのY軸オフセット

    private List<StageData> _allStages;                 // 進行順で全ステージを登録

    // N/A/Gルートの全候補
    [Header("Nルートステージリスト (N1~N8)")]
    [SerializeField] private List<StageData> _nStages;
    [Header("Aルートステージリスト (A2~A8)")]
    [SerializeField] private List<StageData> _aStages;
    [Header("Gルートステージリスト (G4~G8)")]
    [SerializeField] private List<StageData> _gStages;

    private int _currentIndex = 0;      // 現在選択中のステージ番号
    private bool _isMoving = false;     // プレイヤーが移動中かどうか
    private int _moveDirection = 1;     // -1:左, 1:右（移動方向）

    // ルート分岐フラグ
    private bool _isARoute = false;
    private bool _isGRoute = false;
    private string _currentRoute;

    private Transform _player; // プレイヤーのTransform
    private Animator _playerAnimator; // プレイヤーのAnimator

    // === 追加: 道オブジェクトの参照 ===
    [Header("道オブジェクト（ステージ間ごとにセット）")]
    [SerializeField] private List<GameObject> _pathObjects;

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
        foreach (var stage in _nStages)
        {
            PlayerPrefs.DeleteKey("PathAnimationPlayed_" + stage.sceneName);
        }
        PlayerPrefs.Save();
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

        // クリアしたシーン名を取得し、_currentIndexを更新
        string lastClearedStage = PlayerPrefs.GetString("LastClearedStage", "");
        Debug.Log($"[StageSelector] lastClearedStage: {lastClearedStage}");

        if (!string.IsNullOrEmpty(lastClearedStage))
        {
            int stageIndex = _allStages.FindIndex(s => s.sceneName == lastClearedStage);
            Debug.Log($"[StageSelector] stageIndex: {stageIndex}");

            if (stageIndex >= 0)
            {
                _currentIndex = stageIndex;
            }

            // 直前クリアステージの初クリア判定と演出
            var status = DatabaseManager.GetStageStatus(lastClearedStage);
            Debug.Log($"[StageSelector] status: {(status != null ? $"is_clear={status.is_clear}" : "null")}");

            if (status != null && status.is_clear == 1 && !PlayerPrefs.HasKey("PathAnimationPlayed_" + lastClearedStage))
            {
                GameObject pathObj = null;
                string trigger = null;

                // n1クリア時はn1→n2間の道
                if (stageIndex == 0 && _allStages.Count > 1)
                {
                    pathObj = _allStages[1].pathObject;
                    trigger = _allStages[1].animationTriggerName;
                    Debug.Log("[StageSelector] n1クリア時: n1→n2間の道アニメーションを再生");
                }
                else if (stageIndex > 0)
                {
                    pathObj = _allStages[stageIndex].pathObject;
                    trigger = _allStages[stageIndex].animationTriggerName;
                }

                Debug.Log($"[StageSelector] pathObject: {(pathObj != null ? pathObj.name : "null")}, trigger: {trigger}");

                if (pathObj != null)
                {
                    PlayPathAnimation(pathObj, trigger);
                }
                PlayerPrefs.SetInt("PathAnimationPlayed_" + lastClearedStage, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("[StageSelector] PlayPathAnimationの条件を満たしていません");
            }
        }
        else
        {
            Debug.Log("[StageSelector] lastClearedStageが空です");
        }
        if (_playerObject != null)
        {
            _player = _playerObject.transform;
            _playerAnimator = _playerObject.GetComponent<Animator>();
        }
        UpdateStageView();
        MovePlayerInstant();

        // === 追加: すでに解放済みの道は常時表示 ===
        for (int i = 0; i < _pathObjects.Count; i++)
        {
            // i番目の道は、i+1番目のステージがクリア済みなら常時表示
            if (i + 1 < _allStages.Count)
            {
                var status = DatabaseManager.GetStageStatus(_allStages[i + 1].sceneName);
                if (status != null && status.is_clear == 1)
                {
                    if (_pathObjects[i] != null)
                    {
                        _pathObjects[i].SetActive(true);
                        // Animatorで演出済みならスケールはアニメーションに任せる
                    }
                }
            }
        }
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
        // 0番目（最初のステージ）は常に選択可能
        if (newIndex < 0 || newIndex >= _allStages.Count) return;
        if (newIndex > 0)
        {
            // 直前のステージ名を取得
            string prevStageName = _allStages[newIndex - 1].stageName;
            var prevStatus = DatabaseManager.GetStageStatus(prevStageName);
            // 直前のステージが未クリアなら進めない
            if (prevStatus == null || prevStatus.is_clear == 0)
            {
                Debug.Log("前のステージをクリアしていません。");
                return;
            }
        }
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
    /// 道のアニメーションを再生
    /// </summary>
    private void PlayPathAnimation(GameObject pathObject, string triggerName)
    {
        Debug.Log($"[PlayPathAnimation] 呼び出し: pathObject={(pathObject != null ? pathObject.name : "null")}, triggerName={triggerName}");

        if (pathObject == null)
        {
            Debug.LogWarning("[PlayPathAnimation] pathObjectがnullです");
            return;
        }
        var animator = pathObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[PlayPathAnimation] {pathObject.name} にAnimatorがアタッチされていません");
            return;
        }
        Debug.Log($"[PlayPathAnimation] Animator取得成功。Trigger名: {triggerName}");

        pathObject.SetActive(true);
        animator.ResetTrigger(triggerName); // 念のためリセット
        animator.SetTrigger(triggerName);
        Debug.Log($"[PlayPathAnimation] Trigger {triggerName} をセットしました");
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
