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
    public Vector3 startingPosition;
    public Tile StartingTile;
    public bool IsInYard = true; // all pieces start in their yard
    public bool IsScored = false;
    Tile currentTile;

    bool isAnimating = false;
    bool isCurrentPlayerAnimation = false;
    int spacesToMove;

    // movement variables
    Tile[] moveQueue;
    int moveQueueIndex;
    Vector3 targetPosition;
    Vector3 velocity = Vector3.zero;
    float smoothTime = 0.25f;
    [SerializeField] int smoothTimeMultiplier = 1;
    float smoothDistance = 0.01f;
    float maxHeight = 0.5f;
    [SerializeField] int maxHeightMultiplier = 1;
    [SerializeField] AnimationCurve heightCurve;
    Vector3 targetPositionWithHeight;
    float heightTime;
    float targetHeight;

    void Start()
    {
        targetPosition = this.transform.position;
    }

    void Update()
    {
        MovePiece();
    }

    void MovePiece()
    {
        // have we finished animating?
        if (!isAnimating)
            return;

        if (Vector3.Distance(this.transform.position, targetPosition) < smoothDistance)
        {
            AdvanceMoveQueue();
        }

        targetHeight = heightCurve.Evaluate(heightTime / smoothTime) * maxHeight * maxHeightMultiplier;

        targetPositionWithHeight = new Vector3(targetPosition.x, targetPosition.y + targetHeight, targetPosition.z);

        this.transform.position = Vector3.SmoothDamp(this.transform.position, targetPositionWithHeight, ref velocity, (smoothTime / (float)smoothTimeMultiplier));

        heightTime += Time.deltaTime;
    }

    void AdvanceMoveQueue()
    {
        if (moveQueue != null && moveQueueIndex < moveQueue.Length)
        {
            SetNewTargetPosition(moveQueue[moveQueueIndex].transform.position);
            moveQueueIndex++;
        }
        else
        {
            //finished all animations, move to next game state
            isAnimating = false;
            if (isCurrentPlayerAnimation)
                MoveToNextTurn();
        }
    }

    // move this to player manager
    void MoveToNextTurn()
    {
        //allow another roll if a 6 was rolled, otherwise move on to next turn
        if (spacesToMove == 6)
            GameManager.instance.UpdateGameState(GameState.RollAgain);
        else
            GameManager.instance.UpdateGameState(GameState.NextTurn);
    }

    void SetNewTargetPosition(Vector3 pos)
    {
        // update the info text to count to the dice total
        if (moveQueueIndex == 0)
            GameManager.instance.SetInfoText("1");
        else
        {
            GameManager.instance.SetInfoText(GameManager.instance.GetInfoText() + " ... " + (moveQueueIndex + 1).ToString());
        }

        targetPosition = pos;
        velocity = Vector3.zero;
        heightTime = 0f;
    }

    private void OnMouseUp()
    {
        // are we waiting for a click?
        if (GameManager.instance.State != GameState.WaitingForClick)
            return;

        // Check if this piece belongs to us, if not then we can't click on it
        if (PlayerId != GameManager.instance.CurrentPlayerId)
            return;

        SelectPiece();
    }

    void SelectPiece()
    {
        // move this piece
        spacesToMove = GameManager.instance.DiceTotal;

        // if we haven't rolled a 6, and this piece is in its yard, bail
        if (IsInYard && spacesToMove != 6)
            return;

        isCurrentPlayerAnimation = true;

        // if we are on a tile then remove this piece from it
        if (currentTile != null)
            currentTile.PlayerPiece = null;

        // if a 6 is rolled, and this piece is in its yard, move it to the starting tile
        if (IsInYard && spacesToMove == 6)
        {
            moveQueue = new Tile[1];
            moveQueue[0] = StartingTile;
            currentTile = StartingTile;
            IsInYard = false;
        }
        else
        {
            moveQueue = new Tile[spacesToMove];

            for (int i = 0; i < spacesToMove; i++)
            {
                moveQueue[i] = currentTile.NextTile;
                currentTile = moveQueue[i];
            }
        }

        moveQueueIndex = 0;
        isAnimating = true;

        // we have clicked on a valid piece, set it moving
        GameManager.instance.UpdateGameState(GameState.WaitingForAnimation);
    }

    private void SendPieceBackToYard()
    {
        throw new NotImplementedException();
    }
}
