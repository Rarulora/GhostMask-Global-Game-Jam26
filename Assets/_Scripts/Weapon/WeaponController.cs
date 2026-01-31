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

		if (currentWeaponBehavior != null)
		{
			currentWeaponBehavior.Initialize(newData, transform, firePoint, rb);

			// Yeni silah geldiğinde scale'i sıfırla ki ters kalmasın
			transform.localScale = Vector3.one;

			Debug.Log($"Silah Kuşandı: {newData.WeaponName}");
		}
	}
}