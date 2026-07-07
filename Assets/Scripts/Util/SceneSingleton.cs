using UnityEngine;

// 씬에 미리 배치되어 있어야 하는 매니저용 싱글턴 베이스.
// 자동 생성을 하지 않으므로 Inspector에 값을 채워 씬에 놓아둔 인스턴스만 사용된다.
public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T instance => _instance;

	protected virtual void Awake()
	{
		_instance = this as T;
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
