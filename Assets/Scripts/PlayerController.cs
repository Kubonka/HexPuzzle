using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public event Action onMouseLeftPressed;
    public event Action<int> onRotatePressed;
    public event Action onMenuSelect;
    public BoardManager boardmanager;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (boardmanager.currentState == BoardManager.GameState.PlayerTurn)
        {
            //BOARD CONTROL
            if (Input.GetMouseButtonDown(0))
            {
                onMouseLeftPressed?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                onRotatePressed?.Invoke(-1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                onRotatePressed?.Invoke(1);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                onMenuSelect?.Invoke();
            }
        }
        //AGREGAR CAMERA CONTROLS 
    }
}
