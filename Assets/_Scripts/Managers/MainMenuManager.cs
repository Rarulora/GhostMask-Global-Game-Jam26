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

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button skillTreeButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private LeaderboardMenu leaderboardPanel;

    [Header("Name")]
    public char[] turkishChars = { 'ç', 'ð', 'ý', 'ö', 'þ', 'ü' };

    private void Start()
    {
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
        if (skillTreeButton != null)
        {
            skillTreeButton.onClick.AddListener(() => SceneManager.LoadScene("SkillTree"));
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => Application.Quit());
        }
    }

    public void ActivateAllButtons()
    {
		playButton.interactable = true;
		leaderboardButton.interactable = true;
	}

    public void DeactivateAllButtons()
    {
        playButton.interactable = false;
        leaderboardButton.interactable = false;
    }
}
