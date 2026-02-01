using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Enums; // CosmeticType için

[RequireComponent(typeof(Button))]
public class CosmeticMenuElement : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Image iconImage;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private GameObject lockIcon;
	[SerializeField] private GameObject equippedBadge;
	[SerializeField] private GameObject selectionOutline;

	[Header("Visual Settings")]
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color selectedColor = new Color(0.9f, 0.9f, 0.9f);
	[SerializeField] private float selectedScale = 1.15f; // Seçilince ne kadar büyüsün?

	private CosmeticsUI _controller;
	private CharacterDataSO _charData;
	private CosmeticData _cosmeticData;
	private bool _isCharacter;

	// Public Properties for Filtering
	public bool IsCharacter => _isCharacter;

	// Eðer kozmetikse tipini dön, deðilse null (veya varsayýlan) dön
	public CosmeticType? CosmeticType => _isCharacter ? null : _cosmeticData.type;

	public int ID => _isCharacter ? _charData.ID : _cosmeticData.ID;
	public string Name => _isCharacter ? _charData.Name : _cosmeticData.Name;
	public string Description => _isCharacter ? _charData.Description : _cosmeticData.description;
	public int Price => _isCharacter ? _charData.Price : (int)_cosmeticData.price;

	public bool IsOwned { get; private set; }
	public bool IsEquipped { get; private set; }

	private Button _btn;

	private void Awake()
	{
		_btn = GetComponent<Button>();
		_btn.onClick.AddListener(OnClick);
	}

	// --- SETUP KARAKTER ---
	public void Setup(CosmeticsUI controller, CharacterDataSO data)
	{
		_controller = controller;
		_charData = data;
		_isCharacter = true;
		_cosmeticData = null;

		if (iconImage) iconImage.sprite = data.Icon;
		RefreshState();
	}

	// --- SETUP KOZMETÝK ---
	public void Setup(CosmeticsUI controller, CosmeticData data)
	{
		_controller = controller;
		_cosmeticData = data;
		_isCharacter = false;
		_charData = null;

		if (iconImage) iconImage.sprite = data.sprite;
		RefreshState();
	}

	// --- STATE REFRESH ---
	public void RefreshState()
	{
		SaveData save = GameManager.Instance.SaveData;

		if (_isCharacter)
		{
			IsOwned = ID == 0 || save.purchasedCharacterIDs.Contains(ID);
			IsEquipped = save.equippedCharacterID == ID;
		}
		else
		{
			IsOwned = ID == 0 || save.purchasedCosmeticIDs.Contains(ID);

			if (_cosmeticData.type == Enums.CosmeticType.Hat)
				IsEquipped = save.equippedHatID == ID;
			else if (_cosmeticData.type == Enums.CosmeticType.Mask)
				IsEquipped = save.equippedMaskID == ID;
		}

		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (lockIcon) lockIcon.SetActive(!IsOwned);
		if (equippedBadge) equippedBadge.SetActive(IsEquipped);
	}

	// --- FOCUS / SELECT VISUALS ---
	public void SetSelected(bool isSelected)
	{
		if (selectionOutline) selectionOutline.SetActive(isSelected);

		if (backgroundImage)
			backgroundImage.color = isSelected ? selectedColor : normalColor;

		// SCALE UP LOGIC
		Vector3 targetScale = isSelected ? Vector3.one * selectedScale : Vector3.one;
		transform.localScale = targetScale;
	}

	private void OnClick()
	{
		_controller.SelectElement(this);
	}
}