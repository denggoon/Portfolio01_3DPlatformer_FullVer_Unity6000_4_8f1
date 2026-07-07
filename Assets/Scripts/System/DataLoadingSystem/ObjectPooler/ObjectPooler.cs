using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ObjectPooler : AutoCreateSceneSingleton<ObjectPooler>
{
	private bool _isQuitting;

	// 앱/플레이모드 종료 시 다른 오브젝트의 OnDisable 등에서 새 풀 컨테이너를 생성/재배치하려다
	// 함께 파괴되는 중인 _ObjectPooler 하위로 SetParent를 시도해 에러가 나는 것을 막는다
	void OnApplicationQuit()
	{
		_isQuitting = true;
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
		if (_isQuitting)
			return null;

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
		if (_isQuitting)
			return false;

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
