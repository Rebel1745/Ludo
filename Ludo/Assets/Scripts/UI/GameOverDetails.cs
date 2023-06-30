using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverDetails : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image colourImage;

    public void SetDetails(string name, Color col)
    {
        nameText.text = name;
        colourImage.color = col;
    }
}
