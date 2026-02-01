using UnityEngine;
using UnityEngine.UI;

public class GameplayBars : MonoBehaviour
{
	[System.Serializable]
	public class BarSettings
	{
		public Transform panel;
		public Slider slider;

		// State Variables
		[HideInInspector] public Vector3 origScale;
		[HideInInspector] public Quaternion origRot;
		[HideInInspector] public float currentVal;
		[HideInInspector] public float maxVal;
		[HideInInspector] public float impactTimer;
		[HideInInspector] public bool isCritical;
		[HideInInspector] public float lastValue;
	}

	[Header("Bar References")]
	[SerializeField] private BarSettings healthBar = new BarSettings();
	[SerializeField] private BarSettings madnessBar = new BarSettings();

	[Header("Threshold Settings")]
	[Range(0f, 1f)][SerializeField] private float healthCriticalPercent = 0.25f;
	[Range(0f, 1f)][SerializeField] private float madnessCriticalPercent = 0.75f;

	[Header("Shake Sensitivity")]
	[SerializeField] private float minChangeToShake = 2.0f;

	[Header("Impact Effect")]
	[SerializeField] private float impactDuration = 0.2f;
	[SerializeField] private float impactStrength = 5f;
	[SerializeField] private float impactScale = 1.2f;

	[Header("Critical Pulse")]
	[SerializeField] private float pulseSpeed = 5f;
	[SerializeField] private float pulseScale = 1.1f;

	// References
	private PlayerController player;
	private MaskController maskController;
	private PlayerHealthController healthController;

	// YENÝ: Oyunun durumunu kontrol eden bayrak
	private bool _isPlayerDead = false;

	private void Awake()
	{
		if (healthBar.panel)
		{
			healthBar.origScale = healthBar.panel.localScale;
			healthBar.origRot = healthBar.panel.localRotation;
		}
		if (madnessBar.panel)
		{
			madnessBar.origScale = madnessBar.panel.localScale;
			madnessBar.origRot = madnessBar.panel.localRotation;
		}

		GameObject p = GameObject.FindGameObjectWithTag("Player");
		if (p != null)
		{
			player = p.GetComponent<PlayerController>();
			maskController = player.GetComponent<MaskController>();
			healthController = player.GetComponent<PlayerHealthController>();
		}
	}

	private void Start()
	{
		if (healthController != null)
		{
			healthBar.slider.maxValue = healthController.MaxHealth;
			healthBar.slider.value = healthController.CurrentHealth;
			healthBar.currentVal = healthController.CurrentHealth;
			healthBar.maxVal = healthController.MaxHealth;
			healthBar.lastValue = healthController.CurrentHealth;
			CheckCriticalState(healthBar, true);
		}

		if (maskController != null)
		{
			madnessBar.slider.maxValue = maskController.MaxMadness;
			madnessBar.slider.value = maskController.CurrentMadness;
			madnessBar.currentVal = maskController.CurrentMadness;
			madnessBar.maxVal = maskController.MaxMadness;
			madnessBar.lastValue = maskController.CurrentMadness;
			CheckCriticalState(madnessBar, false);
		}
	}

	private void OnEnable()
	{
		EventManager.OnHealthChanged += OnHealthChange;
		EventManager.OnMadnessChanged += OnMadnessChange;

		// YENÝ: Ölüm olayýný dinle
		EventManager.OnPlayerDeath += OnPlayerDeath;
	}

	private void OnDisable()
	{
		EventManager.OnHealthChanged -= OnHealthChange;
		EventManager.OnMadnessChanged -= OnMadnessChange;

		// YENÝ: Abonelikten çýk
		EventManager.OnPlayerDeath -= OnPlayerDeath;
	}

	// --- YENÝ: ÖLÜM MANTIÐI ---
	private void OnPlayerDeath()
	{
		_isPlayerDead = true;

		// Ölünce barlar yamuk veya büyük kalmasýn, resetle.
		ResetBarVisuals(healthBar);
		ResetBarVisuals(madnessBar);
	}

	private void ResetBarVisuals(BarSettings bar)
	{
		if (bar.panel != null)
		{
			bar.panel.localScale = bar.origScale;
			bar.panel.localRotation = bar.origRot;
		}
	}

	// --- EVENT HANDLERS ---

	private void OnHealthChange(float current, float max)
	{
		if (_isPlayerDead) return; // Ölüysen iþlem yapma

		healthBar.currentVal = current;
		healthBar.maxVal = max;
		healthBar.slider.maxValue = max;
		healthBar.slider.value = current;

		float change = healthBar.lastValue - current;
		if (change >= minChangeToShake)
		{
			healthBar.impactTimer = impactDuration;
		}

		healthBar.lastValue = current;
		CheckCriticalState(healthBar, true);
	}

	private void OnMadnessChange(float current, float max)
	{
		if (_isPlayerDead) return; // Ölüysen iþlem yapma

		madnessBar.currentVal = current;
		madnessBar.maxVal = max;
		madnessBar.slider.maxValue = max;
		madnessBar.slider.value = current;

		float change = current - madnessBar.lastValue;
		if (change >= minChangeToShake)
		{
			madnessBar.impactTimer = impactDuration;
		}

		madnessBar.lastValue = current;
		CheckCriticalState(madnessBar, false);
	}

	private void CheckCriticalState(BarSettings bar, bool isHealth)
	{
		if (isHealth)
			bar.isCritical = (bar.currentVal / bar.maxVal) <= healthCriticalPercent && bar.currentVal > 0;
		else
			bar.isCritical = (bar.currentVal / bar.maxVal) >= madnessCriticalPercent;
	}

	// --- UPDATE LOOP ---
	private void Update()
	{
		// YENÝ: Eðer oyuncu öldüyse animasyon hesaplama, buradan dön.
		if (_isPlayerDead) return;

		ProcessBarAnimation(healthBar);
		ProcessBarAnimation(madnessBar);
	}

	private void ProcessBarAnimation(BarSettings bar)
	{
		if (bar.panel == null) return;

		// 1. IMPACT
		if (bar.impactTimer > 0)
		{
			bar.impactTimer -= Time.deltaTime;
			float percent = bar.impactTimer / impactDuration;

			float currentScaleMod = Mathf.Lerp(1f, impactScale, percent);
			bar.panel.localScale = bar.origScale * currentScaleMod;

			float zRot = Random.Range(-1f, 1f) * impactStrength * percent;
			bar.panel.localRotation = Quaternion.Euler(0, 0, zRot);
		}
		// 2. CRITICAL PULSE
		else if (bar.isCritical)
		{
			bar.panel.localRotation = bar.origRot;
			float wave = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
			float scaleMod = 1f + (wave * (pulseScale - 1f));
			bar.panel.localScale = bar.origScale * scaleMod;
		}
		// 3. NORMAL
		else
		{
			bar.panel.localScale = bar.origScale;
			bar.panel.localRotation = bar.origRot;
		}
	}
}