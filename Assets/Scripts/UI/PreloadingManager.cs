using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreloadingManager : MonoBehaviour {

	public Text statusText;
	public bool statusFine;

	public bool isConnectedToNet = false;
	private float netCheckTimeOut = 5F;

	public bool assetLoadingComplete = false;

    public TextAsset triggerSpawnDataFile;

	void Start()
	{
        TriggerSpawnDataLoader.Instance.LoadScript(triggerSpawnDataFile.text);

		StartCoroutine (PhaseOne ());
	}

	IEnumerator PhaseOne()
	{
		yield return StartCoroutine (CheckInternetReachability ());

		if (!isConnectedToNet) 
		{
			InquireOfflineMode();

			yield break;
		}

		yield return StartCoroutine (PhaseTwo ());
	}

	public void InquireOfflineMode() //오프라인으로 진행할것인지 물어보는 곳 
	{
		UIPopupMsgManager.instance.PopQuestion 
			("인터넷에 연결되어있지 않거나 로그인에 실패하여 서버로부터 유저정보를 받아올 수 없습니다. 오프라인모드로 진행합니까?",
			 () => StartPhaseTwo (),
			 () => UIPopupMsgManager.instance.PopMessage ("확인을 눌러 프로그램을 종료합니다", () => Application.Quit ()));
	}

	public void StartPhaseTwo()
	{
		StartCoroutine (PhaseTwo ());
	}

	IEnumerator PhaseTwo()
	{
		statusFine = true;

		UITimelineManager.instance.ExecuteTask ();
		statusText.text = "시작하려면 화면을 터치하세요.";

		yield return null;
	}
	

	IEnumerator CheckInternetReachability()
	{
		float timer = netCheckTimeOut;
		
		statusText.text = "인터넷 연결 상태 확인중..."; 
		while (Application.internetReachability == NetworkReachability.NotReachable && timer > 0) 
		{
			statusText.text = "인터넷 연결 상태 확인중... (타임아웃 : " + timer + " 초.)"; 
			timer -= 1.0F;
			
			yield return new WaitForSeconds(1.0f);
		}
		
		if (timer <= 0) {
			isConnectedToNet = false;
			
		} else {
			isConnectedToNet = true;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (statusFine) 
		{
			if (Input.GetMouseButtonDown (0)) 
			{
				SceneManager.LoadScene("MainScene");
			}
			
		}
		
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
