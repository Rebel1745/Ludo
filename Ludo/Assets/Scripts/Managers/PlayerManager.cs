using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [SerializeField] Transform playerYardHolder; // the holding area for player pieces
    [SerializeField] Material[] playerMaterials;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Tile[] playerStartingTiles;
    [SerializeField] Transform playerScoredTileHolder;
    [SerializeField] GameObject[] allPlayerDetails;

    public Player[] Players;

    // animation
    bool isAnimating = false;
    List<PlayerPieceMovement> movementList;
    Vector3 newPosition;
    Vector3 velocity = Vector3.zero;
    [SerializeField] float smoothTime = 0.25f;
    float timeToMove;

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
            SelectCPUPiece();

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
        if(GameManager.instance.CurrentPlayerRollAgainCount >= GameManager.instance.MaximumRollAgain)
        {
            // we have exceeded the total number of 6's in a row, do something about it
            PlayerPiece pieceToSendHome = GetMostAdvancedPiece();
            if (pieceToSendHome != null)
                pieceToSendHome.SendPieceHome(pieceToSendHome, GameManager.instance.MaximumRollAgain + " 6's thrown in a row. Go back home!", true);
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.CheckForLegalMove);
        }
    }

    PlayerPiece GetMostAdvancedPiece()
    {
        PlayerPiece piece = null;
        int furthestDistance = -1;

        foreach (PlayerPiece pp in Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            if(!pp.IsInYard && !pp.IsScored && pp.TotalDistanceTravelled > furthestDistance)
            {
                piece = pp;
                furthestDistance = pp.TotalDistanceTravelled;
            }
        }

        return piece;
    }

    // This is the AI portion of the code
    void SelectCPUPiece()
    {
        List<PlayerPiece> legalPieces = new List<PlayerPiece>();

        // fisrt get all of the pieces that can make a move
        foreach (PlayerPiece pp in Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            if (PlayerPieceHasLegalMove(pp))
                legalPieces.Add(pp);
        }

        // select a random piece from this list to move
        // TODO: make this AI more complicated
        legalPieces[Random.Range(0, legalPieces.Count)].BuildMovementList();
    }

    void MovePieces()
    {
        // is the movement instant?
        timeToMove = movementList[0].IsInstantMovement ? 0 : smoothTime;

        // first check if we have reached the target of the first element in the moveList
        if (movementList[0].DestinationTile == null)
            newPosition = movementList[0].DestinationPosition;
        else
            newPosition = movementList[0].DestinationTile.transform.position;

        if (Vector3.Distance(movementList[0].PieceToMove.transform.position, newPosition) < 0.01)
        {
            // we are at our destination, update the movementList to remove this movement
            AdvanceMovementList();
        }
        else
        {
            // TODO: Update this with the ability to add height to the movement
            movementList[0].PieceToMove.transform.position = Vector3.SmoothDamp(movementList[0].PieceToMove.transform.position, newPosition, ref velocity, timeToMove);
        }        
    }

    void AdvanceMovementList()
    {
        if(movementList.Count == 1)
        {
            // we have finished our list of movements
            // TODO: Check if this was a scoring move, if it was, check if all the players pieces have been scored
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
            if(movementList[0].InfoTextToDisplay != null && movementList[0].InfoTextToDisplay != "")
                GameManager.instance.SetInfoText(movementList[0].InfoTextToDisplay);
        }
    }

    void CheckScoringMove(PlayerPiece playerPiece)
    {
        playerPiece.IsScored = true;

        bool allPiecesScored = true;

        foreach(PlayerPiece pp in Players[playerPiece.PlayerId].PlayerPieces)
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
        else{
            movementList.Clear();
            MoveToNextTurn();
        }
    }

    void SetPlayerFinished(int pId)
    {
        playersFinished++;

        Players[pId].IsFinished = true;
        Players[pId].FinishedPosition = playersFinished;

        if (playersFinished == Players.Length - 1)
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
            GameManager.instance.UpdateGameState(GameState.GameOver);
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
        if (GameManager.instance.DiceTotal == 6 && GameManager.instance.CurrentPlayerRollAgainCount < GameManager.instance.MaximumRollAgain)
            GameManager.instance.UpdateGameState(GameState.RollAgain);
        else
            GameManager.instance.UpdateGameState(GameState.NextTurn);
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

        GameObject playersHolder = new GameObject { name = "Players" };

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
                newPlayerPiece.SetupPlayerPiece(i, newPlayerDetails.PlayerName, startingYardTile, scoredTile, playerStartingTiles[i]);

                // add this piece to the player array
                Players[i].PlayerPieces[j] = newPlayerPiece;

                // set the PlayerPiece on the tile
                startingYardTile.PlayerPieces.Add(newPlayerPiece);
            }

            // save the player details to PlayerPrefs
            newPlayerDetails.SavePlayerDetails();
        }

        UIManager.instance.HidePlayerSelectScreen();

        // after the players have been created move the game on
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
            if(PlayerPieceHasLegalMove(pp))
                return true;
        }

        // if there are no legal moves, return false and the play moves on to the next player
        return false;
    }

    public bool PlayerPieceHasLegalMove(PlayerPiece pp)
    {
        int diceTotal = GameManager.instance.DiceTotal;

        // if the piece has already scored then it can't have any legal moves
        if (pp.IsScored)
            return false;

        // if the piece is in the yard
        if (pp.IsInYard)
        {
            // we rolled a 6, we can get out of the yard
            if (diceTotal == 6)
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
        foreach(PlayerPiece pp in Players[pId].PlayerPieces)
        {
            if (PlayerPieceHasLegalMove(pp))
                pp.OutlineLegalPiece();
        }
    }

    public void RemoveHighlightFromPlayerPieces(int pId)
    {
        foreach (PlayerPiece pp in Players[pId].PlayerPieces)
        {
            pp.RemoveLegalPieceOutline();
        }
    }
}
