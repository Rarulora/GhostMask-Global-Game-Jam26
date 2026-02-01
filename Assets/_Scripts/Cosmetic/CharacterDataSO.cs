using UnityEngine;

[CreateAssetMenu(menuName = "Database/CharacterData", fileName = "Character Data")]
public class CharacterDataSO : ScriptableObject
{
	[Header("Configuration")]
	[SerializeField] private int id;
	[SerializeField] private string cname;
	[SerializeField] private string description;
	[SerializeField] private int price;
	[SerializeField] private Sprite icon;
	[SerializeField] private Sprite sprite;
	[SerializeField] private RuntimeAnimatorController animatorController;
	[Header("Stats")]
	[SerializeField] private float health = 5f;
	[SerializeField] private float damage = 1f;
	[SerializeField] private float attackRate = 2f;
	[SerializeField] private float moveSpeed = 4f;


	public int ID => id;
	public string Name => cname;
	public string Description => description;
	public Sprite Icon => icon;
	public Sprite Sprite => sprite;
	public RuntimeAnimatorController AnimatorController => animatorController;
	public int Price => price;
	public float Health => health;
	public float Damage => damage;
	public float AttackRate => attackRate;
	public float MoveSpeed => moveSpeed;
}
