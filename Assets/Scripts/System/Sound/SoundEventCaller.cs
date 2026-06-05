using UnityEngine;
using System.Collections;
using FMODUnity;

public enum SOUND_TYPE
{
    NONE = 0,
    ONESHOT = 1,
    LOOP = 2,
}

public class SoundEventCaller : MonoBehaviour
{
    public int type = (int)SOUND_TYPE.NONE;

    public EventReference asset;

    public float delayTime = 0.0f;
    public float actionCueTime = 0.0f;

    private Vector3 position = Vector3.zero;
    private FMOD.Studio.EventInstance fmodSoundEvent;

    public void PlaySound(Vector3 pos)
    {
        if (FMODSoundManager.instance == null)
            return;

        if (type == (int)SOUND_TYPE.NONE)
            return;

        position = pos;

        Invoke("ActionSound", delayTime);
    }

    private void ActionSound()
    {
        switch (type)
        {
            case (int)SOUND_TYPE.ONESHOT:

                RuntimeManager.PlayOneShot(asset, position);

                break;

            case (int)SOUND_TYPE.LOOP:

                fmodSoundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                fmodSoundEvent = SoundBoard.instance.PlayLoopSoundFromBoard(
                    "",
                    this.gameObject
                );

                break;

            default:
                break;
        }
    }

    private void ActionCue()
    {
        if (type == (int)SOUND_TYPE.LOOP)
        {
            FMOD.Studio.EventDescription desc;

            fmodSoundEvent.getDescription(out desc);

            bool IsKeyOffExists = false;

            desc.hasSustainPoint(out IsKeyOffExists);

            if (IsKeyOffExists)
            {
                fmodSoundEvent.keyOff();
            }
        }
    }

    void Update()
    {
        if (type == (int)SOUND_TYPE.LOOP)
        {
            fmodSoundEvent.set3DAttributes(
                RuntimeUtils.To3DAttributes(this.gameObject)
            );

            if (actionCueTime > 0)
            {
                actionCueTime -= Time.deltaTime;
            }
            else if (actionCueTime < 0)
            {
                actionCueTime = 0.0f;

                ActionCue();
            }
        }
    }

    void OnDisable()
    {
        ActionCue();
    }
}
