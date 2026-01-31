using UnityEngine;

public interface IDamageable
{
	public void TakeDamage(float amount, bool isCritical, Vector2 knockbackDir , float knockbackForce);
	void Die();
	public bool CanTakeDamage();
}