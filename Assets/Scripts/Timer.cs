using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float currentTime;
    private float timeOut;

    public Timer(float _timeOut)
    {
        timeOut = _timeOut;
        currentTime = Time.time;
    }
    public bool CheckTime()
    {
        if (Time.time - currentTime > timeOut)
        {
            currentTime = Time.time;
            return true;
        }
        else
            return false;
    }
}
