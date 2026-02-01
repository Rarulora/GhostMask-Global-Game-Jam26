using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

public class StatsController : MonoBehaviour
{
	public static StatsController I;
	[System.Serializable]
	public struct StatInitializer
	{
		public StatType Type;
		public float BaseValue;
	}

	[Header("Base Stats Configuration")]
	[SerializeField] private List<StatInitializer> startingStats;

	private Dictionary<StatType, PlayerStat> _stats;
	private List<TimedStatModifier> _timedModifiers = new List<TimedStatModifier>();

	private void OnEnable()
	{
		EventManager.OnCharacterChanged += InitializeCharacterData;
	}
	private void OnDisable()
	{
		EventManager.OnCharacterChanged -= InitializeCharacterData;
	}

	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;

		InitializeStats();
	}

    private void Start()
    {
        AssignStatModifiersForPurchasedSkills(GetPurchasedSkills());
    }

    private void Update()
	{
		for (int i = _timedModifiers.Count - 1; i >= 0; i--)
		{
			var timedMod = _timedModifiers[i];

			timedMod.Timer -= Time.deltaTime;

			if (timedMod.Timer <= 0)
			{
				GetStat(timedMod.TargetStat).RemoveModifier(timedMod.Modifier);

				_timedModifiers.RemoveAt(i);
			}
		}
	}
	private void InitializeStats()
	{
		_stats = new Dictionary<StatType, PlayerStat>();

		foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
		{
			_stats.Add(type, new PlayerStat(0));
		}

		foreach (var init in startingStats)
		{
			if (_stats.ContainsKey(init.Type))
			{
				_stats[init.Type].BaseValue = init.BaseValue;
			}
		}
	}

	private void InitializeCharacterData(CharacterDataSO data)
	{
		PlayerStat health;
		if (_stats.ContainsKey(StatType.health))
			health = _stats[StatType.health];
		else
		{
			health = new PlayerStat(0);
			_stats.Add(StatType.health, health);
		}
		health.BaseValue = data.Health;

		PlayerStat damage;
		if (_stats.ContainsKey(StatType.damage))
			damage = _stats[StatType.damage];
		else
		{
			damage = new PlayerStat(0);
			_stats.Add(StatType.damage, damage);
		}
		damage.BaseValue = data.Damage;

		PlayerStat attackRate;
		if (_stats.ContainsKey(StatType.attackRate))
			attackRate = _stats[StatType.attackRate];
		else
		{
			attackRate = new PlayerStat(0);
			_stats.Add(StatType.attackRate, attackRate);
		}
		attackRate.BaseValue = data.AttackRate;

		PlayerStat moveSpeed;
		if (_stats.ContainsKey(StatType.moveSpeed))
			moveSpeed = _stats[StatType.moveSpeed];
		else
		{
			moveSpeed = new PlayerStat(0);
			_stats.Add(StatType.moveSpeed, moveSpeed);
		}
		moveSpeed.BaseValue = data.MoveSpeed;
	}

	private SkillData[] GetPurchasedSkills()
	{
		int[] purchasedSkillIDs = GameManager.Instance.SaveData.gainedSkillIDs;
		SkillDatabase skillDatabase = Resources.Load<SkillDatabase>("SkillDatabase");

		List<SkillData> purchasedSkills = new List<SkillData>();
		foreach (var skill in skillDatabase.data)
		{
			if (purchasedSkillIDs.Contains<int>(skill.ID))
                purchasedSkills.Add(skill);
        }

		return purchasedSkills.ToArray();
	}

	private void AssignStatModifiersForPurchasedSkills(SkillData[] purchasedSkills)
	{
		foreach (var skill in purchasedSkills)
		{
			StatModifier modifier = new StatModifier(skill.value, skill.affectType);
			_stats[skill.affectedStat].AddModifier(modifier);
		}
	}

	public void AddTimedModifier(StatType statType, float value, StatModType modType, float duration, string effectID = "")
	{
		if (!string.IsNullOrEmpty(effectID))
		{
			var existingMod = _timedModifiers.Find(x => x.ID == effectID);
			if (existingMod != null)
			{
				existingMod.ResetTimer();
				return;
			}
		}

		StatModifier mod = new StatModifier(value, modType, this);

		GetStat(statType).AddModifier(mod);

		_timedModifiers.Add(new TimedStatModifier(effectID, duration, mod, statType));
	}

	public PlayerStat GetStat(StatType type)
	{
		if (_stats.ContainsKey(type))
			return _stats[type];

		Debug.LogError($"Stat {type} bulunamadı! Dictionary başlatılmamış olabilir.");
		return null;
	}

	public float GetValue(StatType type)
	{
		return GetStat(type).Value;
	}
}