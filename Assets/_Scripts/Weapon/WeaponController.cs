using Enums;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
	[Header("Weapon Database")]
	// Tüm olası silahların Data dosyalarını buraya sürükle (Inspector)
	private WeaponData[] allWeapons;
	private WeaponData startWeapon;

	private WeaponBase currentWeaponBehavior;
	private WeaponData currentWeaponData;

	private Rigidbody2D rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();

		allWeapons = Resources.LoadAll<WeaponData>("Weapons");
	}
	private void Start()
	{
		EquipWeapon(startWeapon);
	}
	private void OnEnable()
	{
		// EventManager dinlemeye başla
		// Not: EventManager'da OnPerkSelected(string perkName) veya benzeri bir yapı olduğunu varsayıyorum.
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		// Sol tık ile saldırı
		if (Input.GetMouseButton(0) && currentWeaponBehavior != null)
		{
			currentWeaponBehavior.Attack();
		}
	}


	public void EquipWeapon(WeaponData newData)
	{
		// Eski silahı temizle
		if (currentWeaponBehavior != null)
		{
			Destroy(currentWeaponBehavior); // Component'i siliyoruz
		}

		currentWeaponData = newData;

		// Yeni silahın türüne göre doğru Script'i AddComponent yapıyoruz
		switch (newData.Category)
		{
			case WeaponCategory.Melee:
				currentWeaponBehavior = gameObject.AddComponent<MeleeWeapon>();
				break;
			case WeaponCategory.Ranged:
				currentWeaponBehavior = gameObject.AddComponent<RangedWeapon>();
				break;
			case WeaponCategory.Dash:
				currentWeaponBehavior = gameObject.AddComponent<DashWeapon>();
				break;
		}

		// Silahı başlat
		if (currentWeaponBehavior != null)
		{
			currentWeaponBehavior.Initialize(newData, transform, rb);
			Debug.Log($"Silah Kuşandı: {newData.WeaponName}");
		}
	}

	// Basit bir arama fonksiyonu (Bunu geliştirebilirsin)
	private WeaponData FindWeaponByPerk(string perkName)
	{
		foreach (var w in allWeapons)
		{
			// Data dosyasındaki isimle perk ismini eşleştiriyoruz
			// Örn: Perk adı "Sword" ise, WeaponData adı da "Sword" olmalı.
			if (perkName.Contains(w.WeaponName))
			{
				return w;
			}
		}
		return null;
	}
}