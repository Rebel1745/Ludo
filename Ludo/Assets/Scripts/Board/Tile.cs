using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile NextTile; // the next tile to move to from this one
    public bool IsBranchTile; // a tile which boarder a players' 'safe zone'
    public Tile TileToBranchTo; // the first safe zone tile
    public int PlayerIdForBranch; // the Id of the player than can move to this safe zone
    public List<PlayerPiece> PlayerPieces; // the piece currently on this tile
    public bool IsScoringTile;
    public Transform[] DoublePiecePositions; // if there are two of the same players pieces on this square, put them on these positions
    public Transform[] TriplePiecePositions; // if there are three of the same players pieces on this square, put them on these positions
    public Transform[] QuadrouplePiecePositions; // if there are four of the same players pieces on this square, put them on these positions
}
