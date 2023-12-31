using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject PlayerSelectUI;
    public GameObject GameUI;
    public GameObject GameOverUI;
    public GameObject StartScreenUI;
    public GameObject SettingsScreenUI;
    public GameObject PauseScreenUI;

    // Colours for the info box for each player
    public Color[] PlayerInfoBoxColours;
    public Color[] PlayerInfoTextColours;
    public Image InfoBoxImage;
    public TMP_Text InfoText;
    public Texture2D[] CursorImages;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeInfoBoxColour(int playerId)
    {
        InfoBoxImage.color = PlayerInfoBoxColours[playerId];
        InfoText.color = PlayerInfoTextColours[playerId];
        Cursor.SetCursor(CursorImages[playerId], Vector2.zero, CursorMode.Auto);
    }

    public void ShowHideUIElement(GameObject element, bool show, GameState referer)
    {
        element.SetActive(show);

        UINavigation nav = element.GetComponent<UINavigation>();
        if(nav != null)
        {
            if (show)
                nav.SetReferer(referer);
            else
                nav.CloseUIAndRevertToReferer();
        }
    }

    public void PlayAgain()
    {
        PlayerManager.instance.ResetGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        GameManager.instance.UpdateGameState(GameState.StartScreen);
    }
}
