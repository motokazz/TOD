using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;         // BGM専用ソース（ループ）
    [SerializeField] private AudioSource seSource;          // SE用（ワンショット）

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float seVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float bgmFadeDuration = 1.5f;

    // 現在のBGM情報
    private string currentBgmName = "";
    private AudioClip currentBgmClip;

    // SE用プール（同時再生用に複数ソース用意）
    private List<AudioSource> seSourcesPool = new List<AudioSource>();
    private const int SE_POOL_SIZE = 8;

    protected override void Awake()
    {
        base.Awake();

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume * masterVolume;
        }

        // SEプール初期化
        for (int i = 0; i < SE_POOL_SIZE; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.volume = seVolume * masterVolume;
            seSourcesPool.Add(source);
        }
    }

    private void Start()
    {
        // 必要ならタイトルBGM再生
        // PlayBGM("Title");
    }

    // ==============================================
    // BGM管理
    // ==============================================

    /// <summary>
    /// BGMを再生（フェード対応）
    /// </summary>
    public void PlayBGM(string bgmName, bool fade = true)
    {
        if (currentBgmName == bgmName) return;

        AudioClip clip = Resources.Load<AudioClip>($"BGM/{bgmName}");
        if (clip == null)
        {
            Debug.LogWarning($"BGMが見つかりません: {bgmName}");
            return;
        }

        StartCoroutine(ChangeBGM(clip, fade));
        currentBgmName = bgmName;
        currentBgmClip = clip;
    }

    private IEnumerator ChangeBGM(AudioClip newClip, bool fade)
    {
        if (fade && bgmSource.isPlaying)
        {
            // 現在のBGMをフェードアウト
            float elapsed = 0f;
            float startVol = bgmSource.volume;

            while (elapsed < bgmFadeDuration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // フェードイン
        if (fade)
        {
            float elapsed = 0f;
            while (elapsed < bgmFadeDuration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume * masterVolume, elapsed / bgmFadeDuration);
                yield return null;
            }
        }
        else
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBGM(bool fade = true)
    {
        if (!bgmSource.isPlaying) return;

        if (fade)
        {
            StartCoroutine(FadeOutBGM());
        }
        else
        {
            bgmSource.Stop();
        }
        currentBgmName = "";
    }

    private IEnumerator FadeOutBGM()
    {
        float elapsed = 0f;
        float startVol = bgmSource.volume;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume * masterVolume;
    }

    // ==============================================
    // SE再生
    // ==============================================

    /// <summary>
    /// SEをワンショット再生（プール使用）
    /// </summary>
    public void PlaySE(string seName, float volumeScale = 1f, float pitch = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>($"SE/{seName}");
        if (clip == null)
        {
            Debug.LogWarning($"SEが見つかりません: {seName}");
            return;
        }

        // 空いているソースを探す
        AudioSource availableSource = null;
        foreach (var source in seSourcesPool)
        {
            if (!source.isPlaying)
            {
                availableSource = source;
                break;
            }
        }

        if (availableSource == null)
        {
            // すべて使用中 → 最初のものを上書き
            availableSource = seSourcesPool[0];
        }

        availableSource.clip = clip;
        availableSource.volume = seVolume * masterVolume * volumeScale;
        availableSource.pitch = pitch;
        availableSource.Play();
    }

    // ==============================================
    // ボリューム制御
    // ==============================================

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume * masterVolume;
        foreach (var source in seSourcesPool)
        {
            source.volume = seVolume * masterVolume;
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource.isPlaying)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        foreach (var source in seSourcesPool)
        {
            source.volume = seVolume * masterVolume;
        }
    }

    // ==============================================
    // ユーティリティ
    // ==============================================

    public bool IsBGMPlaying => bgmSource.isPlaying;
    public string CurrentBGMName => currentBgmName;
}