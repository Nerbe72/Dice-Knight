using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    SoundManager soundManager;

    private void Awake()
    {
        soundManager = SoundManager.Instance;
    }

    public void PlaySFX(Effect _value)
    {
        soundManager.PlayEffect(_value);
    }
}
