using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPieceMovement
{
    public PlayerPiece PieceToMove;
    public Tile DestinationTile;
    public Vector3 DestinationPosition;
    public string InfoTextToDisplay;
    public bool IsScoringMove = false;
    public bool IsInstantMovement = false;
    public bool PlaySound = false;
    public AudioClip SoundToPlay;
    public float minimumPitch = 1f;
    public float maximumPitch = 1f;
}
