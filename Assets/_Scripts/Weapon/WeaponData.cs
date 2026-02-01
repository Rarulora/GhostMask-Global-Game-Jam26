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

	[Header("Stats")]
	public float damage = 1f;
	public float attackRate = 0.25f;
	public float range = 2f;
	public float knockbackForce = 0.5f;
	public int projectileCount = 0;
	public float projectileSpeed = 0f;
	public int pierce = 0;

	[Header("Visuals & Prefabs")]
	public GameObject ProjectilePrefab; // Ranged için
	public GameObject HitVFX;

	[Header("Dash Specific")]
	public float DashSpeed = 20f;
	public float DashDuration = 0.2f;
}