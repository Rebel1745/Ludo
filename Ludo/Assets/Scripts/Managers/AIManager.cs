using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
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
                aggressionBonus = 1f;
                cautionBonus = -0.5f;
                break;
            case AIType.Cautious:
                aggressionBonus = -0.5f;
                cautionBonus = 1f;
                break;
            case AIType.WildCard:
                aggressionBonus = Random.Range(-1f, 1f);
                cautionBonus = Random.Range(-1f, 1f);
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
        if (currentAIType != AIType.Random)
            SelectPieceToMove(legalPieces.ToArray());
        else
            legalPieces[Random.Range(0, legalPieces.Count)].BuildMovementList();
    }

    void SelectPieceToMove(PlayerPiece[] legalPieces)
    {
        PlayerPiece pieceToMove = null;
        float pieceGoodness = -Mathf.Infinity;
        float currentGoodness;

        if (legalPieces.Length == 1)
        {
            legalPieces[0].BuildMovementList();
        }
        else
        {
            foreach (PlayerPiece pp in legalPieces)
            {
                currentGoodness = GetPieceGoodness(pp);

                if (currentGoodness > pieceGoodness)
                {
                    pieceGoodness = currentGoodness;
                    pieceToMove = pp;
                }
            }

            pieceToMove.BuildMovementList();
        }
    }

    float GetPieceGoodness(PlayerPiece pp)
    {
        Tile currentTile = pp.CurrentTile;
        Tile futureTile = BoardManager.instance.GetTileAhead(pp, DiceManager.instance.DiceTotal);

        // add a little random noise to the outcome
        float goodness = Random.Range(-0.1f, 0.1f);
        //float goodness = 0f;

        // if we are currently safe, it is not important that we move (unless we can score)
        if (currentTile.IsSafeTile && !futureTile.IsScoringTile)
            goodness += -0.5f;
        // if our finishing position is dependent on how many pieces score, prioritise scoring
        else if (futureTile.IsScoringTile && SettingsManager.instance.FinishGameAfterOnePlayerFinishes)
            goodness += 1f;

        // if we aren't already safe but we can be, we probably should
        if (!currentTile.IsSafeTile && (futureTile.IsSafeTile || futureTile.IsScoringTile))
            goodness += 0.8f + cautionBonus;

        // if we are currently with ONE other piece we are safe, unless we are already safe 
        if (currentTile.PlayerPieces.Count == 2 && !currentTile.IsSafeTile)
            goodness += -0.5f - cautionBonus;

        // if there is a piece where we will land
        if (futureTile.PlayerPieces.Count > 0)
        {
            // if we would join our ONE own piece on the tile it's good
            if (futureTile.PlayerPieces[0].PlayerId == pp.PlayerId && futureTile.PlayerPieces.Count == 1)
                goodness += 0.5f + cautionBonus;

            // if there is only one enemy piece on the tile we can bop them off
            else if (futureTile.PlayerPieces.Count == 1)
                goodness += 0.8f + aggressionBonus;

            // if there are multiple enemies on that tile, we will get bopped off
            else
                goodness -= 100f;
        }

        if (!currentTile.IsSafeTile)
        {
            // calculate how far around the board the piece is
            float percentComplete = pp.TotalDistanceTravelled / BoardManager.instance.SurroundingTileCount;
            goodness += (percentComplete / 2f) + cautionBonus;
        }

        // special cases for a 6
        if (DiceManager.instance.DiceTotal == 6)
        {
            PlayerPiece[] piecesInYard = PlayerManager.instance.GetPiecesInYard();
            PlayerPiece[] piecesOnBoard = PlayerManager.instance.GetPiecesOnBoard();
            PlayerPiece[] scoredPieces = PlayerManager.instance.GetScoredPieces();

            // if this is our 2nd 6 in a row, get another piece out for safety
            if (GameManager.instance.CurrentPlayerRollAgainCount == (GameManager.instance.MaximumRollAgain - 1) && pp.IsInYard)
                goodness += 0.5f + cautionBonus;

            // if we only have one piece on the board, maybe get another piece out
            if (piecesOnBoard.Length == 1 && pp.IsInYard)
                goodness += 0.25f + cautionBonus;
        }

        return goodness;
    }
}

public enum AIType
{
    Standard,
    Aggressive,
    Cautious,
    WildCard,
    Random
}
