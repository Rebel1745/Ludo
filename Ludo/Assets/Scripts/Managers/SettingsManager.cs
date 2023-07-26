using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Settings")]
    public float SoundVolume = 1f;
    public float PieceHeightMultiplier = 1f;
    public float PieceSpeedMultiplier = 1f;
    public float DiceRollTime = 1f;
    public float TimeBetweenTurns = 1f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadSettings();
    }

    public void SetSoundVolume(float vol)
    {
        SoundVolume = vol;
        PlayerPrefs.SetFloat("soundvolume", SoundVolume);
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

    void LoadSettings()
    {
        SoundVolume = PlayerPrefs.GetFloat("soundvolume", 1f);
        PieceHeightMultiplier = PlayerPrefs.GetFloat("heightmultiplier", 1f);
        PieceSpeedMultiplier = PlayerPrefs.GetFloat("speedmultiplier", 1f);
        DiceRollTime = PlayerPrefs.GetFloat("dicerolltime", 1f);
        TimeBetweenTurns = PlayerPrefs.GetFloat("timebetweenturns", 1f);
    }
}
