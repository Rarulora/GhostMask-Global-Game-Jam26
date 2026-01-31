using UnityEngine;

public class Projectile : MonoBehaviour
{
	private float damage;
	private float speed;
	private Vector3 direction;
	private float lifetime = 5f;
	private float timer;
	private string targetTag;

	// YENİ: Delme Sayısı
	private int pierceCount = 0;
	private bool isCritical = false;

	// Initialize metodunu genişletiyoruz
	public void Initialize(float dmg, Vector3 dir, string tagToHit, float spd, int pierce, bool isCrit)
	{
		damage = dmg;
		direction = dir.normalized;
		targetTag = tagToHit;
		speed = spd;
		pierceCount = pierce;
		isCritical = isCrit;

		// Görsel rotasyon
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle);

		// Kritik vuruşsa mermiyi büyütebilir veya rengini değiştirebilirsin (Opsiyonel)
		if (isCrit) transform.localScale *= 1.5f;
		else transform.localScale = Vector3.one;
	}

	private void OnEnable() => timer = 0f;

	private void Update()
	{
		transform.position += direction * speed * Time.deltaTime;
		timer += Time.deltaTime;

		if (timer >= lifetime) ProjectilePoolManager.Instance.ReturnToPool(gameObject);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Wall"))
		{
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
			return;
		}

		if (other.CompareTag(targetTag))
		{
			IDamageable target = other.GetComponent<IDamageable>();
			if (target != null)
			{
				// Hasarı ve Kritik bilgisini iletiyoruz
				target.TakeDamage(damage, isCritical, direction, 1f); // Knockback eklenebilir
			}

			// --- PIERCE MANTIĞI ---
			if (pierceCount > 0)
			{
				pierceCount--; // Bir delme hakkını kullan
							   // Mermi yok olmaz, yoluna devam eder.
			}
			else
			{
				// Hakkı bitti, havuza dön
				ProjectilePoolManager.Instance.ReturnToPool(gameObject);
			}
		}
	}
}