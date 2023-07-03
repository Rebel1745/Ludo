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

    // Highlighting
    [Space()]
    [Header("Highlighting")]
    [SerializeField] Outline pieceOutline;
    [SerializeField] Color defaultOutlineColour = Color.black;
    [SerializeField] float defaultOutlineWidth = 3f;
    [SerializeField] Color selectableOutlineColour = Color.white;
    [SerializeField] float mouseOverOutlineWidth = 5f;


    void Start()
    {
        movementList = new List<PlayerPieceMovement>();
    }

    public void SetupPlayerPiece(int id, string playerName, Tile yardTile, Tile scoredTile, Tile startingTile)
    {
        PlayerId = id;
        PlayerName = playerName;
        StartingYardTile = yardTile;
        CurrentTile = yardTile;
        ScoringTile = scoredTile;
        StartingTile = startingTile;
    }

    public void OutlineLegalPiece()
    {
        pieceOutline.OutlineColor = selectableOutlineColour;
        pieceOutline.OutlineWidth = defaultOutlineWidth;
    }

    public void RemoveLegalPieceOutline()
    {
        pieceOutline.OutlineColor = defaultOutlineColour;
        pieceOutline.OutlineWidth = defaultOutlineWidth;
    }

    void ShowPossiblePieceMovement()
    {
        Tile[] tilesAhead = BoardManager.instance.GetTilesAhead(this, GameManager.instance.DiceTotal);

        // first, show the stuff from the current tile
        CurrentTile.MovementIndicator.SetActive(true);
        CurrentTile.RotationPoint.LookAt(tilesAhead[0].RotationPoint);

        for (int i = 0; i < tilesAhead.Length; i++)
        {
            tilesAhead[i].CentreBall.SetActive(true);

            // se only want to indicate to the next tile if there is one
            if (i < tilesAhead.Length - 1)
            {
                tilesAhead[i].MovementIndicator.SetActive(true);
                tilesAhead[i].RotationPoint.LookAt(tilesAhead[i + 1].RotationPoint);
            }            
        }
    }

    void RemovePossiblePieceMovement()
    {
        Tile[] tilesAhead = BoardManager.instance.GetTilesAhead(this, GameManager.instance.DiceTotal);

        CurrentTile.MovementIndicator.SetActive(false);

        for (int i = 0; i < tilesAhead.Length; i++)
        {
            if(tilesAhead[i] != null)
            {
                tilesAhead[i].CentreBall.SetActive(false);
                tilesAhead[i].MovementIndicator.SetActive(false);
            }
            else
            {
                Debug.LogError("Seems tilesAhead[" + i + "] is null, what could have caused that?");
            }
        }
    }

    private void OnMouseOver()
    {
        if (CanWeClickIt())
        {
            pieceOutline.OutlineColor = selectableOutlineColour;
            pieceOutline.OutlineWidth = mouseOverOutlineWidth;

            // we only want to see a possible move, if we are on the board
            if(!this.IsInYard)
                ShowPossiblePieceMovement();
        }
    }

    private void OnMouseExit()
    {
        if (CanWeClickIt())
            OutlineLegalPiece();
        else
            RemoveLegalPieceOutline();

        if(!this.IsInYard)
            RemovePossiblePieceMovement();
    }

    bool CanWeClickIt()
    {
        // are we waiting for a click?
        if (GameManager.instance.State != GameState.WaitingForClick)
            return false;

        // Check if this piece belongs to us, if not then we can't click on it
        if (PlayerId != GameManager.instance.CurrentPlayerId)
            return false;

        if (!PlayerManager.instance.PlayerPieceHasLegalMove(this))
            return false;

        // YES WE CAN!
        return true;
    }

    private void OnMouseUp()
    {
        if (CanWeClickIt())
        {
            if(!this.IsInYard)
                RemovePossiblePieceMovement();
            BuildMovementList();
        }            
    }

    public void BuildMovementList()
    {
        Tile[] tilesAhead = new Tile[GameManager.instance.DiceTotal];

        movementList.Clear();

        // if we are on a tile then remove this piece from it
        //if (CurrentTile != null)
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
            tilesAhead = BoardManager.instance.GetTilesAhead(this, GameManager.instance.DiceTotal);

            for (int i = 0; i < tilesAhead.Length; i++)
            {
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationTile = tilesAhead[i]
                    // TODO: sort out some text for moving a piece
                };

                movementList.Add(newMovement);
                CurrentTile = tilesAhead[i];
            }
        }

        // now check to see if we will land on anyone and if we do add them to the movement list with their destination as their yard
        // if there is a piece on the final tile, and that piece does not belong to the player currently moving
        if(BoardManager.instance.TileContainsOpponentsPiece(CurrentTile))
        {
            // we have landed on piece with at least one opponent on it, if it is only one, remove it. if not, remove us!
            if(CurrentTile.PlayerPieces.Count == 1)
            {
                PlayerPiece pieceToSendHome = CurrentTile.PlayerPieces[0];
                // there is only one piece on this tile, send it home
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = pieceToSendHome,
                    DestinationTile = pieceToSendHome.StartingYardTile,
                    InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " landed on " + pieceToSendHome.PlayerName + ". Sending them back home"
                };

                movementList.Add(newMovement);
                pieceToSendHome.CurrentTile = pieceToSendHome.StartingYardTile;
                pieceToSendHome.IsInYard = true;
                CurrentTile.PlayerPieces.Remove(pieceToSendHome);
            }
            else
            {
                // there are more than on opponent pieces on this tile, send us home
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = this,
                    DestinationTile = this.StartingYardTile,
                    InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " landed on " + this.PlayerName + ". Sending them back home"
                };

                movementList.Add(newMovement);
                this.CurrentTile = this.StartingYardTile;
                this.IsInYard = true;
                CurrentTile.PlayerPieces.Remove(this);
            }

            #region Use this code if a future option to remove multiple landed on pieces is active
            /*List<PlayerPiece> removePiecesFromList = new List<PlayerPiece>();
            // Add a new movement for each of the pieces on this tile
            // TODO: if there is more than one opponents piece on the tile, should we send this player home instead of the opponent?
            foreach(PlayerPiece pp in CurrentTile.PlayerPieces)
            {
                newMovement = new PlayerPieceMovement
                {
                    PieceToMove = pp,
                    DestinationTile = pp.StartingYardTile,
                    InfoTextToDisplay = GameManager.instance.CurrentPlayerName + " landed on " + pp.PlayerName + ". Sending " + pp.gameObject.name + " back home"
                };

                movementList.Add(newMovement);

                // set the player going home as having CurrentTile of their yard tile
                pp.CurrentTile = pp.StartingYardTile;
                pp.IsInYard = true;

                // add this piece to a list which will be removed after all pieces have been checked
                removePiecesFromList.Add(pp);
            }
            // remove the non-current player pieces from the list.  This has to be performed after the above to keep the integrity of the CurrentTile.PlayerPieces list
            // if we were to remove it during the loop, it would cause an error
            foreach(PlayerPiece pp in removePiecesFromList)
            {
                // remove the landed on piece from the tile
                CurrentTile.PlayerPieces.Remove(pp);
            }*/
            #endregion
        }
        else
        {
            #region Multiple Pieces on one Tile
            // the current tile doesn't contain any opponent pieces, does it include any of our own?
            if (CurrentTile.PlayerPieces.Count == 1)
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
            #endregion
        }

        // set this PlayerPiece to be on the CurrentTile
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
                InfoTextToDisplay = "Congratulations, " + this.gameObject.name + " has scored!",
                IsScoringMove = true
            };

            movementList.Add(newMovement);

            // remove this piece from the centre tile
            CurrentTile.PlayerPieces.Remove(this);
        }

        PlayerManager.instance.SetMovementList(movementList);
    }
}
