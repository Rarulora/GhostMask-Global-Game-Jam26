using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Enums; // CosmeticType Enum'ý için

public class CosmeticsUI : MonoBehaviour
{
	[Header("Main References")]
	[SerializeField] private GameObject cosmeticMenuElementPrefab;
	[SerializeField] private Transform cosmeticMenuRoot;

	[Header("Navigation Buttons")]
	[SerializeField] private Button mainMenuButton;
	[SerializeField] private Button playButton;

	[Header("Category Tabs")]
	[SerializeField] private Button characterTabButton;
	[SerializeField] private Button hatTabButton;
	[SerializeField] private Button maskTabButton;

	[Header("Visual Settings")]
	[SerializeField] private Color normalTabColor = Color.white;
	[SerializeField] private Color selectedTabColor = new Color(0.6f, 1f, 0.6f); // Açýk yeþil
	[SerializeField] private float selectedScaleAmount = 1.15f; // %15 büyüme
	[SerializeField] private float animationSpeed = 10f; // Lerp hýzý (Update'de kullanýlabilir, þimdilik direkt atama)

	[Header("Preview Images (Scene)")]
	[SerializeField] private Image characterPreviewImage;
	[SerializeField] private Image hatPreviewImage;
	[SerializeField] private Image maskPreviewImage;

	[Header("Info Panel")]
	[SerializeField] private GameObject elementInfoPanel;
	[SerializeField] private TMP_Text elementNameText;
	[SerializeField] private TMP_Text elementDescText;
	[SerializeField] private TMP_Text elementPriceText;
	[SerializeField] private Button elementBuyButton;
	[SerializeField] private TMP_Text elementBuyButtonText;
	[SerializeField] private TMP_Text playerGoldText;

	// --- State ---
	private List<CosmeticMenuElement> _spawnedElements = new List<CosmeticMenuElement>();
	private CosmeticMenuElement _selectedElement;

	// Filter State
	private enum FilterType { All, Character, Hat, Mask }
	private FilterType _currentFilter = FilterType.All;

	private void Awake()
	{
		InitButtons();
		InitMenuElements();
		UpdateGoldUI();
		UpdatePreviewVisuals();

		// Baþlangýçta hepsi gözüksün ve tab görselleri güncellensin
		ApplyFilter(FilterType.All);

		if (elementInfoPanel) elementInfoPanel.SetActive(false);
	}

	private void InitButtons()
	{
		mainMenuButton.onClick.RemoveAllListeners();
		playButton.onClick.RemoveAllListeners();
		elementBuyButton.onClick.RemoveAllListeners();

		characterTabButton.onClick.RemoveAllListeners();
		hatTabButton.onClick.RemoveAllListeners();
		maskTabButton.onClick.RemoveAllListeners();

		mainMenuButton.onClick.AddListener(() => GameManager.Instance.MainMenu());
		playButton.onClick.AddListener(() => GameManager.Instance.Play());
		elementBuyButton.onClick.AddListener(OnActionPressed);

		// Kategori Butonlarý
		characterTabButton.onClick.AddListener(() => OnCategoryClicked(FilterType.Character));
		hatTabButton.onClick.AddListener(() => OnCategoryClicked(FilterType.Hat));
		maskTabButton.onClick.AddListener(() => OnCategoryClicked(FilterType.Mask));
	}

	private void InitMenuElements()
	{
		foreach (Transform child in cosmeticMenuRoot) Destroy(child.gameObject);
		_spawnedElements.Clear();

		// 1. Karakterleri Yükle
		// Not: CharacterDatabase'e GameManager üzerinden eriþiyoruz.
		foreach (var ch in GameManager.Instance.CharacterDatabase.data)
		{
			var e = Instantiate(cosmeticMenuElementPrefab, cosmeticMenuRoot).GetComponent<CosmeticMenuElement>();
			e.Setup(this, ch);
			_spawnedElements.Add(e);
		}

		// 2. Kozmetikleri Yükle
		foreach (var co in GameManager.Instance.CosmeticDatabase.data)
		{
			var e = Instantiate(cosmeticMenuElementPrefab, cosmeticMenuRoot).GetComponent<CosmeticMenuElement>();
			e.Setup(this, co);
			_spawnedElements.Add(e);
		}
	}

