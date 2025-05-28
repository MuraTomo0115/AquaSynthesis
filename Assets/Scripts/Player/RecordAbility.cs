using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordAbility : MonoBehaviour
{
    [SerializeField] private Transform _target; // �L�^����Ώہi�v���C���[�Ȃǁj
    [SerializeField] private float _recordInterval = 0.1f; // ���b���ƂɋL�^���邩
    [SerializeField] private int _maxRecordLength = 100;   // �ő�L�^���i�t���[�����j

    private List<FrameData> _recordedFrames = new List<FrameData>();
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    private bool _isRecording = false;
    private bool _isPlayingBack = false;

    // �L�^�����1�t���[�����̏��
    private struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;

        public FrameData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    /// <summary>
    /// �L�^�J�n
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording) return;

        StopPlayback();
        _recordedFrames.Clear();
        _recordCoroutine = StartCoroutine(RecordCoroutine());
    }

    /// <summary>
    /// �Đ��J�n
    /// </summary>
    public void StartPlayback()
    {
        if (_isPlayingBack || _recordedFrames.Count == 0) return;

        StopRecording();
        _playbackCoroutine = StartCoroutine(PlaybackCoroutine());
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
    }

    /// <summary>
    /// �Đ���~
    /// </summary>
    public void StopPlayback()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }
        _isPlayingBack = false;
    }

    private IEnumerator RecordCoroutine()
    {
        _isRecording = true;

        while (true)
        {
            if (_recordedFrames.Count >= _maxRecordLength)
            {
                _recordedFrames.RemoveAt(0); // �Â��f�[�^����폜
            }

            _recordedFrames.Add(new FrameData(_target.position, _target.rotation));
            yield return new WaitForSeconds(_recordInterval);
        }
    }

    private IEnumerator PlaybackCoroutine()
    {
        _isPlayingBack = true;

        foreach (var frame in _recordedFrames)
        {
            _target.position = frame.position;
            _target.rotation = frame.rotation;
            yield return new WaitForSeconds(_recordInterval);
        }

        _isPlayingBack = false;
    }
}
