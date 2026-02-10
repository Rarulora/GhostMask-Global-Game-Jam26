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

    [Header("Sound Effects (Library)")]
    public AudioClip cash;
    public AudioClip click;
    public AudioClip hit;
    public AudioClip takeDamage;
    public AudioClip explosion;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "Gameplay";
    public string creditsSceneName = "Credits";

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

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Update()
    {
        if (IsGameplayScene(SceneManager.GetActiveScene().name) && !musicSource.isPlaying && musicSource.clip != null)
        {
            // Müzik tamamen durmuþsa (pause deðilse)
            if (musicSource.time == 0 || musicSource.time >= musicSource.clip.length)
            {
                PlayNextGameplayTrack();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            PlayMusic(mainMenuMusic, true);
        }
        else if (scene.name == gameplaySceneName)
        {
            ShufflePlaylist();
            PlayNextGameplayTrack();
        }
        else if (scene.name == creditsSceneName)
        {
            StartCoroutine(FadeOutCurrentMusic());
        }
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == gameplaySceneName;
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeMusicRoutine(clip, loop));
    }

    private IEnumerator FadeMusicRoutine(AudioClip newClip, bool loop)
    {
        float startVolume = musicSource.volume;

        // Fade Out
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // Fade In
        while (musicSource.volume < 1f)
        {
            musicSource.volume += Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }
        musicSource.volume = 1f;
    }

    private IEnumerator FadeOutCurrentMusic()
    {
        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime / fadeDuration;
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = 1f;
    }

    private void ShufflePlaylist()
    {
        if (gameplayMusics == null || gameplayMusics.Count == 0) return;
        playlist = new List<AudioClip>(gameplayMusics);

        for (int i = 0; i < playlist.Count; i++)
        {
            AudioClip temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
        currentTrackIndex = 0;
    }

    private void PlayNextGameplayTrack()
    {
        if (playlist == null || playlist.Count == 0) return;

        AudioClip nextClip = playlist[currentTrackIndex];
        PlayMusic(nextClip, false);

        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariation = 0f)
    {
        if (clip == null) return;

        if (pitchVariation > 0)
            sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        else
            sfxSource.pitch = 1f;

        sfxSource.PlayOneShot(clip, volume);
    }
}