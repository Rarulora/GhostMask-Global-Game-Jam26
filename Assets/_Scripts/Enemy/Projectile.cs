using UnityEngine;

public class Projectile : MonoBehaviour
{
	private float damage;
	private float speed = 10f;
	private Vector3 direction;
	private float lifetime = 5f;
	private float timer;

	// YENİ: Bu mermi kime çarparsa patlayacak?
	private string targetTag;

	// Initialize artık hedef etiketini de alıyor
	// opsiyonel: speedOverride ile her silahın mermi hızı farklı olabilir
	public void Initialize(float dmg, Vector3 dir, string tagToHit, float speedOverride = -1f)
	{
		damage = dmg;
		direction = dir.normalized;
		targetTag = tagToHit;
		// Eğer özel bir hız verilmişse onu kullan, yoksa varsayılanı (10) kullan
		if (speedOverride > 0) speed = speedOverride;
		else speed = 10f;

		// Görsel rotasyon (Ok gibi cisimler için)
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	private void OnEnable()
	{
		timer = 0f;
	}

	private void Update()
	{
		transform.position += direction * speed * Time.deltaTime;

		timer += Time.deltaTime;
		if (timer >= lifetime)
		{
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// 1. DUVAR KONTROLÜ (Herkes için ortak)
		if (other.CompareTag("Wall"))
		{
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
			return;
		}

		// 2. HEDEF KONTROLÜ (Dinamik)
		// Eğer çarptığımız şey, bizim hedeflediğimiz Tag'e sahipse (Örn: "Enemy" veya "Player")
		if (other.CompareTag(targetTag))
		{
			IDamageable target = other.GetComponent<IDamageable>();
			if (target != null)
			{
				// Knockback şu an 0, silah verisinden eklenebilir
				target.TakeDamage(damage, false, direction, 1f);
			}

			// Hedefi vurduk, mermiyi yok et
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
		}

		// NOT: Kendi sahibimize veya başka mermilere çarparsa hiçbir şey yapmaz (Ignore)
	}
}