using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            DestroyImmediate(this);
        }
    }


    public void PlaySound(AudioClip sound, AudioSource source)
    {
        source.PlayOneShot(sound);
    }

    public void PlaySoundWithRandomPitch(AudioClip sound, AudioSource source, float MinPitch, float MaxPitch)
    {
        float randompitch = Random.Range(MinPitch, MaxPitch);
        source.pitch = randompitch;
        source.PlayOneShot(sound);
    }

    public void PlayInterruptableSound(AudioClip sound, AudioSource source)
    {
        source.clip = sound;
        source.Play();
    }
}
