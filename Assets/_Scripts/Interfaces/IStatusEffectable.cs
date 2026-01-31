using Enums; // EffectType enum'ını görebilmesi için gerekli

public interface IStatusEffectable
{
	// PerkManager bu fonksiyonu düşmanlar üzerinde çağıracak
	void ApplyStatus(EffectType type, float duration, float potency);
}