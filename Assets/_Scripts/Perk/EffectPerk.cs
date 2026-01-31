using UnityEngine;
using Enums;

[CreateAssetMenu(fileName = "NewEffectPerk", menuName = "Perk System/Effect Perk")]
public class EffectPerk : PerkBase
{
	[Header("Trigger Logic")]
	public TriggerType Trigger;
	[Range(0, 100)] public float Chance = 100f;
	public float Cooldown = 0f;

	[Header("Action")]
	public EffectType Effect;
	public float Value;     // Damage amount, Heal amount, etc.
	public float Duration;  // For DOTs or Buffs
	public GameObject VFX;  // Optional Visual Effect

	// Conditions (Optional)
	public bool RequiresMaskOn;
	public bool RequiresMaskOff;
	public float HealthThreshold; // e.g., < 0.3 for low health logic

	// Runtime logic is handled by PerkManager, not here.
	// This is just data container.
	public override void OnEquip(GameObject player)
	{
		// Register this perk to the manager
		player.GetComponent<PerkManager>().RegisterEffectPerk(this);
	}
}