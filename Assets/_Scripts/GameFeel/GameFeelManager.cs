using UnityEngine;
using System.Collections;

public class GameFeelManager : MonoBehaviour
{
	public static GameFeelManager I;
	private bool isWaiting = false;

	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;
	}

	public void DoHitStop(float duration)
	{
		if (isWaiting) return;
		StartCoroutine(HitStopRoutine(duration));
	}

	private IEnumerator HitStopRoutine(float duration)
	{
		isWaiting = true;

		// Zamaný neredeyse durdur (0 yaparsan bazen fizik sapýtabilir, 0.1 güvenlidir)
		// Ama tam donma istiyorsan 0 yap.
		float originalScale = Time.timeScale;
		Time.timeScale = 0.0f;

		// WaitForSecondsRealtime kullanmalýsýn çünkü Time.timeScale 0 iken normal süre akmaz.
		yield return new WaitForSecondsRealtime(duration);

		Time.timeScale = originalScale;
		isWaiting = false;
	}
}