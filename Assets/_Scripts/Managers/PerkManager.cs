using UnityEngine;

public class PerkManager : MonoBehaviour
{
    [Header("AttackTypePerks")]
    [SerializeField] private int thresholdLevelForAttackTypePerks = 4;
    [SerializeField] private int maxLevelForAttackTypePerks = 8;
    private bool choosedAttackType = false;

    [Header("PerkCards")]
    [SerializeField] private PerkCard perk1;
    [SerializeField] private PerkCard perk2;
    [SerializeField] private PerkCard perk3;

    private void OnEnable()
    {
        PlayerController.onLevelChanged += ShowPerks;
    }

    private void OnDisable()
    {
        PlayerController.onLevelChanged -= ShowPerks;
    }

    private void ShowPerks(int currentLevel)
    {
        GameManager.Instance.StartPerkSelect();

        if (currentLevel == 2)
        {
            // TODO: Attack type perks
        }
        else if (currentLevel >= thresholdLevelForAttackTypePerks && currentLevel <= maxLevelForAttackTypePerks && !choosedAttackType)
        {
            float range = maxLevelForAttackTypePerks - thresholdLevelForAttackTypePerks;
            float progress = currentLevel - thresholdLevelForAttackTypePerks;
            float probability = (progress + 1) / (range + 1);

            float randomValue = Random.value;
            if (randomValue <= probability)
            {
                choosedAttackType = true;
            }
            else
            {
                choosedAttackType = false;
            }
        }
        else
        {
            // TODO: Random 3 perks
            // Max one weapon perk
        }
    }

    private void OnClickPerk(int perkIndex)
    {
        GameManager.Instance.StopPerkSelect();
    }
}
