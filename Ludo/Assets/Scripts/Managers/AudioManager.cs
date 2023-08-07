using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioSource sourceSFX; // sound effects
    [SerializeField] AudioSource sourceBg; // background music
    public AudioClip PieceMovement;
    public float MinimumMovementSoundPitch = 1f;
    public float MaximumMovementSoundPitch = 1f;
    public AudioClip[] PieceBopped;
    public AudioClip[] PieceScored;
    public AudioClip DiceRoll;
    public AudioClip[] DiceRollOne;
    public AudioClip[] DiceRollTwo;
    public AudioClip[] DiceRollThree;
    public AudioClip[] DiceRollFour;
    public AudioClip[] DiceRollFive;
    public AudioClip[] DiceRollSix;
    public AudioClip[] DiceRollThirdSix;

    private void Awake()
    {
        instance = this;
    }

    public void PlayAudioClip(AudioClip clip, float minPitch = 1f, float maxPitch = 1f)
    {
        sourceSFX.pitch = Random.Range(minPitch, maxPitch);
        sourceSFX.volume = SettingsManager.instance.SFXVolume;
        sourceSFX.PlayOneShot(clip);
    }

    public void PlayBackgroundMusic()
    {
        sourceBg.Play();
    }

    public void SetMusicVolume(float vol)
    {
        sourceBg.volume = vol;
    }
}
