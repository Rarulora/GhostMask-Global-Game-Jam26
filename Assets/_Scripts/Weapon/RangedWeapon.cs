using UnityEngine;

public class RangedWeapon : WeaponBase
{
	public override void Attack()
	{
		if (!canAttack) return;
		StartCoroutine(CooldownRoutine());

		// Mouse pozisyonunu al (Nişan için)
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (mousePos - playerTransform.position).normalized;

		// Projectile Pool'dan mermi çek (Daha önce yazdığımız sistemi kullanıyoruz)
		if (data.ProjectilePrefab != null)
		{
			// Mermi rotasyonunu ayarla
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0, 0, angle);

			GameObject proj = ProjectilePoolManager.Instance.Get(data.ProjectilePrefab, playerTransform.position, rotation);

			Projectile pScript = proj.GetComponent<Projectile>();
			if (pScript != null)
			{
				pScript.Initialize(data.Damage, direction, "Enemy", StatsController.I.GetStat(Enums.StatType.projectileSpeed).Value);
			}
		}

		Debug.Log($"{data.WeaponName} fırlatıldı!");
	}
}