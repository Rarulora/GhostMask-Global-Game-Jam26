using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
	public static SceneTransitionManager Instance;

	[Header("Settings")]
	[Tooltip("Animasyonun oynama süresi (saniye)")]
	[SerializeField] private float transitionDuration = 1f;

	[Tooltip("Unity'de UI objesine verdiðin Tag ismi")]
	[SerializeField] private string transitionTag = "SceneTransition";

	// Yönetilecek animatörlerin listesi
	private List<Animator> _transitionAnimators = new List<Animator>();

	

	private void Awake()
	{
		// --- SINGLETON & HATA DÜZELTMESÝ ---
		// Eðer sahnede zaten bir Manager varsa, kendini yok et.
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 1. Yeni sahnedeki doðru animatörleri bul
		RefreshAnimators();

		// 2. Animasyonlarý "AÇIK" (Open = true) hale getir (Siyah ekran kalksýn)
		foreach (var anim in _transitionAnimators)
		{
			if (anim != null)
				anim.SetBool("Open", true);
		}
	}

	private void RefreshAnimators()
	{
		_transitionAnimators.Clear();

		// Sahnedeki TÜM animatörleri bul
		var allAnimators = FindObjectsByType<Animator>(FindObjectsSortMode.None);

		// Sadece Tag'i "SceneTransition" olanlarý listeye al
		foreach (var anim in allAnimators)
		{
			if (anim.CompareTag(transitionTag))
			{
				_transitionAnimators.Add(anim);
			}
		}
	}

	// Dýþarýdan çaðýracaðýn fonksiyon
	public void OpenScene(string sceneName)
	{
		StartCoroutine(LoadSceneRoutine(sceneName));
	}

	// --- COROUTINE ÝLE GEÇÝÞ ---
	private IEnumerator LoadSceneRoutine(string sceneName)
	{
		// 1. Kapanýþ animasyonunu tetikle (Open = false -> Ekran kararsýn)
		foreach (var anim in _transitionAnimators)
		{
			if (anim != null)
				anim.SetBool("Open", false);
		}

		// 2. Animasyonun bitmesi için bekle! (Kritik nokta burasý)
		yield return new WaitForSeconds(transitionDuration);

		// 3. Bekleme bitti, þimdi sahneyi yükle
		SceneManager.LoadScene(sceneName);
	}
}