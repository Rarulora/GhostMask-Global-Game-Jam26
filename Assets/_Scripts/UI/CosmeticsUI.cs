using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Enums;

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
	[SerializeField] private Color selectedTabColor = new Color(0.6f, 1f, 0.6f);
	[SerializeField] private float selectedScaleAmount = 1.15f;
	[SerializeField] private float animationSpeed = 10f;

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

	[Header("Sepet")]
	[SerializeField] private Animator sepetAnim;

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

		// Baþlangýçta kayýtlý olanlarý yükle (Reset)
		ResetPreviewToSaveData();

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

		foreach (var ch in GameManager.Instance.CharacterDatabase.data)
		{
			var e = Instantiate(cosmeticMenuElementPrefab, cosmeticMenuRoot).GetComponent<CosmeticMenuElement>();
			e.Setup(this, ch);
			_spawnedElements.Add(e);
		}

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
		// Kategori deðiþince önizlemeyi sýfýrlayalým ki karýþýklýk olmasýn
		// (Ýsteðe baðlý: istersen sýfýrlamayabilirsin ama kafa karýþtýrabilir)
		ResetPreviewToSaveData();

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
		_currentFilter = type;

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

		UpdateTabVisuals();
	}

	private void UpdateTabVisuals()
	{
		SetTabState(characterTabButton, _currentFilter == FilterType.Character);
		SetTabState(hatTabButton, _currentFilter == FilterType.Hat);
		SetTabState(maskTabButton, _currentFilter == FilterType.Mask);
	}

	private void SetTabState(Button btn, bool isActive)
	{
		if (btn == null) return;

		var colors = btn.colors;
		colors.normalColor = isActive ? selectedTabColor : normalTabColor;
		colors.selectedColor = isActive ? selectedTabColor : normalTabColor;
		btn.colors = colors;

		Vector3 targetScale = isActive ? Vector3.one * selectedScaleAmount : Vector3.one;
		btn.transform.localScale = targetScale;
	}

	// --- PREVIEW LOGIC (GÜNCELLENDÝ) ---

	// Bu fonksiyon ARTIK SADECE SAVE DATA'YI YÜKLER (Resetlemek için)
	private void ResetPreviewToSaveData()
	{
		SaveData data = GameManager.Instance.SaveData;

		// 1. Karakter
		if (characterPreviewImage)
		{
			CharacterDataSO charData = GameManager.Instance.CharacterDatabase.data.FirstOrDefault(x => x.ID == data.equippedCharacterID);
			if (charData != null) characterPreviewImage.sprite = charData.Sprite;
			characterPreviewImage.gameObject.SetActive(true);
		}

		// 2. Maske
		if (maskPreviewImage)
		{
			CosmeticData maskData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == data.equippedMaskID);
			if (maskData != null)
			{
				maskPreviewImage.sprite = maskData.sprite;
				maskPreviewImage.gameObject.SetActive(true);
			}
			else
			{
				maskPreviewImage.gameObject.SetActive(false);
			}
		}

		// 3. Þapka
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
	}

	// --- SELECTION LOGIC ---

	public void SelectElement(CosmeticMenuElement element)
	{
		_selectedElement = element;
		foreach (var e in _spawnedElements) e.SetSelected(e == element);

		UpdateInfoPanel();

		// YENÝ: Týklandýðý an görseli önizlemeye yansýt
		ApplyTemporaryPreview(element);
	}

	// YENÝ: Geçici olarak önizleme görselini deðiþtirir
	private void ApplyTemporaryPreview(CosmeticMenuElement element)
	{
		// 1. Eðer element bir KARAKTER ise
		if (element.IsCharacter)
		{
			// Element'in ID'si ile veritabanýndan sprite'ý buluyoruz
			// (Not: CosmeticMenuElement scriptinde Sprite tutuyorsan direkt onu da kullanabilirsin)
			// Þimdilik veritabanýndan çekelim garanti olsun:
			CharacterDataSO charData = GameManager.Instance.CharacterDatabase.data.FirstOrDefault(x => x.ID == element.ID);
			if (charData != null && characterPreviewImage != null)
			{
				characterPreviewImage.sprite = charData.Sprite;
				characterPreviewImage.gameObject.SetActive(true);
			}
		}
		// 2. Eðer element bir KOZMETÝK ise
		else
		{
			CosmeticData cosData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == element.ID);
			if (cosData != null)
			{
				if (cosData.type == CosmeticType.Hat && hatPreviewImage != null)
				{
					hatPreviewImage.sprite = cosData.sprite;
					hatPreviewImage.gameObject.SetActive(true);
				}
				else if (cosData.type == CosmeticType.Mask && maskPreviewImage != null)
				{
					maskPreviewImage.sprite = cosData.sprite;
					maskPreviewImage.gameObject.SetActive(true);
				}
			}
		}
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

			if (_selectedElement.IsCharacter || _selectedElement.CosmeticType == CosmeticType.Mask)
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
			if (!_selectedElement.IsCharacter && _selectedElement.CosmeticType != CosmeticType.Mask)
			{
				UnequipItem(_selectedElement);
			}
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
			CheckAllBuyed();

			// Satýn alýnca otomatik equip etsin mi? 
			// Genelde oyuncu aldýðý þeyi hemen dener. Ýstersen buraya EquipItem(element) ekleyebilirsin.
		}
	}

	private void CheckAllBuyed()
	{
		SaveData data = GameManager.Instance.SaveData;
		int allDataCount = 0;
		int currentCount = 0;
		allDataCount += GameManager.Instance.CharacterDatabase.data.Length;
		allDataCount += GameManager.Instance.CosmeticDatabase.data.Length;

		currentCount += data.purchasedCharacterIDs.Length;
		currentCount += data.purchasedCosmeticIDs.Length;

		if (currentCount >= allDataCount)
			sepetAnim.SetTrigger("Special");
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
			if (element.CosmeticType == CosmeticType.Hat)
			{
				data.equippedHatID = element.ID;
			}
			else if (element.CosmeticType == CosmeticType.Mask)
			{
				data.equippedMaskID = element.ID;
			}
		}

		GameManager.Instance.SaveGame();
		RefreshAllElements();
		// ResetPreviewToSaveData(); // Buna gerek yok çünkü zaten seçili olaný gösteriyoruz
		UpdateInfoPanel();
	}

	private void UnequipItem(CosmeticMenuElement element)
	{
		SaveData data = GameManager.Instance.SaveData;

		if (element.CosmeticType == CosmeticType.Hat)
		{
			data.equippedHatID = 0;

			GameManager.Instance.SaveGame();
			RefreshAllElements();

			// Þapka çýktýysa görseli kaldýr
			if (hatPreviewImage) hatPreviewImage.gameObject.SetActive(false);

			UpdateInfoPanel();
		}
	}

	private void RefreshAllElements()
	{
		foreach (var e in _spawnedElements) e.RefreshState();
	}
}