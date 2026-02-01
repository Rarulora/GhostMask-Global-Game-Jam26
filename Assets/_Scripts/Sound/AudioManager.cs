using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public List<AudioClip> gameplayMusics;

    [Header("Sound Effects")]
    public AudioClip cash;
    public AudioClip click;
    public AudioClip hit;
    public AudioClip takeDamage;
    public AudioClip explosion;

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "Gameplay";

    private bool isGameplay = false;
    private List<AudioClip> playlist;
    private int currentTrackIndex = 0;
    private Coroutine currentFadeRoutine;

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
            return;
        }
    }

    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == mainMenuSceneName)
        {
            isGameplay = false;
            PlayMusic(mainMenuMusic, true);
        }
        else if (currentSceneName == gameplaySceneName)
        {
            isGameplay = true;
            ShufflePlaylist();
            PlayNextGameplayTrack();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (isGameplay && !musicSource.isPlaying && musicSource.clip != null)
        {
            if (musicSource.volume > 0.1f)
                PlayNextGameplayTrack();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            isGameplay = false;
            PlayMusic(mainMenuMusic, true);
        }
        else if (scene.name == gameplaySceneName)
        {
            isGameplay = true;
            ShufflePlaylist();
            PlayNextGameplayTrack();
        }
    }

    private void PlayNextGameplayTrack()
    {
        if (playlist == null || playlist.Count == 0) return;

        AudioClip nextClip = playlist[currentTrackIndex];
        PlayMusic(nextClip, false);

        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
            currentTrackIndex = 0;
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeMusic(clip, loop));
    }

    private IEnumerator FadeMusic(AudioClip newClip, bool loop)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0;

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        while (musicSource.volume < 1f)
        {
            musicSource.volume += Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }

        musicSource.volume = 1f;
    }

    private void ShufflePlaylist()
    {
        playlist = new List<AudioClip>(gameplayMusics);
        currentTrackIndex = 0;

        for (int i = 0; i < playlist.Count; i++)
        {
            AudioClip temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }
}