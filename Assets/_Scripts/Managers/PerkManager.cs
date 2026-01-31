using System.Collections.Generic;
using System.Linq; // List filtreleme için gerekli
using UnityEngine;
using Enums; // Enumlarýn olduðu namespace

public class PerkManager : MonoBehaviour
{
	public static PerkManager I;

	[Header("AttackTypePerks Logic")]
	[SerializeField] private int thresholdLevelForAttackTypePerks = 4;
	[SerializeField] private int maxLevelForAttackTypePerks = 8;
	private bool choosedAttackType = false;

	[Header("UI References")]
	[SerializeField] private PerkCard perk1;
	[SerializeField] private PerkCard perk2;
	[SerializeField] private PerkCard perk3;

	// --- YENÝ: DATABASE & ENVANTER ---
	private List<PerkBase> _allPerksDatabase; // Tüm oyunun perk havuzu
	private List<PerkBase> _currentOfferedPerks = new List<PerkBase>(); // Ekranda duran 3 perk

	// --- YENÝ: RUNTIME EFFECT LÝSTELERÝ ---
	// Efekt perklerini kategorize edip saklýyoruz
	private List<EffectPerk> _onHitPerks = new List<EffectPerk>();
	private List<EffectPerk> _onKillPerks = new List<EffectPerk>();
	private List<EffectPerk> _onTakeDamagePerks = new List<EffectPerk>();

