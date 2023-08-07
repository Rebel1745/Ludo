using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject PlayerSelectUI;
    public GameObject GameUI;
    public GameObject GameOverUI;
    public GameObject StartScreenUI;
    public GameObject SettingsScreenUI;
    public GameObject PauseScreenUI;

    private void Awake()
    {
        instance = this;
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
