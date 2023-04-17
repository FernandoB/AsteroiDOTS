using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource clipSource;

    public AudioSource loopSource;

    public AudioSO audioData;

    public void PlaySound(FXEnum fxId)
    {
        if(fxId == FXEnum.AUDIO_STOP_LOOP)
        {
            StopLoop();
            return;
        }

        AudioDataElement ele = GetElementById(fxId);
        if (ele.loop)
        {
            loopSource.clip = ele.clip;
            loopSource.loop = true;
            loopSource.Play();
        }
        else
        {
            clipSource.PlayOneShot(ele.clip);
        }
    }

    public void StopLoop()
    {
        loopSource.Stop();
        loopSource.clip = null;
        loopSource.loop = false;
    }

    public void StopAll()
    {
        clipSource.Stop();
        StopLoop();
    }

    private AudioDataElement GetElementById(FXEnum fxId)
    {
        AudioDataElement element = audioData.elements.Find((ele) => { return ele.id == fxId;  } );
        if(element == null)
        {
            Debug.LogWarning("FX id not found: " + fxId);
        }
        return element;
    }
}
