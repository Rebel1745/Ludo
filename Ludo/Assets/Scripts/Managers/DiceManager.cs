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
    [SerializeField] float timeForDiceToRoll = 1f;
    [SerializeField] float timeBetweenDiceUpdates = 0.05f;
    float currentTimeBetweenDiceUpdates;

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

        if (currentTimeBetweenDiceUpdates >= timeBetweenDiceUpdates)
        {
            SetDice(Random.Range(1, 7));
            currentTimeBetweenDiceUpdates = 0;
        }

    }

    void StartRolling()
    {
        currentTimeBetweenDiceUpdates = 0f;
        isRolling = true;
        Invoke("StopRolling", timeForDiceToRoll);
    }

    void StopRolling()
    {
        // random number between 1 and 6 for the dice roll
        int rolledNum = Random.Range(1, 7);
        GameManager.instance.DiceTotal = rolledNum;
        SetDice(rolledNum);

        isRolling = false;

        // move on to the piece clicking
        GameManager.instance.UpdateGameState(GameState.WaitingForClick);
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
