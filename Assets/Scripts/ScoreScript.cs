using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour
{
    public BoardManager boardmanager;
    private TextMeshProUGUI score;
    void Start()
    {
        boardmanager.onScoreChange += OnScoreChangeEvent;
        score = this.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        score.text = "0";
    }

    private void OnScoreChangeEvent(int value)
    {
        int temp = int.Parse(score.text);
        temp = temp + value;
        score.text = temp.ToString();
    }
        
    void Update()
    {
        
    }
}
