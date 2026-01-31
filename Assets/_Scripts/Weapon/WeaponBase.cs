using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
	protected WeaponData data;
	protected Transform playerTransform;
	protected Rigidbody2D playerRb;
	protected bool canAttack = true;

	// Silahı başlatan fonksiyon
	public virtual void Initialize(WeaponData weaponData, Transform owner, Rigidbody2D rb)
	{
		data = weaponData;
		playerTransform = owner;
		playerRb = rb;
	}

	// Her silah bunu kendine göre dolduracak
	public abstract void Attack();

	protected IEnumerator CooldownRoutine()
	{
		canAttack = false;
		yield return new WaitForSeconds(1f / data.AttackRate);
		canAttack = true;
	}
}