using UnityEngine;
using UnityEngine.UI;

public class LeaderboardMenu : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private AudioSource audioSource;
    //private MainMenuManager mainMenuManager;
    private LeaderboardManager leaderboardManager;

    [Header("UI")]

    [SerializeField]
    private Button backButton;


    [Header("Audio")]

    [SerializeField]
    private AudioClip fissSound;

    [Header("Flags")]
    private bool opened = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        //mainMenuManager = GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManager>();
        leaderboardManager = GameObject.FindGameObjectWithTag("LeaderboardManager").GetComponent<LeaderboardManager>();
    }

    private void Update()
    {
        if (opened && Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }
    }

    /*
     * desc: brings the leaderboard to the scene
     */
    public async void Enter()
    {
        await leaderboardManager.RefreshLeaderboardAsync();

        opened = true;
        anim.SetBool("Open", true);
        ActivateBackButton();

        audioSource.PlayOneShot(fissSound);

        //GameManager.Instance.SetState(GameState.Leaderboard);
    }

    /*
     * desc: removes the leaderboard from the scene
     */
    public void Exit()
    {
        opened = false;
        anim.SetBool("Open", false);
        DeactivateBackButton();

        //GameManager.Instance.SetState(GameState.MainMenu);
    }

    /*
     * desc: Makes all buttons' interactable flag true and sets backButton as active
     */
    private void ActivateBackButton()
    {
        //mainMenuManager.DeactivateAllButtons();

        backButton.gameObject.SetActive(true);
        backButton.interactable = true;
    }

    /*
     * desc: Makes all buttons' interactable flag false and sets backButton as deactive
     */
    private void DeactivateBackButton()
    {
        backButton.interactable = false;
        backButton.gameObject.SetActive(false);

        //mainMenuManager.ActivateAllButtons();
    }
}
