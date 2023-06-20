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

    private void Awake()
    {
        instance = this;
    }

    public void CreatePlayers()
    {
        GameObject newPlayerParent;
        GameObject newPlayer;
        PlayerPiece newPlayerPiece;
        string newPlayerName;

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
                IsPlayerCPU = false
            };

            // loop through the tiles inside the players yard
            for (int j = 0; j < playerYardHolder.GetChild(i).childCount; j++)
            {
                // create a player piece on the yard tile
                newPlayer = Instantiate(playerPrefab, playerYardHolder.GetChild(i).GetChild(j).position, Quaternion.identity, newPlayerParent.transform);

                newPlayerPiece = newPlayer.GetComponent<PlayerPiece>();
                newPlayerPiece.PlayerId = i;
                newPlayerPiece.startingPosition = playerYardHolder.GetChild(i).GetChild(j).position;
                newPlayerPiece.GetComponentInChildren<Renderer>().material = playerMaterials[i];
                newPlayerPiece.StartingTile = playerStartingTiles[i];
            }
        }

        // after the players have been created move the game on
        GameManager.instance.UpdateGameState(GameState.SetupGame);
    }
}
