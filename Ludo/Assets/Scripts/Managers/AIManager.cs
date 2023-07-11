using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

    // AI Traits
    AIType currentAIType;
    float aggressionBonus;
    float cautionBonus;

    private void Awake()
    {
        instance = this;
    }

    void SetBonuses(AIType aiType)
    {
        currentAIType = aiType;

        switch (aiType)
        {
            case AIType.Standard:
                aggressionBonus = 0f;
                cautionBonus = 0f;
                break;
            case AIType.Aggressive:
                aggressionBonus = 0.5f;
                cautionBonus = -0.5f;
                break;
            case AIType.Cautious:
                aggressionBonus = -0.5f;
                cautionBonus = 0.5f;
                break;
            case AIType.Random:
                break;
        }
    }

    public void DoAI()
    {
        // set the aitype and the bonuses
        SetBonuses(PlayerManager.instance.Players[GameManager.instance.CurrentPlayerId].CPUType);

        List<PlayerPiece> legalPieces = new List<PlayerPiece>();

        // fisrt get all of the pieces that can make a move
        foreach (PlayerPiece pp in PlayerManager.instance.Players[GameManager.instance.CurrentPlayerId].PlayerPieces)
        {
            if (PlayerManager.instance.PlayerPieceHasLegalMove(pp))
                legalPieces.Add(pp);
        }

        // for testing, player 1 gets good AI, other players get random
        if(currentAIType != AIType.Random)
            SelectPieceToMove(legalPieces.ToArray());
        else
            legalPieces[Random.Range(0, legalPieces.Count)].BuildMovementList();
    }

    void SelectPieceToMove(PlayerPiece[] legalPieces)
    {
        PlayerPiece pieceToMove = null;
        float pieceGoodness = -Mathf.Infinity;
        float currentGoodness;

        foreach(PlayerPiece pp in legalPieces)
        {
            currentGoodness = GetPieceGoodness(pp);

            if(currentGoodness > pieceGoodness)
            {
                pieceGoodness = currentGoodness;
                pieceToMove = pp;
            }
        }
        
        pieceToMove.BuildMovementList();
    }

    float GetPieceGoodness(PlayerPiece pp)
    {
        Tile currentTile = pp.CurrentTile;
        Tile futureTile = BoardManager.instance.GetTileAhead(pp, GameManager.instance.DiceTotal);
        float goodness = 0f;

        // add a little random noise to the outcome
        //float goodness = Random.Range(-0.1f, 0.1f);

        // if we are currently safe, it is not important that we move
        if (currentTile.IsSafeTile)
            goodness -= 0.5f;

        // if we aren't already safe but we can be, we probably should
        if (!currentTile.IsSafeTile && (futureTile.IsSafeTile || futureTile.IsScoringTile))
            goodness += 0.8f;

        // if we are currently with friends we are safe
        if (currentTile.PlayerPieces.Count > 1)
            goodness -= 0.5f;

        // if there is a piece where we will land
        if(futureTile.PlayerPieces.Count > 0)
        {
            // if we would join our own piece on the tile it's good
            if (futureTile.PlayerPieces[0].PlayerId == pp.PlayerId)
                goodness += 0.5f;

            // if there is only one enemy piece on the tile we can bop them off
            else if (futureTile.PlayerPieces.Count == 1)
                goodness += 0.8f;

            // if there are multiple enemies on that tile, we will get bopped off
            else
                goodness -= 100f;
        }

        // special cases for a 6
        if(GameManager.instance.DiceTotal == 6)
        {
            PlayerPiece[] piecesInYard = PlayerManager.instance.GetPiecesInYard();
            PlayerPiece[] piecesOnBoard = PlayerManager.instance.GetPiecesOnBoard();
            PlayerPiece[] scoredPieces = PlayerManager.instance.GetScoredPieces();

            // if this is our 2nd 6 in a row, get another piece out for safety
            if (GameManager.instance.CurrentPlayerRollAgainCount == (GameManager.instance.MaximumRollAgain - 1) && pp.IsInYard)
                goodness += 0.5f;

            // if we only have one piece on the board, maybe get another piece out
            if (piecesOnBoard.Length == 1 && pp.IsInYard)
                goodness += 0.25f;
        }

        return goodness;
    }
}

public enum AIType
{
    Standard,
    Aggressive,
    Cautious,
    Random
}
