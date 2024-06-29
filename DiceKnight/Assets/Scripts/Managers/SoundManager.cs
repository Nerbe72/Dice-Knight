using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AudioData
{
    public AudioClip audio;
    [Range(0, 1)]public float volume;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private List<AudioSource> audioSources = new List<AudioSource>();

    [SerializeField] private List<AudioData> effectClips;
    [SerializeField] private List<AudioData> backgroundClips;

    #region VOLUME
    public float EffectVolume;
    public float BackgroundVolume;
    #endregion
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        audioSources.AddRange(GetComponents<AudioSource>().ToList());

        InitVolume();
    }


    private void InitVolume()
    {
        if (!PlayerPrefs.HasKey("BGMVol"))
        {
            PlayerPrefs.SetInt("BGMVol", 100);
        }
        BackgroundVolume = PlayerPrefs.GetInt("BGMVol");

        if (!PlayerPrefs.HasKey("EffectVol"))
        {
            PlayerPrefs.SetInt("EffectVol", 100);
        }
        EffectVolume = PlayerPrefs.GetInt("EffectVol");

        audioSources[0].volume = BackgroundVolume;
        audioSources[1].volume = EffectVolume;
    }

    public void PlayBackground(Background _bgm)
    {
        audioSources[0].loop = true;
        audioSources[0].volume = EffectVolume * backgroundClips[(int)_bgm].volume;
        audioSources[0].clip = backgroundClips[(int)_bgm].audio;
        audioSources[0].Play();
    }

    public void StopBackground()
    {
        audioSources[0].Stop();
    }

    public void PlayEffect(Effect _effect)
    {
        audioSources[1].volume = effectClips[(int)_effect].volume;
        audioSources[1].PlayOneShot(effectClips[(int)_effect].audio);
    }

    public void StopEffect()
    {
        audioSources[1].Stop();
    }
}