	private void UpdateGoldUI()
	{
		if (playerGoldText)
			playerGoldText.text = $"Gold: {GameManager.Instance.SaveData.gold}";
	}

	// --- CATEGORY & FILTER LOGIC ---

	private void OnCategoryClicked(FilterType clickedType)
	{
		// Eðer zaten seçili olan kategoriye týklandýysa filtreyi kaldýr (Show All)
		if (_currentFilter == clickedType)
		{
			_currentFilter = FilterType.All;
		}
		else
		{
			_currentFilter = clickedType;
		}

		ApplyFilter(_currentFilter);
	}

	private void ApplyFilter(FilterType type)
	{
		_currentFilter = type; // State'i güncelle

		// 1. Elemanlarý Filtrele
		foreach (var elem in _spawnedElements)
		{
			bool shouldShow = false;

			switch (type)
			{
				case FilterType.All:
					shouldShow = true;
					break;
				case FilterType.Character:
					shouldShow = elem.IsCharacter;
					break;
				case FilterType.Hat:
					shouldShow = !elem.IsCharacter && elem.CosmeticType == CosmeticType.Hat;
					break;
				case FilterType.Mask:
					shouldShow = !elem.IsCharacter && elem.CosmeticType == CosmeticType.Mask;
					break;
			}

			elem.gameObject.SetActive(shouldShow);
		}

		// 2. Tab Görsellerini Güncelle (Renk + Scale)
		UpdateTabVisuals();
	}

	private void UpdateTabVisuals()
	{
		// Helper fonksiyon ile butonlarý güncelle
		SetTabState(characterTabButton, _currentFilter == FilterType.Character);
		SetTabState(hatTabButton, _currentFilter == FilterType.Hat);
		SetTabState(maskTabButton, _currentFilter == FilterType.Mask);
	}

	private void SetTabState(Button btn, bool isActive)
	{
		if (btn == null) return;

		// Renk Deðiþimi
		var colors = btn.colors;
		colors.normalColor = isActive ? selectedTabColor : normalTabColor;
		colors.selectedColor = isActive ? selectedTabColor : normalTabColor;
		btn.colors = colors;

		// Scale Deðiþimi
		Vector3 targetScale = isActive ? Vector3.one * selectedScaleAmount : Vector3.one;
		btn.transform.localScale = targetScale;
	}

	// --- PREVIEW LOGIC ---

	private void UpdatePreviewVisuals()
	{
		SaveData data = GameManager.Instance.SaveData;

		// Karakter
		if (characterPreviewImage)
		{
			CharacterDataSO charData = GameManager.Instance.CharacterDatabase.data.FirstOrDefault(x => x.ID == data.equippedCharacterID);
			// CharacterDataSO içinde Sprite yoksa bu satýrý açma veya dummy sprite kullan
			if (charData != null) characterPreviewImage.sprite = charData.Icon;
			characterPreviewImage.gameObject.SetActive(true);
		}

		// Hat
		if (hatPreviewImage)
		{
			if (data.equippedHatID != 0)
			{
				CosmeticData hatData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == data.equippedHatID);
				if (hatData != null)
				{
					hatPreviewImage.sprite = hatData.sprite;
					hatPreviewImage.gameObject.SetActive(true);
				}
			}
			else hatPreviewImage.gameObject.SetActive(false);
		}

