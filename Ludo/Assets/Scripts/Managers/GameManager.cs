using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] TMP_Text infoText;
    [SerializeField] GameObject gameUI;

    public GameState State;

    public int DiceTotal;

    // Players
    int totalPlayers;
    public int CurrentPlayerId;
    public string CurrentPlayerName;
    public bool IsCurrentPlayerCPU = false;

    public int CurrentPlayerRollAgainCount = 0;
    public int MaximumRollAgain = 3;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateGameState(GameState.SelectPlayerDetails);
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.SelectPlayerDetails:
                SelectPlayerDetails();
                break;
            case GameState.SetupGame:
                SetupGame();
                break;
            case GameState.WaitingForRoll:
                WaitingForRoll();
                break;
            case GameState.CheckForLegalMove:
                // if the player has a legal move, wait for it, otherwise move on to the next player
                if (PlayerManager.instance.PlayerHasLegalMove())
                    UpdateGameState(GameState.WaitingForClick);
                else
                    UpdateGameState(GameState.NextTurn);
                break;
            case GameState.WaitingForClick:
                WaitingForClick();
                break;
            case GameState.NextTurn:
                NextTurn();
                break;
            case GameState.RollAgain:
                RollAgain();
                break;
        }
    }

    void SetupGame()
    {
        // Setup the variables that control player names and ids
        totalPlayers = PlayerManager.instance.Players.Length;
        CurrentPlayerId = 0;
        UpdateCurrentPlayerDetails();
        // Activate the GameUI
        gameUI.SetActive(true);
        // Start the game by changing the state to WaitingForRoll
        UpdateGameState(GameState.WaitingForRoll);
    }

    void UpdateCurrentPlayerDetails()
    {
        CurrentPlayerName = PlayerManager.instance.Players[CurrentPlayerId].PlayerName;
        IsCurrentPlayerCPU = PlayerManager.instance.Players[CurrentPlayerId].IsPlayerCPU;
    }

    private void RollAgain()
    {
        if (CurrentPlayerRollAgainCount <= MaximumRollAgain)
            UpdateGameState(GameState.WaitingForRoll);
        else
            print("If I see this message, something went wrong");
    }

    private void NextTurn()
    {
        // advance player
        CurrentPlayerId = (CurrentPlayerId + 1) % totalPlayers;
        UpdateCurrentPlayerDetails();
        CurrentPlayerRollAgainCount = 0;

        UpdateGameState(GameState.WaitingForRoll);
    }

    private void WaitingForClick()
    {
        SetInfoText(CurrentPlayerName + " click piece to move");
    }

    private void WaitingForRoll()
    {
        DiceManager.instance.SetDice(0);
        SetInfoText(CurrentPlayerName + " to roll");
    }

    private void SelectPlayerDetails()
    {
        PlayerManager.instance.ShowPlayerSelectScreen();
    }

    public void SetInfoText(string newText)
    {
        infoText.text = newText;
    }

    public string GetInfoText()
    {
        return infoText.text;
    }
}

public enum GameState
{
    SelectPlayerDetails,
    SetupGame,
    WaitingForRoll,
    CheckForLegalMove,
    WaitingForClick,
    WaitingForAnimation,
    NextTurn,
    RollAgain
}