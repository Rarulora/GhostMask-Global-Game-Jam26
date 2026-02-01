using UnityEngine;
using Enums; // StatType için

public class RangedWeapon : WeaponBase
{
	[Header("Spread Settings")]
	[SerializeField] private float spreadAngle = 15f; // Çoklu mermiler arası açı farkı

	public override void Attack()
	{
		if (!canAttack) return;
		PlayAnimation();
		// 1. STATLARI ÇEK
		// StatsController.I.GetValue helper fonksiyonunu yazdığını varsayıyorum, yoksa .GetStat().Value kullan.
		float currentDamage = StatsController.I.GetValue(StatType.damage);
		float currentSpeed = StatsController.I.GetValue(StatType.projectileSpeed);
		int projCount = Mathf.RoundToInt(StatsController.I.GetValue(StatType.projectileCount));
		int pierceCount = Mathf.RoundToInt(StatsController.I.GetValue(StatType.pierce));
		float critMult = StatsController.I.GetValue(StatType.critMultiplier);


		// Güvenlik: En az 1 mermi atılmalı
		if (projCount < 1) projCount = 1;

		StartCoroutine(CooldownRoutine());

		// 2. HEDEF VE AÇI HESABI
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 baseDirection = (mousePos - playerTransform.position).normalized;

		// Mouse'a bakan ana açı (Derece cinsinden)
		float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

		// Çoklu mermi yayılımı için başlangıç açısı
		// Eğer 3 mermi atacaksak: -15, 0, +15 gibi dağılmalı.
		// Formül: (Toplam Mermi - 1) * (Aralık / 2) kadar geriden başla.
		float startAngle = baseAngle;
		if (projCount > 1)
		{
			startAngle = baseAngle - (spreadAngle * (projCount - 1) / 2f);
		}

		// 3. MERMİ FIRLATMA DÖNGÜSÜ
		for (int i = 0; i < projCount; i++)
		{
			// A. Mevcut merminin açısını hesapla
			float currentAngle = startAngle + (spreadAngle * i);

			// Açıyı Vektöre çevir (Quaternion * Vector3.right)
			Vector2 finalDir = Quaternion.Euler(0, 0, currentAngle) * Vector3.right;

			// B. Kritik Vuruş Hesabı (Her mermi için ayrı şans)
			// critChance genellikle 0-100 arası tutulur, kontrol et. Eğer 0-1 ise ona göre düzenle.
			// Örn: critChance = 10 ise (%10), Random.value (0.0-1.0) * 100 <= 10
			bool isCrit = IsCrit();
			float finalDamage = isCrit ? currentDamage * critMult : currentDamage;

			// C. Mermiyi Oluştur
			if (data.ProjectilePrefab != null)
			{
				// Merminin görsel rotasyonu (Gittiği yöne baksın)
				Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

				GameObject proj = ProjectilePoolManager.Instance.Get(
					data.ProjectilePrefab,
					firePoint != null ? firePoint.position : transform.position,
					rotation
				);

				Projectile pScript = proj.GetComponent<Projectile>();
				if (pScript != null)
				{
					// GÜNCELLENMİŞ INITIALIZE ÇAĞRISI
					pScript.Initialize(
						finalDamage,
						finalDir,
						"Enemy",
						currentSpeed,
						pierceCount,
						isCrit
					);
				}
			}

			if (isCrit)
			{
				CameraController.Instance.ShakeCamera(0.1f, 0.2f); // Hafif titreme
			}
		}
	}
}