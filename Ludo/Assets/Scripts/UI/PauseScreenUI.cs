using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UINavigation))]
public class PauseScreenUI : MonoBehaviour
{
    public void Back()
    {
        gameObject.GetComponent<UINavigation>().CloseUIAndRevertToReferer();
    }

    public void ShowSettingsScreen()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.PauseScreenUI, false, GameState.PauseScreen);
        GameManager.instance.UpdateGameState(GameState.SettingsScreen);
    }

    public void ShowHidePauseMenu()
    {
        if (UIManager.instance.PauseScreenUI.activeSelf)
        {
            gameObject.GetComponent<UINavigation>().CloseUIAndRevertToReferer();
        }
        else
        {
            GameManager.instance.UpdateGameState(GameState.PauseScreen);
        }
    }
}
