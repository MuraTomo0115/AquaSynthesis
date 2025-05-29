using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordAbility : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _recordInterval = 0.1f;
    [SerializeField] private float _maxRecordTime = 10f;
    [SerializeField] private GameObject _ghostPrefab;

    private Animator _animator;

    private struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public string animClipName;

        public FrameData(Vector3 pos, Quaternion rot, string clipName)
        {
            position = pos;
            rotation = rot;
            animClipName = clipName;
        }
    }

    private List<FrameData> _savedRecord = new List<FrameData>();
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    private Transform _ghostInstanceTransform;
    private Animator _ghostAnimator;
    private bool _isRecording = false;
    private bool _isPlayingBack = false;

    private void Awake()
    {
        if (_target != null)
        {
            _animator = _target.GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogWarning("RecordAbility: _targetÇ…AnimatorÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ");
            }
        }
    }

    public void StartRecording()
    {
        if (_isRecording || _recordCoroutine != null) return;
        _recordCoroutine = StartCoroutine(RecordCoroutine());
    }

    public void StopRecording()
    {
        if (_recordCoroutine != null)
        {
            StopCoroutine(_recordCoroutine);
            _recordCoroutine = null;
        }
        _isRecording = false;
    }

    private IEnumerator RecordCoroutine()
    {
        _isRecording = true;
        _savedRecord.Clear();

        float timer = 0f;
        while (timer < _maxRecordTime)
        {
            timer += _recordInterval;

            string clipName = GetCurrentAnimationClipName();

            _savedRecord.Add(new FrameData(_target.position, _target.rotation, clipName));
            yield return new WaitForSeconds(_recordInterval);
        }

        _isRecording = false;
        _recordCoroutine = null;
    }

    public void StartPlayback()
    {
        if (_isPlayingBack || _savedRecord.Count == 0 || _ghostPrefab == null) return;

        GameObject ghost = Instantiate(_ghostPrefab, _savedRecord[0].position, _savedRecord[0].rotation);
        _ghostInstanceTransform = ghost.transform;
        _ghostAnimator = ghost.GetComponent<Animator>();
        if (_ghostAnimator == null)
        {
            Debug.LogWarning("RecordAbility: ÉSÅ[ÉXÉgÇ…AnimatorÇ™Ç†ÇËÇ‹ÇπÇÒ");
        }

        _playbackCoroutine = StartCoroutine(PlaybackCoroutine(_ghostInstanceTransform, _savedRecord));
    }

    public void StopPlayback()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }

        _isPlayingBack = false;

        if (_ghostInstanceTransform != null)
        {
            Destroy(_ghostInstanceTransform.gameObject);
            _ghostInstanceTransform = null;
            _ghostAnimator = null;
        }
    }

    private IEnumerator PlaybackCoroutine(Transform playbackTarget, List<FrameData> framesToPlay)
    {
        _isPlayingBack = true;

        foreach (var frame in framesToPlay)
        {
            playbackTarget.position = frame.position;
            playbackTarget.rotation = frame.rotation;

            if (_ghostAnimator != null && !string.IsNullOrEmpty(frame.animClipName))
            {
                _ghostAnimator.CrossFade(frame.animClipName, 0f);
            }

            yield return new WaitForSeconds(_recordInterval);
        }

        _isPlayingBack = false;
    }

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
