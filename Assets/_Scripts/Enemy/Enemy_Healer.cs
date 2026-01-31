using UnityEngine;

public class Enemy_Healer : EnemyBase
{
	[Header("Healer Settings")]
	[SerializeField] private float healRadius = 4f;
	[SerializeField] private GameObject healVFX;

	protected override void FixedUpdate()
	{
		if (isDead || isStunned || playerTarget == null) return;

		// 1. MESAFE HESABI
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

		// --- DÜZELTME BURADA ---
		// FixedUpdate'i override ettiğimiz için base'deki görünürlük kodunu
		// elle çağırmamız gerekiyor.
		HandleVisibility(distanceToPlayer);
		// -----------------------

		// 2. SALDIRI (İYİLEŞTİRME) MANTIĞI
		// Healer hareket ederken de iyileştirme yapabilsin diye buraya koyduk
		AttackLogic();

		// 3. HAREKET KARARI
		if (distanceToPlayer <= stats.AttackRange)
		{
			StopMovement();
			RotateTowardsTarget(); // Dururken de oyuncuya baksın (Görsel bütünlük için)
		}
		else
		{
			Move();
		}
	}

	protected override void PerformAttack()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, healRadius);
		bool didHeal = false;

		foreach (var hit in hits)
		{
			if (hit.CompareTag("Enemy"))
			{
				// Kendini de iyileştirebilir, eğer istemiyorsan:
				// if (hit.gameObject == gameObject) continue; 

				EnemyStats allyStats = hit.GetComponent<EnemyStats>();

				if (allyStats != null && allyStats.CurrentHealth < allyStats.MaxHealth)
				{
					allyStats.Heal(stats.Damage); // Damage değişkenini Heal miktarı olarak kullanıyoruz
					didHeal = true;
				}
			}
		}

		if (didHeal && healVFX)
		{
			Instantiate(healVFX, transform.position, Quaternion.identity);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, healRadius);
	}
}