using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Tile[] GetTilesAhead(PlayerPiece pp, int spacesAhead)
    {
        Tile[] tilesAhead = new Tile[spacesAhead];
        Tile currentTile = pp.CurrentTile;
        Tile destTile;

        for (int i = 0; i < GameManager.instance.DiceTotal; i++)
        {
            // if there is no next tile then we have reached the end
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
