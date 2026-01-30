using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enums;

[Serializable]
public class PlayerStat
{
	public float BaseValue; // Başlangıç değeri (Örn: 10)

	protected bool isDirty = true; // Değer değişti mi kontrolü
	protected float _value; // Hesaplanmış son değer (Cache)

	// Modifier listesi
	protected readonly List<StatModifier> statModifiers;
	public readonly ReadOnlyCollection<StatModifier> StatModifiers;

	public PlayerStat()
	{
		statModifiers = new List<StatModifier>();
		StatModifiers = statModifiers.AsReadOnly();
	}

	public PlayerStat(float baseValue) : this()
	{
		BaseValue = baseValue;
	}

	// Dışarıdan değeri okurken bu çağrılır
	public float Value
	{
		get
		{
			if (isDirty || BaseValue != lastBaseValue)
			{
				lastBaseValue = BaseValue;
				_value = CalculateFinalValue();
				isDirty = false;
			}
			return _value;
		}
	}

	protected float lastBaseValue = float.MinValue;

	public virtual void AddModifier(StatModifier mod)
	{
		isDirty = true;
		statModifiers.Add(mod);
		statModifiers.Sort(CompareModifierOrder); // İşlem sırasına göre diz
	}

	public virtual bool RemoveModifier(StatModifier mod)
	{
		if (statModifiers.Remove(mod))
		{
			isDirty = true;
			return true;
		}
		return false;
	}

	// Belirli bir kaynaktan gelen tüm bonusları sil (Örn: Kılıcı çıkarınca)
	public virtual bool RemoveAllModifiersFromSource(object source)
	{
		bool didRemove = false;

		for (int i = statModifiers.Count - 1; i >= 0; i--)
		{
			if (statModifiers[i].Source == source)
			{
				isDirty = true;
				didRemove = true;
				statModifiers.RemoveAt(i);
			}
		}
		return didRemove;
	}

	protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
	{
		if (a.Order < b.Order) return -1;
		else if (a.Order > b.Order) return 1;
		return 0;
	}

	// Matematiksel Hesaplama
	protected virtual float CalculateFinalValue()
	{
		float finalValue = BaseValue;
		float sumPercentAdd = 0;

		for (int i = 0; i < statModifiers.Count; i++)
		{
			StatModifier mod = statModifiers[i];

			if (mod.Type == StatModType.Flat)
			{
				finalValue += mod.Value;
			}
			else if (mod.Type == StatModType.PercentAdd)
			{
				sumPercentAdd += mod.Value;

				// Eğer listenin sonuna geldiysek veya bir sonraki modifier farklı tipteyse
				if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
				{
					finalValue *= 1 + sumPercentAdd;
					sumPercentAdd = 0;
				}
			}
			else if (mod.Type == StatModType.PercentMult)
			{
				finalValue *= mod.Value;
			}
		}

		// Rogue-lite oyunlarda küsuratlarla (10.00002 gibi) uğraşmamak için yuvarlama
		return (float)Math.Round(finalValue, 4);
	}
}