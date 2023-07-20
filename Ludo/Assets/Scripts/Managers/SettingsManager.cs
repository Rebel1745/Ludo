using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    private void Awake()
    {
        instance = this;
    }

    public float SoundVolume = 1f;

    void SetSoundVolume(float vol)
    {
        SoundVolume = vol;
    }

    void LoadSettings()
    {
        SoundVolume = PlayerPrefs.GetFloat("soundvolume", 1f);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("soundvolume", SoundVolume);
    }
}
