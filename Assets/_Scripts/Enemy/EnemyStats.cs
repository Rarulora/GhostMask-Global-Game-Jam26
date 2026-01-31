using UnityEngine;

public class EnemyStats : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ParticleSystem[] healVFXs;
	[Header("Data Source")]
	[SerializeField] private EnemyData _data;

	public float CurrentHealth { get; private set; }
	public float MaxHealth { get; private set; }
	public float MoveSpeed { get; set; }
	public float Damage { get; private set; }
	public float AttackRate { get; private set; }
	public float AttackRange { get; private set; }
	public float ProjectileSpeed { get; private set; }
	public int Pierce {  get; private set; }

	// Ödüller
	public int XPReward { get; private set; }
	public int GoldReward { get; private set; }
	public float GoldDropChance { get; private set; }

	// Durum Kontrolü
	public bool IsDead => CurrentHealth <= 0;


	public void Initialize(float difficultyMultiplier)
	{
		if (_data == null)
		{
			Debug.LogError($"{gameObject.name} EnemyData is null!");
			return;
		}

		MaxHealth = _data.MaxHealth * difficultyMultiplier;
		CurrentHealth = MaxHealth;
		Damage = _data.Damage * difficultyMultiplier;

		MoveSpeed = _data.MoveSpeed;
		AttackRate = _data.AttackRate;
		AttackRange = _data.AttackRange;
		ProjectileSpeed = _data.ProjectileSpeed;
		Pierce = _data.Pierce;

		XPReward = Mathf.RoundToInt(_data.XPDrop * difficultyMultiplier);
		GoldReward = _data.GoldDrop;
		GoldDropChance = _data.GoldDropChance;

		SetupPhysics();
	}

	private void SetupPhysics()
	{
		var rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			if (_data.ImmuneToKnockback)
			{
				rb.mass = 10000f;
				rb.linearDamping = 10f;
			}
			else
			{
				rb.mass = _data.Mass;
				rb.linearDamping = 1f;
			}
		}
	}

	public void TakeDamage(float amount)
	{
		if (IsDead) return;

		CurrentHealth -= amount;

		if (CurrentHealth <= 0)
		{
			CurrentHealth = 0;
		}
	}

	public void Heal(float amount)
	{
		if (IsDead) return;
		CurrentHealth += amount;
		if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

		if (healVFXs.Length > 0)
		{
			foreach(var kvp in healVFXs) kvp.Play();
		}
	}

	public void ModifySpeed(float multiplier)
	{
		MoveSpeed = _data.MoveSpeed * multiplier;
	}

	public void ResetSpeed()
	{
		MoveSpeed = _data.MoveSpeed;
	}
}