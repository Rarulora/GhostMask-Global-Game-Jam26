using Enums;

[System.Serializable]
public class TimedStatModifier
{
	public string ID;             // Efektin adı (Örn: "Adrenaline") - Üst üste binmeyi önlemek için
	public float Duration;        // Toplam süre
	public float Timer;           // Kalan süre
	public StatModifier Modifier; // Stat sistemine eklediğimiz asıl modifier
	public StatType TargetStat;   // Hangi statı etkiliyor?

	public TimedStatModifier(string id, float duration, StatModifier modifier, StatType targetStat)
	{
		ID = id;
		Duration = duration;
		Timer = duration;
		Modifier = modifier;
		TargetStat = targetStat;
	}

	public void ResetTimer()
	{
		Timer = Duration;
	}
}