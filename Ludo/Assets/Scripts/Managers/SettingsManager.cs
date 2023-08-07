using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Settings")]
    public float SFXVolume = 1f;
    public float MusicVolume = 1f;
    public float PieceHeightMultiplier = 1f;
    public float PieceSpeedMultiplier = 1f;
    public float DiceRollTime = 1f;
    public float TimeBetweenTurns = 1f;
    public bool PlayDiceReadout = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadSettings();
    }

    public void SetSFXVolume(float vol)
    {
        SFXVolume = vol;
        PlayerPrefs.SetFloat("sfxvolume", SFXVolume);
    }

    public void SetMusicVolume(float vol)
    {
        MusicVolume = vol;
        PlayerPrefs.SetFloat("musicvolume", MusicVolume);
        AudioManager.instance.SetMusicVolume(MusicVolume);
    }

    public void SetHeightMultiplier(float amount)
    {
        PieceHeightMultiplier = amount;
        PlayerPrefs.SetFloat("heightmultiplier", PieceHeightMultiplier);
    }

    public void SetSpeedMultiplier(float amount)
    {
        PieceSpeedMultiplier = amount;
        PlayerPrefs.SetFloat("speedmultiplier", PieceSpeedMultiplier);
    }

    public void SetDiceRollTime(float amount)
    {
        DiceRollTime = amount;
        PlayerPrefs.SetFloat("dicerolltime", DiceRollTime);
    }

    public void SetTimeBetweenTurns(float amount)
    {
        TimeBetweenTurns = amount;
        PlayerPrefs.SetFloat("timebetweenturns", TimeBetweenTurns);
    }

    public void SetPlayDiceReadout(bool play)
    {
        PlayDiceReadout = play;
        if (play)
            PlayerPrefs.SetInt("playdicereadout", 1);
        else
            PlayerPrefs.SetInt("playdicereadout", 0);
    }

    void LoadSettings()
    {
        SFXVolume = PlayerPrefs.GetFloat("sfxvolume", 1f);
        MusicVolume = PlayerPrefs.GetFloat("backgroundsoundvolume", 1f);
        AudioManager.instance.SetMusicVolume(MusicVolume);
        PieceHeightMultiplier = PlayerPrefs.GetFloat("heightmultiplier", 1f);
        PieceSpeedMultiplier = PlayerPrefs.GetFloat("speedmultiplier", 1f);
        DiceRollTime = PlayerPrefs.GetFloat("dicerolltime", 1f);
        TimeBetweenTurns = PlayerPrefs.GetFloat("timebetweenturns", 1f);
        PlayDiceReadout = PlayerPrefs.GetInt("playdicereadout", 0) == 0 ? false : true;
    }
}
