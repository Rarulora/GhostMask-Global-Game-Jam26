using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
	private float damage;
	private float speed = 10f;
	private Vector3 direction;
	private float lifetime = 5f; // Merminin ömrü
	private float timer;

	public void Initialize(float dmg, Vector3 dir)
	{
		damage = dmg;
		direction = dir.normalized;
		// Açıyı yöne göre ayarla (Opsiyonel görsel detay)
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	private void OnEnable()
	{
		// Havuzdan çıkınca zamanlayıcıyı sıfırla
		timer = 0f;
	}

	private void Update()
	{
		// Hareket
		transform.position += direction * speed * Time.deltaTime;

		// Zaman aşımı kontrolü (Destroy yerine ReturnToPool)
		timer += Time.deltaTime;
		if (timer >= lifetime)
		{
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Kendisine veya başka düşmanlara çarpmasın
		if (other.CompareTag("Enemy") || other.CompareTag("Projectile")) return;

		if (other.CompareTag("Player"))
		{
			IDamageable player = other.GetComponent<IDamageable>();
			if (player != null)
			{
				player.TakeDamage(damage, false, Vector2.zero, 0);
			}
			// Çarptı -> Havuza dön
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
		}
		else if (other.CompareTag("Wall")) // Duvar
		{
			// Çarptı -> Havuza dön
			ProjectilePoolManager.Instance.ReturnToPool(gameObject);
		}
	}
}