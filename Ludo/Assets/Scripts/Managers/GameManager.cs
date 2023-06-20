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
    string currentPlayerName;
    public bool IsCurrentPlayerCPU = false;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateGameState(GameState.SetupPlayers);
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.SelectPlayerDetails:
                SelectPlayerDetails();
                break;
            case GameState.SetupPlayers:
                SetupPlayers();
                break;
            case GameState.SetupGame:
                SetupGame();
                break;
            case GameState.WaitingForRoll:
                WaitingForRoll();
                break;
            case GameState.WaitingForClick:
                WaitingForClick();
                break;
            case GameState.NextTurn:
                NewTurn();
                break;
            case GameState.RollAgain:
                RollAgain();
                break;
        }
    }

    private void SetupPlayers()
    {
        PlayerManager.instance.CreatePlayers();
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
        currentPlayerName = PlayerManager.instance.Players[CurrentPlayerId].PlayerName;
        IsCurrentPlayerCPU = PlayerManager.instance.Players[CurrentPlayerId].IsPlayerCPU;
    }

    private void RollAgain()
    {
        throw new NotImplementedException();
    }

    private void NewTurn()
    {
        throw new NotImplementedException();
    }

    private void WaitingForClick()
    {
        SetInfoText(currentPlayerName + " click piece to move");
    }

    private void WaitingForRoll()
    {
        DiceManager.instance.SetDice(0);
        SetInfoText(currentPlayerName + " to roll");
    }

    private void SelectPlayerDetails()
    {
        throw new NotImplementedException();
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
    SetupPlayers,
    SetupGame,
    WaitingForRoll,
    WaitingForClick,
    WaitingForAnimation,
    NextTurn,
    RollAgain
}