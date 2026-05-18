using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0.5f, 2f)] public float musicPitch = 1f;

    [Header("SFX Prefab")]
    [Tooltip("Prefab con AudioSource configurado. Si es null se crea uno básico.")]
    public AudioSource soundFXObject;

    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;

    [Header("Background Music")]
    [Tooltip("Lista de pistas de música. El índice 0 se reproduce al arrancar.")]
    public AudioClip[] backgroundMusics;

    [Tooltip("Si está activado, al llamar NextMusic() vuelve al índice 0 al llegar al final.")]
    public bool loopPlaylist = true;

    [Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
    }

    private AudioSource _musicSource;
    private readonly Dictionary<string, SoundEffect> _sfxDict = new();

    // Índice de la pista actual en backgroundMusics
    private int currentMusicIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = musicVolume;
        musicPitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        _musicSource.pitch = musicPitch;
        _musicSource.spatialBlend = 0f;

        foreach (var sfx in soundEffects)
            if (!string.IsNullOrEmpty(sfx.name))
                _sfxDict[sfx.name] = sfx;

        if (backgroundMusics is { Length: > 0 })
            PlayMusicByIndex(0);
    }

    // ──────────────────────────────────────────
    //  PLAYLIST METHODS
    // ──────────────────────────────────────────

    /// <summary>Reproduce la pista en la posición indicada del array.</summary>
    public void PlayMusicByIndex(int index)
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;
        index = Mathf.Clamp(index, 0, backgroundMusics.Length - 1);
        currentMusicIndex = index;
        PlayMusic(backgroundMusics[currentMusicIndex]);
    }

    public void NextMusic()
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;

        var next = currentMusicIndex + 1;
        if (next >= backgroundMusics.Length)
        {
            Debug.Log("[AudioManager] Ya estás en la última pista.");
            return;
        }

        PlayMusicByIndex(next);
    }

    public void PreviousMusic()
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;

        var prev = currentMusicIndex - 1;
        if (prev < 0)
        {
            Debug.Log("[AudioManager] Ya estás en la primera pista.");
            return;
        }

        PlayMusicByIndex(prev);
    }

    /// <summary>Alterna directamente entre dos índices (útil para combat/exploration music, etc.).</summary>
    public void ToggleMusic(int indexA, int indexB) => PlayMusicByIndex(currentMusicIndex == indexA ? indexB : indexA);

    /// <summary>Devuelve el índice de la pista que está sonando actualmente.</summary>
    public int CurrentMusicIndex => currentMusicIndex;

    // ──────────────────────────────────────────
    //  SFX
    // ──────────────────────────────────────────

    public void PlaySfx(string sfxName, Transform spawnTransform)
    {
        if (!_sfxDict.TryGetValue(sfxName, out var sfx))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: '{sfxName}'");
            return;
        }

        if (!sfx.clip)
        {
            Debug.LogWarning($"[AudioManager] Clip is null on SFX: '{sfxName}'");
            return;
        }

        var audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.transform.position = spawnTransform.position;
        audioSource.transform.SetParent(null);
        audioSource.clip = sfx.clip;
        audioSource.volume = sfx.volume * sfxVolume;
        audioSource.pitch = sfx.pitch;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.Play();

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public void PlayExample(Transform t) => PlaySfx("Example", t);

    // ──────────────────────────────────────────
    //  MUSIC CORE
    // ──────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.volume = musicVolume;
        musicPitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        _musicSource.pitch = musicPitch;
        _musicSource.Play();
    }

    public void StopMusic() => _musicSource.Stop();
    public void PauseMusic() => _musicSource.Pause();
    public void ResumeMusic()
    {
        musicPitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        _musicSource.pitch = musicPitch;
        _musicSource.UnPause();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        _musicSource.volume = musicVolume;
    }

    public void SetMusicPitch(float value)
    {
        musicPitch = Mathf.Clamp(value, 0.5f, 2f);
        _musicSource.pitch = musicPitch;
    }

    public void SetSFXVolume(float value) => sfxVolume = Mathf.Clamp01(value);

    public void PlayWinSequence(Transform t, string sfxName = "PowerUp")
    {
        StartCoroutine(WinSequenceRoutine(t, sfxName));
    }

    private IEnumerator WinSequenceRoutine(Transform t, string sfxName)
    {
        StopMusic();

        if (_sfxDict.TryGetValue(sfxName, out var sfx) && sfx.clip)
        {
            PlaySfx(sfxName, t);
            yield return new WaitForSeconds(sfx.clip.length);
        }

        PlayMusicByIndex(2);
    }
}
