using UnityEngine;
using System.Collections;

public enum FILE_LOAD_MODE
{
	OFFLINE = 0,
	ONLINE,
}

// 앱 전역에서 유지되어야 하는 데이터라 씬 배치 없이도 찾아지는 Singleton<T>를 사용한다
// (TriggerSpawnDataLoader와 같은 부류 — GameRuleManager 등 레벨마다 새로 시작해야 하는
// 씬 스코프 매니저는 SceneSingleton/AutoCreateSceneSingleton을 사용한다)
public class DataFileManager : Singleton<DataFileManager> {

	public FILE_LOAD_MODE eFileLoadMode = FILE_LOAD_MODE.ONLINE;

	void Awake()
	{
		PlayerPrefs.DeleteKey ("FileLoadMode");

		PlayerPrefs.SetInt(PrefKeys.FileLoadMode, System.Convert.ToInt32(eFileLoadMode));

		DontDestroyOnLoad (this.gameObject);
	}
}
