using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int PlayerId;
    public string PlayerName;
    public Color PlayerColour;
    public bool IsPlayerCPU = false;
    public PlayerPiece[] PlayerPieces;
    public bool IsFinished = false;
    public int FinishedPosition = 0;
}
