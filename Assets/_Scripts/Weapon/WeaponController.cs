using Enums;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
	[Header("Configurations")]
	public Transform firePoint;
	[Header("Weapon Database")]
	[SerializeField] private WeaponData startWeapon;

	private WeaponData[] allWeapons;
	private WeaponBase currentWeaponBehavior;
	private WeaponData currentWeaponData;

	private Rigidbody2D rb;
	private Camera mainCam; // Performans için kamerayı cache'liyoruz

	private void Awake()
	{
		rb = GetComponentInParent<Rigidbody2D>();
		allWeapons = Resources.LoadAll<WeaponData>("Weapons");
		mainCam = Camera.main; // Kamerayı al
	}

	private void Start()
	{
		EquipWeapon(startWeapon);
	}

	private void OnEnable()
	{
		// EventManager dinlemeye başla
	}

	private void OnDisable()
	{
		// EventManager dinlemeyi bırak
	}

	private void Update()
	{
		// 1. ROTASYON MANTIĞI (YENİ EKLENEN KISIM)
		HandleRotation();

		// 2. SALDIRI MANTIĞI
		if (Input.GetMouseButton(0) && currentWeaponBehavior != null)
		{
			currentWeaponBehavior.Attack();
		}
	}

	// --- SİLAH DÖNDÜRME FONKSİYONU ---
	private void HandleRotation()
	{
		// Eğer oyun duraklatıldıysa veya karakter ölü ise dönmesin (Opsiyonel kontrol eklenebilir)

		// A. Mouse Pozisyonunu Bul
		Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

		// B. Yön Vektörünü Hesapla (Mouse - Silah Pozisyonu)
		Vector3 direction = transform.position - mousePos;

		// C. Açıyı Hesapla (Radyan -> Derece)
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;

		// D. Rotasyonu Uygula
		transform.rotation = Quaternion.Euler(0, 0, angle);

	}

	public void EquipWeapon(WeaponData newData)
	{
		if (currentWeaponBehavior != null)
		{
			Destroy(currentWeaponBehavior);
		}
		if (currentWeaponData != null)
		{
			RemoveWeaponStats(currentWeaponData);
		}
		currentWeaponData = newData;

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
		if (currentWeaponData != null)
		{
			AddWeaponStats(currentWeaponData);
		}
		if (currentWeaponBehavior != null)
		{
			currentWeaponBehavior.Initialize(newData, transform, firePoint, rb);

			// Yeni silah geldiğinde scale'i sıfırla ki ters kalmasın
			transform.localScale = Vector3.one;

			Debug.Log($"Silah Kuşandı: {newData.WeaponName}");
		}
	}

	private void AddWeaponStats(WeaponData data)
	{
		var stats = StatsController.I;
		if (stats == null) return;

		// StatModifier oluştururken 'source' olarak 'data' (WeaponData) veriyoruz.
		// Bu sayede silerken "Bu WeaponData'dan gelenleri sil" diyebileceğiz.

		if (data.damage > 0)
			stats.GetStat(StatType.damage).AddModifier(new StatModifier(data.damage, StatModType.Flat, data));

		if (data.attackRate > 0)
			stats.GetStat(StatType.attackRate).AddModifier(new StatModifier(data.attackRate, StatModType.Flat, data));

		if (data.range > 0)
			stats.GetStat(StatType.range).AddModifier(new StatModifier(data.range, StatModType.Flat, data));

		if (data.knockbackForce > 0)
			stats.GetStat(StatType.knockbackForce).AddModifier(new StatModifier(data.knockbackForce, StatModType.Flat, data));

		if (data.projectileCount > 0)
			stats.GetStat(StatType.projectileCount).AddModifier(new StatModifier(data.projectileCount, StatModType.Flat, data));

		if (data.projectileSpeed > 0)
			stats.GetStat(StatType.projectileSpeed).AddModifier(new StatModifier(data.projectileSpeed, StatModType.Flat, data));

		if (data.pierce > 0)
			stats.GetStat(StatType.pierce).AddModifier(new StatModifier(data.pierce, StatModType.Flat, data));

		Debug.Log(stats.GetValue(StatType.projectileSpeed));
	}

	private void RemoveWeaponStats(WeaponData data)
	{
		var stats = StatsController.I;
		if (stats == null) return;

		// PlayerStat sınıfında daha önce yazdığımız RemoveAllModifiersFromSource metodunu kullanıyoruz.
		// Tek tek modifier referansı tutmaya gerek kalmıyor.

		stats.GetStat(StatType.damage).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.attackRate).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.range).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.knockbackForce).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.projectileCount).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.projectileSpeed).RemoveAllModifiersFromSource(data);
		stats.GetStat(StatType.pierce).RemoveAllModifiersFromSource(data);
	}
}