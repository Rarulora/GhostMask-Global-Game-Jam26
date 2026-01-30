using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
	public static ProjectilePoolManager Instance { get; private set; }

	// Prefab Adı -> Mermi Kuyruğu (Queue) eşleşmesi
	// Bu sayede hem "Arrow" hem "Fireball" için ayrı havuzlar tutabiliriz.
	private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

	private void Awake()
	{
		// Singleton
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		// Prefabın adını anahtar (Key) olarak kullanıyoruz
		string key = prefab.name;

		// 1. Bu anahtarda bir havuz yoksa oluştur
		if (!poolDictionary.ContainsKey(key))
		{
			poolDictionary.Add(key, new Queue<GameObject>());
		}

		// 2. Havuzda kullanılabilir obje var mı?
		GameObject objToSpawn;

		if (poolDictionary[key].Count > 0)
		{
			// Var: Kuyruktan çek
			objToSpawn = poolDictionary[key].Dequeue();
		}
		else
		{
			// Yok: Yeni yarat (Scalable kısmı burası)
			// Parent'ı bu obje yapıyoruz ki Hierarchy temiz dursun
			objToSpawn = Instantiate(prefab, transform);
			objToSpawn.name = key; // Adını düzelt (Clone ibaresini kaldır ki key bozulmasın)
		}

		// 3. Objeyi hazırla ve gönder
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;
		objToSpawn.SetActive(true); // OnEnable tetiklenir

		return objToSpawn;
	}

	// Mermi işi bitince buraya geri gönderilir
	public void ReturnToPool(GameObject obj)
	{
		obj.SetActive(false);
		string key = obj.name; // Adından hangi havuza ait olduğunu anlıyoruz

		if (!poolDictionary.ContainsKey(key))
		{
			poolDictionary.Add(key, new Queue<GameObject>());
		}

		poolDictionary[key].Enqueue(obj);
	}
}