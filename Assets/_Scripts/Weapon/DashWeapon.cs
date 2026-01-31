using UnityEngine;
using System.Collections;

public class DashWeapon : WeaponBase
{
	private bool isDashing = false;

	// Dash yolunun genişliği (Karakterin genişliği kadar veya biraz daha fazla)
	[SerializeField] private float dashPathWidth = 1.5f;

	Collider2D coll;
	private void OnDisable()
	{
		EventManager.RaiseDashStatusChanged(false);
	}

	public override void Initialize(WeaponData weaponData, Transform owner, Transform firePoint, Rigidbody2D rb)
	{
		base.Initialize(weaponData, owner, firePoint, rb);
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		coll = owner.GetComponentInParent<Collider2D>();
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
		coll.isTrigger = true;
		EventManager.RaiseDashStatusChanged(true);

		// 1. BAŞLANGIÇ POZİSYONUNU KAYDET
		Vector2 startPos = playerTransform.position;

		// --- Mouse Yön Hesabı ---
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseWorldPos.z = 0f;
		Vector2 dashDir = (mouseWorldPos - playerTransform.position).normalized;
		if (dashDir == Vector2.zero) dashDir = playerTransform.right;
		// ------------------------

		// Fiziği Hazırla
		float originalDrag = playerRb.linearDamping;
		playerRb.linearDamping = 0;
		playerRb.linearVelocity = dashDir * data.DashSpeed;

		// 2. DASH HAREKETİ (Burada hasar yok, sadece hareket)
		float timer = 0f;
		while (timer < data.DashDuration)
		{
			timer += Time.deltaTime;
			// CheckCollisions() BURADAN KALDIRILDI
			yield return null;
		}

		// 3. DASH BİTTİ, FİZİĞİ DURDUR
		playerRb.linearVelocity = Vector2.zero;
		playerRb.linearDamping = originalDrag;

		// 4. BİTİŞ POZİSYONUNU KAYDET VE HASARI HESAPLA
		Vector2 endPos = playerTransform.position;
		PerformPathDamage(startPos, endPos);

		coll.isTrigger = false;
		isDashing = false;
		EventManager.RaiseDashStatusChanged(false);

		// Cooldown
		yield return new WaitForSeconds(1f / GetStat(Enums.StatType.attackRate));
		canAttack = true;
	}

	// --- YENİ HASAR SİSTEMİ (BOXCAST) ---
	private void PerformPathDamage(Vector2 start, Vector2 end)
	{
		// Başlangıç ve Bitiş arasındaki mesafe ve yön
		Vector2 direction = (end - start).normalized;
		float distance = Vector2.Distance(start, end);

		// Physics2D.BoxCastAll: Bir kutuyu uzayda süpürür (Sweep).
		// Origin: Başlangıç noktası
		// Size: Kutunun boyutu (Genişlik, Kalınlık)
		// Angle: Kutunun açısı (Dönme)
		// Direction: Süpürme yönü
		// Distance: Süpürme mesafesi
		// LayerMask: Sadece düşmanları vurmak için (Opsiyonel ama performanslı olur)

		RaycastHit2D[] hits = Physics2D.BoxCastAll(start, new Vector2(dashPathWidth, 0.1f), 0f, direction, distance);

		// Debug Çizgisi (Sahne ekranında yolu görmek için - Oyunda görünmez)
		Debug.DrawLine(start, end, Color.cyan, 1f);

		foreach (var hit in hits)
		{
			if (hit.collider.CompareTag("Enemy"))
			{
				IDamageable enemy = hit.collider.GetComponent<IDamageable>();
				if (enemy != null)
				{
					// Tek seferlik, sert hasar.
					// Knockback yok (0), çünkü içinden geçip kestik.
					enemy.TakeDamage(GetStat(Enums.StatType.damage), false, Vector2.zero, 0);

					// VFX İPUCU: Burada düşman üzerinde "Slash" efekti oluşturabilirsin.
					// Instantiate(SlashVFX, hit.transform.position, ...);
				}
			}
		}
	}
}