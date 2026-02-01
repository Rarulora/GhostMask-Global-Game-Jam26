using System.Collections;
using UnityEngine;

public class Enemy_Bomber : EnemyBase
{
	[Header("Bomber Settings")]
	[SerializeField] private float explosionRadius = 2.5f;
	[SerializeField] private float fuseTime = 1.0f; // Patlamadan önceki bekleme süresi
	[SerializeField] private GameObject explosionVFX; // Patlama efekti

	private bool isExploding = false; // Patlama süreci başladı mı?

	protected override void Move()
	{
		// Eğer patlama sürecine girdiyse kımıldamasın
		if (isExploding) return;
		base.Move();
	}

	protected override void PerformAttack()
	{
		if (isExploding) return;

		// Patlama sürecini başlat
		StartCoroutine(ExplodeRoutine());
	}

	private IEnumerator ExplodeRoutine()
	{
		isExploding = true;

		// 1. Uyarı Aşaması (Şişme veya Renk Değişimi)
		float timer = 0f;
		while (timer < fuseTime)
		{
			timer += Time.deltaTime;
			// Basit bir titreme veya kırmızılaşma efekti (Feedback)
			float flashSpeed = Mathf.PingPong(Time.time * 10, 1f);
			spriteRenderer.color = Color.Lerp(Color.white, Color.red, flashSpeed);
			yield return null;
		}

		// 2. Patlama Aşaması
		Explode();
	}

	private void Explode()
	{
		// Alan hasarı hesapla (Physics2D.OverlapCircle)
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

		foreach (var hit in hits)
		{
			if (hit.CompareTag("Player"))
			{
				IDamageable player = hit.GetComponent<IDamageable>();
				// Patlama merkezinden dışarı doğru itme kuvveti
				Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

				// Patlama hasarı normal hasarın 2-3 katı olabilir
				player?.TakeDamage(stats.Damage * 3f, false, knockbackDir, 10f);
			}
		}

		// Efekt oluştur
		if (explosionVFX) explosionVFX.GetComponent<ParticleSystem>().Play();

		TakeDamage(99999f, true, Vector2.zero, 0);
	}

	// Editor'de patlama alanını görmek için
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}

	// Obje havuza geri dönerken değişkeni sıfırla
	protected override void OnEnable()
	{
		base.OnEnable();
		isExploding = false;
	}
}