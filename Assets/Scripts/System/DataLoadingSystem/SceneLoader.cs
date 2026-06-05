using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;

    // true: Addressables로 씬 로드 / false: SceneManager 직접 로드 (폴백)
    public bool useAddressables = true;

    public bool isComplete;
    public string statusMsg;
    public float progress;

    void Awake()
    {
        this.enabled = false;
    }

    void Update()
    {
        PreSceneDataLoader.instance.uiTxtProgress.text = statusMsg;
    }

    public IEnumerator Execute()
    {
        this.enabled = true;
        sceneName = PlayerPrefs.GetString(PrefKeys.LoadingSceneName);

        if (useAddressables)
        {
            yield return StartCoroutine(LoadSceneAddressable(sceneName));
        }
        else
        {
            yield return StartCoroutine(LoadSceneDirect(sceneName));
        }

        isComplete = true;
        this.enabled = false;
    }

    private IEnumerator LoadSceneAddressable(string address)
    {
        var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Single);

        while (!handle.IsDone)
        {
            progress = handle.PercentComplete * 100F;
            statusMsg = "스테이지 로딩중:" + address + "(" + Mathf.RoundToInt(progress) + "%)";
            yield return null;
        }

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("SceneLoader: Addressables 씬 로드 실패 — " + address + ". 직접 로드로 폴백.");
            yield return StartCoroutine(LoadSceneDirect(address));
        }
    }

    private IEnumerator LoadSceneDirect(string name)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            SceneManager.LoadScene(name);
            yield break;
        }

        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(name);
        do
        {
            progress = sceneAsync.progress * 100F;
            statusMsg = "스테이지 로딩중:" + name + "(" + Mathf.RoundToInt(progress) + "%)";
            yield return null;
        } while (!sceneAsync.isDone);
    }
}
