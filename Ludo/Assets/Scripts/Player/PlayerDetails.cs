using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDetails : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_Dropdown cpuDropdown;
    [SerializeField] string defaultPlayerName;
    public string PlayerName;
    public bool isCPU = false;

    // for loading and saving PlayerPrefs
    string playerNameString;
    string playerCPUString;

    private void Start()
    {
        playerNameString = defaultPlayerName + "name";
        playerCPUString = defaultPlayerName + "cpu";
        //ResetPlayerDetails();
        LoadPlayerDetails();
    }

    void SetDefaultPlayerName(string dPName)
    {
        playerNameInputField.text = dPName;
        SetPlayerName(dPName);
    }

    void SetDefaultCPU(string cpu)
    {
        if(cpu == "False")
        {
            cpuDropdown.value = 0;
            isCPU = false;
        }
        else
        {
            cpuDropdown.value = 1;
            isCPU = true;
        }
    }

    public void SetPlayerName(string newPlayerName)
    {
        PlayerName = newPlayerName;
    }

    public void EndEditPlayerName(string newPlayerName)
    {
        if (newPlayerName == "")
        {
            SetPlayerName(defaultPlayerName);
            playerNameInputField.text = defaultPlayerName;
        }
    }

    public void SetPlayerType(int type)
    {
        if (type == 0)
            isCPU = false;
        else
            isCPU = true;
    }

    public void SavePlayerDetails()
    {
        PlayerPrefs.SetString(playerNameString, PlayerName);
        PlayerPrefs.SetString(playerCPUString, isCPU.ToString());
    }

    void LoadPlayerDetails()
    {
        SetDefaultPlayerName( PlayerPrefs.GetString(playerNameString, defaultPlayerName));
        SetDefaultCPU( PlayerPrefs.GetString(playerCPUString, "False"));
    }

    void ResetPlayerDetails()
    {
        PlayerPrefs.DeleteKey(playerNameString);
        PlayerPrefs.DeleteKey(playerCPUString);
    }
}
