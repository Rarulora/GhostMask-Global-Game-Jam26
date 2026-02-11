using UnityEngine;

public abstract class PerkBase : ScriptableObject
{
	[Header("UI Info")]
	public string ID;
	public string PerkName;
	[TextArea] public string Description;
	public Sprite Icon;
	public PerkBase prequisite;
	public bool oneTime;

	// Called once when the perk is acquired
	public virtual void OnEquip(GameObject player) { }

	// Called once if the perk is removed (optional)
	public virtual void OnUnequip(GameObject player) { }
}