using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyBase : MonoBehaviour, IDamageable
{
	[Header("Movement Settings")]
	[SerializeField] protected float separationRadius = 2.0f; // Ne kadar yakındakilerden kaçsın?
	[SerializeField] protected float separationForce = 1.5f;  // İtme gücü (Hafifçe artırdım)

	protected EnemyStats stats;
	protected Rigidbody2D rb;
	protected SpriteRenderer spriteRenderer;
	protected Collider2D col;
	protected Transform playerTarget;

	protected bool isDead = false;
	protected bool isStunned = false;
	private float nextAttackTime = 0f;

	private Color originalColor = Color.white;
	private Color invisibleColor;
	private Coroutine flashRoutine;
	private Coroutine stunRoutine;
	private Vector3 originalScale;

	public EnemyStats Stats => stats;

	protected virtual void Awake()
	{
		stats = GetComponent<EnemyStats>();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		col = GetComponent<Collider2D>();

		originalColor = spriteRenderer.color;
		invisibleColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

		originalScale = transform.localScale;
	}

	protected virtual void OnEnable()
	{
		isDead = false;
		isStunned = false;
		col.enabled = true;
		rb.simulated = true;
		spriteRenderer.color = originalColor;

		// Scale'i başlangıç haline getir
		transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

		if (flashRoutine != null) StopCoroutine(flashRoutine);
		if (stunRoutine != null) StopCoroutine(stunRoutine);

		if (playerTarget == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) playerTarget = p.transform;
		}

		EventManager.OnMaskChanged += UpdateVisibility;
	}

	protected virtual void OnDisable()
	{
		EventManager.OnMaskChanged -= UpdateVisibility;
	}

	protected virtual void FixedUpdate()
	{
		if (isDead || isStunned || playerTarget == null) return;

		// Mesafeyi ölç
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

		// NOT: Ranged Enemy için buradaki AttackRange kontrolü, alt sınıfta (override Move) ezilebilir
		// veya stats.AttackRange doğru ayarlandıysa burada da çalışır.
		if (distanceToPlayer <= stats.AttackRange)
		{
			// Menzildeyiz: Dur ve Saldır
			StopMovement();
			AttackLogic();
			// Dururken de oyuncuya dönmesi için:
			RotateTowardsTarget();
		}
		else
		{
			// Menzilde değiliz: Yürü
			Move();
		}
	}

	// --- HAREKET MANTIĞI (Separation + Flip Entegre Edildi) ---
	protected virtual void Move()
	{
		if (playerTarget == null) return;

		// 1. Hedefe Yönelim (Target Direction)
		Vector2 targetDir = (playerTarget.position - transform.position).normalized;

		// 2. Ayrılma (Separation) ve Yanal Kayma (Avoidance)
		Vector2 separation = Vector2.zero;
		Vector2 avoidance = Vector2.zero; // YENİ: Yana kaçma vektörü

		Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

		foreach (var neighbor in neighbors)
		{
			if (neighbor.gameObject != gameObject && neighbor.CompareTag("Enemy"))
			{
				Vector2 toNeighbor = neighbor.transform.position - transform.position;
				float dist = toNeighbor.magnitude;

				// A. Standart İtme (Separation)
				Vector2 pushBack = -toNeighbor.normalized;
				separation += pushBack / (dist + 0.1f);

				// B. Yanal Kayma (Tangential Avoidance) - YENİ
				// Eğer önümdeki düşmanla hedefe giden yolum çakışıyorsa, yana kaymalıyım.
				// Düşmanın olduğu yönün 90 derece dikine (perpendicular) bir vektör alıyoruz.
				Vector2 sideStep = new Vector2(-toNeighbor.y, toNeighbor.x).normalized;

				// Hangi tarafın boş olduğuna karar vermek için ufak bir "Gürültü" ekliyoruz
				// GetInstanceID() sayesinde her düşman farklı tarafa kırmak ister.
				float noise = Mathf.PerlinNoise(Time.time * 0.5f, GetInstanceID() * 0.1f) - 0.5f;

				avoidance += sideStep * (Mathf.Sign(noise));
			}
		}

		// 3. Vektörleri Birleştir
		// Target + (Separation * Force) + (Avoidance * Force)
		// Avoidance gücünü Separation'dan biraz daha düşük tutuyoruz ki çok savrulmasınlar.
		Vector2 finalDir = (targetDir + (separation * separationForce) + (avoidance * separationForce * 0.8f)).normalized;

		// 4. Doğal Salınım (Opsiyonel: Yılan gibi kıvrılarak gelmeleri için)
		// Düşmanlar dümdüz ip gibi gelmek yerine hafif sağ-sol yaparak gelir, bu da üst üste binmeyi azaltır.
		float wave = Mathf.Sin(Time.time * 2f + GetInstanceID()) * 0.2f;
		finalDir += new Vector2(finalDir.y, -finalDir.x) * wave; // Hareket yönüne dik salınım ekle
		finalDir.Normalize();

		// 5. Fiziksel Hareket
		rb.MovePosition(rb.position + finalDir * stats.MoveSpeed * Time.fixedDeltaTime);

		// 6. Dönme
		RotateTowardsTarget();
	}

	// Alt sınıflar (Ranged Enemy gibi) kullanabilsin diye buraya ekledim
	protected void RotateTowardsTarget()
	{
		// Eğer sadece Scale kullanıyorsak, dururken de scale güncelleyelim
		Vector2 dir = playerTarget.position - transform.position;
		if (dir.x > 0)
			transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
		else if (dir.x < 0)
			transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
	}

	protected void StopMovement()
	{
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

	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir, float knockbackForce)
	{
		if (isDead) return;

		stats.TakeDamage(amount);

		if (flashRoutine != null) StopCoroutine(flashRoutine);
		flashRoutine = StartCoroutine(HitFlashRoutine());

		ApplyKnockback(knockbackDir, knockbackForce);
        DamagePopup.Create(transform.position, amount, isCritical);

        if (stats.IsDead)
		{
			Die();
		}
	}

	private void ApplyKnockback(Vector2 dir, float force)
	{
		if (stunRoutine != null) StopCoroutine(stunRoutine);
		stunRoutine = StartCoroutine(StunRoutine(0.2f));

		rb.AddForce(dir * force, ForceMode2D.Impulse);
	}

	private IEnumerator StunRoutine(float duration)
	{
		isStunned = true;
		yield return new WaitForSeconds(duration);
		isStunned = false;
		rb.linearVelocity = Vector2.zero;
	}

	private IEnumerator HitFlashRoutine()
	{
		spriteRenderer.color = Color.white;
		yield return new WaitForSeconds(0.1f);

		spriteRenderer.color = originalColor;
	}

	public void UpdateVisibility(bool isMaskOn)
	{
		if (isMaskOn)
			spriteRenderer.color = originalColor;
		else
			spriteRenderer.color = invisibleColor;
	}

	public virtual void Die()
	{
		if (isDead) return;
		isDead = true;

		col.enabled = false;
		rb.linearVelocity = Vector2.zero;
		rb.simulated = false;

		// TODO: Loot Drop 
		// TODO: Death VFX

		StartCoroutine(DisableRoutine());
	}

	private IEnumerator DisableRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		gameObject.SetActive(false);
	}
}