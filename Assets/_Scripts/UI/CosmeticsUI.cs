using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticsUI : MonoBehaviour
{
	[SerializeField] private GameObject cosmeticMenuElementPrefab;
	[SerializeField] private Transform cosmeticMenuRoot;

	[Header("Buttons")]
	[SerializeField] private Button mainMenuButton;
	[SerializeField] private Button playButton;

	[Header("Info Panel")]
	[SerializeField] private GameObject elementInfoPanel;
	[SerializeField] private TMP_Text elementNameText;
	[SerializeField] private TMP_Text elementDescText;
	[SerializeField] private TMP_Text elementPriceText;
	[SerializeField] private Button elementBuyButton;
	[SerializeField] private TMP_Text elementBuyButtonText; // Reference to the text inside the button
	[SerializeField] private TMP_Text playerGoldText; // To show current gold

	private List<CosmeticMenuElement> _spawnedElements = new List<CosmeticMenuElement>();
	private CosmeticMenuElement _selectedElement;

	private void Awake()
	{
		InitButtons();
		InitMenuElements();
		UpdateGoldUI();

		// Hide info panel at start
		if (elementInfoPanel) elementInfoPanel.SetActive(false);
	}

	private void InitButtons()
	{
		mainMenuButton.onClick.RemoveAllListeners();
		playButton.onClick.RemoveAllListeners();
		elementBuyButton.onClick.RemoveAllListeners();

		mainMenuButton.onClick.AddListener(() => GameManager.Instance.MainMenu());
		playButton.onClick.AddListener(() => GameManager.Instance.Play());
		elementBuyButton.onClick.AddListener(OnActionPressed);
	}

	private void InitMenuElements()
	{
		// Clear existing if any (for safety)
		foreach (Transform child in cosmeticMenuRoot) Destroy(child.gameObject);
		_spawnedElements.Clear();

		// Spawn Characters
		foreach (var ch in GameManager.Instance.CharacterDatabase.data)
		{
			var e = Instantiate(cosmeticMenuElementPrefab, cosmeticMenuRoot).GetComponent<CosmeticMenuElement>();
			e.Setup(this, ch);
			_spawnedElements.Add(e);
		}

		// Spawn Cosmetics
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

	// --- Selection Logic ---

	public void SelectElement(CosmeticMenuElement element)
	{
		_selectedElement = element;

		// Visual feedback on list items
		foreach (var e in _spawnedElements)
		{
			e.SetSelected(e == element);
		}

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

		// Logic for the Action Button
		elementBuyButton.interactable = true;

		if (_selectedElement.IsEquipped)
		{
			elementPriceText.text = "Owned";
			elementBuyButtonText.text = "Equipped";
			elementBuyButton.interactable = false;
		}
		else if (_selectedElement.IsOwned)
		{
			elementPriceText.text = "Owned";
			elementBuyButtonText.text = "Equip";
		}
		else
		{
			// Not Owned
			elementPriceText.text = $"{_selectedElement.Price} Gold";
			elementPriceText.color = canAfford ? Color.white : Color.red;
			elementBuyButtonText.text = "Buy";
			elementBuyButton.interactable = canAfford;
		}
	}

	// --- Transaction Logic ---

	private void OnActionPressed()
	{
		if (_selectedElement == null) return;

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
			// Deduct Gold
			data.gold -= element.Price;

			// Add to Inventory
			if (element.ID > 0) // Never buy ID 0
			{
				// We determine type by checking the element's internal data source in a cleaner way, 
				// but since we abstracted it, we check the database source via the element's Setup type.
				// However, CosmeticMenuElement knows if it is a character.

				// Since the Element logic is encapsulated, let's verify simply:
				// We need to know if it's a character or cosmetic to add to the right list.
				// We can check if it exists in the Cosmetic Database. 
				// If yes -> Cosmetic. If no -> Character.

				bool isCosmetic = GameManager.Instance.CosmeticDatabase.data.Any(x => x.ID == element.ID);

				if (isCosmetic)
				{
					List<int> owned = data.purchasedCosmeticIDs.ToList();
					if (!owned.Contains(element.ID))
					{
						owned.Add(element.ID);
						data.purchasedCosmeticIDs = owned.ToArray();
					}
				}
				else // Is Character
				{
					List<int> owned = data.purchasedCharacterIDs.ToList();
					if (!owned.Contains(element.ID))
					{
						owned.Add(element.ID);
						data.purchasedCharacterIDs = owned.ToArray();
					}
				}
			}

			// Save
			GameManager.Instance.SaveGame();

			// Refresh UI
			UpdateGoldUI();
			RefreshAllElements();

			// Auto Equip on Buy? Optional. Let's just update panel.
			UpdateInfoPanel();
		}
	}

	private void EquipItem(CosmeticMenuElement element)
	{
		SaveData data = GameManager.Instance.SaveData;

		// Determine type again to set the correct ID
		// Check Cosmetic DB first
		CosmeticData cosData = GameManager.Instance.CosmeticDatabase.data.FirstOrDefault(x => x.ID == element.ID);

		if (cosData != null)
		{
			if (cosData.type == CosmeticType.Hat)
			{
				data.equippedHatID = element.ID;
			}
			// Add other cosmetic types here
		}
		else
		{
			// Must be character
			data.equippedCharacterID = element.ID;
		}

		GameManager.Instance.SaveGame();
		RefreshAllElements();
		UpdateInfoPanel();
	}

	private void RefreshAllElements()
	{
		foreach (var e in _spawnedElements)
		{
			e.RefreshState();
		}
	}
}