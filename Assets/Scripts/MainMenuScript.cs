using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public Canvas canvas;
    public event Action onNewGame;
    void Start()
    {
        
    }    
    
    public void NewGame()
    {
        onNewGame?.Invoke();
    }
    public void QuitGame()
    {       

    }
}
