using UnityEngine;
using Enums;

[CreateAssetMenu(menuName = "Perk System/Ability Perk", fileName = "NewAbilityPerk")]
public class AbilityPerk : PerkBase
{
	[Header("Ability Settings")]
	public WeaponData WeaponToUnlock;
	public string EventID;

	// Override ismini OnEquip yaptık
	public override void OnEquip(GameObject player)
	{
		if (WeaponToUnlock != null)
		{
			var wc = player.GetComponentInChildren<WeaponController>();
			if (wc != null) wc.EquipWeapon(WeaponToUnlock);
		}

		if (!string.IsNullOrEmpty(EventID))
		{
			Debug.Log($"Ability Unlocked: {EventID}");
			// EventManager.RaisePerkEvent(EventID);
		}
	}
}