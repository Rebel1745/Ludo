using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public static DiceManager instance;

    [SerializeField] TMP_Text diceCountText;
    [SerializeField] Sprite[] diceImages;
    [SerializeField] Image diceImage;
    [SerializeField] Button rollTheDiceButton;

    bool isRolling = false;
    public bool IsUsingDiceImage = false;
    float currentTimeBetweenDiceUpdates;
    float currentRollingTime;

    public int DiceTotal { get; protected set; }

    void Start()
    {
        instance = this;
        SetDiceText("?");
    }

    private void Update()
    {
        if (GameManager.instance.State != GameState.WaitingForRoll)
        {
            ShoHideButton(false);
            return;
        }
        else
        {
            // hide the button if the player is a cpu
            if (!isRolling && !GameManager.instance.IsCurrentPlayerCPU)
            {
                ShoHideButton(true);
            }
        }

        if (GameManager.instance.IsCurrentPlayerCPU)
            RollTheDice();

        if (isRolling)
            UpdateDice();
    }

    void ShoHideButton(bool show)
    {
        rollTheDiceButton.gameObject.SetActive(show);
    }

    public void RollTheDice()
    {
        ShoHideButton(false);

        // Have we already rolled the dice?
        if (GameManager.instance.State != GameState.WaitingForRoll || isRolling)
            return;

        StartRolling();
    }

    void UpdateDice()
    {
        currentTimeBetweenDiceUpdates += Time.deltaTime;
        currentRollingTime += Time.deltaTime;

        if (currentTimeBetweenDiceUpdates >= SettingsManager.instance.TimeBetweenDiceUpdates)
        {
            SetDice(Random.Range(1, 7));
            currentTimeBetweenDiceUpdates = 0;
        }

        if (currentRollingTime >= SettingsManager.instance.DiceRollTime)
            StopRolling();

    }

    void StartRolling()
    {
        currentTimeBetweenDiceUpdates = 0f;
        isRolling = true;
        AudioManager.instance.PlayAudioClip(AudioManager.instance.DiceRoll);
        currentRollingTime = 0;
        // removing this Invoke as it messes with the pause function, use a time instead
        //Invoke("StopRolling", SettingsManager.instance.DiceRollTime);
    }

    void StopRolling()
    {
        // random number between 1 and 6 for the dice roll
        DiceTotal = Random.Range(1, 7);
        /*if (GameManager.instance.CurrentPlayerId == 0)
           rolledNum = 6;*/

        SetDice(DiceTotal);

        isRolling = false;

        AudioClip randomAudioClip = GetAudioClipFromDiceRoll(DiceTotal);

        if(DiceTotal == 6)
        {
            GameManager.instance.CurrentPlayerRollAgainCount++;
            GameManager.instance.UpdateGameState(GameState.CheckForMultipleSixes);
        }
        else
        {
            // move on to check if the player has a legal move
            if (SettingsManager.instance.PlayDiceReadout)
                AudioManager.instance.PlayAudioClip(randomAudioClip);
            GameManager.instance.UpdateGameState(GameState.CheckForLegalMove);
        }
    }

    AudioClip GetAudioClipFromDiceRoll(int roll)
    {
        AudioClip returnClip = null;

        switch (roll) {
            case 1:
                returnClip = AudioManager.instance.DiceRollOne[Random.Range(0, AudioManager.instance.DiceRollOne.Length - 1)];
                break;
            case 2:
                returnClip = AudioManager.instance.DiceRollTwo[Random.Range(0, AudioManager.instance.DiceRollTwo.Length - 1)];
                break;
            case 3:
                returnClip = AudioManager.instance.DiceRollThree[Random.Range(0, AudioManager.instance.DiceRollThree.Length - 1)];
                break;
            case 4:
                returnClip = AudioManager.instance.DiceRollFour[Random.Range(0, AudioManager.instance.DiceRollFour.Length - 1)];
                break;
            case 5:
                returnClip = AudioManager.instance.DiceRollFive[Random.Range(0, AudioManager.instance.DiceRollFive.Length - 1)];
                break;
        }

        return returnClip;
    }

    public void SetDice(int number)
    {
        if (IsUsingDiceImage)
            SetDiceImage(number);
        else
            SetDiceText(number.ToString());
    }

    void SetDiceText(string text)
    {
        diceCountText.text = text == "0" ? "?" : text;
    }

    void SetDiceImage(int number)
    {
        if (number != 0)
            diceCountText.text = "";
        else
            SetDiceText("0");
        diceImage.sprite = diceImages[number];
    }
}
