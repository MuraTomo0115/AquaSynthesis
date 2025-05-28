using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordAbility : MonoBehaviour
{
    [SerializeField] private Transform _target; // 記録する対象（プレイヤーなど）
    [SerializeField] private float _recordInterval = 0.1f; // 何秒ごとに記録するか
    [SerializeField] private int _maxRecordLength = 100;   // 最大記録数（フレーム数）

    private List<FrameData> _recordedFrames = new List<FrameData>();
    private Coroutine _recordCoroutine = null;
    private Coroutine _playbackCoroutine = null;

    private bool _isRecording = false;
    private bool _isPlayingBack = false;

    // 記録される1フレーム分の情報
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
    /// 記録開始
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording) return;

        StopPlayback();
        _recordedFrames.Clear();
        _recordCoroutine = StartCoroutine(RecordCoroutine());
    }

    /// <summary>
    /// 再生開始
    /// </summary>
    public void StartPlayback()
    {
        if (_isPlayingBack || _recordedFrames.Count == 0) return;

        StopRecording();
        _playbackCoroutine = StartCoroutine(PlaybackCoroutine());
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
    }

    /// <summary>
    /// 再生停止
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
                _recordedFrames.RemoveAt(0); // 古いデータから削除
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
