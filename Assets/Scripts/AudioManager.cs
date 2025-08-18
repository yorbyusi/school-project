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

    public void PlayBGM(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        StartCoroutine(FadeSwitch(bgmSource, clip, bgmVolume));
    }

    public void PlayAmbience(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        StartCoroutine(FadeSwitch(ambienceSource, clip, ambienceVolume));
    }

    public void PlaySFX(string clipName)
    {
        if (!clipCache.TryGetValue(clipName, out var clip)) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
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
