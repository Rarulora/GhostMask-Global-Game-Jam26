using UnityEngine;
using System.Collections;

public class DashWeapon : WeaponBase
{
	private bool isDashing = false;
	private Collider2D dashHitbox; // Oyuncunun üzerindeki Trigger Collider

	public override void Initialize(WeaponData weaponData, Transform owner, Rigidbody2D rb)
	{
		base.Initialize(weaponData, owner, rb);
		// Dash hitbox'ını bul veya ekle (Child object'te olması iyidir)
		// Basitlik adına oyuncunun collider'ını kullanabiliriz ama ayrı trigger daha iyidir.
	}

	public override void Attack()
	{
		if (!canAttack || isDashing) return;
		StartCoroutine(DashRoutine());
	}

	private IEnumerator DashRoutine()
	{
		canAttack = false;
		isDashing = true;

		// 1. Dash Başlat (Hareketi kilitlemek gerekebilir, PlayerController ile konuşmalı)
		// Şimdilik sadece fiziksel itme yapıyoruz.
		Vector2 dashDir = playerRb.linearVelocity.normalized;
		if (dashDir == Vector2.zero) dashDir = playerTransform.right; // Duruyorsa sağa

		// Yerçekimi ve sürtünmeyi kapat (Kusursuz dash için)
		float originalDrag = playerRb.linearDamping;
		playerRb.linearDamping = 0;
		playerRb.linearVelocity = dashDir * data.DashSpeed;

		// 2. Dash Sırasında Hasar Verme Döngüsü
		float timer = 0f;
		while (timer < data.DashDuration)
		{
			timer += Time.deltaTime;
			CheckCollisions(); // Elle collision kontrolü (Raycast veya Overlap)
			yield return null;
		}

		// 3. Dash Bitir
		playerRb.linearVelocity = Vector2.zero; // Durdur veya yavaşlat
		playerRb.linearDamping = originalDrag;
		isDashing = false;

		// Cooldown
		yield return new WaitForSeconds(1f / data.AttackRate);
		canAttack = true;
	}

	private void CheckCollisions()
	{
		// Dash atarken vücudumuzun etrafındaki düşmanları bul
		Collider2D[] hits = Physics2D.OverlapCircleAll(playerTransform.position, 1f); // 1f karakter boyutu
		foreach (var hit in hits)
		{
			if (hit.CompareTag("Enemy"))
			{
				IDamageable enemy = hit.GetComponent<IDamageable>();
				if (enemy != null)
				{
					// Dash silahı geri tepme uygulamaz, içinden geçer
					enemy.TakeDamage(data.Damage, false, Vector2.zero, 0);
				}
			}
		}
	}
}