	// Cooldown takibi (Perk ID -> Ne zaman hazýr olacaðý)
	private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();
	private bool _isMaskActive;

	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;
		// Resources/Perks klasöründeki tüm perkleri yükle
		_allPerksDatabase = Resources.LoadAll<PerkBase>("Perks").ToList();
	}

	private void OnEnable()
	{
		PlayerController.onLevelChanged += ShowPerks;

		// Runtime Event Baðlantýlarý
		EventManager.OnEnemyKilled += HandleEnemyKilled;
		EventManager.OnMaskChanged += (status) => _isMaskActive = status;
	}

	private void OnDisable()
	{
		PlayerController.onLevelChanged -= ShowPerks;

		EventManager.OnEnemyKilled -= HandleEnemyKilled;
		EventManager.OnMaskChanged -= (status) => _isMaskActive = status;
	}

	// --- MEVCUT FONKSÝYONUN (ÝÇÝ DOLDURULDU) ---
	private void ShowPerks(int currentLevel)
	{
		GameManager.Instance.StartPerkSelect();
		_currentOfferedPerks.Clear();

		if (currentLevel == 2)
		{
			// Level 2: Sadece Silah Kategorisi Seçimi (Attack Type)
			// AbilityPerk türünde ve "UnlockWeapon" özelliði olanlarý getir
			var weaponPerks = _allPerksDatabase.OfType<AbilityPerk>()
											   .Where(p => p.WeaponToUnlock != null)
											   .Cast<PerkBase>().ToList();

			FillCards(GetRandomPerks(weaponPerks, 3));
		}
		else if (currentLevel >= thresholdLevelForAttackTypePerks && currentLevel <= maxLevelForAttackTypePerks && !choosedAttackType)
		{
			// Olasýlýk Hesabý (Senin kodun)
			float range = maxLevelForAttackTypePerks - thresholdLevelForAttackTypePerks;
			float progress = currentLevel - thresholdLevelForAttackTypePerks;
			float probability = (progress + 1) / (range + 1);

			if (Random.value <= probability)
			{
				choosedAttackType = true;
				// BURASI ÖNEMLÝ: Alt kategori (Style) açan perkleri filtrele
				// Örn: AbilityPerk olup WeaponStyle deðiþtirenler
				var stylePerks = _allPerksDatabase.OfType<AbilityPerk>()
												  .Where(p => !string.IsNullOrEmpty(p.EventID) && p.EventID.Contains("UnlockStyle"))
												  .Cast<PerkBase>().ToList();

				FillCards(GetRandomPerks(stylePerks, 3));
			}
			else
			{
				// Standart daðýtým
				FillCards(GetBalancedRandomPerks());
			}
		}
		else
		{
			// Standart daðýtým (Max 1 weapon perk kuralý ile)
			FillCards(GetBalancedRandomPerks());
		}
	}

	// --- MEVCUT FONKSÝYONUN (LOGIC EKLENDÝ) ---
	// PerkCard butonuna týklandýðýnda Unity Event'ten int index (0, 1, 2) gönderilmeli
	public void OnClickPerk(int perkIndex)
	{
		if (perkIndex >= _currentOfferedPerks.Count) return;

		PerkBase selectedPerk = _currentOfferedPerks[perkIndex];

		// 1. Perki Uygula (Inventory'ye ekle, Statlarý iþle)
		EquipPerk(selectedPerk);

		// 2. Oyunu Devam Ettir
		GameManager.Instance.StopPerkSelect();
	}

	// ========================================================================
	// YENÝ: YARDIMCI SEÇÝM FONKSÝYONLARI (LOGIC)
	// ========================================================================

	private List<PerkBase> GetBalancedRandomPerks()
	{
		List<PerkBase> candidates = new List<PerkBase>(_allPerksDatabase);
		List<PerkBase> selected = new List<PerkBase>();

		// Kural: Max 1 Weapon Perk
		bool weaponPerkAdded = false;

		for (int i = 0; i < 3; i++)
		{
			if (candidates.Count == 0) break;

			PerkBase pick = candidates[Random.Range(0, candidates.Count)];

			// Eðer silah perki geldiyse ve zaten bir tane seçtiysek, bunu atla
			if (pick is AbilityPerk ab && ab.WeaponToUnlock != null)
			{
				if (weaponPerkAdded)
				{
					i--; // Hakký yakma, tekrar dene
					candidates.Remove(pick); // Listeden çýkar
					continue;
				}
				weaponPerkAdded = true;
			}

			selected.Add(pick);
			candidates.Remove(pick); // Ayný perki 2 kere koyma
		}
		return selected;
	}

	private List<PerkBase> GetRandomPerks(List<PerkBase> pool, int count)
	{
		List<PerkBase> result = new List<PerkBase>();
		List<PerkBase> tempPool = new List<PerkBase>(pool);

		for (int i = 0; i < count; i++)
		{
			if (tempPool.Count == 0) break;
			var pick = tempPool[Random.Range(0, tempPool.Count)];
			result.Add(pick);
			tempPool.Remove(pick);
		}
		return result;
	}

	private void FillCards(List<PerkBase> perks)
	{
		_currentOfferedPerks = perks;

		// PerkCard scriptinde "Setup(PerkBase data)" metodu olmalý
		if (perks.Count > 0) perk1.Setup(perks[0], 0); else perk1.gameObject.SetActive(false);
		if (perks.Count > 1) perk2.Setup(perks[1], 1); else perk2.gameObject.SetActive(false);
		if (perks.Count > 2) perk3.Setup(perks[2], 2); else perk3.gameObject.SetActive(false);
	}

	// ========================================================================
	// YENÝ: PERK UYGULAMA VE RUNTIME LOGIC (BRAIN)
	// ========================================================================

	public void EquipPerk(PerkBase perk)
	{
		// 1. Ýsim Deðiþikliði: ApplyPerk -> OnEquip
		perk.OnEquip(PlayerController.I.gameObject);

		// 2. Effect Perk ise listelere kaydet (Runtime dinleme için)
		if (perk is EffectPerk effectPerk)
		{
			RegisterEffectPerk(effectPerk);
		}

		Debug.Log($"Perk Seçildi: {perk.PerkName}");
	}

	public void RegisterEffectPerk(EffectPerk perk)
	{
		switch (perk.Trigger)
		{
			case TriggerType.OnHit: _onHitPerks.Add(perk); break;
			case TriggerType.OnKill: _onKillPerks.Add(perk); break;
			case TriggerType.OnTakeDamage: _onTakeDamagePerks.Add(perk); break;
		}
	}

	// --- SÝLAHLAR BU FONKSÝYONU ÇAÐIRACAK ---
	public void TriggerOnHit(IDamageable target, Vector3 hitPos)
	{
		ProcessPerkList(_onHitPerks, target, hitPos);
	}

	public void TriggerOnTakeDamage(float amount)
	{
		ProcessPerkList(_onTakeDamagePerks, null, transform.position);
	}

	private void HandleEnemyKilled(EnemyBase enemy)
	{
		ProcessPerkList(_onKillPerks, enemy, enemy.transform.position);
	}

	private void ProcessPerkList(List<EffectPerk> perks, IDamageable target, Vector3 pos)
	{
		foreach (var perk in perks)
		{
			// Þart Kontrolleri
			if (perk.RequiresMaskOn && !_isMaskActive) continue;
			if (perk.RequiresMaskOff && _isMaskActive) continue;

			// Cooldown Kontrolü
			if (perk.Cooldown > 0)
			{
				if (_cooldowns.ContainsKey(perk.PerkName) && Time.time < _cooldowns[perk.PerkName]) continue;
				_cooldowns[perk.PerkName] = Time.time + perk.Cooldown;
			}

			// Þans Kontrolü
			if (Random.Range(0f, 100f) > perk.Chance) continue;

			// Efekti Uygula
			ApplyEffect(perk, target, pos);
		}
	}

	private void ApplyEffect(EffectPerk perk, IDamageable target, Vector3 pos)
	{
		// VFX
		if (perk.VFX != null) Instantiate(perk.VFX, pos, Quaternion.identity);

		switch (perk.Effect)
		{
			case EffectType.Heal:
				PlayerController.I.gameObject.GetComponent<PlayerHealthController>().Heal(perk.Value);
				break;

			case EffectType.Burn:
			case EffectType.Slow:
			case EffectType.Poison:
			case EffectType.Stun:
				if (target is IStatusEffectable effectable)
				{
					effectable.ApplyStatus(perk.Effect, perk.Duration, perk.Value);
				}
				break;

			case EffectType.Explosion:
				// Basit bir alan hasarý örneði
				var hits = Physics2D.OverlapCircleAll(pos, 3f);
				foreach (var hit in hits) if (hit.CompareTag("Enemy"))
						hit.GetComponent<IDamageable>()?.TakeDamage(perk.Value, false, Vector2.zero, 0);
				break;
		}
	}
}