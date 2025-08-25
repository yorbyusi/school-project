using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips (register here)")]
    [SerializeField] private AudioClip[] bgm;
    [SerializeField] private AudioClip[] ambience;
    [SerializeField] private AudioClip[] sfx;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private Dictionary<string, AudioClip> clipCache = new();

    private float bgmVolume;
    private float ambienceVolume;
    private float sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cache initial volumes
        bgmVolume = bgmSource.volume;
        ambienceVolume = ambienceSource.volume;
        sfxVolume = sfxSource.volume;

        // Build dictionary from inspector clips
        CacheClips(bgm);
        CacheClips(ambience);
        CacheClips(sfx);
    }

    private void CacheClips(AudioClip[] clips)
    {
        foreach (var clip in clips)
        {
            if (clip != null && !clipCache.ContainsKey(clip.name))
            {
                clipCache[clip.name] = clip;
            }
        }
    }

    private Coroutine bgmCoroutine;
    private Coroutine ambienceCoroutine;

    public void PlayBGM(string clipName)
    {
        Debug.Log($"[AudioManager] PlayBGM: {clipName}");
        if (!clipCache.TryGetValue(clipName, out var clip)) return;

        //if (bgmCoroutine != null)
        //    StopCoroutine(bgmCoroutine);

        StartCoroutine(FadeSwitch(bgmSource, clip, bgmVolume));
    }

    public void StopBGM()
    {
        bgmCoroutine = StartCoroutine(FadeSwitch(bgmSource, null, 0f));
    }

    public void PlayAmbience(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        if (ambienceCoroutine != null)
            StopCoroutine(ambienceCoroutine);
        ambienceCoroutine = StartCoroutine(FadeSwitch(ambienceSource, clip, ambienceVolume));
    }

    public void StopAmbience()
    {
        ambienceCoroutine = StartCoroutine(FadeSwitch(ambienceSource, null, 0f));
    }

    public void PlaySFX(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // play sfx with pitch variation
    public void PlaySFX(string clipName, float pitchMin, float pitchMax)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, sfxVolume);
        sfxSource.pitch = 1f; // reset pitch
    }

    private System.Collections.IEnumerator FadeSwitch(AudioSource source, AudioClip newClip, float targetVolume)
    {
        if (source.clip == newClip) yield break;

        // Fade out
        float t = 0f;
        float startVol = source.volume;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        source.clip = newClip;
        source.Play();

        // Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }
    }
}
