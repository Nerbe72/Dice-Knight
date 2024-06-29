using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private List<AudioSource> audioSources = new List<AudioSource>();

    public class AudioData
    {
        public AudioClip audio;
        public float volume;
    }

    [SerializeField] private List<AudioClip> effectClips;
    [SerializeField] private List<AudioClip> backgroundClips;

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
    }

    public void PlayEffect(Effect _effect)
    {
        audioSources[1].PlayOneShot(effectClips[(int)_effect]);
    }

    public void PlayBaclground(Background _bg)
    {
        audioSources[0].clip = backgroundClips[(int)_bg];
        audioSources[0].Play();
    }
}
