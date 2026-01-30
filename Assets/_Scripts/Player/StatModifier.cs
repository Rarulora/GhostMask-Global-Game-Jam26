
using Enums;
[System.Serializable]
public class StatModifier
{
	public float Value;
	public StatModType Type;
	public int Order;
	public object Source;

	public StatModifier(float value, StatModType type, int order, object source)
	{
		Value = value;
		Type = type;
		Order = order;
		Source = source;
	}

	public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
	public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }
}