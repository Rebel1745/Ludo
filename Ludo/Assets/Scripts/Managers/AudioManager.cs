using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioSource source;

    private void Awake()
    {
        instance = this;
    }

    public void PlayAudioClip(AudioClip clip, float minPitch = 1f, float maxPitch = 1f)
    {
        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip);
    }
}
