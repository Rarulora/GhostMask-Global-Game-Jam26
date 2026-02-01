using UnityEngine;
using Enums;

[CreateAssetMenu(fileName = "NewDynamicPerk", menuName = "Perk System/Dynamic Perk")]
public class DynamicStatPerk : PerkBase
{
	public StatType TargetStat;     // Stat to modify (e.g., Damage)
	public StatType SourceStat;     // Stat to read (e.g., MoveSpeed)
	public float Multiplier = 0.5f; // How much does source affect target?

	// Special flag for Madness since it's not a standard stat
	public SourceExtension sourceExtensionStat;

	[Header("Condition")]
	public bool HaveCondition = false;
	public string ConditionPerkID = "";

	public override void OnEquip(GameObject player)
	{
		// Add a component to player that handles the update logic
		var handler = player.AddComponent<DynamicPerkHandler>();
		handler.Initialize(this);
	}
}

// Runtime component created automatically
public class DynamicPerkHandler : MonoBehaviour
{
	private DynamicStatPerk _data;
	private StatsController _stats;
	private StatModifier _modifier;
	private MaskController _maskController; // Assuming this holds current madness

	public void Initialize(DynamicStatPerk data)
	{
		_data = data;
		_stats = GetComponent<StatsController>();
		_maskController = GetComponent<MaskController>();

		// Create a dirty modifier that we will update
		_modifier = new StatModifier(0, StatModType.Flat, this);
		_stats.GetStat(_data.TargetStat).AddModifier(_modifier);
	}

	private void Update()
	{
		float sourceValue = 0;

		if (_data.sourceExtensionStat == SourceExtension.currentMadness)
		{
			// You need to expose current madness via property in MaskController
			sourceValue = _maskController != null ? _maskController.CurrentMadness : 0;
		}
		else if (_data.sourceExtensionStat == SourceExtension.currentHealth)
		{
			// You need to expose current madness via property in MaskController
			sourceValue = PlayerController.I != null ? PlayerController.I.GetCurrentHealth() : 0;
		}
		else if (_data.sourceExtensionStat == SourceExtension.lostHealth)
		{
			// You need to expose current madness via property in MaskController
			sourceValue = PlayerController.I != null ? PlayerController.I.GetLostHealth() : 0;
		}
		else
		{
			sourceValue = _stats.GetValue(_data.SourceStat);
		}

		// Update modifier value
		// E.g., Momentum: Damage += Speed * 0.5
		_modifier.Value = sourceValue * _data.Multiplier;


		_stats.GetStat(_data.TargetStat).SetDirty();
	}

	private void OnDestroy()
	{
		if (_stats != null) _stats.GetStat(_data.TargetStat).RemoveModifier(_modifier);
	}
}