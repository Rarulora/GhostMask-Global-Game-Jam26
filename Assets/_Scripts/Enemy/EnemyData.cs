using UnityEngine;

[CreateAssetMenu(menuName = "Database/EnemyData", fileName = "New_EnemyData")]
public class EnemyData : ScriptableObject
{
	[Header("Base Stats")]
	[SerializeField] private float maxHealth = 5f;
	[SerializeField] private float moveSpeed = 3f;
	[SerializeField] private float damage = 1f;
	[SerializeField] private float attackRate = 2f;
	[SerializeField] private float attackRange = 1.5f;

	[Header("Rewards")]
	[SerializeField] private int xpDrop = 1;
	[SerializeField] private int goldDrop = 1;

	[Header("Settings")]
	[SerializeField] private float mass = 1f;
	[SerializeField] private bool immuneToKnockback = false;

	public float MaxHealth => maxHealth;
	public float MoveSpeed => moveSpeed;
	public float Damage => damage;
	public float AttackRate => attackRate;
	public float AttackRange => attackRange;
	public int XPDrop => xpDrop;
	public int GoldDrop => goldDrop;
	public float Mass => mass;
	public bool ImmuneToKnockback => immuneToKnockback;
}