using Enums;
using System.Collections;
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
	[SerializeField] protected Color shadowColor = new Color(0, 0, 0, 0.4f); // Siyah ve %40 opaklık

	protected EnemyStats stats;
	protected Rigidbody2D rb;
	protected SpriteRenderer spriteRenderer;
	protected Collider2D col;
	protected Transform playerTarget;
	protected Animator anim;

	protected bool isDead = false;
	protected bool isStunned = false;

	// Maske durumunu takip etmek için değişken
	protected bool isMaskActive = false;

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
		anim = GetComponentInChildren<Animator>();

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

		transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

		if (flashRoutine != null) StopCoroutine(flashRoutine);
		if (stunRoutine != null) StopCoroutine(stunRoutine);

		if (playerTarget == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) playerTarget = p.transform;
		}

		// Event'e abone ol
		EventManager.OnMaskChanged += UpdateMaskStatus;

		UpdateMaskStatus(MaskController.I.IsMaskActive);
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

		// --- YENİ: GÖRÜNÜRLÜK YÖNETİMİ ---
		HandleVisibility(distanceToPlayer);
		// ---------------------------------

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

	// --- YENİ EKLENEN GÖRÜNÜRLÜK FONKSİYONU ---
	protected void HandleVisibility(float distance)
	{
		// Eğer hasar animasyonu (Flash) oynuyorsa rengi elleme
		if (flashRoutine != null) return;

		if (isMaskActive)
		{
			// Maske takılıysa normal renginde görün
			if (spriteRenderer.color != originalColor)
				spriteRenderer.color = originalColor;
		}
		else
		{
			// Maske YOKSA ve Gölge mesafesindeyse
			if (distance <= shadowDistance)
			{
				// Gölge rengini uygula (Siyah saydam)
				// Lerp kullanarak yumuşak geçiş yapabiliriz, şimdilik direkt atıyoruz.
				spriteRenderer.color = shadowColor;
			}
			else
			{
				// Uzaktaysa tamamen görünmez ol
				spriteRenderer.color = invisibleColor;
			}
		}
	}

	protected virtual void Move()
	{
		if (playerTarget == null)
		{
			if (anim != null) anim.SetBool("isMoving", true);
			return;
		}
		if (anim != null) anim.SetBool("isMoving", true);

		Vector2 targetDir = (playerTarget.position - transform.position).normalized;

		Vector2 separation = Vector2.zero;
		Vector2 avoidance = Vector2.zero;

		Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

		foreach (var neighbor in neighbors)
		{
			if (neighbor.gameObject != gameObject && neighbor.CompareTag("Enemy"))
			{
				Vector2 toNeighbor = neighbor.transform.position - transform.position;
				float dist = toNeighbor.magnitude;

				Vector2 pushBack = -toNeighbor.normalized;
				separation += pushBack / (dist + 0.1f);

				Vector2 sideStep = new Vector2(-toNeighbor.y, toNeighbor.x).normalized;
				float noise = Mathf.PerlinNoise(Time.time * 0.5f, GetInstanceID() * 0.1f) - 0.5f;

				avoidance += sideStep * (Mathf.Sign(noise));
			}
		}

		Vector2 finalDir = (targetDir + (separation * separationForce) + (avoidance * separationForce * 0.8f)).normalized;

		float wave = Mathf.Sin(Time.time * 2f + GetInstanceID()) * 0.2f;
		finalDir += new Vector2(finalDir.y, -finalDir.x) * wave;
		finalDir.Normalize();

		rb.MovePosition(rb.position + finalDir * stats.MoveSpeed * Time.fixedDeltaTime);
		RotateTowardsTarget();
	}

	protected void RotateTowardsTarget()
	{
		Vector2 dir = playerTarget.position - transform.position;
		if (dir.x > 0)
			transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
		else if (dir.x < 0)
			transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
	}

	protected void StopMovement()
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

	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir, float knockbackForce)
	{
		if (isDead) return;

		stats.TakeDamage(amount);

		if (flashRoutine != null) StopCoroutine(flashRoutine);
		flashRoutine = StartCoroutine(HitFlashRoutine());

		ApplyKnockback(knockbackDir, knockbackForce);
		// DamagePopup.Create(transform.position, amount, isCritical); // Bunu kapattım derleme hatası olmasın diye, sende açıksa aç

		if (stats.IsDead)
		{
			Die();
		}
	}

	public void ApplyStatus(EffectType type, float duration, float potency)
	{
		// Buraya daha önceki adımlarda yazdığımız status mantığı gelecek (Freeze, Burn vs.)
		// Eğer o kodu henüz yazmadıysan geçici olarak boş bırakabilirsin:
		Debug.Log($"Enemy Status Applied: {type}");
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

		// Flash bittiğinde anında bir renk atamıyoruz,
		// bir sonraki Update döngüsünde HandleVisibility doğru rengi (Gölge veya Normal) atayacak.
		// Ancak geçişin "titrememesi" için görünmezlik rengine çekebiliriz veya olduğu gibi bırakabiliriz.
		// HandleVisibility her frame çalıştığı için burada renk atamaya gerek yok.
		flashRoutine = null;
	}

	// İsmini değiştirdim: Artık sadece bool değerini güncelliyor
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

		StartCoroutine(DisableRoutine());
	}

	private IEnumerator DisableRoutine()
	{
		if (anim != null) anim.SetTrigger("Death");
		yield return new WaitForSeconds(0.2f);
		gameObject.SetActive(false);
	}
}