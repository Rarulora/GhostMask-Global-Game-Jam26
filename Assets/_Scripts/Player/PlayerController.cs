using Enums;
using System;
using System.Collections.Generic;
using TMPro;
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

    [Header("Inventory")]
    public List<string> AcquiredPerkIDs = new List<string>();

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
        var saveData = GameManager.Instance.SaveData;
        var cosmeticDB = GameManager.Instance.CosmeticDatabase;
        var charDB = GameManager.Instance.CharacterDatabase;

        character = (CharacterType)saveData.equippedCharacterID;

        if (saveData.equippedHatID == 0)
        {
            hatCosmeticSR.sprite = null;
        }
        else
        {
            CosmeticData hat = cosmeticDB.GetDataByID(saveData.equippedHatID);
            if (hat != null && hat.sprite != null)
            {
                hatCosmeticSR.sprite = hat.sprite;
            }
            else
            {
                hatCosmeticSR.sprite = null;
            }
        }

		CosmeticData mask = cosmeticDB.GetDataByID(saveData.equippedMaskID);
		if (mask != null && mask.sprite != null)
		{
			maskCosmeticSR.sprite = mask.sprite;
		}
		else
		{
			maskCosmeticSR.sprite = null;
		}

		CharacterDataSO characterData = charDB.GetCharacterByID(saveData.equippedCharacterID);
        if (characterData != null)
        {
            if (characterData.Sprite != null)
                characterSR.sprite = characterData.Sprite;

            if (characterData.AnimatorController != null)
                anim.runtimeAnimatorController = characterData.AnimatorController;
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

            CalculateNextLevelXP();
        }

        if (levelUp)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.levelUpSound);
            onLevelChanged?.Invoke(currentLevel);
        }    
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
        int finalScore = Mathf.FloorToInt(ScoreManager.Instance.CurrentScore);
        int currentCharID = (int)character;
        int currentAtkID = (int)attackType;

        var saveData = GameManager.Instance.SaveData;
        saveData.gold += goldCollected;

        if (saveData.bestRunData == null)
            saveData.bestRunData = new HighScoreData();

        if (finalScore > saveData.bestRunData.highScore)
        {
            saveData.bestRunData.highScore = finalScore;
            saveData.bestRunData.character = (CharacterType)currentCharID;
            saveData.bestRunData.attackType = (AttackType)currentAtkID;
        }

        if (saveData.bestRunData.highScore > 0)
        {
            _ = LeaderboardManager.SubmitScoreAsync(
                Mathf.FloorToInt(saveData.bestRunData.highScore),       // Þu anki skoru deðil, REKORU yolla
                (int)saveData.bestRunData.character,  // Rekorun kýrýldýðý karakteri yolla
                (int)saveData.bestRunData.attackType  // Rekorun saldýrý tipini yolla
            );
        }

        GameManager.Instance.SaveGame();
    }

    public float GetLostHealth() => HealthController.MaxHealth - HealthController.CurrentHealth;
    public float GetCurrentHealth() => HealthController.CurrentHealth;
    public float GetCurrentMadness() => MaskController.I.CurrentMadness;

    public bool HasEquippedPerk(string perkID)
    {
        if (AcquiredPerkIDs.Contains(perkID))
            return true;

        return false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}