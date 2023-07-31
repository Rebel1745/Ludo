using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenUI : MonoBehaviour
{
    public void StartGame()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, false, GameManager.instance.State);
        GameManager.instance.UpdateGameState(GameState.BuildBoard);
    }

    public void ShowSettingsScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.StartScreenUI, false, GameManager.instance.State);
        GameManager.instance.UpdateGameState(GameState.SettingsScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
