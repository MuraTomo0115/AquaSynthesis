using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�̓������L�^���A�S�[�X�g�Ƃ��čĐ�����@�\
/// </summary>
public class RecordAbility : MonoBehaviour
{
    [Header("�L�^�E�Đ��Ώ�")]
    [SerializeField] private Transform _target; // �L�^�Ώہi�v���C���[�j
    [SerializeField] private float _recordInterval = 0.1f; // �L�^�Ԋu�i�b�j
    [SerializeField] private float _maxRecordTime = 10f;   // �ő�L�^���ԁi�b�j
    [SerializeField] private GameObject _ghostPrefab;      // �S�[�X�g�̃v���n�u
    private WaterWallController[] _waterWalls;

    // �����Q��
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerMovement _playerMovement;

    /// <summary>
    /// 1�t���[�����̋L�^�f�[�^
    /// </summary>
    private struct FrameData
    {
        public Vector3 position;         // �ʒu
        public Quaternion rotation;      // ��]
        public string animClipName;      // �A�j���[�V�����N���b�v��
        public bool isFacingLeft;        // ��������
        public bool didAttack;           // �ߐڍU���g���K�[
        public bool didPistol;           // �s�X�g���g���K�[
        public Vector2 input;            // ���͒l

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

    // �L�^�f�[�^
    private List<FrameData> _savedRecord = new List<FrameData>();
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    // �S�[�X�g�Đ��p
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
    /// �L�^�J�n
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording || _recordCoroutine != null) return;
        if (_playerMovement != null)
        {
            _playerMovement.IsRecording = true;
            _playerMovement.SetLayerByRecording(); // �ǉ�
        }
        _savedRecord.Clear();
        SaveCurrentFrame();
        _recordCoroutine = StartCoroutine(RecordCoroutine());
        SetFireWallTransparency(true);
    }

    /// <summary>
    /// ���݂̏�Ԃ�1�t���[�����L�^
    /// </summary>
    private void SaveCurrentFrame()
    {
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
            _playerMovement.IsRecording = false;
            _playerMovement.SetLayerByRecording(); // �ǉ�
        }
        SetFireWallTransparency(false);
    }

    /// <summary>
    /// �v���C���[�̓�������Ԋu�ŋL�^����R���[�`��
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

            // ���݂̏�Ԃ��擾
            string clipName = GetCurrentAnimationClipName();
            bool isFacingLeft = _spriteRenderer != null ? _spriteRenderer.flipX : false;
            bool didAttack = _playerMovement != null && _playerMovement.DidAttack;
            bool didPistol = _playerMovement != null && _playerMovement.DidPistol;

            // �g���K�[�i�����オ��j�����L�^
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

        // �v���C���[�̍U���Z���T�[�E�e�v���n�u���擾
        GameObject playerAttackSensor = null;
        GameObject bulletPrefab = null;
        if (_playerMovement != null)
        {
            playerAttackSensor = _playerMovement.AttackSensorGameObject;
            bulletPrefab = _playerMovement.BulletPrefab;
        }

        // �S�[�X�g�ɏ����f�[�^��n���ď�����
        var ghostMovement = ghost.GetComponent<GhostMovement>();
        if (ghostMovement != null)
        {
            var first = _savedRecord[0];
            ghostMovement.Initialize(
                first.position,
                first.input,
                false, // �W�����v���͕K�v�ɉ�����
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
    /// �S�[�X�g�̓�����L�^�f�[�^�ɏ]���čĐ�����R���[�`��
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

            // �A�j���[�V�����E�����Č�
            if (_ghostAnimator != null && !string.IsNullOrEmpty(current.animClipName))
                _ghostAnimator.CrossFade(current.animClipName, 0f);
            if (_ghostSpriteRenderer != null)
                _ghostSpriteRenderer.flipX = current.isFacingLeft;

            // �S�[�X�g�̓��͂𔽉f
            if (ghostMovement != null)
                ghostMovement.SetRecordedInput(current.input);

            // �ߐڍU���g���K�[
            if (ghostMovement != null && current.didAttack)
                ghostMovement.StartAttack();

            // �s�X�g���g���K�[
            if (ghostMovement != null && current.didPistol)
                ghostMovement.ShootPistol();

            // ��ԂłȂ߂炩�Ɉړ��E��]
            while (t < _recordInterval)
            {
                float lerpFactor = t / _recordInterval;
                playbackTarget.position = Vector3.Lerp(current.position, next.position, lerpFactor);
                playbackTarget.rotation = Quaternion.Lerp(current.rotation, next.rotation, lerpFactor);

                t += Time.deltaTime;
                yield return null;
            }
        }

        // �Ō�̃t���[�����Z�b�g
        var last = framesToPlay[framesToPlay.Count - 1];
        playbackTarget.position = last.position;
        playbackTarget.rotation = last.rotation;

        _isPlayingBack = false;

        // �S�[�X�g��j��
        Destroy(playbackTarget.gameObject);
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
