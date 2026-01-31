using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController I;
    private int goldCollected = 0;
    private int xpCollected = 0;

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

        EventManager.RaiseCharacterChanged
            (GameManager.Instance.CharacterDatabase.GetCharacterByID(GameManager.Instance.SaveData.equippedCharacterID));
	}

	private void Start()
    {
        CalculateNextLevelXP();
        UpdateUI();
        onLevelChanged?.Invoke(1);
    }

    private void Update()
    {
        CheckForCollectables();
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
        if (level < 1) return (int)baseXP;
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

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}