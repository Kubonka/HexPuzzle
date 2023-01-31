using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RemainingTilesScript : MonoBehaviour
{
    public BoardManager boardmanager;
    private TextMeshProUGUI remainingTiles;
    void Start()
    {
        boardmanager.onRemainingTilesChange += ChangeTextValue;
        remainingTiles = this.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        remainingTiles.text = "0";
    }

    
    void Update()
    {
        
    }
    private void ChangeTextValue(int value)
    {
        int temp = int.Parse(remainingTiles.text);
        temp = temp + value;
        remainingTiles.text = temp.ToString();
    }
}
