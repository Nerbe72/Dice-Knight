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
    /// <summary>
    /// Title:0, Tutorial:1, 이후부터 난이도
    /// </summary>
    [SerializeField] private List<AudioData> backgroundClips;

    #region VOLUME
    public float MainVolume;
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

        PlayBackground(Background.Title);
    }

    private void InitVolume()
    {
        if (!PlayerPrefs.HasKey(VolumeType.MainVol.ToString()))
        {
            PlayerPrefs.SetInt(VolumeType.MainVol.ToString(), 100);
        }
        MainVolume = PlayerPrefs.GetInt(VolumeType.MainVol.ToString()) / 100f;

        if (!PlayerPrefs.HasKey(VolumeType.BGMVol.ToString()))
        {
            PlayerPrefs.SetInt(VolumeType.BGMVol.ToString(), 100);
        }
        BackgroundVolume = PlayerPrefs.GetInt(VolumeType.BGMVol.ToString()) / 100f;

        if (!PlayerPrefs.HasKey(VolumeType.EffectVol.ToString()))
        {
            PlayerPrefs.SetInt(VolumeType.EffectVol.ToString(), 100);
        }
        EffectVolume = PlayerPrefs.GetInt(VolumeType.EffectVol.ToString()) / 100f;

        
        audioSources[0].volume = MainVolume * BackgroundVolume;
        audioSources[1].volume = MainVolume * EffectVolume;
    }

    public void SetVolume()
    {
        audioSources[0].volume = MainVolume * BackgroundVolume;
        audioSources[1].volume = MainVolume * EffectVolume;
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetInt(VolumeType.MainVol.ToString(), (int)(MainVolume * 100));
        PlayerPrefs.SetInt(VolumeType.BGMVol.ToString(), (int)(BackgroundVolume * 100));
        PlayerPrefs.SetInt(VolumeType.EffectVol.ToString(), (int)(EffectVolume * 100));
    }

    public void PlayBackground(Background _bgm)
    {
        audioSources[0].loop = true;
        audioSources[0].volume = MainVolume * EffectVolume * backgroundClips[(int)_bgm].volume;
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
