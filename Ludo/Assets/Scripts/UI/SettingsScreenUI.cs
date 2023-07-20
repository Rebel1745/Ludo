using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsScreenUI : MonoBehaviour
{
    public void Back()
    {
        SaveSettings();
        UIManager.instance.ShowHideUIElement(UIManager.instance.SettingsScreenUI, false);
        GameManager.instance.RevertToPreviousState();
    }

    void SaveSettings()
    {

    }
}
