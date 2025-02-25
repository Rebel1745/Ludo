using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [Header("Configuration")]
    Transform playerYardHolder; // the holding area for player pieces
    public Material[] playerMaterials;
    [SerializeField] GameObject playerPrefab;
    Tile[] playerStartingTiles;
    Transform playerScoredTileHolder;
    [SerializeField] GameObject[] allPlayerDetails;
    GameObject playersHolder;

    public Player[] Players;

    // animation
    [Space]
    [Header("Animation")]
    bool isAnimating = false;
    List<PlayerPieceMovement> movementList;
    Vector3 newPosition;
    Vector3 velocity = Vector3.zero;
    [SerializeField] float smoothTime = 0.25f;
    [SerializeField] float smoothDistance = 0.01f;
    [SerializeField] float audioDistance = 0.1f;
    float timeToMove;

    // height
    [Header("Height")]
    float maxHeight = 0.5f;
    [SerializeField] AnimationCurve heightCurve;
    float heightTime;
    float targetHeight;

    // finished players
    int playersFinished = 0;

    private void Awake()
    {
        instance = this;
        movementList = new List<PlayerPieceMovement>();
    }

    private void Update()
    {
        if (GameManager.instance.State == GameState.WaitingForClick && GameManager.instance.IsCurrentPlayerCPU)
            AIManager.instance.DoAI();

        if (GameManager.instance.State != GameState.WaitingForAnimation || !isAnimating)
            return;

        if (movementList.Count == 0)
            return;

        MovePieces();
    }

    public void InitialiseBoardHolders(Transform yardHolder, Tile[] startingTiles, Transform scoredTileHolder)
    {
        playerYardHolder = yardHolder;
        playerStartingTiles = startingTiles;
        playerScoredTileHolder = scoredTileHolder;

        GameManager.instance.UpdateGameState(GameState.SelectPlayerDetails);
    }

    public void CheckForMultipleSixes()
    {
        if (GameManager.instance.CurrentPlayerRollAgainCount >= GameManager.instance.MaximumRollAgain)
        {
            // we have exceeded the total number of 6's in a row, do something about it
            PlayerPiece pieceToSendHome = GetMostAdvancedPiece();
            if (pieceToSendHome != null)
                pieceToSendHome.SendPieceHome(pieceToSendHome, GameManager.instance.MaximumRollAgain + " 6's thrown in a row. Go back home!", true);
            else
                GameManager.instance.UpdateGameState(GameState.NextTurn, SettingsManager.instance.TimeBetweenTurns);
        }
        else
        {
            AudioClip clip = AudioManager.instance.DiceRollSix[Random.Range(0, AudioManager.instance.DiceRollSix.Length - 1)];
            if (SettingsManager.instance.PlayDiceReadout)
                AudioManager.instance.PlayAudioClip(clip);
            GameManager.instance.UpdateGameState(GameState.CheckForLegalMove);
        }
    }

    PlayerPiece GetMostAdvancedPiece()
    {
        PlayerPiece piece = null;
        int furthestDistance = -1;

        foreach (PlayerPiece pp in Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            if (!pp.IsInYard && !pp.IsScored && !pp.CurrentTile.IsSafeTile && pp.TotalDistanceTravelled > furthestDistance)
            {
                piece = pp;
                furthestDistance = pp.TotalDistanceTravelled;
            }
        }

        return piece;
    }

    void MovePieces()
    {
        // is the movement instant?
        timeToMove = movementList[0].IsInstantMovement ? 0 : (smoothTime / SettingsManager.instance.PieceSpeedMultiplier);

        // calculate the height to add at the current position
        targetHeight = heightCurve.Evaluate(heightTime / smoothTime) * maxHeight * SettingsManager.instance.PieceHeightMultiplier;

        // if we aren't moving to a new tile and are just rearranging pieces on a single tile, make the movement instantaneous
        if (movementList[0].DestinationTile == null)
            newPosition = movementList[0].DestinationPosition;
        else
            newPosition = new Vector3(movementList[0].DestinationTile.transform.position.x, targetHeight, movementList[0].DestinationTile.transform.position.z);

        // audio check (this happens before the smoothDistance check to counter the lag caused by the SmoothDamp function)
        if (Vector3.Distance(movementList[0].PieceToMove.transform.position, newPosition) < audioDistance)
        {
            // we are at our destination play the piece movement sound
            if (movementList[0].PlaySound)
            {
                AudioManager.instance.PlayAudioClip(movementList[0].SoundToPlay, movementList[0].minimumPitch, movementList[0].maximumPitch);
                movementList[0].PlaySound = false;
            }
        }

        // first check if we have reached the target of the first element in the moveList
        if (Vector3.Distance(movementList[0].PieceToMove.transform.position, newPosition) < smoothDistance)
        {
            // update the movementList to remove this movement
            AdvanceMovementList();
        }
        else
        {
            movementList[0].PieceToMove.transform.position = Vector3.SmoothDamp(movementList[0].PieceToMove.transform.position, newPosition, ref velocity, timeToMove);
        }

        heightTime += Time.deltaTime;
    }

    void AdvanceMovementList()
    {
        heightTime = 0f;

        if (movementList.Count == 1)
        {
            // we have finished our list of movements
            isAnimating = false;

            // if the movement is a scoring one check to see if it wins
            if (movementList[0].IsScoringMove)
                CheckScoringMove(movementList[0].PieceToMove);
            else
            {
                movementList.Clear();
                MoveToNextTurn();
            }
        }
        else
        {
            // we still have movements left, remove the one we have just performed
            movementList.RemoveAt(0);
            // set the infoText to the new info text
            if (movementList[0].InfoTextToDisplay != null && movementList[0].InfoTextToDisplay != "")
                GameManager.instance.SetInfoText(movementList[0].InfoTextToDisplay);
        }
    }

    void CheckScoringMove(PlayerPiece playerPiece)
    {
        playerPiece.IsScored = true;

        bool allPiecesScored = true;

        foreach (PlayerPiece pp in Players[playerPiece.PlayerId].PlayerPieces)
        {
            if (!pp.IsScored)
            {
                allPiecesScored = false;
                break;
            }
        }

        if (allPiecesScored)
        {
            SetPlayerFinished(playerPiece.PlayerId);
        }
        else
        {
            movementList.Clear();
            MoveToNextTurn();
        }
    }

    void SetPlayerFinished(int pId)
    {
        playersFinished++;

        Players[pId].IsFinished = true;
        Players[pId].FinishedPosition = playersFinished;

        // if the first player has finished, and the setting for game over after one player finishing is true, finish
        if (SettingsManager.instance.FinishGameAfterOnePlayerFinishes)
        {
            // figure out the order of the remaining players.
            // first consideration is counting the number of pieces that have made it home safely
            Dictionary<int, int> playersList = new Dictionary<int, int>();
            foreach (Player p in Players)
            {
                playersList.Add(p.PlayerId, p.FinishingOrderNumber);
            }
            playersList = playersList.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (int playerId in playersList.Keys)
            {
                Players[playerId].IsFinished = true;
                Players[playerId].FinishedPosition = playersFinished;
                playersFinished++;
            }
            GameManager.instance.UpdateGameState(GameState.GameOver, 1f);
        }
        else if (playersFinished == Players.Length - 1)
        {
            // find the player that has not finished, and mark them as finished and last
            for (int i = 0; i < Players.Length; i++)
            {
                if (!Players[i].IsFinished)
                {
                    Players[i].IsFinished = true;
                    Players[i].FinishedPosition = playersFinished + 1;
                }
            }
            GameManager.instance.UpdateGameState(GameState.GameOver, 1f);
        }
        else
        {
            movementList.Clear();
            MoveToNextTurn();
        }
    }

    void MoveToNextTurn()
    {
        //allow another roll if a 6 was rolled, otherwise move on to next turn
        if (DiceManager.instance.DiceTotal == 6 && GameManager.instance.CurrentPlayerRollAgainCount < GameManager.instance.MaximumRollAgain)
            GameManager.instance.UpdateGameState(GameState.RollAgain);
        else
            GameManager.instance.UpdateGameState(GameState.NextTurn, SettingsManager.instance.TimeBetweenTurns);
    }

    public void SetMovementList(List<PlayerPieceMovement> ppms)
    {
        movementList = ppms;
        // when we set the movementList, set the InfoText to the first movements text
        if (movementList[0].InfoTextToDisplay != null && movementList[0].InfoTextToDisplay != "")
            GameManager.instance.SetInfoText(movementList[0].InfoTextToDisplay);

        isAnimating = true;
        GameManager.instance.UpdateGameState(GameState.WaitingForAnimation);
    }

    public void CreatePlayers()
    {
        GameObject newPlayerParent;
        GameObject newPlayer;
        PlayerPiece newPlayerPiece;
        PlayerDetails newPlayerDetails;
        Tile startingYardTile;
        Tile scoredTile;

        playersHolder = new GameObject { name = "Players" };

        // init players array
        Players = new Player[playerYardHolder.childCount];

        // loop through the player yards
        for (int i = 0; i < playerYardHolder.childCount; i++)
        {
            newPlayerDetails = allPlayerDetails[i].GetComponent<PlayerDetails>();

            // create an empty game object for this parent, inside the playersHolder
            newPlayerParent = new GameObject
            {
                name = newPlayerDetails.PlayerName + " Pieces"
            };

            newPlayerParent.transform.parent = playersHolder.transform;

            // setup the player details
            Players[i] = new Player
            {
                PlayerId = i,
                PlayerName = newPlayerDetails.PlayerName,
                IsPlayerCPU = newPlayerDetails.isCPU,
                CPUType = (AIType)newPlayerDetails.CPUType,
                PlayerPieces = new PlayerPiece[playerYardHolder.GetChild(i).childCount],
                PlayerColour = newPlayerDetails.ColourImage.color
            };

            // loop through the tiles inside the players yard
            for (int j = 0; j < playerYardHolder.GetChild(i).childCount; j++)
            {
                startingYardTile = playerYardHolder.GetChild(i).GetChild(j).GetComponent<Tile>();
                scoredTile = playerScoredTileHolder.GetChild(i).GetChild(j).GetComponent<Tile>();
                // create a player piece on the yard tile
                newPlayer = Instantiate(playerPrefab, startingYardTile.transform.position, Quaternion.identity, newPlayerParent.transform);
                newPlayer.name = newPlayerDetails.PlayerName + " Piece " + (j + 1);

                newPlayerPiece = newPlayer.GetComponent<PlayerPiece>();
                newPlayerPiece.GetComponentInChildren<Renderer>().material = playerMaterials[i];
                // setup the piece
                newPlayerPiece.SetupPlayerPiece(i, newPlayerDetails.PlayerName, startingYardTile, scoredTile, playerStartingTiles[i], newPlayerDetails.isCPU);

                // add this piece to the player array
                Players[i].PlayerPieces[j] = newPlayerPiece;

            }

            // save the player details to PlayerPrefs
            newPlayerDetails.SavePlayerDetails();
        }

        //UIManager.instance.HidePlayerSelectScreen();
        UIManager.instance.ShowHideUIElement(UIManager.instance.PlayerSelectUI, false, GameManager.instance.State);

        // after the players have been created move the game on
        GameManager.instance.UpdateGameState(GameState.SetupGame);
    }

    public void ResetGame()
    {
        // reset the board
        BoardManager.instance.ResetBoard();

        //UIManager.instance.HideGameOverUI();
        UIManager.instance.ShowHideUIElement(UIManager.instance.GameOverUI, false, GameManager.instance.State);
        playersFinished = 0;

        PlayerPiece pp;

        // loop through the players
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].IsFinished = false;
            Players[i].FinishedPosition = 0;
            Players[i].MissedTurns = 0;
            // loop through the player pieces
            for (int j = 0; j < Players[i].PlayerPieces.Length; j++)
            {
                pp = Players[i].PlayerPieces[j];
                pp.SetupPlayerPiece(i, pp.PlayerName, pp.StartingYardTile, pp.ScoringTile, playerStartingTiles[i], Players[i].IsPlayerCPU);
            }
        }

        GameManager.instance.UpdateGameState(GameState.SetupGame);
    }

    public bool PlayerHasLegalMove()
    {
        // the dice has been rolled, does the player have a leagl move?
        // if not, move on to the next turn

        // first loop through all of the pieces for this player
        foreach (PlayerPiece pp in Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            // if any piece hs a legal move then the play can continue
            if (PlayerPieceHasLegalMove(pp))
                return true;
        }

        // if there are no legal moves, return false and the play moves on to the next player
        return false;
    }

    public int PlayerLegalMoveCount()
    {
        int moveCount = 0;
        foreach (PlayerPiece pp in Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            // if any piece hs a legal move then the play can continue
            if (PlayerPieceHasLegalMove(pp))
                moveCount++;
        }

        // if there are no legal moves, return false and the play moves on to the next player
        return moveCount;
    }

    public bool PlayerPieceHasLegalMove(PlayerPiece pp)
    {
        int diceTotal = DiceManager.instance.DiceTotal;

        // if the piece has already scored then it can't have any legal moves
        if (pp.IsScored)
            return false;

        // if the piece is in the yard
        if (pp.IsInYard)
        {
            // we rolled a 6, we can get out of the yard
            if (diceTotal == 6 || (SettingsManager.instance.Use1Or6ToEscapeYard && diceTotal == 1 && GetPiecesInYard().Length == 4))
                return true;
            // no getting out of the yard
            else
                return false;
        }

        // get the tiles ahead of this piece if it were to move the number on the dice
        Tile[] tilesAhead = BoardManager.instance.GetTilesAhead(pp, diceTotal);
        for (int i = 0; i < tilesAhead.Length; i++)
        {
            // if tilesAhead[i] is null that means we have overshot the end square, therefore we can't move
            if (tilesAhead[i] == null)
                return false;
        }

        return true;
    }

    public void HighlightPiecesWithLegalMove(int pId)
    {
        int moveCount = 0;
        PlayerPiece pieceToMove = null;

        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            if (PlayerPieceHasLegalMove(pp))
            {
                moveCount++;
                pieceToMove = pp;
                pp.OutlineLegalPiece();
            }
        }
        if (moveCount == 1 && SettingsManager.instance.AutomaticallyMoveIfOneLegalMove)
        {
            // we only have one possible move, so move it automatically if that setting is enabled
            pieceToMove.MovePiece(true);
        }
    }

    public void RemoveHighlightFromPlayerPieces(int pId)
    {
        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            pp.RemoveLegalPieceOutline();
        }
    }

    public PlayerPiece[] GetPiecesInYard()
    {
        return GetPiecesInYard(GameManager.instance.CurrentPlayerId);
    }

    public PlayerPiece[] GetPiecesInYard(int pId)
    {
        List<PlayerPiece> piecesInYard = new List<PlayerPiece>();

        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            if (!pp.IsScored && pp.IsInYard)
                piecesInYard.Add(pp);
        }

        return piecesInYard.ToArray();
    }

    public PlayerPiece[] GetPiecesOnBoard()
    {
        return GetPiecesOnBoard(GameManager.instance.CurrentPlayerId);
    }

    public PlayerPiece[] GetPiecesOnBoard(int pId)
    {
        List<PlayerPiece> piecesOnBoard = new List<PlayerPiece>();

        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            if (!pp.IsScored && !pp.IsInYard)
                piecesOnBoard.Add(pp);
        }

        return piecesOnBoard.ToArray();
    }

    public PlayerPiece[] GetScoredPieces()
    {
        return GetScoredPieces(GameManager.instance.CurrentPlayerId);
    }

    public PlayerPiece[] GetScoredPieces(int pId)
    {
        List<PlayerPiece> scoredPieces = new List<PlayerPiece>();

        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            if (pp.IsScored)
                scoredPieces.Add(pp);
        }

        return scoredPieces.ToArray();
    }
}
