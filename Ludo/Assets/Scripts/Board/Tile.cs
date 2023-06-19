using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile NextTile; // the next tile to move to from this one
    public bool IsBranchTile; // a tile which boarder a players' 'safe zone'
    public Tile TileToBranchTo; // the first safe zone tile
    public int PlayerIdForBranch; // the Id of the player than can move to this safe zone
}
