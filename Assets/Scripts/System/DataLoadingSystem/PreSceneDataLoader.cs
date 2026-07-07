using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PreSceneDataLoader : SceneSingleton<PreSceneDataLoader>
{
	public BankLoader bankLoader;
	public SceneLoader sceneLoader;

	public Text uiTxtProgress;

	public bool finishedDownloadingScene = false;

	IEnumerator Start ()
	{
		yield return StartCoroutine (bankLoader.Execute ());
		yield return StartCoroutine (sceneLoader.Execute ());
	}
}
