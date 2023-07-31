using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UINavigation))]
public class SettingsScreenUI : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] TMP_Dropdown heightDropdown;
    [SerializeField] TMP_Dropdown speedDropdown;
    [SerializeField] TMP_Dropdown diceSpeedDropdown;
    [SerializeField] TMP_Dropdown turnTimeDropdown;

    private void Start()
    {
        SetCurrentValues();
    }

    void SetCurrentValues()
    {
        // volume
        volumeText.text = (SettingsManager.instance.SoundVolume * 100).ToString();
        volumeSlider.value = SettingsManager.instance.SoundVolume * 100;

        // height
        int heightChoice = 0;

        switch (SettingsManager.instance.PieceHeightMultiplier)
        {
            case 0f:
                heightChoice = 0;
            break;
            case 0.5f:
                heightChoice = 1;
            break;
            case 1f:
                heightChoice = 2;
            break;
            case 3f:
                heightChoice = 3;
            break;
        }

        heightDropdown.value = heightChoice;

        // speed
        int speedChoice = 0;

        switch (SettingsManager.instance.PieceSpeedMultiplier)
        {
            case 0.5f:
                speedChoice = 0;
                break;
            case 1f:
                speedChoice = 1;
                break;
            case 3f:
                speedChoice = 2;
                break;
            case 100f:
                speedChoice = 3;
                break;
        }

        speedDropdown.value = speedChoice;

        // dice speed
        int diceSpeedChoice = 0;

        switch (SettingsManager.instance.DiceRollTime)
        {
            case 2f:
                diceSpeedChoice = 0;
                break;
            case 1f:
                diceSpeedChoice = 1;
                break;
            case 0.5f:
                diceSpeedChoice = 2;
                break;
            case 0f:
                diceSpeedChoice = 3;
                break;
        }

        diceSpeedDropdown.value = diceSpeedChoice;

        // time between turns
        int turnTime = 0;

        switch (SettingsManager.instance.TimeBetweenTurns)
        {
            case 2f:
                turnTime = 0;
                break;
            case 1f:
                turnTime = 1;
                break;
            case 0.5f:
                turnTime = 2;
                break;
            case 0f:
                turnTime = 3;
                break;
        }

        turnTimeDropdown.value = turnTime;
    }

    public void UpdateVolumeAmount()
    {
        volumeText.text = volumeSlider.value.ToString();
        SettingsManager.instance.SetSoundVolume((float)volumeSlider.value/100f);
    }

    public void SetPieceHeight(int opt)
    {
        float heightModifier = 0f;

        switch (opt)
        {
            case 0:
                heightModifier = 0f;
            break;
            case 1:
                heightModifier = 0.5f;
            break;
            case 2:
                heightModifier = 1f;
            break;
            case 3:
                heightModifier = 3f;
            break;            
        }

        SettingsManager.instance.SetHeightMultiplier(heightModifier);
    }

    public void SetPieceSpeed(int opt)
    {
        float speedModifier = 0f;

        switch (opt)
        {
            case 0:
                speedModifier = 0.5f;
            break;
            case 1:
                speedModifier = 1f;
            break;
            case 2:
                speedModifier = 3f;
            break;
            case 3:
                speedModifier = 100f;
            break;            
        }

        SettingsManager.instance.SetSpeedMultiplier(speedModifier);
    }

    public void SetDiceRollTime(int opt)
    {
        float timeModifier = 0f;

        switch (opt)
        {
            case 0:
                timeModifier = 2f;
            break;
            case 1:
                timeModifier = 1f;
            break;
            case 2:
                timeModifier = 0.5f;
            break;
            case 3:
                timeModifier = 0f;
            break;            
        }

        SettingsManager.instance.SetDiceRollTime(timeModifier);
    }

    public void SetTimeBetweenTurns(int opt)
    {
        float timeModifier = 0f;

        switch (opt)
        {
            case 0:
                timeModifier = 2f;
            break;
            case 1:
                timeModifier = 1f;
            break;
            case 2:
                timeModifier = 0.5f;
            break;
            case 3:
                timeModifier = 0f;
            break;            
        }

        SettingsManager.instance.SetTimeBetweenTurns(timeModifier);
    }

    public void Back()
    {
        gameObject.GetComponent<UINavigation>().CloseUIAndRevertToReferer();
        //UIManager.instance.ShowHideUIElement(UIManager.instance.SettingsScreenUI, false);
        //GameManager.instance.RevertToPreviousState();
    }
}
