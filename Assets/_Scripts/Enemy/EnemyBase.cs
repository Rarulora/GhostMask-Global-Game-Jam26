using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyBase : MonoBehaviour, IDamageable, IStatusEffectable
{
	[Header("Movement Settings")]
	[SerializeField] protected float separationRadius = 2.0f;
	[SerializeField] protected float separationForce = 1.5f;

	[Header("Visibility Settings")]
	[SerializeField] protected float shadowDistance = 5.0f; // Gölge olarak görünme mesafesi
	[SerializeField] protected Color shadowColor = new Color(0, 0, 0, 0.1f); // Siyah ve %40 opaklık

	[Header("Status Colors & Settings")]
	[Tooltip("Saniyede kaç kez renk değişsin? (Birden fazla efekt varsa)")]
	[SerializeField] private float colorSwitchSpeed = 4f;
	[SerializeField] private Color stunColor = Color.white;
	[SerializeField] private Color slowColor = new Color(0, 0.5f, 1f); // Mavi
	[SerializeField] private Color burnColor = new Color(1f, 0.4f, 0f); // Turuncu
	[SerializeField] private Color poisonColor = new Color(0.2f, 1f, 0.2f); // Yeşil

	// --- DURUM BAYRAKLARI (FLAGS) ---
	protected bool _isFrozen = false;
	protected bool _isSlowed = false;
	protected bool _isBurning = false;
	protected bool _isPoisoned = false;

	// --- BİLEŞENLER ---
	protected EnemyStats stats;
	protected Rigidbody2D rb;
	protected SpriteRenderer spriteRenderer;
	protected Collider2D col;
	protected Transform playerTarget;
	protected Animator anim;

	// --- DURUM DEĞİŞKENLERİ ---
	protected bool isDead = false;
	protected bool isStunned = false; // Fiziksel stun (Knockback vb.)
	protected bool isMaskActive = false;

	private float nextAttackTime = 0f;

	// Renk Yönetimi
	private Color originalColor = Color.white;
	private Color invisibleColor;

	// Coroutine Referansları
	private Coroutine flashRoutine;
	private Coroutine stunRoutine;  // Fiziksel Stun/Freeze
	private Coroutine slowRoutine;
	private Coroutine burnRoutine;
	private Coroutine poisonRoutine;

	private Vector3 originalScale;

	public EnemyStats Stats => stats;

	protected virtual void Awake()
	{
		stats = GetComponent<EnemyStats>();
		rb = GetComponent<Rigidbody2D>();

		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		col = GetComponent<Collider2D>();
		anim = GetComponentInChildren<Animator>();

		originalColor = spriteRenderer.color;
		invisibleColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

		originalScale = transform.localScale;
	}

	protected virtual void OnEnable()
	{
		// Durumları Sıfırla
		isDead = false;
		isStunned = false;
		_isFrozen = _isSlowed = _isBurning = _isPoisoned = false;

		col.enabled = true;
		rb.simulated = true;
		spriteRenderer.color = originalColor;

		// Scale düzelt
		transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

		StopAllCoroutines();

		if (playerTarget == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) playerTarget = p.transform;
		}

		// Event Aboneliği
		EventManager.OnMaskChanged += UpdateMaskStatus;
		if (MaskController.I != null) UpdateMaskStatus(MaskController.I.IsMaskActive);
	}

	protected virtual void OnDisable()
	{
		EventManager.OnMaskChanged -= UpdateMaskStatus;
	}

	protected virtual void FixedUpdate()
	{
		if (isDead || isStunned || playerTarget == null) return;

		// Mesafeyi ölç
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

		// Görünürlük Yönetimi
		HandleVisibility(distanceToPlayer);

		// Saldırı Menzili Kontrolü
		if (distanceToPlayer <= stats.AttackRange)
		{
			StopMovement();
			AttackLogic();
			RotateTowardsTarget();
		}
		else
		{
			Move();
		}
	}

	// ========================================================================
	// GÖRÜNÜRLÜK VE RENK SİSTEMİ
	// ========================================================================

	private Color GetCyclicStatusColor()
	{
		// 1. Aktif olan efekt renklerini topla
		List<Color> activeColors = new List<Color>();

		if (_isFrozen) activeColors.Add(stunColor);
		if (_isSlowed) activeColors.Add(slowColor);
		if (_isBurning) activeColors.Add(burnColor);
		if (_isPoisoned) activeColors.Add(poisonColor);

		// Hiç efekt yoksa orijinal rengi dön
		if (activeColors.Count == 0) return originalColor;

		// Tek efekt varsa direkt onu dön
		if (activeColors.Count == 1) return activeColors[0];

		// 2. Döngüsel Seçim (Cycle/Strobe Effect)
		// Zamanı hıza bölüp mod alarak sıradaki rengin indexini buluyoruz.
		int index = (int)(Time.time * colorSwitchSpeed) % activeColors.Count;

		return activeColors[index];
	}

	protected void HandleVisibility(float distance)
	{
		// Hasar yeme flash efekti en üst önceliktir
		if (flashRoutine != null) return;

		// O anki (sıradaki) durum rengini hesapla
		Color cycleColor = GetCyclicStatusColor();

		if (isMaskActive)
		{
			// Maske takılıyken: Direkt hesaplanan rengi uygula
			spriteRenderer.color = cycleColor;
		}
		else
		{
			// Maske YOKSA: Gölge veya Görünmezlik
			if (distance <= shadowDistance)
			{
				// Gölge Rengi: Sıradaki rengin RGB'si + Gölge Alpha'sı
				spriteRenderer.color = new Color(cycleColor.r, cycleColor.g, cycleColor.b, shadowColor.a);
			}
			else
			{
				// Çok uzakta tamamen görünmez
				spriteRenderer.color = invisibleColor;
			}
		}
	}

	// ========================================================================
	// STATUS EFFECT SİSTEMİ (IStatusEffectable Implementation)
	// ========================================================================

	public void ApplyStatus(EffectType type, float duration, float potency)
	{
		if (isDead) return;

		switch (type)
		{
			case EffectType.Freeze:
				if (stunRoutine != null) StopCoroutine(stunRoutine);
				stunRoutine = StartCoroutine(FreezeRoutine(duration));
				break;

			case EffectType.Slow:
				if (slowRoutine != null) StopCoroutine(slowRoutine);
				slowRoutine = StartCoroutine(SlowRoutine(duration, potency));
				break;

			case EffectType.Burn:
				if (burnRoutine != null) StopCoroutine(burnRoutine);
				burnRoutine = StartCoroutine(BurnRoutine(duration, potency));
				break;

			case EffectType.Poison:
				if (poisonRoutine != null) StopCoroutine(poisonRoutine);
				poisonRoutine = StartCoroutine(PoisonRoutine(duration, potency));
				break;
		}
	}

	private IEnumerator FreezeRoutine(float duration)
	{
		isStunned = true;
		_isFrozen = true; // Flag Aç
		rb.linearVelocity = Vector2.zero;

		yield return new WaitForSeconds(duration);

		_isFrozen = false; // Flag Kapat
		isStunned = false;
	}

	private IEnumerator SlowRoutine(float duration, float slowPercent)
	{
		float originalSpeed = stats.MoveSpeed;
		// Yavaşlatma formülü: %30 slow -> Hız * 0.7
		stats.MoveSpeed = originalSpeed * (1f - (slowPercent / 100f));

		_isSlowed = true; // Flag Aç

		yield return new WaitForSeconds(duration);

		_isSlowed = false; // Flag Kapat
		stats.MoveSpeed = originalSpeed;
	}

	private IEnumerator BurnRoutine(float duration, float damagePerSec)
	{
		_isBurning = true; // Flag Aç

		float timer = 0f;
		while (timer < duration)
		{
			yield return new WaitForSeconds(1f);
			// Knockback olmadan hasar ver
			TakeDamage(damagePerSec, false, Vector2.zero, 0);
			timer += 1f;
		}

		_isBurning = false; // Flag Kapat
	}

	private IEnumerator PoisonRoutine(float duration, float damagePerSec)
	{
		_isPoisoned = true; // Flag Aç

		float timer = 0f;
		while (timer < duration)
		{
			yield return new WaitForSeconds(1f);
			TakeDamage(damagePerSec, false, Vector2.zero, 0);
			timer += 1f;
		}

		_isPoisoned = false; // Flag Kapat
	}

	// ========================================================================
	// HAREKET VE SALDIRI MANTIĞI
	// ========================================================================

	protected virtual void Move()
	{
		if (playerTarget == null)
		{
			if (anim != null) anim.SetBool("isMoving", true);
			return;
		}
		if (anim != null) anim.SetBool("isMoving", true);

		// 1. Hedef Yönü
		Vector2 targetDir = (playerTarget.position - transform.position).normalized;
		Vector2 separation = Vector2.zero;
		Vector2 avoidance = Vector2.zero;

		// 2. Çarpışma Önleme (Separation)
		Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);
		foreach (var neighbor in neighbors)
		{
			if (neighbor.gameObject != gameObject && neighbor.CompareTag("Enemy"))
			{
				Vector2 toNeighbor = neighbor.transform.position - transform.position;
				float dist = toNeighbor.magnitude;

				Vector2 pushBack = -toNeighbor.normalized;
				separation += pushBack / (dist + 0.1f);

				// Yanal Kayma
				Vector2 sideStep = new Vector2(-toNeighbor.y, toNeighbor.x).normalized;
				float noise = Mathf.PerlinNoise(Time.time * 0.5f, GetInstanceID() * 0.1f) - 0.5f;
				avoidance += sideStep * (Mathf.Sign(noise));
			}
		}

		// 3. Vektörleri Birleştir
		Vector2 finalDir = (targetDir + (separation * separationForce) + (avoidance * separationForce * 0.8f)).normalized;

		// 4. Doğal Salınım
		float wave = Mathf.Sin(Time.time * 2f + GetInstanceID()) * 0.2f;
		finalDir += new Vector2(finalDir.y, -finalDir.x) * wave;
		finalDir.Normalize();
		Debug.Log(stats.MoveSpeed);
		rb.MovePosition(rb.position + finalDir * stats.MoveSpeed * Time.fixedDeltaTime);
		RotateTowardsTarget();
	}

	protected virtual void RotateTowardsTarget()
	{
		if (playerTarget == null) return;
		Vector2 dir = playerTarget.position - transform.position;
		if (dir.x > 0)
			transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
		else if (dir.x < 0)
			transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
	}

	protected virtual void StopMovement()
	{
		if (anim != null) anim.SetBool("isMoving", false);
		rb.linearVelocity = Vector2.zero;
	}

	protected virtual void AttackLogic()
	{
		if (Time.time >= nextAttackTime)
		{
			float cooldown = 1f / stats.AttackRate;
			nextAttackTime = Time.time + cooldown;
			PerformAttack();
		}
	}

	protected virtual void PerformAttack()
	{
		IDamageable player = playerTarget.GetComponent<IDamageable>();
		if (player != null)
		{
			player.TakeDamage(stats.Damage, false, Vector2.zero, 0);
		}
	}

	// ========================================================================
	// HASAR VE ÖLÜM (IDamageable Implementation)
	// ========================================================================

	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir, float knockbackForce)
	{
		if (isDead) return;


		stats.TakeDamage(amount);

		DamagePopup.Create(transform.position, amount, isCritical);

		// Flash Efekti
		if (flashRoutine != null) StopCoroutine(flashRoutine);
		flashRoutine = StartCoroutine(HitFlashRoutine());

		// Knockback
		ApplyKnockback(knockbackDir, knockbackForce);

		// Damage Popup (Eğer sistemin varsa aç)
		// DamagePopup.Create(transform.position, amount, isCritical); 

		if (stats.IsDead)
		{
			Die();
		}
	}

	public bool CanTakeDamage() => true;

	private void ApplyKnockback(Vector2 dir, float force)
	{
		// Donmuşsa (Freeze) fiziksel olarak itilmesin, glitch olmasın
		if (_isFrozen) return;

		// Normal Stun (Kısa süreli hareket kitleme)
		if (stunRoutine != null) StopCoroutine(stunRoutine);
		stunRoutine = StartCoroutine(StunRoutine(0.2f));

		rb.AddForce(dir * force, ForceMode2D.Impulse);
	}

	private IEnumerator StunRoutine(float duration)
	{
		isStunned = true;
		yield return new WaitForSeconds(duration);

		// Eğer donma efekti yoksa stunu kaldır (Donma varsa o kendi kaldıracak)
		if (!_isFrozen) isStunned = false;

		rb.linearVelocity = Vector2.zero;
	}

	private IEnumerator HitFlashRoutine()
	{
		// Vurulduğunda anlık Beyaz olsun
		spriteRenderer.color = Color.white;
		yield return new WaitForSeconds(0.1f);

		flashRoutine = null;
		// Rengi geri döndürmeye gerek yok, HandleVisibility bir sonraki karede düzeltecek
	}

	public void UpdateMaskStatus(bool isMaskOn)
	{
		isMaskActive = isMaskOn;
	}

	public virtual void Die()
	{
		if (isDead) return;
		isDead = true;

		col.enabled = false;
		rb.linearVelocity = Vector2.zero;
		rb.simulated = false;
		EventManager.RaiseEnemyKilled(this);
		StartCoroutine(DisableRoutine());
	}

	private IEnumerator DisableRoutine()
	{
		if (anim != null) anim.SetTrigger("Death");
		yield return new WaitForSeconds(0.2f); // Animasyon süresi kadar bekle
		gameObject.SetActive(false);
	}
}