using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    // Player info
    public int PlayerId;
    public string PlayerName;
    public bool IsCPU;
    //public Vector3 StartingPositionInYard;
    public Tile StartingYardTile;
    public Tile StartingTile;
    public bool IsInYard = true; // all pieces start in their yard
    public bool IsScored = false;
    public Tile currentTile;

    bool isAnimating = false;
    public bool isCurrentPlayerAnimation = false;
    int spacesToMove;

    // movement variables
    Tile[] moveQueue;
    int moveQueueIndex;
    Vector3 targetPosition;
    Vector3 velocity = Vector3.zero;
    [SerializeField] float smoothTime = 0.25f;
    [SerializeField] int smoothTimeMultiplier = 1;
    float smoothDistance = 0.01f;
    float maxHeight = 0.5f;
    [SerializeField] int maxHeightMultiplier = 1;
    [SerializeField] AnimationCurve heightCurve;
    Vector3 targetPositionWithHeight;
    float heightTime;
    float targetHeight;

    // new movement variables
    List<PlayerPieceMovement> movementList;

    void Start()
    {
        movementList = new List<PlayerPieceMovement>();
    }

    private void OnMouseUp()
    {
        // are we waiting for a click?
        if (GameManager.instance.State != GameState.WaitingForClick)
            return;

        // Check if this piece belongs to us, if not then we can't click on it
        if (PlayerId != GameManager.instance.CurrentPlayerId)
            return;

        if (!PlayerManager.instance.PlayerPieceHasLegalMove(this))
            return;

        //SelectPiece();

        BuildMovementList();
    }

    void BuildMovementList()
    {
        movementList.Clear();

        // if we are on a tile then remove this piece from it
        if (currentTile != null)
            currentTile.PlayerPiece = null;

        PlayerPieceMovement newMovement;

        if (IsInYard && GameManager.instance.DiceTotal == 6)
        {
            newMovement = new PlayerPieceMovement
            {
                PieceToMove = this,
                DestinationTile = StartingTile,
                InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " moving to starting square"
            };

            movementList.Add(newMovement);
            currentTile = StartingTile;
            IsInYard = false;
        }
        else
        {
            for (int i = 0; i < GameManager.instance.DiceTotal; i++)
            {
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationTile = currentTile.NextTile,
                    InfoTextToDisplay = GameManager.instance.GetInfoText() + " ... " + i
                };

                movementList.Add(newMovement);
                currentTile = currentTile.NextTile;
            }
        }

        // now check to see if we will land on anyone and if we do add them to the movement list with their destination as their yard
        // if there is a piece on the final tile, and that piece does not belong to the player currently moving
        if(currentTile.PlayerPiece != null && currentTile.PlayerPiece.PlayerId != GameManager.instance.CurrentPlayerId)
        {
            newMovement = new PlayerPieceMovement
            {
                PieceToMove = currentTile.PlayerPiece,
                DestinationTile = currentTile.PlayerPiece.StartingYardTile,
                InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " landed on " + currentTile.PlayerPiece.PlayerName + ". Sending " + currentTile.PlayerPiece.PlayerName + " back home"
            };

            movementList.Add(newMovement);

            // set the player going home as having no currentTile
            currentTile.PlayerPiece.currentTile = null;
            currentTile.PlayerPiece.IsInYard = true;
        }

        // finally, set this PlayerPiece to be on the currentTile
        currentTile.PlayerPiece = this;

        PlayerManager.instance.SetMoveQueue(movementList);
    }
}
