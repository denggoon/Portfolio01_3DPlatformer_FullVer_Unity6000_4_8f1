using UnityEngine;
using System.Collections;

public class PlayerSpawn : MonoBehaviour
{
    public string playerObjStr;
    public string spawnFxObjStr;
    public string closeFxObjStr;

    [SerializeField]
    private GameObject playerObj;

    public float spawnDelayTime;
    public float destroyDelayTime;

    IEnumerator Start()
    {
        // Addressables를 통한 비동기 로드 — 코루틴에서 yield로 대기하여 프레임 히치 없이 로드
        yield return ResourcesManager.instance.LoadGameObjectAsync(playerObjStr, obj => playerObj = obj);

        yield return new WaitForSeconds(spawnDelayTime);

        if (SoundBoard.instance != null)
            SoundBoard.instance.PlayFromSoundBoard(SoundID.FX_PortalSpawn, this.transform.position);

        ResourcesManager.instance.PopEffect(spawnFxObjStr, this.transform.position);

        if (playerObj != null)
            GameObject.Instantiate(playerObj, this.transform.position, this.transform.rotation);

        playerObj = null;

        float timer = GameRuleManager.instance.gameReadyTimer;
        yield return new WaitForSeconds(timer >= 1F ? GameRuleManager.instance.gameReadyCount - 1F : 1F);

        WarpClose();
    }

    public void WarpClose()
    {
        if (SoundBoard.instance != null)
            SoundBoard.instance.PlayFromSoundBoard(SoundID.FX_PortalDespawn, this.transform.position);

        ResourcesManager.instance.PopEffect(closeFxObjStr, this.transform.position);

        Destroy(this.gameObject, destroyDelayTime);
    }
}
