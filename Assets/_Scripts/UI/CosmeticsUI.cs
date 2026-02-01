using UnityEngine;
using UnityEngine.UI;

public class CosmeticsUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playButton;


	private void Awake()
	{
		InitButtons();
	}
	private void InitButtons()
	{
		mainMenuButton.onClick.RemoveAllListeners();
		playButton.onClick.RemoveAllListeners();

		mainMenuButton.onClick.AddListener(() => GameManager.Instance.MainMenu());
		playButton.onClick.AddListener(() => GameManager.Instance.Play());
	}
}
