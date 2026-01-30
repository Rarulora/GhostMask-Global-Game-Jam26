using UnityEngine;

public class EnemyStats : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField] private ParticleSystem healVFX;
	[Header("Data Source")]
	[SerializeField] private EnemyData _data;

	public float CurrentHealth { get; private set; }
	public float MaxHealth { get; private set; }
	public float MoveSpeed { get; private set; }
	public float Damage { get; private set; }
	public float AttackRate { get; private set; }
	public float AttackRange { get; private set; }

	// Ödüller
	public int XPReward { get; private set; }
	public int GoldReward { get; private set; }

	// Durum Kontrolü
	public bool IsDead => CurrentHealth <= 0;

	private void Awake()
	{
		//Test
		Initialize(1);
	}

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

		XPReward = Mathf.RoundToInt(_data.XPDrop * difficultyMultiplier);
		GoldReward = _data.GoldDrop;

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

		healVFX?.Play();
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