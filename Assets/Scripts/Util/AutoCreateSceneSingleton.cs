using UnityEngine;

// 씬 배치 없이 최초 접근 시 자동으로 GameObject를 만들어 붙는 싱글턴 베이스.
// DontDestroyOnLoad는 걸지 않으므로 씬이 전환되면 파괴되고 다음 접근 시 새로 생성된다.
public class AutoCreateSceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = new GameObject("_" + typeof(T).Name);
				_instance = obj.AddComponent<T>();
			}
			return _instance;
		}
	}

	protected virtual void Awake()
	{
		_instance = this as T;
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
