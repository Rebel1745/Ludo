using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverDetails : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image colourImage;
    public Player player { get; protected set; }

    public void SetDetails(Player p, string name, Color col)
    {
        player = p;
        nameText.text = name;
        colourImage.color = col;
    }
}
