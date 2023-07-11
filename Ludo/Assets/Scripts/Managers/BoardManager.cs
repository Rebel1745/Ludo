using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    [SerializeField] Transform boardParent;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject scoredTilePrefab;
    [SerializeField] int crossLength = 5; // the number of tiles making up the board shape for the longer edge portions
    readonly int crossWidth = 3; // the number of tiles making up the board shape for the smaller edge portions. TODO: Allow this to be changed to increase the size? Would require more calculations in the creation code

    GameObject safeZoneHolder;

    // important tiles
    GameObject scoringTileGO;
    [SerializeField] Material scoringTileMaterial;
    [SerializeField] Material[] playerTileColours;
    Vector3[] safeZoneDirections = { Vector3.back, Vector3.left, Vector3.forward, Vector3.right }; // which directions do the safe zones go relative to the scoring tile
    Vector3[] gameBoardDirections = {
        Vector3.forward, Vector3.left, Vector3.forward, // starting from the player1 start position moving up the board
        Vector3.right, Vector3.forward, Vector3.right,// starting from the player2 start position moving up the board
        Vector3.back, Vector3.right, Vector3.back, // starting from the player3 start position moving down the board
        Vector3.left, Vector3.back, Vector3.left // starting from the player4 start position moving down the board
    };

    GameObject newTileGO, prevTileGO = null, firstTileGO;
    Tile newTile, prevTile = null, firstTile;
    
    // different tile holders
    GameObject yardHolder, scoredHolder, tileHolder;
    Tile[] startingTiles = new Tile[4];

    private void Awake()
    {
        instance = this;
    }

    public void BuildBoard()
    {
        ClearBoard();
        CreateScoringTile();
        CreateSafeZones();
        CreateSurroundingTiles();
        CreateYardTiles();
        CreateScoredPiecesTiles();

        // after the board has been created, move on to the player select phase
        PlayerManager.instance.InitialiseBoardHolders(yardHolder.transform, startingTiles, scoredHolder.transform);
    }

    public void ResetBoard()
    {
        Tile[] allTiles = boardParent.GetComponentsInChildren<Tile>();

        for (int i = 0; i < allTiles.Length; i++)
        {
            if(allTiles[i].PlayerPieces.Count != 0)
            {
                // the player that finished 4th would still have had piece(s) on the board, remove them from the tiles when we start again
                allTiles[i].PlayerPieces.Clear();
            }
        }
    }

    void CreateScoredPiecesTiles()
    {
        scoredHolder = new GameObject { name = "Scored Pieces" };
        scoredHolder.transform.parent = boardParent;

        Vector3 startPos, tilePos;
        int scoredNo;

        for (int i = 0; i < 4; i++)
        {
            GameObject playerScoredHolder = new GameObject { name = "Player " + (i + 1) + " Scored Pieces" };
            playerScoredHolder.transform.parent = scoredHolder.transform;
            
            startPos = (safeZoneDirections[i] + safeZoneDirections[(i + 1) % 4]) * (crossLength + 1);            
            scoredNo = 0;

            for (int j = 0; j < 4; j++)
            {
                scoredNo++;

                if(i % 2 == 0)
                {
                    tilePos = startPos - (safeZoneDirections[i]) * j;
                    tilePos.z -= safeZoneDirections[i].z;
                }
                else
                {
                    tilePos = startPos - (safeZoneDirections[(i + 1) % 4] * j);
                    tilePos.z -= safeZoneDirections[(i + 1) % 4].z;
                }                    

                newTileGO = Instantiate(scoredTilePrefab, tilePos, Quaternion.identity, playerScoredHolder.transform);
                newTileGO.name = "Player" + (i + 1) + "Scored" + scoredNo;
            }
        }
    }

    void CreateYardTiles()
    {
        yardHolder = new GameObject { name = "Player Yards"  };
        yardHolder.transform.parent = boardParent;

        Vector3 startPos, tilePos;
        int yardNo = 0;

        for (int i = 0; i < 4; i++)
        {
            GameObject playerYardHolder = new GameObject { name = "Player " + (i + 1) + " Yard" };
            playerYardHolder.transform.parent = yardHolder.transform;

            // TODO: Change the '*3' to a variable to scale nicely for other board sizes
            startPos = (safeZoneDirections[i] + safeZoneDirections[(i + 1) % 4]) * 3;
            yardNo = 0;

            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    yardNo++;

                    // TODO: I'm not really sure how this bit works... maybe clarify it in my mind a bit?
                    if(i % 2 == 0)
                        tilePos = new Vector3(startPos.x + safeZoneDirections[(i + 1) % 4].x * k, 0f, startPos.z + safeZoneDirections[i].z * j);
                    else
                        tilePos = new Vector3(startPos.x + safeZoneDirections[i].x * k, 0f, startPos.z + safeZoneDirections[(i + 1) % 4].z * j);

                    newTileGO = Instantiate(tilePrefab, tilePos, Quaternion.identity, playerYardHolder.transform);
                    newTileGO.GetComponentInChildren<Renderer>().material = playerTileColours[i];
                    newTileGO.name = "Player" + (i + 1) + "Yard" + yardNo;
                    newTile = newTileGO.GetComponent<Tile>();
                    newTile.IsYardTile = true;
                }
            }
        }



    }

    void CreateSurroundingTiles()
    {
        int[] gameBoardLengths = {
            crossLength, crossLength, crossWidth,
            crossLength, crossLength, crossWidth,
            crossLength, crossLength, crossWidth,
            crossLength, crossLength, crossWidth
        };

        tileHolder = new GameObject { name = "Game Tiles" };
        tileHolder.transform.parent = boardParent;

        // start on the player1 starting square
        float startPosX = -(crossWidth - 1) / 2;
        float startPosZ = -crossLength;
        Vector3 nextPos = new Vector3(startPosX, 0f, startPosZ);

        int dirCount = 0, tileCount = 0;

        // reset the previous tile info
        prevTileGO = null;
        prevTile = null;

        // loop through all of the board directions
        for (int i = 0; i < gameBoardDirections.Length; i++)
        {
            dirCount++;
            // loop through the tile lengths for each direction
            for (int j = 0; j < gameBoardLengths[i] - 1; j++)
            {
                tileCount++;

                newTileGO = Instantiate(tilePrefab, nextPos, Quaternion.identity);
                newTileGO.transform.parent = tileHolder.transform;
                newTile = newTileGO.GetComponent<Tile>();

                // should this tile be a starting square?
                if (dirCount % 3 == 1 && j == 0)
                {
                    newTileGO.GetComponentInChildren<Renderer>().material = playerTileColours[Mathf.FloorToInt(dirCount / 3)];
                    newTileGO.name = "Player" + (Mathf.FloorToInt(dirCount / 3) + 1) + "StartingSquare";
                    startingTiles[Mathf.FloorToInt(dirCount / 3)] = newTile;

                    if(prevTileGO != null)
                    {
                        // the tile before a starting square is a branching square for that player
                        CreateBranchingTile(prevTileGO, prevTile, Mathf.FloorToInt(dirCount / 3));
                    }
                }
                else
                {
                    newTileGO.name = "Tile" + tileCount;
                }

                if(prevTileGO != null)
                {
                    prevTile.NextTile = newTile;
                }

                nextPos += gameBoardDirections[i];
                prevTileGO = newTileGO;
                prevTile = newTile;

                if(tileCount == 1)
                {
                    firstTile = newTile;
                    firstTileGO = newTileGO;
                }
            }
        }

        // the final tile will also be a branching tile for player1
        CreateBranchingTile(prevTileGO, prevTile, 0);
        // player1's branchingtile also as the nextTile as the first tile
        prevTile.NextTile = firstTile;
    }

    void CreateBranchingTile(GameObject tileGO, Tile tile, int playerId)
    {
        tileGO.name = "Player" + (playerId + 1) + "BranchingSquare";
        tile.IsBranchTile = true;
        tile.PlayerIdForBranch = playerId;
        // TODO: If the tiles are rearranged in CreateSafeZones() the second getChild will need to be changed to GetChild(0)
        tile.TileToBranchTo = safeZoneHolder.transform.GetChild(playerId).GetChild(safeZoneHolder.transform.GetChild(playerId).childCount - 1).GetComponent<Tile>();
    }

    void CreateSafeZones()
    {
        safeZoneHolder = new GameObject { name = "Safe Zones" };
        safeZoneHolder.transform.parent = boardParent;

        int safeZoneLength = crossLength - 1;
        Vector3 pos;

        // TODO: change the below to use the details of the Players[] (i.e. name). 
        //So change the BuildBoard to run after the players have been selected, but before the player pieces have been created
        for (int i = 0; i < 4; i++)
        {
            // reset the previous tile info
            prevTileGO = null;
            prevTile = null;

            // loop through the players
            GameObject playerHolder = new GameObject { name = "Player " + (i + 1) };
            playerHolder.transform.parent = safeZoneHolder.transform;

            // TODO: Make the array decrement instead of increment so that the tiles are labled in the correct direction
            for (int j = 0; j < safeZoneLength; j++)
            {
                pos = scoringTileGO.transform.position + (safeZoneDirections[i] * ( j + 1));
                newTileGO = Instantiate(tilePrefab, pos, Quaternion.identity, playerHolder.transform);
                newTileGO.name = "Player" + (i + 1) + "Safe" + (j + 1);
                newTileGO.GetComponentInChildren<Renderer>().material = playerTileColours[i];
                newTile = newTileGO.GetComponent<Tile>();
                newTile.IsSafeTile = true;

                // if this is the first safe zone tile, the next tile will be the scoring tile
                if (j == 0)
                    newTile.NextTile = scoringTileGO.GetComponent<Tile>();
                else
                    newTile.NextTile = prevTile;

                prevTileGO = newTileGO;
                prevTile = newTile;
            }
        }
    }

    void CreateScoringTile()
    {
        GameObject scoringTileHolder = new GameObject { name = "Scoring Tile" };

        scoringTileHolder.transform.parent = boardParent;

        scoringTileGO = Instantiate(tilePrefab, scoringTileHolder.transform);
        scoringTileGO.name = "Scoring Tile";
        scoringTileGO.GetComponentInChildren<Renderer>().material = scoringTileMaterial;

        Tile scoringTile = scoringTileGO.GetComponent<Tile>();
        scoringTile.IsScoringTile = true;
    }

    void ClearBoard()
    {
        for (int i = 0; i < boardParent.childCount; i++)
        {
            Destroy(boardParent.GetChild(i).gameObject);
        }
    }

    public Tile[] GetTilesAhead(PlayerPiece pp, int spacesAhead)
    {
        Tile[] tilesAhead = new Tile[spacesAhead];
        Tile currentTile = pp.CurrentTile;
        Tile destTile;

        // if this move would take us out of the yard then tilesAhead would just be the starting tile
        if(pp.IsInYard && spacesAhead == 6)
        {
            tilesAhead = new Tile[1];
            tilesAhead[0] = pp.StartingTile;
            return tilesAhead;
        }

        for (int i = 0; i < GameManager.instance.DiceTotal; i++)
        {
            // if there is no next tile then we have reached the scoring
            // break out of the loops and return null values for tiles that aren't there
            if (currentTile.NextTile == null)
                break;

            if (currentTile.IsBranchTile && currentTile.PlayerIdForBranch == GameManager.instance.CurrentPlayerId)
                destTile = currentTile.TileToBranchTo;
            else
                destTile = currentTile.NextTile;

            tilesAhead[i] = destTile;
            currentTile = destTile;
        }

        return tilesAhead;
    }

    public Tile GetTileAhead(PlayerPiece pp, int spacesAhead)
    {
        Tile[] tilesAhead = GetTilesAhead(pp, spacesAhead);

        return tilesAhead[tilesAhead.Length - 1];
    }

    public bool TileContainsOpponentsPiece(Tile tile)
    {
        // if there are no pieces on this tile, all good
        if (tile.PlayerPieces.Count == 0)
            return false;

        // check  each of the pieces on this tile, if one has a different Id to the current player, we have landed on an enemy
        foreach (PlayerPiece pp in tile.PlayerPieces)
        {
            if (pp.PlayerId != GameManager.instance.CurrentPlayerId)
                return true;
        }

        return false;
    }

}
