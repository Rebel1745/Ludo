using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] TMP_Text infoText;
    [SerializeField] GameObject gameOverPlayerDetailsHolder;

    public GameState State;
    public GameState PreviousState;

    [Space()]
    [Header("Current Player Info")]
    int totalPlayers;
    public int CurrentPlayerId;
    public string CurrentPlayerName;
    public bool IsCurrentPlayerCPU = false;

    public int CurrentPlayerRollAgainCount = 0;
    public int MaximumRollAgain = 3;

    [Space()]
    [Header("Timing")]
    public float TimeBetweenDiceUpdates = 0.05f;

    [Space()]
    [Header("AI Testing")]
    public bool IsAITesting = false;
    public int MaxGamesToTest = 10;
    int gamesTested = 0;
    int standardPoints, aggressivePoints, cautiousPoints, wildcardPoints, randomPoints, roundPoints;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateGameState(GameState.StartScreen);
        AudioManager.instance.PlayBackgroundMusic();
    }

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.StartScreen:
                ShowStartScreen();
                break;
            case GameState.SettingsScreen:
                ShowSettingsScreen();
                break;
            case GameState.PauseScreen:
                ShowPauseScreen();
                break;
            case GameState.BuildBoard:
                BoardManager.instance.BuildBoard();
                break;
            case GameState.SelectPlayerDetails:
                SelectPlayerDetails();
                break;
            case GameState.SetupGame:
                StartCoroutine("SetupGame", 1f);
                break;
            case GameState.WaitingForRoll:
                WaitingForRoll();
                break;
            case GameState.CheckForMultipleSixes:
                PlayerManager.instance.CheckForMultipleSixes();
                break;
            case GameState.CheckForLegalMove:
                // if the player has a legal move, wait for it, otherwise move on to the next player
                if (PlayerManager.instance.PlayerHasLegalMove())
                    UpdateGameState(GameState.WaitingForClick);
                else
                {
                    SetInfoText(CurrentPlayerName + ", You have no legal moves");
                    UpdateGameState(GameState.NextTurn, SettingsManager.instance.TimeBetweenTurns);
                }
                break;
            case GameState.WaitingForClick:
                WaitingForClick();
                break;
            case GameState.WaitingForAnimation:
                WaitingForAnimation();
                break;
            case GameState.NextTurn:
                StartCoroutine( "NextTurn", delay);
                break;
            case GameState.RollAgain:
                RollAgain();
                break;
            case GameState.GameOver:
                StartCoroutine("GameOver", delay);
                break;
        }
    }

    void ShowStartScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, true, PreviousState);
    }

    void ShowSettingsScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.SettingsScreenUI, true, PreviousState);
    }

    void ShowPauseScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.PauseScreenUI, true, PreviousState);
    }

    IEnumerator GameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        // show game over screen with positions
        UIManager.instance.ShowHideUIElement(UIManager.instance.GameOverUI, true, State);
        Player currentPlayer;

        // loop through the players and set the game over details according to their finished positions
        for (int i = 0; i < PlayerManager.instance.Players.Length; i++)
        {
            currentPlayer = PlayerManager.instance.Players[i];
            gameOverPlayerDetailsHolder.transform.GetChild(currentPlayer.FinishedPosition - 1).GetComponent<GameOverDetails>().SetDetails(currentPlayer, currentPlayer.PlayerName, currentPlayer.PlayerColour);
        }

        if (IsAITesting)
        {
            gamesTested++;

            for (int i = 0; i < 4; i++)
            {
                currentPlayer = gameOverPlayerDetailsHolder.transform.GetChild(i).GetComponent<GameOverDetails>().player;

                // assign points for finishing position
                switch (currentPlayer.FinishedPosition)
                {
                    case 1:
                        roundPoints = 3;
                        break;
                    case 2:
                        roundPoints = 2;
                        break;
                    case 3:
                        roundPoints = 1;
                        break;
                    case 4:
                        roundPoints = 0;
                        break;
                }

                // add the points to the AIType
                switch (currentPlayer.CPUType) {
                    case AIType.Standard:
                        standardPoints += roundPoints;
                        break;
                    case AIType.Aggressive:
                        aggressivePoints += roundPoints;
                        break;
                    case AIType.Cautious:
                        cautiousPoints += roundPoints;
                        break;
                    case AIType.WildCard:
                        wildcardPoints += roundPoints;
                        break;
                    case AIType.Random:
                        randomPoints += roundPoints;
                        break;

                }
            }
            print("Standard: " + standardPoints + ". Aggressive: " + aggressivePoints + ". Cautious: " + cautiousPoints + ". Wild Card: " + wildcardPoints + ". Random: " + randomPoints);

            if(gamesTested < MaxGamesToTest)
                PlayerManager.instance.ResetGame();
        }        
    }

    void SetupGame(float delay)
    {
        // Setup the variables that control player names and ids
        totalPlayers = PlayerManager.instance.Players.Length;
        CurrentPlayerId = 0;
        UpdateCurrentPlayerDetails();
        // Activate the GameUI
        //UIManager.instance.ShowGameUI();
        UIManager.instance.ShowHideUIElement(UIManager.instance.GameUI, true, State);
        // Start the game by changing the state to WaitingForRoll
        UpdateGameState(GameState.WaitingForRoll);
    }

    void UpdateCurrentPlayerDetails()
    {
        CurrentPlayerName = PlayerManager.instance.Players[CurrentPlayerId].PlayerName;
        IsCurrentPlayerCPU = PlayerManager.instance.Players[CurrentPlayerId].IsPlayerCPU;
    }

    void RollAgain()
    {
        // another turn is as easy as just going back to before the dice has been rolled
        UpdateGameState(GameState.WaitingForRoll);
    }

    IEnumerator NextTurn(float delay)
    {
        // first remove the selectable highlighting from the previous players pieces
        PlayerManager.instance.RemoveHighlightFromPlayerPieces(CurrentPlayerId);

        yield return new WaitForSeconds(delay);

        // advance player
        CurrentPlayerId = (CurrentPlayerId + 1) % totalPlayers;
        UpdateCurrentPlayerDetails();
        CurrentPlayerRollAgainCount = 0;

        // if the player has finished the game, move on to the next player
        if (PlayerManager.instance.Players[CurrentPlayerId].IsFinished)
            UpdateGameState(GameState.NextTurn, 0f);
        else
            UpdateGameState(GameState.WaitingForRoll);     
    }

    private void WaitingForClick()
    {
        PlayerManager.instance.HighlightPiecesWithLegalMove(CurrentPlayerId);
        SetInfoText(CurrentPlayerName + " click piece to move");
    }

    private void WaitingForRoll()
    {
        DiceManager.instance.SetDice(0);
        SetInfoText(CurrentPlayerName + " to roll");
    }

    private void WaitingForAnimation()
    {
        // while we wait for the animation, remove the player piece highlighting
        PlayerManager.instance.RemoveHighlightFromPlayerPieces(CurrentPlayerId);
    }

    private void SelectPlayerDetails()
    {
        //UIManager.instance.ShowPlayerSelectScreen();
        UIManager.instance.ShowHideUIElement(UIManager.instance.PlayerSelectUI, true, State);
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
    StartScreen,
    SettingsScreen,
    PauseScreen,
    BuildBoard,
    SelectPlayerDetails,
    SetupGame,
    WaitingForRoll,
    CheckForMultipleSixes,
    CheckForLegalMove,
    WaitingForClick,
    WaitingForAnimation,
    NextTurn,
    RollAgain,
    GameOver
}