		// Mask
		if (maskPreviewImage)
		{
			if (data.equippedMaskID != 0)
			{
				CosmeticData maskData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == data.equippedMaskID);
				if (maskData != null)
				{
					maskPreviewImage.sprite = maskData.sprite;
					maskPreviewImage.gameObject.SetActive(true);
				}
			}
			else maskPreviewImage.gameObject.SetActive(false);
		}
	}

	// --- SELECTION LOGIC ---

	public void SelectElement(CosmeticMenuElement element)
	{
		_selectedElement = element;
		foreach (var e in _spawnedElements) e.SetSelected(e == element);
		UpdateInfoPanel();
	}

	private void UpdateInfoPanel()
	{
		if (_selectedElement == null) return;

		elementInfoPanel.SetActive(true);
		elementNameText.text = _selectedElement.Name;
		elementDescText.text = _selectedElement.Description;

		int playerGold = GameManager.Instance.SaveData.gold;
		bool canAfford = playerGold >= _selectedElement.Price;

		elementBuyButton.interactable = true;

		if (_selectedElement.IsEquipped)
		{
			elementPriceText.text = "Owned";
			if (_selectedElement.IsCharacter)
			{
				elementBuyButtonText.text = "Equipped";
				elementBuyButton.interactable = false;
			}
			else
			{
				elementBuyButtonText.text = "Unequip";
			}
		}
		else if (_selectedElement.IsOwned)
		{
			elementPriceText.text = "Owned";
			elementBuyButtonText.text = "Equip";
		}
		else
		{
			elementPriceText.text = $"{_selectedElement.Price} Gold";
			elementPriceText.color = canAfford ? Color.white : Color.red;
			elementBuyButtonText.text = "Buy";
			elementBuyButton.interactable = canAfford;
		}
	}

	// --- TRANSACTION LOGIC ---

	private void OnActionPressed()
	{
		if (_selectedElement == null) return;

		if (_selectedElement.IsEquipped)
		{
			if (!_selectedElement.IsCharacter) UnequipItem(_selectedElement);
			return;
		}

		if (_selectedElement.IsOwned)
		{
			EquipItem(_selectedElement);
		}
		else
		{
			BuyItem(_selectedElement);
		}
	}

	private void BuyItem(CosmeticMenuElement element)
	{
		SaveData data = GameManager.Instance.SaveData;

		if (data.gold >= element.Price)
		{
			data.gold -= element.Price;

			if (element.ID > 0)
			{
				if (element.IsCharacter)
				{
					List<int> owned = data.purchasedCharacterIDs.ToList();
					if (!owned.Contains(element.ID))
					{
						owned.Add(element.ID);
						data.purchasedCharacterIDs = owned.ToArray();
					}
				}
				else
				{
					List<int> owned = data.purchasedCosmeticIDs.ToList();
					if (!owned.Contains(element.ID))
					{
						owned.Add(element.ID);
						data.purchasedCosmeticIDs = owned.ToArray();
					}
				}
			}

			GameManager.Instance.SaveGame();
			UpdateGoldUI();
			RefreshAllElements();
			UpdateInfoPanel();
		}
	}

	private void EquipItem(CosmeticMenuElement element)
	{
		SaveData data = GameManager.Instance.SaveData;

		if (element.IsCharacter)
		{
			data.equippedCharacterID = element.ID;
		}
		else
		{
			// Veritabanýndan tipi bul
			CosmeticData cosData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == element.ID);
			if (cosData != null)
			{
				if (cosData.type == CosmeticType.Hat) data.equippedHatID = element.ID;
				else if (cosData.type == CosmeticType.Mask) data.equippedMaskID = element.ID;
			}
		}

		GameManager.Instance.SaveGame();
		RefreshAllElements();
		UpdatePreviewVisuals();
		UpdateInfoPanel();
	}

	private void UnequipItem(CosmeticMenuElement element)
	{
		SaveData data = GameManager.Instance.SaveData;
		CosmeticData cosData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == element.ID);

		if (cosData != null)
		{
			if (cosData.type == CosmeticType.Hat) data.equippedHatID = 0;
			else if (cosData.type == CosmeticType.Mask) data.equippedMaskID = 0;

			GameManager.Instance.SaveGame();
			RefreshAllElements();
			UpdatePreviewVisuals();
			UpdateInfoPanel();
		}
	}

	private void RefreshAllElements()
	{
		foreach (var e in _spawnedElements) e.RefreshState();
	}
}