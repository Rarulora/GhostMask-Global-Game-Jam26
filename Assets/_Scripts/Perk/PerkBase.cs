using UnityEngine;

public abstract class PerkBase : ScriptableObject
{
	[Header("UI Info")]
	public string ID;
	public string PerkName;
	[TextArea] public string Description;
	public Sprite Icon;

	// Called once when the perk is acquired
	public virtual void OnEquip(GameObject player) { }

	// Called once if the perk is removed (optional)
	public virtual void OnUnequip(GameObject player) { }
}