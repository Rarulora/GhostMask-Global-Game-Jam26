using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Base Settings")]
    [SerializeField] private float scorePerSecond = 5f;       // Saniye baþý kazanýlan skor
    [SerializeField] private float scorePerKill = 100f;       // Düþman baþýna taban skor

    [Header("Madness Multiplier")]
    [SerializeField] private float madnessThreshold = 75f;    // Hangi deðerden sonra çarpan devreye girsin?
    [SerializeField] private float madnessScoreMultiplier = 2f; // Madness yüksekken kaç kat puan gelsin?

    [Header("Combo System")]
    [SerializeField] private int maxCombo = 50;               // Maksimum kombo sýnýrý

    // Runtime Variables
    private float currentScore = 0f;
    private int currentCombo = 1;
    private float lastKnownHealth; // Hasar alýp almadýðýný kontrol için

    public float CurrentScore => currentScore;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Baþlangýç canýný kaydet ki hasar kontrolü yapabilelim
        if (PlayerController.I != null && PlayerController.I.HealthController != null)
        {
            lastKnownHealth = PlayerController.I.HealthController.CurrentHealth;
        }

        UpdateComboUI();
    }

    private void OnEnable()
    {
        EventManager.OnEnemyKilled += HandleEnemyKilled;
        EventManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        EventManager.OnEnemyKilled -= HandleEnemyKilled;
        EventManager.OnHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        HandleTimeScore();
    }

    // 1. ZAMANLA SKOR KAZANMA
    private void HandleTimeScore()
    {
        float multiplier = GetMadnessMultiplier();
        float gainedScore = scorePerSecond * multiplier * Time.deltaTime;

        AddScore(gainedScore);
    }

    // 2. DÜÞMAN ÖLDÜRME VE KOMBO ARTIÞI
    private void HandleEnemyKilled(EnemyBase enemy)
    {
        // Madness kontrolü
        float madMultiplier = GetMadnessMultiplier();

        // Formül: (Taban Puan * Madness Çarpaný * Kombo Çarpaný)
        float totalKillScore = scorePerKill * madMultiplier * currentCombo;

        AddScore(totalKillScore);

        // Komboyu artýr
        IncreaseCombo();
    }

    // 3. HASAR ALINCA KOMBO SIFIRLAMA
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        // Eðer þu anki can, son bildiðimiz candan az ise hasar yemiþizdir.
        if (currentHealth < lastKnownHealth)
        {
            ResetCombo();
        }

        // Deðeri güncelle (Ýyileþme durumunda sorun çýkmamasý için)
        lastKnownHealth = currentHealth;
    }

    // --- YARDIMCI METOTLAR ---

    private void AddScore(float amount)
    {
        currentScore += amount;
        EventManager.RaiseScoreChanged(currentScore);
    }

    private void IncreaseCombo()
    {
        if (currentCombo < maxCombo)
        {
            currentCombo++;
            UpdateComboUI();
        }
    }

    private void ResetCombo()
    {
        if (currentCombo > 1) // Sadece zaten kombo varsa sýfýrla
        {
            currentCombo = 1;
            Debug.Log("Combo Lost!");
            UpdateComboUI();

            // Ýstersen burada "Combo Broken" efekti veya sesi çaldýrabilirsin
        }
    }

    private void UpdateComboUI()
    {
        EventManager.RaiseComboChanged(currentCombo);
    }

    private float GetMadnessMultiplier()
    {
        // PlayerController üzerinden Madness deðerini çekiyoruz
        if (PlayerController.I != null)
        {
            float currentMadness = PlayerController.I.GetCurrentMadness();
            if (currentMadness >= madnessThreshold)
            {
                return madnessScoreMultiplier;
            }
        }
        return 1f; // Threshold altýnda ise çarpan yok (1x)
    }
}