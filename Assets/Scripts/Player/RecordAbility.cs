using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの動きを記録し、ゴーストとして再生する機能
/// </summary>
public class RecordAbility : MonoBehaviour
{
    [SerializeField] private Transform _target; // 記録対象
    [SerializeField] private float _recordInterval = 0.1f; // 記録間隔（秒）
    [SerializeField] private float _maxRecordTime = 10f; // 最大記録時間（秒）
    [SerializeField] private GameObject _ghostPrefab; // ゴーストのプレハブ

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;

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
        public Vector2 input; // ★追加

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

    private List<FrameData> _savedRecord = new List<FrameData>(); // 記録データ
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

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
    }

    /// <summary>
    /// 記録開始
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording || _recordCoroutine != null) return;
        if (_playerMovement != null)
        {
            _playerMovement.IsRecording = true; // internal setで操作
        }
        _recordCoroutine = StartCoroutine(RecordCoroutine());
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
            _playerMovement.IsRecording = false; // internal setで操作
        }
    }

    /// <summary>
    /// 記録処理コルーチン
    /// </summary>
    private IEnumerator RecordCoroutine()
    {
        _isRecording = true;
        _savedRecord.Clear();

        float timer = 0f;
        bool prevDidPistol = false;
        bool prevDidAttack = false;

        while (timer < _maxRecordTime)
        {
            timer += _recordInterval;

            string clipName = GetCurrentAnimationClipName();
            bool isFacingLeft = _spriteRenderer != null ? _spriteRenderer.flipX : false;
            bool didAttack = _playerMovement != null && _playerMovement.DidAttack;
            bool didPistol = _playerMovement != null && _playerMovement.DidPistol;

            // 立ち上がりだけ記録
            bool pistolTrigger = didPistol && !prevDidPistol;
            bool attackTrigger = didAttack && !prevDidAttack;

            _savedRecord.Add(new FrameData(
                _target.position,
                _target.rotation,
                clipName,
                isFacingLeft,
                attackTrigger,
                pistolTrigger,
                _playerMovement != null ? _playerMovement.MovementInput : Vector2.zero // ★ここを追加
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
        if (_isPlayingBack || _savedRecord.Count == 0 || _ghostPrefab == null) return;

        // ゴースト生成
        GameObject ghost = Instantiate(_ghostPrefab, _savedRecord[0].position, _savedRecord[0].rotation);
        _ghostInstanceTransform = ghost.transform;
        _ghostAnimator = ghost.GetComponent<Animator>();
        _ghostSpriteRenderer = ghost.GetComponent<SpriteRenderer>();

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
    /// ゴースト再生処理コルーチン
    /// </summary>
    /// <param name="playbackTarget">再生対象のTransform</param>
    /// <param name="framesToPlay">再生するフレームデータリスト</param>
    private IEnumerator PlaybackCoroutine(Transform playbackTarget, List<FrameData> framesToPlay)
    {
        _isPlayingBack = true;
        var ghostMovement = playbackTarget.GetComponent<GhostMovement>();

        for (int i = 0; i < framesToPlay.Count - 1; i++)
        {
            var current = framesToPlay[i];
            var next = framesToPlay[i + 1];

            float t = 0f;
            while (t < _recordInterval)
            {
                float lerpFactor = t / _recordInterval;
                playbackTarget.position = Vector3.Lerp(current.position, next.position, lerpFactor);
                playbackTarget.rotation = Quaternion.Lerp(current.rotation, next.rotation, lerpFactor);

                // アニメーション・向き・攻撃再現（必要ならこの中で）
                if (_ghostAnimator != null && !string.IsNullOrEmpty(current.animClipName))
                    _ghostAnimator.CrossFade(current.animClipName, 0f);
                if (_ghostSpriteRenderer != null)
                    _ghostSpriteRenderer.flipX = current.isFacingLeft;
                if (ghostMovement != null)
                {
                    // ゴーストの移動入力を更新
                    ghostMovement.SetRecordedInput(current.input);

                    if (current.didPistol) ghostMovement.ShootPistol();
                    if (current.didAttack) ghostMovement.StartAttack();
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        // 最後のフレームをセット
        var last = framesToPlay[framesToPlay.Count - 1];
        playbackTarget.position = last.position;
        playbackTarget.rotation = last.rotation;

        _isPlayingBack = false;
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
}
