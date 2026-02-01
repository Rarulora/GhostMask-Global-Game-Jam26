using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVolumeManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private Button pauseButton;

    [Header("Pause Menu UI")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Menu pauseMenu;

    [Header("Game Over Menu UI")]
    [SerializeField] private Button mainMenuButtonGO;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI characterText;
    [SerializeField] private TextMeshProUGUI attackTypeText;
    [SerializeField] private Menu gameOverMenu;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void Start()
    {
        OnSliderValueChange(masterSlider);
        OnSliderValueChange(musicSlider);
        OnSliderValueChange(sfxSlider);

        masterSlider.onValueChanged.AddListener((v) => OnSliderValueChange(masterSlider));
        musicSlider.onValueChanged.AddListener((v) => OnSliderValueChange(musicSlider));
        sfxSlider.onValueChanged.AddListener((v) => OnSliderValueChange(sfxSlider));

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (mainMenuButtonGO != null)
            mainMenuButtonGO.onClick.AddListener(OnMainMenuClicked);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => GameManager.Instance.Pause());
    }

    private void OnEnable()
    {
        EventManager.OnPlayerDeath += InitializeGameOverMenu;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDeath -= InitializeGameOverMenu;
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance.MainMenu();
    }

    private void OnContinueClicked()
    {
        pauseMenu.SetState(false);
        GameManager.Instance.SwitchState(GameManager.Instance.States.Play());
    }

    public void OnSliderValueChange(Slider slider)
    {
        if (slider == masterSlider)
        {
            PlayerPrefs.SetFloat("MasterVolume", slider.value);
            ApplyVolume("MasterVolume", slider.value);
        }
        else if (slider == musicSlider)
        {
            PlayerPrefs.SetFloat("MusicVolume", slider.value);
            ApplyVolume("MusicVolume", slider.value);
        }
        else if (slider == sfxSlider)
        {
            PlayerPrefs.SetFloat("SFXVolume", slider.value);
            ApplyVolume("SFXVolume", slider.value);
        }

        PlayerPrefs.Save();
    }

    private void ApplyVolume(string exposedParam, float value)
    {
        float dB = value <= 0f ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(exposedParam, dB);
    }

    private void InitializeGameOverMenu()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        ScoreManager scoreManager = player.GetComponent<ScoreManager>();

        scoreText.text = $"Score: {Mathf.FloorToInt(scoreManager.CurrentScore)}";
        if (scoreManager.CurrentScore > GameManager.Instance.SaveData.bestRunData.highScore)
        {
            HighScoreData newBestRunData = new HighScoreData(scoreManager.CurrentScore, player.character, player.attackType);
            SaveData saveData = GameManager.Instance.SaveData;
            saveData.bestRunData = newBestRunData;
            GameManager.Instance.SetSaveData(saveData);
        }

        switch (player.character)
        {
            case Enums.CharacterType.Mouse:
                characterText.text = "Character: Mouse";
                break;
            case Enums.CharacterType.Raccoon:
                characterText.text = "Character: Raccoon";
                break;
            case Enums.CharacterType.Platipus:
                characterText.text = "Character: Platipus";
                break;
            case Enums.CharacterType.Cat:
                characterText.text = "Character: Cat";
                break;
            case Enums.CharacterType.Monkey:
                characterText.text = "Character: Monkey";
                break;
        }

        switch (player.attackType)
        {
            case Enums.AttackType.Melee:
                attackTypeText.text = "Attack Type: Melee";
                break;
            case Enums.AttackType.Ranged:
                attackTypeText.text = "Attack Type: Ranged";
                break;
            case Enums.AttackType.Dash:
                attackTypeText.text = "Attack Type: Dash";
                break;
        }

        GameManager.Instance.SwitchState(GameManager.Instance.States.GameOver());
    }
}
