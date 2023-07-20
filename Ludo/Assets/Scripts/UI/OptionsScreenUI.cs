using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsScreenUI : MonoBehaviour
{
    public void Back()
    {
        SaveOptions();
        UIManager.instance.ShowHideUIElement(UIManager.instance.OptionsScreenUI, false);
        GameManager.instance.RevertToPreviousState();
    }

    void SaveOptions()
    {

    }
}
