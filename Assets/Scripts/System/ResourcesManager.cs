using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourcesManager : MonoBehaviour
{
    private static ResourcesManager instance_;

    public static ResourcesManager instance
    {
        get
        {
            if (instance_ == null)
            {
                GameObject obj = new GameObject("_ResourcesManager");
                instance_ = obj.AddComponent<ResourcesManager>();
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
        // Addressables가 PlayMode 종료 시 자체 정리를 먼저 수행하므로
        // 핸들 Release 없이 캐시 참조만 제거한다
        _goHandleCache.Clear();
        instance_ = null;
    }

    private Dictionary<string, AsyncOperationHandle<GameObject>> _goHandleCache =
        new Dictionary<string, AsyncOperationHandle<GameObject>>();

    // 동기 로드 (WaitForCompletion) — 호환성 유지용.
    // 대용량 에셋에서는 프레임 히치 발생 가능. 가능하면 LoadGameObjectAsync 사용 권장.
    public GameObject LoadGameObject(string address)
    {
        if (_goHandleCache.TryGetValue(address, out var cached) && cached.IsValid())
            return cached.Result;

        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        var result = handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _goHandleCache[address] = handle;
            return result;
        }

        Addressables.Release(handle);
        return null;
    }

    // 비동기 로드 — 코루틴 기반. PlayerSpawn 등 이미 코루틴인 흐름에서 사용.
    public IEnumerator LoadGameObjectAsync(string address, System.Action<GameObject> onLoaded)
    {
        if (_goHandleCache.TryGetValue(address, out var cached) && cached.IsValid())
        {
            onLoaded?.Invoke(cached.Result);
            yield break;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _goHandleCache[address] = handle;
            onLoaded?.Invoke(handle.Result);
        }
        else
        {
            Addressables.Release(handle);
            onLoaded?.Invoke(null);
        }
    }

    public void PopEffect(GameObject fxObj, Vector3 pos) => PopEffect(fxObj.name, pos);

    public void PopEffect(string fxName, Vector3 pos)
    {
        if (ObjectPooler.instance != null)
        {
            var pooled = ObjectPooler.instance.ObjPop(fxName, pos);
            if (pooled != null) return;
        }

        var obj = LoadGameObject("fxs/" + fxName);
        if (obj != null)
            GameObject.Instantiate(obj, pos, obj.transform.rotation);
    }

    public void AttachEffect(string fxName, Transform parent)
    {
        GameObject loadedObj = null;
        GameObject attachFx = null;

        if (ObjectPooler.instance != null)
        {
            loadedObj = ObjectPooler.instance.ObjPop(fxName, parent.position);
            attachFx = loadedObj;
        }

        if (loadedObj == null)
        {
            loadedObj = LoadGameObject("fxs/" + fxName);
            if (loadedObj != null)
                attachFx = GameObject.Instantiate(loadedObj, parent.position, parent.rotation);
        }

        if (attachFx != null)
            attachFx.transform.SetParent(parent);
    }

    public AudioClip LoadAudioClip(string address)
    {
        var handle = Addressables.LoadAssetAsync<AudioClip>(address);
        var result = handle.WaitForCompletion();
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return result;
        Addressables.Release(handle);
        return null;
    }

    public Object ResourcesLoadCached(string address)
    {
        var handle = Addressables.LoadAssetAsync<Object>(address);
        var result = handle.WaitForCompletion();
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return result;
        Addressables.Release(handle);
        return null;
    }

    public void ClearCache()
    {
        foreach (var handle in _goHandleCache.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        _goHandleCache.Clear();
    }
}
