using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Needed for array checks

[RequireComponent(typeof(Button))]
public class CosmeticMenuElement : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Image iconImage;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private GameObject lockIcon;
	[SerializeField] private GameObject equippedBadge;
	[SerializeField] private GameObject selectionOutline;

	[Header("Colors")]
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color selectedColor = new Color(0.9f, 0.9f, 0.9f);

	// Internal Data
	private CosmeticsUI _controller;
	private CharacterDataSO _charData;
	private CosmeticData _cosmeticData;
	private bool _isCharacter;

	public int ID => _isCharacter ? _charData.ID : _cosmeticData.ID;
	public string Name => _isCharacter ? _charData.Name : _cosmeticData.Name;
	public string Description => _isCharacter ? _charData.Description : _cosmeticData.description;
	public int Price => _isCharacter ? _charData.Price : (int)_cosmeticData.price;
	public Sprite Icon => _isCharacter ? null : _cosmeticData.sprite; // Note: CharacterSO didn't have a sprite in your provided code, handle accordingly.

	// State
	public bool IsOwned { get; private set; }
	public bool IsEquipped { get; private set; }

	private Button _btn;

	private void Awake()
	{
		_btn = GetComponent<Button>();
		_btn.onClick.AddListener(OnClick);
	}

	// --- Setup Methods ---

	public void Setup(CosmeticsUI controller, CharacterDataSO data)
	{
		_controller = controller;
		_charData = data;
		_isCharacter = true;
		_cosmeticData = null;

		if (iconImage) iconImage.sprite = data.Icon;

		RefreshState();
	}

	public void Setup(CosmeticsUI controller, CosmeticData data)
	{
		_controller = controller;
		_cosmeticData = data;
		_isCharacter = false;
		_charData = null;

		if (iconImage) iconImage.sprite = data.sprite;

		RefreshState();
	}

	// --- State Management ---

	public void RefreshState()
	{
		SaveData save = GameManager.Instance.SaveData;

		if (_isCharacter)
		{
			// ID 0 is usually default, assume owned. Or check array.
			IsOwned = ID == 0 || save.purchasedCharacterIDs.Contains(ID);
			IsEquipped = save.equippedCharacterID == ID;
		}
		else
		{
			IsOwned = ID == 0 || save.purchasedCosmeticIDs.Contains(ID);

			// Check specific slots based on type
			if (_cosmeticData.type == CosmeticType.Hat)
				IsEquipped = save.equippedHatID == ID;
			// Add Wing logic here if CosmeticType expands
		}

		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (lockIcon) lockIcon.SetActive(!IsOwned);
		if (equippedBadge) equippedBadge.SetActive(IsEquipped);
	}

	public void SetSelected(bool isSelected)
	{
		if (selectionOutline) selectionOutline.SetActive(isSelected);
		if (backgroundImage) backgroundImage.color = isSelected ? selectedColor : normalColor;
	}

	private void OnClick()
	{
		_controller.SelectElement(this);
	}
}