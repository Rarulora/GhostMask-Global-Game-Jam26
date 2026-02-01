using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVolumeManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioMixer mixer;

    [Header("UI")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button continueButton;

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
}
