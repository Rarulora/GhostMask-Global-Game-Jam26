using UnityEngine;

public class MeleeWeapon : WeaponBase
{
	[SerializeField] private float attackAngle = 140f;

	public override void Attack()
	{
		if (!canAttack) return;
		PlayAnimation();
		StartCoroutine(CooldownRoutine());

		Collider2D[] hits = Physics2D.OverlapCircleAll(playerTransform.position, GetStat(Enums.StatType.range));

		// 1. Merkez açımızı (Silahın açısı + 90) vektöre çeviriyoruz
		// Böylece "transform.right" yerine ofsetli yönü kullanıyoruz.
		float centerAngle = transform.eulerAngles.z + 100f;
		Vector2 facingDir = DirFromAngle(centerAngle);

		foreach (var hit in hits)
		{
			if (hit.CompareTag("Enemy"))
			{
				IDamageable enemy = hit.GetComponent<IDamageable>();
				if (enemy != null)
				{
					Vector2 dirToTarget = (hit.transform.position - playerTransform.position).normalized;

					// 2. Açı Hesabı: Artık ofsetli "facingDir" ile karşılaştırıyoruz
					float angleToTarget = Vector2.Angle(facingDir, dirToTarget);
					bool isCrit = IsCrit();
					if (angleToTarget < attackAngle / 2f)
					{
						Vector2 knockbackDir = (hit.transform.position - playerTransform.position).normalized;
						enemy.TakeDamage(GetStat(Enums.StatType.damage), isCrit, knockbackDir, GetStat(Enums.StatType.knockbackForce));
					}
					CameraController.Instance.ShakeCamera(0.1f, 0.2f);
				}
			}
		}
		
	}

	private void OnDrawGizmosSelected()
	{
		if (playerTransform == null || data == null) return;

		Gizmos.color = new Color(1, 0, 0, 0.1f);
		Gizmos.DrawWireSphere(playerTransform.position, GetStat(Enums.StatType.range));

		Gizmos.color = Color.yellow;
		Vector3 origin = playerTransform.position;

		// --- GİZMOS DÜZELTMESİ (+90 EKLENDİ) ---
		float currentZ = transform.eulerAngles.z;
		float offsetZ = currentZ + 100f; // İSTEDİĞİN KISIM BURASI

		// Açının sağı ve solu (+70 ve -70)
		Vector3 angleA = DirFromAngle(offsetZ + attackAngle / 2f);
		Vector3 angleB = DirFromAngle(offsetZ - attackAngle / 2f);

		Gizmos.DrawLine(origin, origin + angleA * GetStat(Enums.StatType.range));
		Gizmos.DrawLine(origin, origin + angleB * GetStat(Enums.StatType.range));
	}

	// Açıyı Vektöre Çeviren Yardımcı Fonksiyon
	private Vector3 DirFromAngle(float angleInDegrees)
	{
		return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
	}
}