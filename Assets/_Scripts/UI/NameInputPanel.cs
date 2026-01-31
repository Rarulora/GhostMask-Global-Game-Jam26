using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputPanel : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private AudioSource audioSource;
    private MainMenuManager mainMenuManager;
    private LeaderboardManager leaderboardManager;

    [Header("UI")]

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Button submitButton;

    [Header("Audio")]

    [SerializeField]
    private AudioClip fissSound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        mainMenuManager = GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManager>();
        leaderboardManager = GameObject.FindGameObjectWithTag("LeaderboardManager").GetComponent<LeaderboardManager>();
    }

    private void Update()
    {
        if (inputField.text.IndexOfAny(mainMenuManager.turkishChars) >= 0 || inputField.text.Length < 3)
            submitButton.interactable = false;
        else
            submitButton.interactable = true;
    }

    /*
     * desc: saves player's selected name and default score to the leaderboard
     */
    public async void OnSubmitClicked()
    {
        try
        {
            SaveData newPlayer = new SaveData();
            newPlayer.Name = inputField.text;
            newPlayer.hasAChosenName = true;

            bool containsSameName = await leaderboardManager.CheckForNameAsync(newPlayer.Name);
            if (containsSameName)
            {
                newPlayer.Name = newPlayer.Name + "_";
                // TODO: Yeni isim seçtirsin
            }

            GameManager.Instance.SetSaveData(newPlayer);
            _ = LeaderboardManager.SetNewEntry();

            Exit();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Submit Name Error: " + e.Message);
        }
    }

    /*
     * desc: brings the name input panel to the scene
     */
    public void Enter()
    {
        anim.SetBool("Open", true);
        if (fissSound != null)
            audioSource.PlayOneShot(fissSound);

        submitButton.interactable = true;
    }

    /*
     * desc: removes the name input panel from the scene
     */
    public void Exit()
    {
        anim.SetBool("Open", false);
        mainMenuManager.ActivateAllButtons();
    }
}
