using System.Collections.Generic;
using UnityEngine;
using Enums;

public class StatsController : MonoBehaviour
{
	public static StatsController I;
	// Inspector'dan ayar yapmak için yardımcı sınıf
	[System.Serializable]
	public struct StatInitializer
	{
		public StatType Type;
		public float BaseValue;
	}

	[Header("Base Stats Configuration")]
	[SerializeField] private List<StatInitializer> startingStats;

	// Asıl Stat Veritabanımız (Dictionary ile O(1) erişim)
	private Dictionary<StatType, PlayerStat> _stats;
	private List<TimedStatModifier> _timedModifiers = new List<TimedStatModifier>();
	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;

		InitializeStats();
	}
	private void Update()
	{
		for (int i = _timedModifiers.Count - 1; i >= 0; i--)
		{
			var timedMod = _timedModifiers[i];

			// Süreyi azalt
			timedMod.Timer -= Time.deltaTime;

			// Süre bitti mi?
			if (timedMod.Timer <= 0)
			{
				// 1. Asıl stattan modifier'ı kaldır
				GetStat(timedMod.TargetStat).RemoveModifier(timedMod.Modifier);

				// 2. Listeden takipçiyi sil
				_timedModifiers.RemoveAt(i);
			}
		}
	}
	private void InitializeStats()
	{
		_stats = new Dictionary<StatType, PlayerStat>();

		// 1. Önce Dictionary'i Enumdaki her şeyle doldur (Boş kalmasın)
		foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
		{
			_stats.Add(type, new PlayerStat(0)); // Varsayılan 0
		}

		// 2. Inspector'dan girilen değerleri ata
		foreach (var init in startingStats)
		{
			if (_stats.ContainsKey(init.Type))
			{
				_stats[init.Type].BaseValue = init.BaseValue;
			}
		}
	}

	public void AddTimedModifier(StatType statType, float value, StatModType modType, float duration, string effectID = "")
	{
		// ÖNCEKİ VAR MI KONTROL ET (Opsiyonel - Stacklenmesini istemiyorsan)
		// Eğer ID verilmişse ve aynısı listede varsa, sadece süresini yenile
		if (!string.IsNullOrEmpty(effectID))
		{
			var existingMod = _timedModifiers.Find(x => x.ID == effectID);
			if (existingMod != null)
			{
				existingMod.ResetTimer(); // Süreyi başa sar (Refresh Duration)
				return;
			}
		}

		// Yeni modifier oluştur
		StatModifier mod = new StatModifier(value, modType, this);

		// Stat'a ekle (Anında etki eder)
		GetStat(statType).AddModifier(mod);

		// Takip listesine ekle (Süresi bitince silinsin diye)
		_timedModifiers.Add(new TimedStatModifier(effectID, duration, mod, statType));
	}

	// Stat Getirme Metodu
	public PlayerStat GetStat(StatType type)
	{
		if (_stats.ContainsKey(type))
			return _stats[type];

		Debug.LogError($"Stat {type} bulunamadı! Dictionary başlatılmamış olabilir.");
		return null;
	}

	// Hızlı Erişim Yardımcısı (Opsiyonel ama kullanışlı)
	public float GetValue(StatType type)
	{
		return GetStat(type).Value;
	}
}