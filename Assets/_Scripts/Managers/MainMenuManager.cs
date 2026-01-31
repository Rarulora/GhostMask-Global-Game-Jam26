using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioMixer mixer;

    [Header("UI")]
    [SerializeField] private NameInputPanel nameInputPanel;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Language")]
    [SerializeField] private Toggle languageChanger;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaderboardButton;

    [Header("Panels")]
    [SerializeField] private LeaderboardMenu leaderboardPanel;

    [Header("Name")]
    public char[] turkishChars = { 'ç', 'ð', 'ý', 'ö', 'þ', 'ü' };

    private void Awake()
    {
        /*
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        */
    }

    private void Start()
    {
        OnSliderValueChange(masterSlider);
        OnSliderValueChange(musicSlider);
        OnSliderValueChange(sfxSlider);

        if (!GameManager.Instance.DidPlayerChooseAName())
        {
            DeactivateAllButtons();
            nameInputPanel.Enter();
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(() => GameManager.Instance.Play());
        }
		if (leaderboardButton != null)
		{
            leaderboardButton.onClick.AddListener(() => leaderboardPanel.Enter());
		}
	}


    /*
     * desc: Applies the change made in the specified slider to the mixer
     * param1 slider: Slider with modified value
    */
    public void OnSliderValueChange(Slider slider)
    {
        /*
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
        */
    }

    /*
     * desc: Applies the given value to the mixer's specified parameter by adjusting it to the [-80, 0] decibel range
     * param1 exposedParam: Sound parameter to be modified
     * param2 value: New normalized [0, 1] value of the sound parameter
    */
    private void ApplyVolume(string exposedParam, float value)
    {
        float dB = value <= 0f ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(exposedParam, dB);
    }

    /*
     * desc: Makes all buttons' interactable flag true
    */
    public void ActivateAllButtons()
    {
		playButton.interactable = true;
		leaderboardButton.interactable = true;
	}

    /*
     * desc: Makes all buttons' interactable flag false
    */
    public void DeactivateAllButtons()
    {
        playButton.interactable = false;
        leaderboardButton.interactable = false;
    }
}
