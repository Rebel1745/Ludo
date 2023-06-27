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
    public Tile StartingYardTile;
    public Tile ScoringTile;
    public Tile StartingTile;
    public bool IsInYard = true; // all pieces start in their yard
    public bool IsScored = false;
    public Tile CurrentTile;
    
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

    public void BuildMovementList()
    {
        Tile[] tilesAhead = new Tile[GameManager.instance.DiceTotal];

        movementList.Clear();

        // if we are on a tile then remove this piece from it
        if (CurrentTile != null)
            CurrentTile.PlayerPieces.Remove(this);

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
            CurrentTile = StartingTile;
            IsInYard = false;
        }
        else
        {
            tilesAhead = PlayerManager.instance.GetTilesAhead(this, GameManager.instance.DiceTotal);

            for (int i = 0; i < tilesAhead.Length; i++)
            {
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationTile = tilesAhead[i],
                    InfoTextToDisplay = GameManager.instance.GetInfoText() + " ... " + i
                };

                movementList.Add(newMovement);
                CurrentTile = tilesAhead[i];
            }
        }

        // now check to see if we will land on anyone and if we do add them to the movement list with their destination as their yard
        // if there is a piece on the final tile, and that piece does not belong to the player currently moving
        if(PlayerManager.instance.TileContainsOpponentsPiece(CurrentTile))
        {
            // Add a new movement for each of the pieces on this tile
            // TODO: if there is more than one opponents piece on the tile, should we send this player home instead of the opponent?
            foreach(PlayerPiece pp in CurrentTile.PlayerPieces)
            {
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = pp,
                    DestinationTile = pp.StartingYardTile,
                    InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " landed on " + pp.PlayerName + ". Sending " + pp.PlayerName + " back home"
                };

                movementList.Add(newMovement);

                // set the player going home as having no CurrentTile
                pp.CurrentTile = null;
                pp.IsInYard = true;
            }
        }
        else
        {
            // the current tile doesn't contain any opponent pieces, does it include any of our own?
            if(CurrentTile.PlayerPieces.Count == 1)
            {
                // there is one of our pieces on this tile, move it to the first DoublePiecePositions location, and this tile to the other
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[0],
                    DestinationPosition = CurrentTile.DoublePiecePositions[0].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationPosition = CurrentTile.DoublePiecePositions[1].position
                };

                movementList.Add(newMovement);
            }
            else if(CurrentTile.PlayerPieces.Count == 2)
            {
                // there are two of our pieces on this tile, move them to the TriplePiecePositions locations
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[0],
                    DestinationPosition = CurrentTile.TriplePiecePositions[0].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[1],
                    DestinationPosition = CurrentTile.TriplePiecePositions[1].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationPosition = CurrentTile.TriplePiecePositions[2].position
                };

                movementList.Add(newMovement);
            }
            else if (CurrentTile.PlayerPieces.Count == 3)
            {
                // there are three of our pieces on this tile, move them to the QuadrouplePiecePositions locations
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[0],
                    DestinationPosition = CurrentTile.QuadrouplePiecePositions[0].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[1],
                    DestinationPosition = CurrentTile.QuadrouplePiecePositions[1].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = CurrentTile.PlayerPieces[2],
                    DestinationPosition = CurrentTile.QuadrouplePiecePositions[2].position
                };

                movementList.Add(newMovement);

                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationPosition = CurrentTile.QuadrouplePiecePositions[3].position
                };

                movementList.Add(newMovement);
            }
        }

        // set this PlayerPiece to be on the CurrentTile
        // this code comes before the scoring check, as if a tile scores, it is removed from the board and from the centre tile
        CurrentTile.PlayerPieces.Add(this);

        // check the current tile to see if it is the scoring tile.
        // if it is, add a movement to the 'scored' area
        if (CurrentTile.IsScoringTile)
        {
            // this piece has scored
            newMovement = new PlayerPieceMovement
            {
                PieceToMove = this,
                DestinationTile = ScoringTile,
                InfoTextToDisplay = "Congratulations, this piece has scored!",
                IsScoringMove = true
            };

            movementList.Add(newMovement);
            //TODO: Instead of scoring the piece now, wait until after the animation has finished and then score it.  Perhaps in AdvanceMovementList
            this.IsScored = true;
            // remove this piece from the centre tile
            CurrentTile.PlayerPieces.Remove(this);
        }

        PlayerManager.instance.SetMoveQueue(movementList);
    }
}
