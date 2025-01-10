using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int PlayerId;
    public string PlayerName;
    public Color PlayerColour;
    public bool IsPlayerCPU = false;
    public AIType CPUType;
    public PlayerPiece[] PlayerPieces;
    public bool IsFinished = false;
    public int FinishedPosition = 0;
    public int NumberOfFinishedPieces
    {
        get
        {
            int finishedPieces = 0;
            foreach (PlayerPiece pp in PlayerPieces)
            {
                if (pp.IsScored) finishedPieces++;
            }
            return finishedPieces;
        }
    }
    public int CumulativeNonFinishedPiecesDistance
    {
        get
        {
            int totalNonFinishedPiecesDistance = 0;
            foreach (PlayerPiece pp in PlayerPieces)
            {
                if (!pp.IsScored) totalNonFinishedPiecesDistance += pp.TotalDistanceTravelled;
            }
            return totalNonFinishedPiecesDistance;
        }
    }
    // the finishing order number is used when the game finishes after only one player finishes
    // this number will allow the players to be orderd on the basis of their completion of the game
    // first, they are orderd on how many pieces have finished, then by how far along their unfinished pieces are
    public int FinishingOrderNumber
    {
        get
        {
            return int.Parse(((NumberOfFinishedPieces * 1000) + CumulativeNonFinishedPiecesDistance).ToString());
        }
    }
}
