using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [SerializeField] Transform playerYardHolder; // the holding area for player pieces
    [SerializeField] Material[] playerMaterials;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform playersHolder;
    [SerializeField] Tile[] playerStartingTiles;

    public Player[] Players;

    // animation
    bool isAnimating = false;
    List<PlayerPieceMovement> movementList;
    Vector3 velocity = Vector3.zero;
    float smoothTime = 0.25f;

    private void Awake()
    {
        instance = this;
        movementList = new List<PlayerPieceMovement>();
    }

    private void Update()
    {
        if (GameManager.instance.State != GameState.WaitingForAnimation || !isAnimating)
            return;

        if (movementList.Count == 0)
            return;

        MovePieces();
    }

    void MovePieces()
    {
        // first check if we have reached the target of the first element in the moveList
        if(Vector3.Distance(movementList[0].PieceToMove.transform.position, movementList[0].DestinationTile.transform.position) < 0.01)
        {
            // we are at our destination, update the movementList to remove this movement
            AdvanceMovementList();
        }
        else
        {
            movementList[0].PieceToMove.transform.position = Vector3.SmoothDamp(movementList[0].PieceToMove.transform.position, movementList[0].DestinationTile.transform.position, ref velocity, smoothTime);
        }        
    }

    void AdvanceMovementList()
    {
        if(movementList.Count == 1)
        {
            // we have finished our list of movements
            isAnimating = false;
            movementList.Clear();
            MoveToNextTurn();
        }
        else
        {
            // we still have movements left, remove the one we have just performed
            movementList.RemoveAt(0);
        }
    }

    
    void MoveToNextTurn()
    {
        //allow another roll if a 6 was rolled, otherwise move on to next turn
        if (GameManager.instance.DiceTotal == 6)
            GameManager.instance.UpdateGameState(GameState.RollAgain);
        else
            GameManager.instance.UpdateGameState(GameState.NextTurn);
    }

    public void SetMoveQueue(List<PlayerPieceMovement> ppms)
    {
        movementList = ppms;
        isAnimating = true;
        GameManager.instance.UpdateGameState(GameState.WaitingForAnimation);
    }

    public void CreatePlayers()
    {
        GameObject newPlayerParent;
        GameObject newPlayer;
        PlayerPiece newPlayerPiece;
        string newPlayerName;
        Tile startingYardTile;

        // init players array
        Players = new Player[playerYardHolder.childCount];

        // loop through the player yards
        for (int i = 0; i < playerYardHolder.childCount; i++)
        {
            newPlayerName = "Player " + (i + 1);

            // create an empty game object for this parent, inside the playersHolder
            newPlayerParent = new GameObject
            {
                name = newPlayerName + " Pieces"
            };

            newPlayerParent.transform.parent = playersHolder;

            // setup the player details
            Players[i] = new Player
            {
                PlayerId = i,
                PlayerName = newPlayerName,
                IsPlayerCPU = false,
                PlayerPieces = new PlayerPiece[playerYardHolder.GetChild(i).childCount]
            };

            // loop through the tiles inside the players yard
            for (int j = 0; j < playerYardHolder.GetChild(i).childCount; j++)
            {
                startingYardTile = playerYardHolder.GetChild(i).GetChild(j).GetComponent<Tile>();
                // create a player piece on the yard tile
                newPlayer = Instantiate(playerPrefab, startingYardTile.transform.position, Quaternion.identity, newPlayerParent.transform);

                // TODO: move the below into a function on the PlayerPiece
                newPlayerPiece = newPlayer.GetComponent<PlayerPiece>();
                newPlayerPiece.PlayerId = i;
                //newPlayerPiece.StartingPositionInYard = playerYardHolder.GetChild(i).GetChild(j).position;
                newPlayerPiece.StartingYardTile = startingYardTile;
                newPlayerPiece.GetComponentInChildren<Renderer>().material = playerMaterials[i];
                newPlayerPiece.StartingTile = playerStartingTiles[i];

                // add this piece to the player array
                Players[i].PlayerPieces[j] = newPlayerPiece;
            }
        }

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
            if(PlayerPieceHasLegalMove(pp))
                return true;
        }

        return false;
    }

    public bool PlayerPieceHasLegalMove(PlayerPiece pp)
    {
        // if a 6 is rolled, we can always do someting. 
        //TODO: This is not true when only one piece remains and it is in the safe zone
        if (GameManager.instance.DiceTotal == 6)
            return true;

        // roll 1 - 5 at least one piece is out of the yard - can move
        if (!pp.IsInYard && !pp.IsScored)
            return true;

        return false;
    }
}
