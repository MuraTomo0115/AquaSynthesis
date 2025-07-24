using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの動きを記録し、ゴーストとして再生する機能
/// </summary>
public class RecordAbility : MonoBehaviour
{
    [Header("記録・再生対象")]
    [SerializeField] private Transform _target; // 記録対象（プレイヤー）
    [SerializeField] private float _recordInterval = 0.1f; // 記録間隔（秒）
    [SerializeField] private float _maxRecordTime = 10f;   // 最大記録時間（秒）
    [SerializeField] private GameObject _ghostPrefab;      // ゴーストのプレハブ
    [SerializeField] private GameObject _dummyPlayerPrefab; // ダミープレイヤーのプレハブ
    private WaterWallController[] _waterWalls;

    // 内部参照
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;
    private bool _isCanRecord = true;

    // ダミーインスタンス参照
    private GameObject _dummyPlayerInstance;

    /// <summary>
    /// 1フレーム分の記録データ
    /// </summary>
    private struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public string animClipName;
        public bool isFacingLeft;
        public bool didAttack;
        public bool didPistol;
        public Vector2 input;

        public FrameData(Vector3 pos, Quaternion rot, string clipName, bool facingLeft, bool attack, bool pistol, Vector2 input)
        {
            position = pos;
            rotation = rot;
            animClipName = clipName;
            isFacingLeft = facingLeft;
            didAttack = attack;
            didPistol = pistol;
            this.input = input;
        }
    }

    // 記録データ
    private List<FrameData> _savedRecord = new List<FrameData>();
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    // ゴースト再生用
    private Transform _ghostInstanceTransform;
    private Animator _ghostAnimator;
    private SpriteRenderer _ghostSpriteRenderer;
    private bool _isRecording = false;
    private bool _isPlayingBack = false;

    /// <summary>
    /// 初期化。_targetから必要なコンポーネントを取得
    /// </summary>
    private void Awake()
    {
        if (_target != null)
        {
            _animator = _target.GetComponent<Animator>();
            _spriteRenderer = _target.GetComponent<SpriteRenderer>();
            _playerMovement = _target.GetComponent<PlayerMovement>();
        }
        _waterWalls = FindObjectsOfType<WaterWallController>();
    }

    private void SetFireWallTransparency(bool transparent)
    {
        if (_waterWalls == null) return;
        foreach (var wall in _waterWalls)
        {
            wall.SetTransparent(transparent);
        }
    }

    /// <summary>
    /// 記録開始
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording || _recordCoroutine != null || !_isCanRecord) return;
        if (_playerMovement != null)
        {
            _playerMovement.IsRecording = true;
            _playerMovement.SetLayerByRecording(); // 追加
        }
        _savedRecord.Clear();
        SaveCurrentFrame();
        _recordCoroutine = StartCoroutine(RecordCoroutine());
        SetFireWallTransparency(true);

        // ダミー生成
        if (_dummyPlayerPrefab != null && _dummyPlayerInstance == null)
        {
            _dummyPlayerInstance = Instantiate(_dummyPlayerPrefab, _target.position, _target.rotation);
        }
    }

    /// <summary>
    /// 現在の状態を1フレーム分記録
    /// </summary>
    private void SaveCurrentFrame()
    {
        if(!_isCanRecord || _target == null || _spriteRenderer == null || _playerMovement == null) return;

        string clipName = GetCurrentAnimationClipName();
        bool isFacingLeft = _spriteRenderer != null ? _spriteRenderer.flipX : false;
        bool didAttack = _playerMovement != null && _playerMovement.DidAttack;
        bool didPistol = _playerMovement != null && _playerMovement.DidPistol;
        _savedRecord.Add(new FrameData(
            _target.position,
            _target.rotation,
            clipName,
            isFacingLeft,
            didAttack,
            didPistol,
            _playerMovement != null ? _playerMovement.MovementInput : Vector2.zero
        ));
    }

    /// <summary>
    /// 記録停止
    /// </summary>
    public void StopRecording()
    {
        if (_recordCoroutine != null)
        {
            StopCoroutine(_recordCoroutine);
            _recordCoroutine = null;
        }
        _isRecording = false;
        if (_playerMovement != null)
        {
            _playerMovement.IsRecording = false;
            _playerMovement.SetLayerByRecording(); // 追加
        }
        SetFireWallTransparency(false);

        // ダミー削除
        if (_dummyPlayerInstance != null)
        {
            Destroy(_dummyPlayerInstance);
            _dummyPlayerInstance = null;
        }
    }

    /// <summary>
    /// プレイヤーの動作を一定間隔で記録するコルーチン
    /// </summary>
    private IEnumerator RecordCoroutine()
    {
        if (_isRecording || _recordCoroutine != null || !_isCanRecord) yield break;

        _isRecording = true;
        _savedRecord.Clear();

        float timer = 0f;
        bool prevDidPistol = false;
        bool prevDidAttack = false;

        while (timer < _maxRecordTime)
        {
            timer += _recordInterval;

            // 現在の状態を取得
            string clipName = GetCurrentAnimationClipName();
            bool isFacingLeft = _spriteRenderer != null ? _spriteRenderer.flipX : false;
            bool didAttack = _playerMovement != null && _playerMovement.DidAttack;
            bool didPistol = _playerMovement != null && _playerMovement.DidPistol;

            // トリガー（立ち上がり）だけ記録
            bool pistolTrigger = didPistol && !prevDidPistol;
            bool attackTrigger = didAttack && !prevDidAttack;

            _savedRecord.Add(new FrameData(
                _target.position,
                _target.rotation,
                clipName,
                isFacingLeft,
                attackTrigger,
                pistolTrigger,
                _playerMovement != null ? _playerMovement.MovementInput : Vector2.zero
            ));

            prevDidPistol = didPistol;
            prevDidAttack = didAttack;

            yield return new WaitForSeconds(_recordInterval);
        }

        _isRecording = false;
        _recordCoroutine = null;
    }

    /// <summary>
    /// ゴースト再生開始
    /// </summary>
    public void StartPlayback()
    {
        if (_isPlayingBack || _savedRecord.Count == 0 || _ghostPrefab == null || !_isCanRecord) return;

        // ゴースト生成
        GameObject ghost = Instantiate(_ghostPrefab, _savedRecord[0].position, _savedRecord[0].rotation);
        _ghostInstanceTransform = ghost.transform;
        _ghostAnimator = ghost.GetComponent<Animator>();
        _ghostSpriteRenderer = ghost.GetComponent<SpriteRenderer>();

        // プレイヤーの攻撃センサー・弾プレハブを取得
        GameObject playerAttackSensor = null;
        GameObject bulletPrefab = null;
        if (_playerMovement != null)
        {
            playerAttackSensor = _playerMovement.AttackSensorGameObject;
            bulletPrefab = _playerMovement.BulletPrefab;
        }

        // ゴーストに初期データを渡して初期化
        var ghostMovement = ghost.GetComponent<GhostMovement>();
        if (ghostMovement != null)
        {
            var first = _savedRecord[0];
            ghostMovement.Initialize(
                first.position,
                first.input,
                false, // ジャンプ等は必要に応じて
                first.didAttack,
                first.didPistol,
                false, false, // summonA, summonB
                first.isFacingLeft,
                playerAttackSensor,
                bulletPrefab
            );
        }

        _playbackCoroutine = StartCoroutine(PlaybackCoroutine(_ghostInstanceTransform, _savedRecord));
    }

    /// <summary>
    /// ゴースト再生停止
    /// </summary>
    public void StopPlayback()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }

        _isPlayingBack = false;

        // ゴーストを破棄
        if (_ghostInstanceTransform != null)
        {
            Destroy(_ghostInstanceTransform.gameObject);
            _ghostInstanceTransform = null;
            _ghostAnimator = null;
            _ghostSpriteRenderer = null;
        }
    }

    /// <summary>
    /// ゴーストの動作を記録データに従って再生するコルーチン
    /// </summary>
    /// <param name="playbackTarget">再生対象のTransform</param>
    /// <param name="framesToPlay">再生するフレームデータリスト</param>
    private IEnumerator PlaybackCoroutine(Transform playbackTarget, List<FrameData> framesToPlay)
    {
        if (_isPlayingBack || framesToPlay.Count == 0 || playbackTarget == null || !_isCanRecord) yield break;

        _isPlayingBack = true;
        var ghostMovement = playbackTarget.GetComponent<GhostMovement>();

        for (int i = 0; i < framesToPlay.Count - 1; i++)
        {
            var current = framesToPlay[i];
            var next = framesToPlay[i + 1];

            float t = 0f;

            // アニメーション・向き再現
            if (_ghostAnimator != null && !string.IsNullOrEmpty(current.animClipName))
                _ghostAnimator.CrossFade(current.animClipName, 0f);
            if (_ghostSpriteRenderer != null)
                _ghostSpriteRenderer.flipX = current.isFacingLeft;

            // ゴーストの入力を反映
            if (ghostMovement != null)
                ghostMovement.SetRecordedInput(current.input);

            // 近接攻撃トリガー
            if (ghostMovement != null && current.didAttack)
                ghostMovement.StartAttack();

            // ピストルトリガー
            if (ghostMovement != null && current.didPistol)
                ghostMovement.ShootPistol();

            // 補間でなめらかに移動・回転
            while (t < _recordInterval)
            {
                float lerpFactor = t / _recordInterval;
                playbackTarget.position = Vector3.Lerp(current.position, next.position, lerpFactor);
                playbackTarget.rotation = Quaternion.Lerp(current.rotation, next.rotation, lerpFactor);

                t += Time.deltaTime;
                yield return null;
            }
        }

        // 最後のフレームをセット
        var last = framesToPlay[framesToPlay.Count - 1];
        playbackTarget.position = last.position;
        playbackTarget.rotation = last.rotation;

        _isPlayingBack = false;

        // ゴーストを破棄
        Destroy(playbackTarget.gameObject);
    }

    /// <summary>
    /// 現在再生中のアニメーションクリップ名を取得
    /// </summary>
    /// <returns>再生中のアニメーションクリップ名。なければnull</returns>
    private string GetCurrentAnimationClipName()
    {
        if (_animator == null) return null;

        var clips = _animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
        {
            return clips[0].clip.name;
        }
        return null;
    }
    /// <summary>
    /// 近くのギミックをアクティブ化できるかチェックし、アクティブ化する
    /// </summary>
    /// <returns></returns>
    private bool TryActivateNearbyGimmickRecorder()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, NozzleController.activationRadius);
        foreach (var hit in hits)
        {
            var gimmick = hit.GetComponent<IGimmickActivatable>();
            if (gimmick != null && gimmick.IsPlayerInRange(gameObject))
            {
                // Echo可否を判定
                int echoLayer = LayerMask.NameToLayer("Echo");
                if (gameObject.layer == echoLayer && !gimmick.CanActivateByEcho)
                    continue;

                gimmick.Activate(gameObject);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ゴーストの記録が可能かどうかを設定
    /// </summary>
    /// <param name="canRecord">ゴーストの記録が可能かどうか</param>
    public void SetCanRecord(bool canRecord)
    {
        _isCanRecord = canRecord;
    }
}
