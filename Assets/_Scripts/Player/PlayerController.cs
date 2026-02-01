using Enums;
using System;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController I;
    private int goldCollected = 0;
    private int xpCollected = 0;
    public CharacterType character;
    public AttackType attackType;

    [Header("Cosmetic Settings")]
    [SerializeField] private SpriteRenderer hatCosmeticSR;
    [SerializeField] private SpriteRenderer maskCosmeticSR;
	[SerializeField] private SpriteRenderer characterSR;
    [SerializeField] private Animator anim;

	[Header("Collectable")]
    public float collectRadius = 2f;
    public LayerMask collectableLayer;

    [Header("Levels")]
    [Tooltip("Level artýþý için üs deðeri.")]
    public float power = 1.3f;
    public float baseXP = 10f;
    public int currentLevel = 1;
    private int xpNeededForNextLevel;

    [Header("UI")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI nextLevelXP;
    [SerializeField] private TextMeshProUGUI currentLevelNumber;

    public PlayerHealthController HealthController { get; private set; }

	public static event Action<int> onLevelChanged;

	private void Awake()
	{
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }
        I = this;

        HealthController = GetComponent<PlayerHealthController>();

        InitializePlayerData();


		
        CalculateNextLevelXP();

        EventManager.RaiseCharacterChanged
            (GameManager.Instance.CharacterDatabase.GetCharacterByID(GameManager.Instance.SaveData.equippedCharacterID));
	}
	private void OnEnable()
	{
		EventManager.OnPlayerDeath += Die;
	}
	private void OnDisable()
	{
		EventManager.OnPlayerDeath -= Die;
	}
	private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        CheckForCollectables();
    }

	void InitializePlayerData()
	{
		// Eriþimleri kýsaltalým
		var saveData = GameManager.Instance.SaveData;
		var cosmeticDB = GameManager.Instance.CosmeticDatabase;
		var charDB = GameManager.Instance.CharacterDatabase;

		// --- 1. ÞAPKA (HAT) AYARLAMA ---
		if (saveData.equippedHatID == 0)
		{
			// Þapka takýlý deðil, görünmez yap
			hatCosmeticSR.sprite = null;
			// Veya performans için: hatCosmeticSR.enabled = false;
		}
		else
		{
			CosmeticData hat = cosmeticDB.GetDataByID(saveData.equippedHatID);
			if (hat != null && hat.sprite != null)
			{
				hatCosmeticSR.sprite = hat.sprite;
				// hatCosmeticSR.enabled = true; // Eðer kapatýyorsan açmayý unutma
			}
			else
			{
				// ID var ama veri bulunamadý (Hata durumu)
				hatCosmeticSR.sprite = null;
			}
		}

		// --- 2. MASKE (MASK) AYARLAMA ---
		if (saveData.equippedMaskID == 0)
		{
			maskCosmeticSR.sprite = null;
		}
		else
		{
			CosmeticData mask = cosmeticDB.GetDataByID(saveData.equippedMaskID);
			if (mask != null && mask.sprite != null)
			{
				maskCosmeticSR.sprite = mask.sprite;
			}
			else
			{
				maskCosmeticSR.sprite = null;
			}
		}

		// --- 3. KARAKTER (CHARACTER) AYARLAMA ---
		// Karakter her zaman var olmalý (ID 0 olsa bile default karakter vardýr)
		CharacterDataSO character = charDB.GetCharacterByID(saveData.equippedCharacterID);

		if (character != null)
		{
			// Karakterin Sprite'ý (Idle duruþu vs.)
			if (character.Sprite != null)
				characterSR.sprite = character.Sprite;

			// Karakterin Animasyon Kontrolcüsü (Farklý karakterlerin farklý animasyonlarý olabilir)
			if (character.AnimatorController != null)
				anim.runtimeAnimatorController = character.AnimatorController;
		}
		else
		{
			Debug.LogError($"Character Data Not Found for ID: {saveData.equippedCharacterID}");
		}
	}

	public void IncreaseGold(int amount = 1)
    {
        goldCollected += amount;
    }

    public void IncreaseXP(int amount = 1)
    {
        xpCollected += amount;

        CheckLevelUp();
        UpdateUI();
    }

    public void DecreaseGold(int amount = 1)
    {
        goldCollected -= amount;
        if (goldCollected < 0)
            goldCollected = 0;
    }

    public void DecreaseXP(int amount = 1)
    {
        xpCollected -= amount;
        if (xpCollected < 0)
            xpCollected = 0;

        UpdateUI();
    }

    private void CheckForCollectables()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, collectRadius, collectableLayer);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].TryGetComponent(out Collectable collectable))
            {
                collectable.MarkAsDetected();
            }
        }
    }

    private void CheckLevelUp()
    {
        bool levelUp = false;
        while (xpCollected >= xpNeededForNextLevel)
        {
            xpCollected -= xpNeededForNextLevel;
            currentLevel++;
            levelUp = true;

            // TODO: PlayLevelUpEffect(); 
            CalculateNextLevelXP();
        }

        if (levelUp)
            onLevelChanged?.Invoke(currentLevel);
    }

    private void CalculateNextLevelXP()
    {
        xpNeededForNextLevel = GetXPForNextLevel(currentLevel);
    }

    public int GetXPForNextLevel(int level)
    {
        if (level <= 1) return (int)baseXP;
        return Mathf.RoundToInt(baseXP * Mathf.Pow(level, power));
    }

    private void UpdateUI()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpNeededForNextLevel;
            xpSlider.value = xpCollected;
        }

        if (nextLevelXP != null)
            nextLevelXP.text = $"{xpCollected} / {xpNeededForNextLevel}";

        if (currentLevelNumber != null)
            currentLevelNumber.text = "Level " + currentLevel.ToString();
    }

    private void Die()
    {
        // TODO: Die animation
        int finalScore = Mathf.FloorToInt(ScoreManager.Instance.CurrentScore);
        int charID = (int)character;
        int atkID = (int)attackType;

        _ = LeaderboardManager.SubmitScoreAsync(finalScore, charID, atkID);

        // TODO: open game over panel
    }

    public float GetLostHealth() => HealthController.MaxHealth - HealthController.CurrentHealth;
    public float GetCurrentHealth() => HealthController.CurrentHealth;
    public float GetCurrentMadness() => MaskController.I.CurrentMadness;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}