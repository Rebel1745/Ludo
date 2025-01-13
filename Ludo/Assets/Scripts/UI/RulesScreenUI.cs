using UnityEngine;
using TMPro;

public class RulesScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _rulesText;
    private string _rulesTextString;

    private void OnEnable()
    {
        BuildRulesTextString();
    }

    private void BuildRulesTextString()
    {
        _rulesTextString = "Players take turns to throw the dice. In order to move one of their pieces from their yard (their starting position off the board) onto the start position, they must throw a ";

        if (SettingsManager.instance.Use1Or6ToEscapeYard) _rulesTextString += "1 (only if all of the player's pieces are in their yard) or a 6 (this can be changed to only a 6 in the settings). ";
        else _rulesTextString += "6 (this can be changed to require a 6 OR 1 to exit the yard). ";

        _rulesTextString += "If a 6 is thrown, the player is then entitled to one further move. ";

        if (SettingsManager.instance.Use1Or6ToEscapeYard) _rulesTextString += "Note: this only happens if a 6 is rolled.  If a 1 is rolled, play moves on to the next player. ";

        _rulesTextString += "If three 6's are thrown in an row, the player's piece furthest along the board (that is not in the player's safe coloured squares) will be sent back to the yard. ";

        _rulesTextString += "\n\nIf a player lands on a square that is occupied by a piece of another player, the other player's piece is sent back to their yard. However, if two or more of another player's pieces are on the same square, they are considered safe, and the player landing on those pieces must return to their yard.\n\n";
        _rulesTextString += "When a player has completed a circuit of the board, they may enter their safe zone, moving along the coloured squares towards the centre.\n\n";

        if (SettingsManager.instance.AutomaticallyMoveIfOneLegalMove) _rulesTextString += "If there is only one legal move that a player can make, this move will be automatically performed (this can be changed in the settings).\n\n";

        if (SettingsManager.instance.FinishGameAfterOnePlayerFinishes) _rulesTextString += "The first player to get all of their pieces home is the winner.";
        else _rulesTextString += "The game finishes when three of the four players have all of their pieces home (this can be changed to only requiring one player to have all of their pieces home to end the game).";

        _rulesText.text = _rulesTextString;
    }

    public void Back()
    {
        gameObject.GetComponent<UINavigation>().CloseUIAndRevertToReferer();
    }
}
