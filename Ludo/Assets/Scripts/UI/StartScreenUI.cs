using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenUI : MonoBehaviour
{
    public void StartGame()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, false);
        GameManager.instance.UpdateGameState(GameState.BuildBoard);
    }

    public void ShowOptionsScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, false);
        GameManager.instance.UpdateGameState(GameState.OptionsScreen);
    }

    public void ShowSettingsScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, false);
        GameManager.instance.UpdateGameState(GameState.SettingsScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
