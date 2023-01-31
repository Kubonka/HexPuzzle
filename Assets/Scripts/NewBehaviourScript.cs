using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(coroutineA());
    }

    IEnumerator coroutineA()
    {
        // wait for 1 second
        Debug.Log("coroutineA created");
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(coroutineB());
        Debug.Log("coroutineA running again");
    }

    IEnumerator coroutineB()
    {
        Debug.Log("coroutineB created");
        yield return new WaitForSeconds(1f);
        Debug.Log("coroutineB enables coroutineA to run");
    }


    // Update is called once per frame
    void Update()
    {

    }
}

