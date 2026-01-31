using UnityEngine;

public class Enemy_Healer : EnemyBase
{
	[Header("Healer Settings")]
	[SerializeField] private float healRadius = 4f;
	[SerializeField] private GameObject healVFX;
	protected override void FixedUpdate()
	{
		if (isDead || isStunned || playerTarget == null) return;

		AttackLogic();

		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

		if (distanceToPlayer <= stats.AttackRange)
		{
			StopMovement();
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
				EnemyStats allyStats = hit.GetComponent<EnemyStats>();

				if (allyStats != null && allyStats.CurrentHealth < allyStats.MaxHealth)
				{
					allyStats.Heal(stats.Damage);
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