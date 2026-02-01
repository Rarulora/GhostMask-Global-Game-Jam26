using Enums;
using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
	protected WeaponData data;
	protected Transform playerTransform;
	protected Transform firePoint;
	protected Rigidbody2D playerRb;
	protected Animator anim;
	protected bool canAttack = true;


	// Silahı başlatan fonksiyon
	public virtual void Initialize(WeaponData weaponData, Transform owner, Transform firePoint, Rigidbody2D rb, Animator anim)
	{
		data = weaponData;
		playerTransform = owner;
		this.firePoint = firePoint;
		playerRb = rb;
		this.anim = anim;
	}

	// Her silah bunu kendine göre dolduracak
	public abstract void Attack();

	protected IEnumerator CooldownRoutine()
	{
		canAttack = false;
		yield return new WaitForSeconds(1f / GetStat(Enums.StatType.attackRate));
		canAttack = true;
	}

	protected float GetStat(Enums.StatType type)
	{
		float result = 0f;
		if (StatsController.I != null) result = StatsController.I.GetStat(type).Value;
		return result;
	}
	protected bool IsCrit()
	{
		return (Random.value * 100f) <= StatsController.I.GetValue(StatType.critChance);
	}
	protected void PlayAnimation()
	{
		anim.Play("Attack_" + data.WeaponName);
	}
}