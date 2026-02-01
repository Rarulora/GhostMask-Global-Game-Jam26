using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))] // Bu scriptin olduğu objede Kamera olmak zorunda
public class CameraController : MonoBehaviour
{
	public static CameraController Instance;

	[Header("Target Settings")]
	[SerializeField] private Transform target;
	[SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
	[SerializeField] private float smoothTime = 0.25f;

	[Header("Map Boundaries (Absolute Edges)")]
	[Tooltip("Haritanın mutlak sol-alt köşesi")]
	[SerializeField] private bool useLimits = true;
	[SerializeField] private Vector2 minPosition;
	[Tooltip("Haritanın mutlak sağ-üst köşesi")]
	[SerializeField] private Vector2 maxPosition;

	private Vector3 _velocity = Vector3.zero;
	private Vector3 _currentShakeOffset = Vector3.zero;
	private Coroutine _shakeCoroutine;
	private Camera _cam;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);

		_cam = GetComponent<Camera>();
	}
	private void OnEnable()
	{
		EventManager.OnPlayerDeath += () => StopAllCoroutines();
	}
	private void OnDisable()
	{
		EventManager.OnPlayerDeath -= () => StopAllCoroutines();
	}
	private void Start()
	{
		if (target == null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null) target = player.transform;
		}
	}

	private void LateUpdate()
	{
		if (target == null) return;

		// 1. Hedef Pozisyon
		Vector3 targetPosition = target.position + offset;

		// 2. Sınırlandırma (GÖRÜŞ ALANI DAHİL)
		if (useLimits)
		{
			// Kameranın dikey yarım boyutu (Orthographic Size)
			float vertExtent = _cam.orthographicSize;
			// Kameranın yatay yarım boyutu (Yükseklik * En/Boy oranı)
			float horzExtent = vertExtent * Screen.width / Screen.height;

			// Kameranın MERKEZİNİN gidebileceği sınırları hesapla:
			// Sol sınır = Harita Solu + Kamera Yarım Genişliği
			// Sağ sınır = Harita Sağı - Kamera Yarım Genişliği
			float minX = minPosition.x + horzExtent;
			float maxX = maxPosition.x - horzExtent;
			float minY = minPosition.y + vertExtent;
			float maxY = maxPosition.y - vertExtent;

			// Harita kameradan küçükse (Zoom out yapınca) titremeyi önlemek için kontrol
			if (minX > maxX) minX = maxX = (minPosition.x + maxPosition.x) / 2;
			if (minY > maxY) minY = maxY = (minPosition.y + maxPosition.y) / 2;

			// Clamp işlemini yeni hesaplanan sınırlara göre yap
			float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
			float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

			targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);
		}

		// 3. Smooth Hareket
		Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);

		// 4. Shake Uygula
		transform.position = smoothedPosition + _currentShakeOffset;
	}

	public void ShakeCamera(float duration, float magnitude)
	{
		if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
		_shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
	}

	private IEnumerator ShakeRoutine(float duration, float magnitude)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			float x = Random.Range(-1f, 1f) * magnitude;
			float y = Random.Range(-1f, 1f) * magnitude;
			_currentShakeOffset = new Vector3(x, y, 0);
			elapsed += Time.deltaTime;
			yield return null;
		}
		_currentShakeOffset = Vector3.zero;
	}

	private void OnDrawGizmos()
	{
		if (!useLimits) return;

		// Haritanın gerçek sınırlarını Yeşil kutu ile çiz
		Gizmos.color = Color.green;
		Vector3 center = new Vector3((minPosition.x + maxPosition.x) / 2, (minPosition.y + maxPosition.y) / 2, 0);
		Vector3 size = new Vector3(maxPosition.x - minPosition.x, maxPosition.y - minPosition.y, 1);
		Gizmos.DrawWireCube(center, size);
	}
}