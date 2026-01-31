using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputPanel : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private AudioSource audioSource;
    private MainMenuManager mainMenuManager;
    // LeaderboardManager singleton veya static olduðu için referansa gerek kalmayabilir ama tag ile buluyorsan kalsýn.
    private LeaderboardManager leaderboardManager;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;

    [Header("Audio")]
    [SerializeField] private AudioClip fissSound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Taglerin Unity Editör'de "MainMenuManager" ve "LeaderboardManager" olarak atandýðýndan emin ol!
        GameObject mainMenuObj = GameObject.FindGameObjectWithTag("MainMenuManager");
        if (mainMenuObj) mainMenuManager = mainMenuObj.GetComponent<MainMenuManager>();

        GameObject lbObj = GameObject.FindGameObjectWithTag("LeaderboardManager");
        if (lbObj) leaderboardManager = lbObj.GetComponent<LeaderboardManager>();
    }

    private void Update()
    {
        // Null check ekledik, manager bulunamazsa hata vermesin
        if (mainMenuManager != null)
        {
            if (inputField.text.IndexOfAny(mainMenuManager.turkishChars) >= 0 || inputField.text.Length < 3)
                submitButton.interactable = false;
            else
                submitButton.interactable = true;
        }
    }

    /*
     * desc: saves player's selected name and default score to the leaderboard
     */
    public async void OnSubmitClicked()
    {
        try
        {
            // --- DÜZELTME 1: Yeni SaveData oluþturmak yerine mevcut olana yazýyoruz ---
            // Böylece diðer ayarlar (Ses, Dil vb.) kaybolmaz.
            SaveData currentSave = GameManager.Instance.SaveData;

            // Eðer SaveData null ise (çok düþük ihtimal ama) yeni oluþtur
            if (currentSave == null)
            {
                currentSave = new SaveData();
                GameManager.Instance.SetSaveData(currentSave);
            }

            string tempName = inputField.text;

            // Ýsim kontrolü (Basit versiyon)
            if (leaderboardManager != null)
            {
                bool containsSameName = await leaderboardManager.CheckForNameAsync(tempName);
                if (containsSameName)
                {
                    // Basit bir çözüm: Sonuna rastgele sayý ekle
                    tempName = tempName + "_" + Random.Range(10, 99);
                }
            }

            currentSave.Name = tempName;
            currentSave.hasAChosenName = true;

            // BestRunData yoksa (ilk defa giriyorsa) 0 puanla oluþtur
            if (currentSave.bestRunData == null)
            {
                // Score: 0, CharID: 0, AtkID: 0 (Varsayýlanlar)
                currentSave.bestRunData = new HighScoreData(0, 0, 0);
            }

            GameManager.Instance.SaveGame();

            // --- DÜZELTME 2: Yeni Leaderboard Metodunu Çaðýrýyoruz ---
            // Oyuncu ilk kez isim girdiðinde skoru 0'dýr. 
            // Bu yüzden tabloya 0 puan, 0 karakter, 0 atak tipi ile kayýt açýyoruz.

            // Fire-and-forget (await kullanmadýk çünkü void fonksiyon, async olsa bile UI donmasýn)
            _ = LeaderboardManager.SubmitScoreAsync(0, 0, 0);

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
        if (anim != null) anim.SetBool("Open", true);
        if (fissSound != null && audioSource != null)
            audioSource.PlayOneShot(fissSound);

        if (submitButton != null) submitButton.interactable = true;
    }

    /*
     * desc: removes the name input panel from the scene
     */
    public void Exit()
    {
        if (anim != null) anim.SetBool("Open", false);

        if (mainMenuManager != null)
            mainMenuManager.ActivateAllButtons();
    }
}