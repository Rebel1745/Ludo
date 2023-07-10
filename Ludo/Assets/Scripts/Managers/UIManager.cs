using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] GameObject playerSelectUI;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject gameOverUI;

    private void Awake()
    {
        instance = this;
    }

    public void ShowPlayerSelectScreen()
    {
        playerSelectUI.SetActive(true);
    }

    public void HidePlayerSelectScreen()
    {
        playerSelectUI.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
    }

    public void HideGameOverUI()
    {
        gameOverUI.SetActive(false);
    }

    public void ShowGameUI()
    {
        gameUI.SetActive(true);
    }

    public void PlayAgain()
    {
        PlayerManager.instance.ResetGame();
    }
}
