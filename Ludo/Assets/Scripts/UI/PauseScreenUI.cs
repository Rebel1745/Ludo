using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreenUI : MonoBehaviour
{
    public void Back()
    {
        UIManager.instance.ShowHideUIElement(UIManager.instance.PauseScreenUI, false);
        GameManager.instance.RevertToPreviousState();
    }
}
