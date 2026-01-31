using UnityEngine;

public class InfiniteRotation : MonoBehaviour
{
	// Saniyede kaç derece dönecek? (360 = 1 saniyede tam tur)
	[SerializeField] private float rotationSpeed = 360f;

	private void Update()
	{
		// Z ekseninde (2D için) sürekli döndürür
		transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
	}
}