using UnityEngine;

public class MeleeWeapon : WeaponBase
{
	public override void Attack()
	{
		if (!canAttack) return;

		StartCoroutine(CooldownRoutine());

		// 1. Alan Hasarı (Physics Overlap)
		// Oyuncunun baktığı yöne göre bir daire çizer
		Vector2 attackPos = (Vector2)playerTransform.position + (Vector2)playerTransform.right * (data.Range * 0.5f);

		Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, data.Range);

		foreach (var hit in hits)
		{
			if (hit.CompareTag("Enemy"))
			{
				IDamageable enemy = hit.GetComponent<IDamageable>();
				if (enemy != null)
				{
					// Knockback yönü: Oyuncudan düşmana doğru
					Vector2 knockbackDir = (hit.transform.position - playerTransform.position).normalized;
					enemy.TakeDamage(data.Damage, false, knockbackDir, data.Force);
				}
			}
		}

		// TODO: Kılıç sallama animasyonu/efekti oynat
		Debug.Log($"{data.WeaponName} ile vuruldu!");
	}

	// Gizmos ile editörde menzili görelim
	private void OnDrawGizmosSelected()
	{
		if (playerTransform != null && data != null)
		{
			Gizmos.color = Color.red;
			Vector2 attackPos = (Vector2)playerTransform.position + (Vector2)playerTransform.right * (data.Range * 0.5f);
			Gizmos.DrawWireSphere(attackPos, data.Range);
		}
	}
}