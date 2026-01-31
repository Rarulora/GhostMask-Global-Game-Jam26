namespace Enums
{
	public enum StatModType
	{
		Flat = 100,       // Düz Ekleme (Örn: +10 Hasar)
		PercentAdd = 200, // Yüzdesel Ekleme (Örn: +%10 Hasar -> Base * 0.1)
		PercentMult = 300 // Çarpımsal (Nadir kullanılır, Örn: Hasarı 2'ye katla)
	}

	public enum StatType
	{
		none,
		moveSpeed,
		damage,
		health,
		attackRate,
		critChance,
		critMultiplier,
		range,
		madnessResist,
		goldMultiplier,
		expMultiplier,
		pickupRange,
		knockbackForce,
		projectileCount,
		projectileSpeed,
		pierce
	}

	public enum AttackType
	{
		Melee,
		Ranged,
		Dash
	}

	public enum CharacterType
	{
		Mouse,
		Raccoon,
		Platipus,
		Cat,
		Monkey
	}

	public enum WeaponCategory
	{
		None,
		Melee,
		Ranged,
		Dash
	}

	public enum WeaponStyle
	{
		Base,   // Kategori seçildi ama alt kategori seçilmedi (Örn: Düz Sopalar)
		Sword,  // Melee Alt
		Hammer, // Melee Alt
		Bow,    // Ranged Alt
		Orb,    // Ranged Alt
		Fast,   // Dash Alt
		Strong  // Dash Alt
	}

	public enum TriggerType
	{
		None,
		OnHit,          // Hit enemy
		OnKill,         // Enemy dies
		OnTakeDamage,   // Player takes damage
		OnDashStart,    // Dash begins
		OnDashEnd,      // Dash ends
		OnTouch,        // Collision with enemy (Thorns)
		OnDeath,        // Player dies (Phoenix)
		ConstantUpdate  // For things like "Momentum" (Damage based on Speed)
	}

	public enum EffectType
	{
		None,
		Burn,
		Freeze,
		Slow,
		Poison,
		Bleed,
		Stun,
		Explosion,
		Heal,
		Invincibility,
		Madness
	}

	public enum SourceExtension
	{
		currentMadness,
		lostHealth,
		currentHealth
	}
}