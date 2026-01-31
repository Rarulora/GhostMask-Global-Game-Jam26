using UnityEngine;
using UnityEngine.InputSystem;

public class MaskController : MonoBehaviour
{
	public static MaskController I;
	[SerializeField] private SpriteRenderer maskSR;
	[Header("Madness Settings")]
	[SerializeField] private float maxMadness = 100f;
	[SerializeField] private float baseFillSpeed = 10f; // Maske takýlýyken (KÖTÜ) saniyede dolma hýzý
	[SerializeField] private float baseDrainSpeed = 5f; // Maske yokken (ÝYÝ) saniyede azalma hýzý
	[SerializeField] private float minFillSpeed = 0.5f; // Direnç ne kadar yüksek olursa olsun min dolma hýzý

	private float _currentMadness = 0f;
	private bool _isMaskActive = false;

	public bool IsMaskActive => _isMaskActive;
	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;
	}
	private void Start()
	{
		UpdateMadnessUI();
		// Test amaçlý log, oyun bitince silebilirsin
		InvokeRepeating(nameof(Test), 0, .5f);
	}

	void Test() => Debug.Log($"Madness: {Mathf.RoundToInt(_currentMadness)} / Mask: {_isMaskActive}");

	void Update()
	{
		HandleInput();
		HandleMadness();
	}

	private void HandleInput()
	{
		if (Keyboard.current.spaceKey.wasPressedThisFrame)
		{
			// Eðer delilik tavan yaptýysa maskeyi takmasýna izin verme (Opsiyonel mekanik)
			if (!_isMaskActive && _currentMadness >= maxMadness)
			{
				Debug.Log("Çok delirdin, maskeyi takamazsýn!");
				return;
			}

			MaskStatusChange(!_isMaskActive);
		}
	}

	private void HandleMadness()
	{
		// Direnç deðerini al
		float resistance = StatsController.I != null ? StatsController.I.GetStat(Enums.StatType.madnessResist).Value : 0f;
		float previousValue = _currentMadness;

		if (_isMaskActive)
		{
			// --- KÖTÜ DURUM: MASKE TAKILI, BAR DOLUYOR ---

			// Direnç arttýkça dolum hýzý düþer (Daha geç delirirsin)
			float actualFillSpeed = baseFillSpeed - resistance;

			// Hýz negatif olamaz, en az minFillSpeed kadar dolar
			actualFillSpeed = Mathf.Max(actualFillSpeed, minFillSpeed);

			_currentMadness += actualFillSpeed * Time.deltaTime;

			// Bar tamamen doldu mu?
			if (_currentMadness >= maxMadness)
			{
				_currentMadness = maxMadness;
				OnMadnessOverload();
			}
		}
		else
		{
			// --- ÝYÝ DURUM: MASKE YOK, BAR AZALIYOR ---

			// Direnç arttýkça sakinleþme hýzý artar (Daha çabuk toparlarsýn)
			float actualDrainSpeed = baseDrainSpeed + (resistance * 0.5f);

			_currentMadness -= actualDrainSpeed * Time.deltaTime;

			if (_currentMadness < 0)
			{
				_currentMadness = 0;
			}
		}

		// Sadece deðer deðiþtiyse UI güncelle (Performans için)
		if (_currentMadness != previousValue)
		{
			UpdateMadnessUI();
		}
	}

	private void MaskStatusChange(bool isActive)
	{
		_isMaskActive = isActive;
		if(maskSR != null) maskSR.enabled = _isMaskActive;
		EventManager.RaiseMaskChanged(_isMaskActive);
	}

	private void UpdateMadnessUI()
	{
		EventManager.RaiseMadnessChanged(_currentMadness, maxMadness);
	}

	private void OnMadnessOverload()
	{
		// Bar %100 dolduðunda ne olacak?
		// Örnek: Maske zorla düþsün ve oyuncu sersemsin.
		if (_isMaskActive)
		{
			Debug.Log("AKIL SAÐLIÐINI KAYBETTÝN! Maske düþüyor...");
			MaskStatusChange(false); // Maskeyi zorla çýkar

			// Opsiyonel: Can götür veya sersemlet
			GetComponent<PlayerHealthController>().TakeDamage(100);
		}
	}
}