using UnityEngine;

[CreateAssetMenu(menuName = "Database/CharacterData", fileName = "New_Character_Data")]
public class CharacterDataSO : ScriptableObject
{
	[Header("Configuration")]
	[SerializeField] private int id;
	[SerializeField] private string name;
	[SerializeField] private string description;

	[Header("Stats")]
	[SerializeField] private float health = 5f;
	[SerializeField] private float damage = 1f;
	[SerializeField] private float attackRate = 2f;
	[SerializeField] private float moveSpeed = 4f;


	public int ID => id;
	public string Name => name;
	public string Description => description;
	public float Health => health;
	public float Damage => damage;
	public float AttackRate => attackRate;
	public float MoveSpeed => moveSpeed;
}
