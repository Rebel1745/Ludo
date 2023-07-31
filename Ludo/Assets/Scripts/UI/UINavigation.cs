using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINavigation : MonoBehaviour
{
    GameState refererState;

    public void CloseUIAndRevertToReferer()
    {
        gameObject.SetActive(false);
        GameManager.instance.UpdateGameState(refererState);
    }

    public void SetReferer(GameState referer)
    {
        refererState = referer;
    }
}
