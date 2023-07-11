using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerDetails : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_Dropdown cpuDropdown;
    [SerializeField] GameObject cpuTypeHolder;
    [SerializeField] TMP_Dropdown cpuTypeDropdown;
    [SerializeField] string defaultPlayerName;
    public Image ColourImage;
    public string PlayerName;
    public bool isCPU = false;
    public int CPUType = 0;

    // for loading and saving PlayerPrefs
    string playerNameString;
    string playerCPUString;
    string playerCPUTypeString;

    private void Start()
    {
        playerNameString = defaultPlayerName + "name";
        playerCPUString = defaultPlayerName + "cpu";
        playerCPUTypeString = defaultPlayerName + "cputype";
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
            cpuTypeHolder.SetActive(false);
        }
        else
        {
            cpuDropdown.value = 1;
            isCPU = true;
            cpuTypeHolder.SetActive(true);
        }
    }

    void SetDefaultCPUType(int type)
    {
        cpuTypeDropdown.value = type;
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
        {
            isCPU = false;
            cpuTypeHolder.SetActive(false);
        }
        else
        {
            isCPU = true;
            cpuTypeHolder.SetActive(true);
        }            
    }

    public void SetPlayerCPUType(int type)
    {
        CPUType = type;
    }

    public void SavePlayerDetails()
    {
        PlayerPrefs.SetString(playerNameString, PlayerName);
        PlayerPrefs.SetString(playerCPUString, isCPU.ToString());
        PlayerPrefs.SetInt(playerCPUTypeString, CPUType);
    }

    void LoadPlayerDetails()
    {
        SetDefaultPlayerName( PlayerPrefs.GetString(playerNameString, defaultPlayerName));
        SetDefaultCPU( PlayerPrefs.GetString(playerCPUString, "False"));
        SetDefaultCPUType( PlayerPrefs.GetInt(playerCPUTypeString, 0));
    }

    void ResetPlayerDetails()
    {
        PlayerPrefs.DeleteKey(playerNameString);
        PlayerPrefs.DeleteKey(playerCPUString);
        PlayerPrefs.DeleteKey(playerCPUTypeString);
    }
}
