using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
	private static ObjectPooler instance_;

	public static ObjectPooler instance
	{
		get
		{
			if (instance_ == null)
			{
				GameObject obj = new GameObject("_ObjectPooler");
				instance_ = obj.AddComponent<ObjectPooler>();
			}
			return instance_;
		}
	}

	void Awake()
	{
		instance_ = this;
	}

	void OnDestroy()
	{
		instance_ = null;
	}

	private const int DefaultCapacity = 10;
	private const int MaxSize = 100;

	private class PoolEntry
	{
		public ObjectPool<GameObject> pool;
		public GameObject prefab;
		public Transform root;
		public Vector3 pendingPos;
		public Quaternion pendingRot;
	}

	private readonly Dictionary<string, PoolEntry> _pools = new Dictionary<string, PoolEntry>();

	// name(Addressable 키)별로 최초 요청 시 UnityEngine.Pool.ObjectPool<GameObject>를 지연 생성
	private PoolEntry GetOrCreateEntry(string name)
	{
		if (_pools.TryGetValue(name, out var entry))
			return entry;

		GameObject prefab = ResourcesManager.instance.LoadGameObject(name);
		if (prefab == null)
		{
			Debug.LogError("ObjectPooler: 에셋 로드 실패 — " + name);
			return null;
		}

		Transform root = new GameObject("Pool_" + name).transform;
		root.SetParent(transform);

		entry = new PoolEntry { prefab = prefab, root = root };

		// pendingPos/pendingRot을 createFunc/actionOnGet에서 먼저 적용해야
		// Awake/OnEnable이 (도착 전 자리가 아닌) 실제 스폰 위치를 기준으로 실행된다.
		entry.pool = new ObjectPool<GameObject>(
			createFunc: () =>
			{
				GameObject go = Instantiate(prefab, entry.pendingPos, entry.pendingRot, root);
				go.name = name;
				return go;
			},
			actionOnGet: go =>
			{
				go.transform.SetPositionAndRotation(entry.pendingPos, entry.pendingRot);
				go.SetActive(true);
			},
			actionOnRelease: go => go.SetActive(false),
			actionOnDestroy: Destroy,
			defaultCapacity: DefaultCapacity,
			maxSize: MaxSize);

		_pools[name] = entry;
		return entry;
	}

	public GameObject ObjPop(string name, Vector3 popPos, bool autoActive = true)
	{
		PoolEntry entry = GetOrCreateEntry(name);
		if (entry == null)
			return null;

		entry.pendingPos = popPos;
		entry.pendingRot = entry.prefab.transform.rotation;

		GameObject obj = entry.pool.Get();

		if (!autoActive)
			obj.SetActive(false);

		return obj;
	}

	public bool ObjPush(string name, GameObject go)
	{
		if (!_pools.TryGetValue(name, out var entry))
		{
			Destroy(go);
			return false;
		}

		go.transform.SetParent(entry.root);
		entry.pool.Release(go);
		return true;
	}
}
