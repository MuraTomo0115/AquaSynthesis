using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�̓������L�^���A�S�[�X�g�Ƃ��čĐ�����@�\
/// </summary>
public class RecordAbility : MonoBehaviour
{
    [SerializeField] private Transform _target; // �L�^�Ώ�
    [SerializeField] private float _recordInterval = 0.1f; // �L�^�Ԋu�i�b�j
    [SerializeField] private float _maxRecordTime = 10f; // �ő�L�^���ԁi�b�j
    [SerializeField] private GameObject _ghostPrefab; // �S�[�X�g�̃v���n�u

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;

    /// <summary>
    /// 1�t���[�����̋L�^�f�[�^
    /// </summary>
    private struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public string animClipName;
        public bool isFacingLeft;
        public bool didAttack;
        public bool didPistol;
        public Vector2 input; // ���ǉ�

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

    private List<FrameData> _savedRecord = new List<FrameData>(); // �L�^�f�[�^
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    private Transform _ghostInstanceTransform;
    private Animator _ghostAnimator;
    private SpriteRenderer _ghostSpriteRenderer;
    private bool _isRecording = false;
    private bool _isPlayingBack = false;

    /// <summary>
    /// �������B_target����K�v�ȃR���|�[�l���g���擾
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
    /// �L�^�J�n
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording || _recordCoroutine != null) return;
        if (_playerMovement != null)
        {
            _playerMovement.IsRecording = true; // internal set�ő���
        }
        _recordCoroutine = StartCoroutine(RecordCoroutine());
    }

    /// <summary>
    /// �L�^��~
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
            _playerMovement.IsRecording = false; // internal set�ő���
        }
    }

    /// <summary>
    /// �L�^�����R���[�`��
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

            // �����オ�肾���L�^
            bool pistolTrigger = didPistol && !prevDidPistol;
            bool attackTrigger = didAttack && !prevDidAttack;

            _savedRecord.Add(new FrameData(
                _target.position,
                _target.rotation,
                clipName,
                isFacingLeft,
                attackTrigger,
                pistolTrigger,
                _playerMovement != null ? _playerMovement.MovementInput : Vector2.zero // ��������ǉ�
            ));

            prevDidPistol = didPistol;
            prevDidAttack = didAttack;

            yield return new WaitForSeconds(_recordInterval);
        }

        _isRecording = false;
        _recordCoroutine = null;
    }

    /// <summary>
    /// �S�[�X�g�Đ��J�n
    /// </summary>
    public void StartPlayback()
    {
        if (_isPlayingBack || _savedRecord.Count == 0 || _ghostPrefab == null) return;

        // �S�[�X�g����
        GameObject ghost = Instantiate(_ghostPrefab, _savedRecord[0].position, _savedRecord[0].rotation);
        _ghostInstanceTransform = ghost.transform;
        _ghostAnimator = ghost.GetComponent<Animator>();
        _ghostSpriteRenderer = ghost.GetComponent<SpriteRenderer>();

        _playbackCoroutine = StartCoroutine(PlaybackCoroutine(_ghostInstanceTransform, _savedRecord));
    }

    /// <summary>
    /// �S�[�X�g�Đ���~
    /// </summary>
    public void StopPlayback()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }

        _isPlayingBack = false;

        // �S�[�X�g��j��
        if (_ghostInstanceTransform != null)
        {
            Destroy(_ghostInstanceTransform.gameObject);
            _ghostInstanceTransform = null;
            _ghostAnimator = null;
            _ghostSpriteRenderer = null;
        }
    }

    /// <summary>
    /// �S�[�X�g�Đ������R���[�`��
    /// </summary>
    /// <param name="playbackTarget">�Đ��Ώۂ�Transform</param>
    /// <param name="framesToPlay">�Đ�����t���[���f�[�^���X�g</param>
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

                // �A�j���[�V�����E�����E�U���Č��i�K�v�Ȃ炱�̒��Łj
                if (_ghostAnimator != null && !string.IsNullOrEmpty(current.animClipName))
                    _ghostAnimator.CrossFade(current.animClipName, 0f);
                if (_ghostSpriteRenderer != null)
                    _ghostSpriteRenderer.flipX = current.isFacingLeft;
                if (ghostMovement != null)
                {
                    // �S�[�X�g�̈ړ����͂��X�V
                    ghostMovement.SetRecordedInput(current.input);

                    if (current.didPistol) ghostMovement.ShootPistol();
                    if (current.didAttack) ghostMovement.StartAttack();
                }

                t += Time.deltaTime;
                yield return null;
            }
        }

        // �Ō�̃t���[�����Z�b�g
        var last = framesToPlay[framesToPlay.Count - 1];
        playbackTarget.position = last.position;
        playbackTarget.rotation = last.rotation;

        _isPlayingBack = false;
    }

    /// <summary>
    /// ���ݍĐ����̃A�j���[�V�����N���b�v�����擾
    /// </summary>
    /// <returns>�Đ����̃A�j���[�V�����N���b�v���B�Ȃ����null</returns>
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
