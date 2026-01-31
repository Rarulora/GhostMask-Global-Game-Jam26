using System.Collections.Generic;
using UnityEngine;
using Enums;

[CreateAssetMenu(fileName = "NewStatPerk", menuName = "Perk System/Stat Perk")]
public class StatPerk : PerkBase
{
	[System.Serializable]
	public struct ModifierData
	{
		public StatType Stat;
		public float Value;
		public StatModType Type;
	}

	public List<ModifierData> Modifiers;

	public override void OnEquip(GameObject player)
	{
		var statsController = StatsController.I;
		if (statsController == null) return;

		foreach (var modData in Modifiers)
		{
			// Create a modifier. Source is this SO to allow removal later if needed.
			StatModifier mod = new StatModifier(modData.Value, modData.Type, this);
			statsController.GetStat(modData.Stat).AddModifier(mod);
		}
	}
}