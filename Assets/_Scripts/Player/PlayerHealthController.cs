using System.Collections;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour, IDamageable
{
	[SerializeField] private Animator anim;
	[Header("Health Settings")]
	[SerializeField] private float iFrameDuration = 0.2f; // Hasar aldıktan sonraki ölümsüzlük süresi

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Color damageColor = Color.red;
	private Color originalColor;

	// Durum Değişkenleri
	private float _currentHealth;
	private bool _isDead = false;
	private bool _isInvincible = false; // Şu an ölümsüz mü?
	private float _maxHealth;

	// Componentler
	private Rigidbody2D rb;

	public IDamageable damageable => this;
	public float CurrentHealth => _currentHealth;
	public float MaxHealth => _maxHealth;

	bool isInvinsible = false;
	Coroutine invinsibleRoutine;

	StatsController _stats;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		_stats = GetComponent<StatsController>();
		if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		originalColor = spriteRenderer.color;
	}

	private void Start()
	{
        _maxHealth = _stats.GetStat(Enums.StatType.health).Value;
        _currentHealth = _maxHealth;
        EventManager.RaiseHealthChanged(_currentHealth, _maxHealth);
	}

	// --- IDAMAGEABLE ARAYÜZÜ ---
	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir, float knockbackForce)
	{
		if (_isDead || _isInvincible) return;

		// 1. Hasarı Uygula
		_currentHealth -= amount;

		// UI Güncelle
		EventManager.RaiseHealthChanged(_currentHealth, _maxHealth);
		CameraController.Instance.ShakeCamera(0.15f, 0.3f);
		PerkManager.I.TriggerOnTakeDamage(amount);
		// 2. Ölüm Kontrolü
		if (_currentHealth <= 0)
		{
			Die();
			return;
		}

		AudioManager.Instance.PlaySFX(AudioManager.Instance.takeDamage);

		// 3. Efektler (Knockback & Flash & I-Frame)
		ApplyKnockback(knockbackDir, knockbackForce);
		StartCoroutine(InvincibilityRoutine());
	}
	public void TakeDamage(float amount) => TakeDamage(amount, false, Vector2.zero, 0);
	public void TakeDamage(float amount, bool isCritical) => TakeDamage(amount, isCritical, Vector2.zero, 0);
	// --- CAN YENİLEME (Potion vb. için) ---
	public void Heal(float amount)
	{
		if (_isDead) return;

		_currentHealth += amount;
		if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;

		EventManager.RaiseHealthChanged(_currentHealth, _maxHealth);
	}

	// --- ÖLÜM ---
	public void Die()
	{
		if (_isDead) return;
		_isDead = true;
		_currentHealth = 0;

		// Son kez UI güncelle (0 görünsün)
		EventManager.RaiseHealthChanged(_currentHealth, _maxHealth);

		// GameManager'a veya LevelManager'a haber ver
		EventManager.RaisePlayerDeath();

		// Fiziksel etkileşimi kes ama objeyi yok etme (Animasyon oynayabilir)
		rb.linearVelocity = Vector2.zero;
		rb.simulated = false; // Çarpışmaları kapat
		GameManager.Instance.SwitchState(GameManager.Instance.States.GameOver());
		// Opsiyonel: Ölüm animasyonu
		if(anim != null) anim?.SetTrigger("Death");

	}

	public bool CanTakeDamage()
	{
		return !isInvinsible;
	}

	public void MakeInvinsible(float duration)
	{
		if (invinsibleRoutine != null)
		{
			StopCoroutine(invinsibleRoutine);
			invinsibleRoutine = null;
		}
		invinsibleRoutine = StartCoroutine(InvinsibleCoroutine(duration));
	}
	IEnumerator InvinsibleCoroutine(float duration)
	{
		isInvinsible = true;
		yield return new WaitForSeconds(duration);
		isInvinsible = false;
	}
	// --- YARDIMCI RUTİNLER ---

	private void ApplyKnockback(Vector2 dir, float force)
	{
		// Oyuncunun hareketini kısa süreliğine kilitlemek isteyebilirsin (Stun)
		// Şimdilik sadece fiziksel itme uyguluyoruz.
		if (force > 0)
		{
			rb.AddForce(dir * force, ForceMode2D.Impulse);
		}
	}

	private IEnumerator InvincibilityRoutine()
	{
		_isInvincible = true;

		// Yanıp Sönme Efekti (Flash)
		float flashTimer = 0f;
		while (flashTimer < iFrameDuration)
		{
			spriteRenderer.color = damageColor;
			yield return new WaitForSeconds(0.1f);
			spriteRenderer.color = originalColor;
			yield return new WaitForSeconds(0.1f);
			flashTimer += 0.2f;
		}

		spriteRenderer.color = originalColor;
		_isInvincible = false;
	}

}