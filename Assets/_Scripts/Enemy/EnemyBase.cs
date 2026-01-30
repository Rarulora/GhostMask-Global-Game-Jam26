using Enums;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(StatsController))] // Yazdığımız stat sistemi
public class EnemyBase : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] protected string enemyName = "Mob";
	[SerializeField] protected float attackCooldown = 1f;

	// --- Components ---
	protected Rigidbody2D rb;
	protected SpriteRenderer spriteRenderer;
	protected StatsController stats;
	protected Transform playerTarget;

	// --- State ---
	protected bool isDead = false;
	protected bool canAttack = true;
	protected bool isStunned = false; // Perkler için (Stun Grenade)
	protected float currentHealth;

	// --- Mask Logic ---
	private Color originalColor;
	private Color invisibleColor;

	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		stats = GetComponent<StatsController>();

		originalColor = spriteRenderer.color;
		invisibleColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Tam şeffaf
	}

	protected virtual void Start()
	{
		// Oyuncuyu bul (Singleton veya Tag ile - Jam için Tag uygundur)
		GameObject p = GameObject.FindGameObjectWithTag("Player");
		if (p != null) playerTarget = p.transform;

		// Statları başlat
		currentHealth = stats.GetValue(StatType.health);

		// Maske olayına abone ol (MaskManager Eventi)
		// MaskManager.OnMaskChanged += HandleMaskVisibility;

		// Başlangıçta maske kapalımı kontrol et
		// HandleMaskVisibility(MaskManager.IsMaskActive);
	}

	protected virtual void OnDestroy()
	{
		// Eventten çıkmayı unutma!
		// MaskManager.OnMaskChanged -= HandleMaskVisibility;
	}

	protected virtual void FixedUpdate()
	{
		if (isDead || isStunned || playerTarget == null) return;

		Move();
	}

	// --- 1. HAREKET MANTIĞI (Override Edilebilir) ---
	protected virtual void Move()
	{
		// Varsayılan davranış: Oyuncuya dümdüz yürü
		float speed = stats.GetValue(StatType.moveSpeed);
		Vector2 direction = (playerTarget.position - transform.position).normalized;

		// Rigidbody ile hareket (Fizik tabanlı, itme/kakma için en iyisi)
		rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

		// Yüzünü dön (Sprite Flip)
		if (direction.x > 0) spriteRenderer.flipX = false;
		else if (direction.x < 0) spriteRenderer.flipX = true;
	}

	// --- 2. HASAR ALMA SİSTEMİ ---
	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir, float knockbackForce)
	{
		if (isDead) return;

		// Canı azalt
		currentHealth -= amount;

		// Vuruş Efekti (Flash)
		StartCoroutine(HitFlashRoutine());

		// Knockback (Geri Tepme)
		if (knockbackForce > 0)
		{
			ApplyKnockback(knockbackDir, knockbackForce);
		}

		// TODO: Damage Popup

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	// --- 3. STATUS ETKİLERİ (Perkler İçin) ---
	public void ApplyKnockback(Vector2 dir, float force)
	{
		// Anlık kuvvet uygula (ForceMode2D.Impulse önemli)
		rb.AddForce(dir * force, ForceMode2D.Impulse);

		// Kısa bir süre kontrolü kaybet (Stun)
		StartCoroutine(StunRoutine(0.2f));
	}

	public void Freeze(float duration)
	{
		StartCoroutine(StunRoutine(duration));
		// Opsiyonel: Rengi mavi yap
	}

	private IEnumerator StunRoutine(float duration)
	{
		isStunned = true;
		yield return new WaitForSeconds(duration);
		isStunned = false;
		// Hızı sıfırla ki kaymaya devam etmesin
		rb.linearVelocity = Vector2.zero;
	}

	private IEnumerator HitFlashRoutine()
	{
		// Basit beyaz yanıp sönme
		spriteRenderer.color = Color.white;
		yield return new WaitForSeconds(0.1f);
		// Maske durumuna göre eski rengine dön
		// spriteRenderer.color = MaskManager.IsMaskActive ? originalColor : invisibleColor;
		spriteRenderer.color = originalColor; // Geçici
	}

	// --- 4. MASKE MANTIĞI ---
	// MaskManager eventinden çağırılacak
	public void HandleMaskVisibility(bool isMaskOn)
	{
		if (isMaskOn)
		{
			spriteRenderer.color = originalColor; // Görünür
		}
		else
		{
			spriteRenderer.color = invisibleColor; // Görünmez
		}
	}

	// --- 5. ÖLÜM VE LOOT ---
	public virtual void Die()
	{
		isDead = true;
		rb.linearVelocity = Vector2.zero;
		GetComponent<Collider2D>().enabled = false;

		// Altın ve XP düşür (Refik'in sistemi)
		// LootManager.SpawnLoot(transform.position, EnemyType);

		// Perk Tetiklemesi (Emre'nin eventi)
		// EventManager.OnEnemyKilled?.Invoke(this);

		// Efekt ve yok olma
		Destroy(gameObject, 0.5f); // Animasyon varsa bekle
	}

	// --- 6. SALDIRI (Temas Hasarı) ---
	protected virtual void OnCollisionStay2D(Collision2D collision)
	{
		if (isDead) return;

		if (collision.gameObject.CompareTag("Player"))
		{
			// Oyuncuya hasar ver (Zamanlayıcı ile)
			// PlayerHealth.TakeDamage(stats.GetValue(StatType.Damage));
		}
	}
}