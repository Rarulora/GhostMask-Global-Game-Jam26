using System.Collections.Generic;
using UnityEngine;
using Enums;

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