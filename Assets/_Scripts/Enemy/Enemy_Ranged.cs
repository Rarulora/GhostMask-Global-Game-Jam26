using UnityEngine;

public class Enemy_Ranged : EnemyBase
{
	[Header("Ranged Settings")]
	[SerializeField] private GameObject projectilePrefab;
	[SerializeField] private Transform firePoint0, firePoint1;

	[Header("Animator Settings")]
	[SerializeField] private string attackAnimationStateName = "Attack_";
	private int currentFirePointIndex = 0;
	

	// --- 1. HAREKET VE DÖNME (AYNEN KORUNDU) ---
	protected override void Move()
	{
		if (playerTarget == null) return;

		// A. Hedef Yönü
		Vector2 targetDir = (playerTarget.position - transform.position).normalized;

		// B. Ayrýlma (Separation) ve Yanal Kayma (Avoidance)
		Vector2 separation = Vector2.zero;
		Vector2 avoidance = Vector2.zero; // Yana kaçma vektörü

		Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

		foreach (var neighbor in neighbors)
		{
			if (neighbor.gameObject != gameObject && neighbor.CompareTag("Enemy"))
			{
				Vector2 toNeighbor = neighbor.transform.position - transform.position;
				float dist = toNeighbor.magnitude;

				// 1. Ýtme (Push Back)
				Vector2 pushBack = -toNeighbor.normalized;
				separation += pushBack / (dist + 0.1f);

				// 2. Yanal Kayma (Sidestep)
				// Komþunun konumuna dik (90 derece) bir vektör alýyoruz.
				Vector2 sideStep = new Vector2(-toNeighbor.y, toNeighbor.x).normalized;

				// Rastgelelik (Noise) ekle ki hepsi ayný tarafa kaçmasýn
				float noise = Mathf.PerlinNoise(Time.time * 0.5f, GetInstanceID() * 0.1f) - 0.5f;
				avoidance += sideStep * Mathf.Sign(noise);
			}
		}

		// C. Vektörleri Birleþtir
		// Target + Separation + Avoidance
		Vector2 finalDir = (targetDir + (separation * separationForce) + (avoidance * separationForce * 0.8f)).normalized;

		// D. Doðal Salýným (Wave) - Opsiyonel
		// Menzilli düþmanlarýn da hafif kývrýlarak gelmesi hedef olmayý zorlaþtýrýr
		float wave = Mathf.Sin(Time.time * 2f + GetInstanceID()) * 0.2f;
		finalDir += new Vector2(finalDir.y, -finalDir.x) * wave;
		finalDir.Normalize();

		// E. Fiziksel Hareket
		rb.MovePosition(rb.position + finalDir * stats.MoveSpeed * Time.fixedDeltaTime);

		// F. Rotasyon (Oyuncuya Dönme)
		RotateTowardsTarget();
	}
	// --- 2. SALDIRI DÖNGÜSÜ ---
	protected override void AttackLogic()
	{
		RotateTowardsTarget(); // Ateþ ederken de dönmeye devam et
		base.AttackLogic();    // Base sýnýf süreyi kontrol eder ve PerformAttack çaðýrýr
	}

	private void RotateTowardsTarget()
	{
		if (playerTarget == null) return;

		// Düzeltme: Hedef - Kendim = Hedefe giden vektör
		Vector2 dir = transform.position - playerTarget.position;

		// Atan2 (Y, X) * Rad2Deg
		// Sprite'ýn YUKARI bakýyorsa -90, SAÐA bakýyorsa -90'ý sil.
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

		rb.rotation = angle;

		// Scale düzeltmesi (Negatif scale varsa düzelt)
		if (transform.localScale.x < 0)
		{
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
		}
	}

	// --- 3. SALDIRI KARAR MEKANÝZMASI ---
	// Base sýnýf süresi dolunca burayý çaðýrýr.
	protected override void PerformAttack()
	{
		if (anim != null)
		{
			// Eðer Animator varsa, sadece animasyonu baþlat.
			// Mermiyi oluþturma iþini Animasyon Event'i (ShootProjectile) yapacak.
			anim.SetTrigger(attackAnimationStateName + currentFirePointIndex);
		}
		else
		{
			// Animator yoksa eski usül direkt ateþ et.
			ShootProjectile();
		}
	}

	// --- 4. MERMÝ OLUÞTURMA (ANIMATION EVENT) ---
	// Animasyon klibinde tam atýþ anýna "ShootProjectile" event'i eklemelisin.
	public void ShootProjectile()
	{
		if (projectilePrefab == null || playerTarget == null) return;

		Transform firePoint = currentFirePointIndex == 0 ? firePoint0 : firePoint1;
		// Pool'dan mermi çek
		GameObject proj = ProjectilePoolManager.Instance.Get(
			projectilePrefab,
			firePoint != null ? firePoint.position : transform.position,
			firePoint != null ? firePoint.rotation : transform.rotation
		);

		Projectile pScript = proj.GetComponent<Projectile>();
		if (pScript != null)
		{
			Vector3 dir = (playerTarget.position - transform.position).normalized;
			pScript.Initialize(stats.Damage, dir, "Player", stats.ProjectileSpeed, stats.Pierce, false);
		}

		currentFirePointIndex = currentFirePointIndex == 0 ? 1 : 0;
	}

	// Diðer Animation Eventleri
	public void SetFirePointIndex_0() => currentFirePointIndex = 0;
	public void SetFirePointIndex_1() => currentFirePointIndex = 1;
}