using Enums;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Weapon Data", fileName = "New_Weapon")]
public class WeaponData : ScriptableObject
{
	[Header("Identity")]
	public string WeaponName;
	public WeaponCategory Category;
	public WeaponStyle Style;
	public Sprite Icon;

	[Header("Combat Stats")]
	public float Damage;
	public float AttackRate; // Saniye başı saldırı
	public float Range;      // Melee için alan, Ranged için menzil
	public float Force;      // Knockback gücü

	[Header("Visuals & Prefabs")]
	public GameObject ProjectilePrefab; // Ranged için
	public GameObject HitVFX;

	[Header("Dash Specific")]
	public float DashSpeed = 20f;
	public float DashDuration = 0.2f;
}