using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _loopSfxSource;

    private const string _BGM_VOLUME_KEY = "BGMVolume";
    private const string _SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadVolumeSettings();
    }

    /// <summary>
    /// 効果音を再生
    /// </summary>
    /// <param name="category">カテゴリ</param>
    /// <param name="seName">再生する効果音の名前</param>
    public void PlaySE(string category, string seName)
    {
        string sePath = $"Audio/SE/{category}/{seName}";
        AudioClip clip = Resources.Load<AudioClip>(sePath);
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SEファイルが見つかりません: {sePath}");
        }
    }

    /// <summary>
    /// ループ効果音を再生
    /// </summary>
    /// <param name="category">カテゴリ</param>
    /// <param name="seName">停止する効果音の名前</param>
    public void PlayLoopSE(string category, string seName)
    {
        string sePath = $"Audio/SE/{category}/{seName}";
        AudioClip clip = Resources.Load<AudioClip>(sePath);
        if (clip != null)
        {
            _loopSfxSource.clip = clip;
            _loopSfxSource.loop = true;
            _loopSfxSource.Play();
        }
    }

    /// <summary>
    /// BGMを再生
    /// </summary>
    /// <param name="bgmPath">再生するBGMのパス</param>
    public void PlayBGM(string bgmPath)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/BGM/{bgmPath}");
        if (clip != null)
        {
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGMファイルが見つかりません: Audio/BGM/{bgmPath}");
        }
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    /// <param name="bgmPath">停止するBGMのパス</param>
    public void StopBGM(string bgmPath)
    {
        if (_bgmSource.isPlaying && _bgmSource.clip != null && _bgmSource.clip.name == bgmPath)
        {
            _bgmSource.Stop();
        }
    }

    /// <summary>
    /// BGMの音量を設定
    /// </summary>
    /// <param name="volume">設定する音量</param>
    public void SetBGMVolume(float volume)
    {
        _bgmSource.volume = volume;
        PlayerPrefs.SetFloat(_BGM_VOLUME_KEY, volume);
    }

    /// <summary>
    /// 効果音の音量を設定
    /// </summary>
    /// <param name="volume">設定する音量</param>
    public void SetSFXVolume(float volume)
    {
        _sfxSource.volume = volume;
        PlayerPrefs.SetFloat(_SFX_VOLUME_KEY, volume);
    }

    /// <summary>
    /// 保存された音量設定をロード
    /// </summary>
    private void LoadVolumeSettings()
    {
        _bgmSource.volume = PlayerPrefs.GetFloat(_BGM_VOLUME_KEY, 1.0f);
        _sfxSource.volume = PlayerPrefs.GetFloat(_SFX_VOLUME_KEY, 1.0f);
    }

    /// <summary>
    /// ループ効果音を停止
    /// </summary>
    /// <param name="category">カテゴリ</param>
    /// <param name="seName">停止する効果音の名前</param>
    public void StopLoopSE(string category, string seName)
    {
        string sePath = $"Audio/SE/{category}/{seName}";
        AudioClip clip = Resources.Load<AudioClip>(sePath);
        if (clip != null && _loopSfxSource.isPlaying && _loopSfxSource.clip == clip)
        {
            _loopSfxSource.Stop();
        }
    }
}