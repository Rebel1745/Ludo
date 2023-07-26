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

    public void ShowHideUIElement(GameObject element, bool show)
    {
        element.SetActive(show);
    }

    public void PlayAgain()
    {
        PlayerManager.instance.ResetGame();
    }
}
