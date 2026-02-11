using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enums;

public class PerkManager : MonoBehaviour
{
	public static PerkManager I;

	[Header("AttackTypePerks Logic")]
	[SerializeField] private int thresholdLevelForAttackTypePerks = 4;
	[SerializeField] private int maxLevelForAttackTypePerks = 8;
	private bool choosedAttackType = false;

	[Header("UI References")]
	[SerializeField] private PerkCard perk1;
	[SerializeField] private PerkCard perk2;
	[SerializeField] private PerkCard perk3;

	private List<PerkBase> _allPerksDatabase;
	private List<PerkBase> _currentOfferedPerks = new List<PerkBase>();

	private List<EffectPerk> _onHitPerks = new List<EffectPerk>();
	private List<EffectPerk> _onKillPerks = new List<EffectPerk>();
	private List<EffectPerk> _onTakeDamagePerks = new List<EffectPerk>();

	private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();
	private bool _isMaskActive;

	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;

		_allPerksDatabase = Resources.LoadAll<PerkBase>("Perks").ToList();
	}

	private void OnEnable()
	{
		PlayerController.onLevelChanged += ShowPerks;

		EventManager.OnEnemyKilled += HandleEnemyKilled;
		EventManager.OnMaskChanged += (status) => _isMaskActive = status;
	}

	private void OnDisable()
	{
		PlayerController.onLevelChanged -= ShowPerks;

		EventManager.OnEnemyKilled -= HandleEnemyKilled;
		EventManager.OnMaskChanged -= (status) => _isMaskActive = status;
	}

	private void ShowPerks(int currentLevel)
	{
		GameManager.Instance.StartPerkSelect();
		_currentOfferedPerks.Clear();

		if (currentLevel == 2)
		{
			var weaponPerks = _allPerksDatabase.OfType<AbilityPerk>()
											   .Where(p => p.WeaponToUnlock != null)
											   .Cast<PerkBase>().ToList();

			FillCards(GetRandomPerks(weaponPerks, 3));
        }
		else
		{
			// Standard (Max 1 weapon perk)
			FillCards(GetBalancedRandomPerks());
        }

		perk1.Show();
        perk2.Show();
        perk3.Show();
    }

	public void OnClickPerk(int perkIndex)
	{
		if (perkIndex >= _currentOfferedPerks.Count) return;

		PerkBase selectedPerk = _currentOfferedPerks[perkIndex];
		if (PlayerController.I.currentLevel == 2)
		{
			if (selectedPerk.PerkName.Equals("Melee"))
			{
                PlayerController.I.attackType = AttackType.Melee;
                Debug.Log("Melee");
            }
			else if (selectedPerk.PerkName.Equals("Ranged"))
			{
                PlayerController.I.attackType = AttackType.Ranged;
				Debug.Log("Ranged");
            }
			else
			{
                PlayerController.I.attackType = AttackType.Dash;
                Debug.Log("Dash");
            }
		}

		EquipPerk(selectedPerk);

		perk1.Hide();
		perk2.Hide();
		perk3.Hide();
		GameManager.Instance.StopPerkSelect();
	}

    private List<PerkBase> GetBalancedRandomPerks()
    {
        List<PerkBase> candidates = _allPerksDatabase
            .Where(p => !(p is AbilityPerk))
            .ToList();

        List<PerkBase> selected = new List<PerkBase>();
        int slotsToFill = 3;

        if (candidates.Count <= slotsToFill)
        {
            return candidates;
        }

        int attempts = 0;
        int maxAttempts = 50;

        for (int i = 0; i < slotsToFill; i++)
        {
            if (candidates.Count == 0 || attempts >= maxAttempts) break;

            int randomIndex = Random.Range(0, candidates.Count);
            PerkBase pick = candidates[randomIndex];

            bool isPrerequisiteMet = true;
            if (pick.prequisite != null)
            {
                bool hasAlready = PlayerController.I.HasEquippedPerk(pick.prequisite.ID);
                if (!hasAlready)
                    isPrerequisiteMet = false;
            }

			bool isPickedBefore = false;
			if (pick.oneTime && PlayerController.I.HasEquippedPerk(pick.ID))
				isPickedBefore = true;

            if (isPrerequisiteMet && !isPickedBefore)
            {
                selected.Add(pick);
                candidates.RemoveAt(randomIndex);

                attempts = 0;
            }
            else
            {
                i--;
                attempts++;
            }
        }

        return selected;
    }

    private List<PerkBase> GetRandomPerks(List<PerkBase> pool, int count)
	{
		List<PerkBase> result = new List<PerkBase>();
		List<PerkBase> tempPool = new List<PerkBase>(pool);

		for (int i = 0; i < count; i++)
		{
			if (tempPool.Count == 0) break;
			var pick = tempPool[Random.Range(0, tempPool.Count)];
			result.Add(pick);
			tempPool.Remove(pick);
		}
		return result;
	}

    private void FillCards(List<PerkBase> perks)
    {
        _currentOfferedPerks = perks;

        if (perks.Count > 0)
        {
            perk1.gameObject.SetActive(true);
            perk1.Setup(perks[0], 0);
        }
        else
        {
            perk1.gameObject.SetActive(false);
        }

        if (perks.Count > 1)
        {
            perk2.gameObject.SetActive(true);
            perk2.Setup(perks[1], 1);
        }
        else
        {
            perk2.gameObject.SetActive(false);
        }

        if (perks.Count > 2)
        {
            perk3.gameObject.SetActive(true);
            perk3.Setup(perks[2], 2);
        }
        else
        {
            perk3.gameObject.SetActive(false);
        }
    }

    public void EquipPerk(PerkBase perk)
	{
		perk.OnEquip(PlayerController.I.gameObject);
        PlayerController.I.AcquiredPerkIDs.Add(perk.ID);

        if (perk is EffectPerk effectPerk)
		{
			RegisterEffectPerk(effectPerk);
		}

		Debug.Log($"Perk Seçildi: {perk.PerkName}");
	}

	public void RegisterEffectPerk(EffectPerk perk)
	{
		switch (perk.Trigger)
		{
			case TriggerType.OnHit: _onHitPerks.Add(perk); break;
			case TriggerType.OnKill: _onKillPerks.Add(perk); break;
			case TriggerType.OnTakeDamage: _onTakeDamagePerks.Add(perk); break;
		}
	}

	public void TriggerOnHit(IDamageable target, Vector3 hitPos)
	{
		ProcessPerkList(_onHitPerks, target, hitPos);
	}

	public void TriggerOnTakeDamage(float amount)
	{
		ProcessPerkList(_onTakeDamagePerks, null, transform.position);
	}

	private void HandleEnemyKilled(EnemyBase enemy)
	{
		ProcessPerkList(_onKillPerks, enemy, enemy.transform.position);
	}

	private void ProcessPerkList(List<EffectPerk> perks, IDamageable target, Vector3 pos)
	{
		foreach (var perk in perks)
		{
			if (perk.RequiresMaskOn && !_isMaskActive) continue;
			if (perk.RequiresMaskOff && _isMaskActive) continue;

			if (perk.Cooldown > 0)
			{
				if (_cooldowns.ContainsKey(perk.PerkName) && Time.time < _cooldowns[perk.PerkName]) continue;
				_cooldowns[perk.PerkName] = Time.time + perk.Cooldown;
			}

			if (Random.Range(0f, 100f) > perk.Chance) continue;
			if (perk.targetIsPlayer) target = PlayerController.I.HealthController;

			ApplyEffect(perk, target, pos);
		}
	}

	private void ApplyEffect(EffectPerk perk, IDamageable target, Vector3 pos)
	{
		if (perk.VFX != null) Instantiate(perk.VFX, pos, Quaternion.identity);

		switch (perk.Effect)
		{
			case EffectType.Heal:
				PlayerController.I.gameObject.GetComponent<PlayerHealthController>().Heal(perk.Value);
				break;

			case EffectType.Burn:
			case EffectType.Slow:
			case EffectType.Poison:
			case EffectType.Stun:
				if (target is IStatusEffectable effectable)
				{
					effectable.ApplyStatus(perk.Effect, perk.Duration, perk.Value);
				}
				break;

			case EffectType.Explosion:
				var hits = Physics2D.OverlapCircleAll(pos, 3f);
				foreach (var hit in hits) if (hit.CompareTag("Enemy"))
						hit.GetComponent<IDamageable>()?.TakeDamage(perk.Value, false, Vector2.zero, 0);
				break;
			case EffectType.Madness:
				MaskController.I.IncreaseMadness(perk.Value);
				break;
		}
	}
}