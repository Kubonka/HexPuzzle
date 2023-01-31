using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniHex : MonoBehaviour
{
    public Vector3 velocity;
    public float gravity;
    

    private void Start()
    {
        gravity = 9f;
    }
    void FixedUpdate()
    {

        // apply gravity 

        velocity.y -= gravity * Time.deltaTime;

        // calculate new position

        this.transform.position += velocity * Time.deltaTime;

    }
    
    
}
